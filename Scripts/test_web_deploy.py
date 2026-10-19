"""Focused publication guards; no Unity session or network requests."""
import contextlib
import io
import json
from pathlib import Path
import tempfile
import unittest
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


if __name__ == "__main__":
    unittest.main()
