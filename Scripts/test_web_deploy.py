"""Focused publication guards; no Unity session or network requests."""
import contextlib
import io
import json
from pathlib import Path
import tempfile
import unittest
from types import SimpleNamespace
from unittest.mock import patch

import web_deploy as deploy


class PublicationGuards(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary.name)
        self.addCleanup(self.temporary.cleanup)
        self.state = patch.object(deploy, "STATE", self.root)
        self.state.start()
        self.addCleanup(self.state.stop)
        self.identity = self.root / "identity"
        self.identity.touch()
        identity = patch.object(deploy, "IDENTITY", self.identity)
        identity.start()
        self.addCleanup(identity.stop)
        self.job = self.root / "jobs/1"
        self.job.mkdir(parents=True)
        self.data = {"sha": "a" * 40, "digest": "inputs-a", "state": "queued",
                     "created": deploy.time.time(), "attempts": 0}
        deploy.write(self.job / "job.json", self.data)
        self.config = {"repo": str(self.root), "butler": "/unused/butler"}
        quiet = contextlib.redirect_stdout(io.StringIO())
        quiet.__enter__()
        self.addCleanup(quiet.__exit__, None, None, None)

    def phase(self):
        return deploy.read(self.job / "job.json")["state"]

    def test_main_updates_only(self):
        sha = "a" * 40
        self.assertEqual(deploy.pushed_main(f"refs/heads/main {sha} refs/heads/main {'b'*40}"), sha)
        self.assertIsNone(deploy.pushed_main(f"refs/heads/dev {sha} refs/heads/dev {'b'*40}"))
        self.assertIsNone(deploy.pushed_main(f"(delete) {'0'*40} refs/heads/main {sha}"))

    @patch.object(deploy, "build")
    @patch.object(deploy, "remote_head", return_value="b" * 40)
    def test_failed_push_does_not_build(self, remote, build):
        deploy.worker(self.config)
        build.assert_not_called()
        self.assertEqual(self.phase(), "queued")

    @patch.object(deploy, "build")
    @patch.object(deploy, "remote_head", return_value="a" * 40)
    def test_unchanged_inputs_skip_build(self, remote, build):
        deploy.write(self.root / "published.json", {"digest": "inputs-a"})
        deploy.worker(self.config)
        build.assert_not_called()
        self.assertEqual(self.phase(), "unchanged")

    @patch.object(deploy, "run")
    @patch.object(deploy, "build", side_effect=RuntimeError("compile failed"))
    @patch.object(deploy, "remote_head", return_value="a" * 40)
    def test_build_failure_never_uploads_or_retries_compilation(self, remote, build, run):
        deploy.worker(self.config)
        deploy.worker(self.config)
        self.assertEqual(self.phase(), "failed")
        self.assertEqual(build.call_count, 1)
        run.assert_not_called()

    @patch.object(deploy, "run")
    @patch.object(deploy, "build", return_value=Path("/unused/output"))
    @patch.object(deploy, "remote_head", side_effect=["a" * 40, "b" * 40])
    def test_newer_push_suppresses_old_upload(self, remote, build, run):
        deploy.worker(self.config)
        self.assertEqual(self.phase(), "superseded")
        run.assert_not_called()

    @patch.object(deploy, "run", side_effect=RuntimeError("upload interrupted"))
    @patch.object(deploy, "build", return_value=Path("/unused/output"))
    @patch.object(deploy, "remote_head", return_value="a" * 40)
    def test_upload_failure_preserves_previous_release(self, remote, build, run):
        deploy.write(self.root / "published.json", {"digest": "previous"})
        deploy.worker(self.config)
        self.assertEqual(self.phase(), "upload_retry")
        self.assertEqual(deploy.read(self.root / "published.json")["digest"], "previous")

    @patch.object(deploy, "build")
    @patch.object(deploy, "remote_head", return_value="a" * 40)
    def test_missing_login_does_not_start_unnecessary_build(self, remote, build):
        self.identity.unlink()
        deploy.worker(self.config)
        self.assertEqual(self.phase(), "needs_login")
        build.assert_not_called()

    @patch.object(deploy, "remote_head")
    def test_pause_performs_no_network_work(self, remote):
        (self.root / "paused").touch()
        deploy.worker(self.config)
        remote.assert_not_called()

    def test_clone_does_not_change_source(self):
        source, destination = self.root / "original", self.root / "copy"
        source.write_text("original")
        deploy.clone_file(source, destination)
        destination.write_text("modified")
        self.assertEqual(source.read_text(), "original")

    def test_output_needs_successful_report(self):
        with self.assertRaises(RuntimeError):
            deploy.validate_output(self.root / "web")

    @patch.object(deploy, "run")
    @patch.object(deploy, "build", return_value=Path("/unused/output"))
    @patch.object(deploy, "remote_head", return_value="a" * 40)
    def test_successful_upload_is_not_yet_published(self, remote, build, run):
        deploy.worker(self.config)
        self.assertEqual(self.phase(), "processing")
        self.assertFalse((self.root / "published.json").exists())
        self.assertEqual(deploy.read(self.root / "uploaded.json")["digest"], "inputs-a")

    def processing_receipt(self):
        deploy.status(self.job, "processing")
        deploy.write(self.root / "uploaded.json", {**self.data, "job": self.job.name})

    @patch.object(deploy, "build")
    @patch.object(deploy, "run", return_value=SimpleNamespace(stdout='{"type":"result","value":{"channels":[]}}'))
    def test_missing_channel_waits_without_rebuilding_or_reuploading(self, run, build):
        self.processing_receipt()
        deploy.worker(self.config)
        self.assertEqual(self.phase(), "processing")
        build.assert_not_called()
        self.assertIn("status", run.call_args.args[0])

    @patch.object(deploy, "run")
    def test_only_matching_completed_head_confirms_publication(self, run):
        self.processing_receipt()
        channel = {"name": "web", "uploadId": 42, "head": {
            "id": 123, "state": "completed", "userVersion": "older-version"}}
        def response():
            return SimpleNamespace(stdout=json.dumps({"type": "result", "value": {"channels": [channel]}}))
        run.return_value = response()
        self.assertTrue(deploy.confirm_upload(self.config))
        self.assertFalse((self.root / "published.json").exists())
        channel["head"]["userVersion"] = deploy.release_version(self.data)
        run.return_value = response()
        self.assertFalse(deploy.confirm_upload(self.config))
        self.assertEqual(self.phase(), "published")
        self.assertEqual(deploy.read(self.root / "published.json")["build_id"], 123)

    @patch.object(deploy, "run")
    def test_processing_failure_preserves_previous_release(self, run):
        self.processing_receipt()
        deploy.write(self.root / "published.json", {"digest": "previous"})
        run.return_value = SimpleNamespace(stdout=json.dumps({"type": "result", "value": {"channels": [
            {"name": "web", "pending": {"state": "failed", "userVersion": deploy.release_version(self.data)}}]}}))
        self.assertFalse(deploy.confirm_upload(self.config))
        self.assertEqual(self.phase(), "server_failed")
        self.assertEqual(deploy.read(self.root / "published.json")["digest"], "previous")

    @patch.object(deploy, "confirm_upload", return_value=True)
    @patch.object(deploy, "build")
    @patch.object(deploy, "remote_head", return_value="a" * 40)
    def test_new_push_waits_for_inflight_upload_even_when_reverting(self, remote, build, confirm):
        deploy.write(self.root / "published.json", {"digest": "inputs-a"})
        deploy.worker(self.config)
        self.assertEqual(self.phase(), "waiting_for_itch")
        build.assert_not_called()

    def test_snapshot_pins_commit_hydrates_lfs_and_captures_private_inputs(self):
        repo = self.root / "repository"
        (repo / "Assets").mkdir(parents=True)
        deploy.git(repo, "init", "-q")
        deploy.git(repo, "config", "user.email", "test@example.invalid")
        deploy.git(repo, "config", "user.name", "Publication test")
        (repo / "Assets/code.cs").write_text("committed")
        audio = b"local LFS audio fixture"
        oid = deploy.hashlib.sha256(audio).hexdigest()
        (repo / "Assets/clip.wav").write_text(
            f"version https://git-lfs.github.com/spec/v1\noid sha256:{oid}\nsize {len(audio)}\n")
        deploy.git(repo, "add", "Assets")
        deploy.git(repo, "commit", "-qm", "fixture")
        sha = deploy.git(repo, "rev-parse", "HEAD")
        lfs = repo / ".git/lfs/objects" / oid[:2] / oid[2:4] / oid
        lfs.parent.mkdir(parents=True)
        lfs.write_bytes(audio)
        (repo / "Assets/code.cs").write_text("dirty edit must not ship")
        (repo / "Assets/private.asset").write_text("saved private asset")
        with patch.object(deploy, "ROOTS", ("Assets",)), patch.object(deploy, "PRIVATE", ("Assets/private.asset",)):
            first = deploy.snapshot({"repo": str(repo)}, sha)
            self.assertEqual((first / "source/Assets/code.cs").read_text(), "committed")
            self.assertEqual((first / "source/Assets/clip.wav").read_bytes(), audio)
            self.assertEqual((first / "source/Assets/private.asset").read_text(), "saved private asset")
            self.assertEqual(deploy.snapshot({"repo": str(repo)}, sha), first)
            (repo / "Assets/private.asset").write_text("next saved scene")
            second = deploy.snapshot({"repo": str(repo)}, sha)
            self.assertNotEqual(first, second)
            self.assertEqual((first / "source/Assets/private.asset").read_text(), "saved private asset")


if __name__ == "__main__":
    unittest.main()
