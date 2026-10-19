#!/usr/bin/env python3
"""Local, push-triggered Unity WebGL publisher. Credentials never enter Git."""
import argparse
import ctypes
import fcntl
import hashlib
import json
import os
from pathlib import Path
import plistlib
import re
import shlex
import shutil
import subprocess
import sys
import tarfile
import tempfile
import time
import uuid

STATE = Path.home() / "Library/Application Support/EchoProtocol/WebDeploy"
LABEL = "io.echoprotocol.web-deploy"
TARGET = "lautigr116/echo-protocol:web"
ROOTS = ("Assets", "Packages", "ProjectSettings", "Docs/CREDITS.md", "ArtSource/Design/Folio/Fonts")
PRIVATE = (
    "Assets/_Game/Art/Embodiment/RenderpeoplePrivate",
    "Assets/_Game/Art/Embodiment/RenderpeoplePrivate.meta",
    "Assets/_Game/Prefabs/Embodiment", "Assets/_Game/Prefabs/Embodiment.meta",
    "Assets/_Game/Prefabs/Impact", "Assets/_Game/Prefabs/Impact.meta",
    "Assets/_Game/Scenes/Production/EchoProtocol_Incursion.unity",
    "Assets/_Game/Scenes/Production/EchoProtocol_Incursion.unity.meta",
)
IDENTITY = Path.home() / "Library/Application Support/itch/butler_creds"


def run(args, cwd=None, **kwargs):
    return subprocess.run([str(a) for a in args], cwd=cwd, check=True, **kwargs)


def git(repo, *args):
    return run(["git", *args], cwd=repo, capture_output=True, text=True, timeout=60).stdout.strip()


def read(path, default=None):
    return json.loads(path.read_text()) if path.exists() else (default or {})


def write(path, data):
    temporary = path.with_name(path.name + "." + uuid.uuid4().hex + ".tmp")
    temporary.write_text(json.dumps(data, indent=2) + "\n")
    temporary.replace(path)


def log(message):
    print(time.strftime("%Y-%m-%d %H:%M:%S"), message, flush=True)


def clone_file(source, destination):
    """APFS copy-on-write keeps private snapshots and the build cache inexpensive."""
    source, destination = Path(source), Path(destination)
    if source.is_symlink():
        raise RuntimeError(f"Symlink is not a release input: {source.name}")
    destination.parent.mkdir(parents=True, exist_ok=True)
    if sys.platform == "darwin" and not destination.exists():
        clone = ctypes.CDLL(None, use_errno=True).clonefile
        clone.argtypes = [ctypes.c_char_p, ctypes.c_char_p, ctypes.c_int]
        if clone(os.fsencode(source), os.fsencode(destination), 0) == 0:
            return str(destination)
    return shutil.copy2(source, destination)


def digest_tree(root):
    digest = hashlib.sha256()
    for path in sorted(root.rglob("*")):
        if path.is_symlink():
            raise RuntimeError("Release inputs must not contain symlinks")
        if path.is_file():
            digest.update(path.relative_to(root).as_posix().encode() + b"\0")
            with path.open("rb") as stream:
                while block := stream.read(1024 * 1024):
                    digest.update(block)
            digest.update(b"\0")
    return digest.hexdigest()


def pushed_main(lines):
    for line in lines.splitlines():
        fields = line.split()
        if len(fields) == 4 and fields[2] == "refs/heads/main":
            sha = fields[1]
            if re.fullmatch(r"[0-9a-f]{40,64}", sha) and set(sha) != {"0"}:
                return sha
    return None


def private_stamp(repo):
    result = []
    for relative in PRIVATE:
        root = repo / relative
        paths = [root, *root.rglob("*")] if root.is_dir() else [root]
        for path in paths:
            if path.is_symlink():
                raise RuntimeError("Private release inputs must not contain symlinks")
            item = path.stat()
            result.append((str(path), item.st_size, item.st_mtime_ns))
    return sorted(result)


def snapshot(config, sha):
    repo = Path(config["repo"])
    git(repo, "cat-file", "-e", sha + "^{commit}")
    jobs = STATE / "jobs"
    jobs.mkdir(exist_ok=True)
    staging = Path(tempfile.mkdtemp(prefix=".snapshot-", dir=jobs))
    source = staging / "source"
    source.mkdir()
    try:
        # Export exactly the pushed commit, never dirty tracked working files.
        with tempfile.TemporaryFile() as archive:
            run(["git", "archive", "--format=tar", sha, *ROOTS], cwd=repo, stdout=archive)
            archive.seek(0)
            with tarfile.open(fileobj=archive) as tar:
                for member in tar.getmembers():
                    if not (member.isfile() or member.isdir()):
                        raise RuntimeError("Unsupported release archive entry")
                tar.extractall(source, filter="data")
        # git archive retains LFS pointers; hydrate from this Mac's local objects.
        lfs = Path(git(repo, "rev-parse", "--git-path", "lfs/objects"))
        if not lfs.is_absolute():
            lfs = repo / lfs
        for path in source.rglob("*"):
            if path.is_file() and path.stat().st_size < 1024:
                data = path.read_bytes()
                if data.startswith(b"version https://git-lfs.github.com/spec/v1\n"):
                    match = re.search(rb"oid sha256:([a-f0-9]{64})\nsize (\d+)", data)
                    if not match:
                        raise RuntimeError("Invalid LFS pointer")
                    oid = match[1].decode()
                    original = lfs / oid[:2] / oid[2:4] / oid
                    if not original.is_file() or original.stat().st_size != int(match[2]):
                        raise RuntimeError(f"Missing local LFS object for {path.relative_to(source)}")
                    path.unlink()
                    clone_file(original, path)
        # Only this approved private asset allowlist is copied from disk.
        # Capture it before the push, so subsequent Editor saves cannot alter it.
        before_private = private_stamp(repo)
        for relative in PRIVATE:
            original, destination = repo / relative, source / relative
            if not original.exists():
                raise RuntimeError(f"Missing private release input: {relative}")
            if original.is_dir():
                shutil.copytree(original, destination, copy_function=clone_file)
            else:
                clone_file(original, destination)
        if private_stamp(repo) != before_private:
            raise RuntimeError("Private assets changed during capture; save the Editor and enqueue again")
        digest = digest_tree(source)
        job = {"sha": sha, "digest": digest, "created": time.time(), "state": "queued", "attempts": 0}
        write(staging / "job.json", job)
        destination = jobs / (str(time.time_ns()) + "-" + sha[:12])
        staging.rename(destination)
        log(f"Queued web release {sha[:12]} (saved private scene included)")
        return destination
    except BaseException:
        shutil.rmtree(staging)
        raise


def status(job, phase, **extra):
    data = read(job / "job.json")
    data.update(state=phase, updated=time.time(), **extra)
    write(job / "job.json", data)
    write(STATE / "status.json", {"job": job.name, **data})
    log(f"{data['sha'][:12]}: {phase}")


def remote_head(config):
    value = git(config["repo"], "ls-remote", "--exit-code", "origin", "refs/heads/main")
    return value.split()[0]


def validate_output(output):
    report = read(Path(str(output) + "-report.json"))
    if report.get("result") != "Succeeded" or report.get("errors") != 0:
        raise RuntimeError("Unity did not produce a successful build report")
    for name in ("index.html", "echo-limiter.js", "CREDITS.txt"):
        if not (output / name).is_file():
            raise RuntimeError(f"Missing web output: {name}")
    for extension in ("*.wasm.gz", "*.data.gz", "*.framework.js.gz", "*.loader.js"):
        if not list((output / "Build").glob(extension)):
            raise RuntimeError(f"Missing web payload: {extension}")
    for path in output.rglob("*"):
        if path.is_symlink() or (path.is_file() and path.stat().st_size > 200_000_000):
            raise RuntimeError("Output exceeds the supported itch upload profile")


def build(config, job):
    output = job / "web"
    if (job / "built.json").exists():
        validate_output(output)
        return output
    project = STATE / "project"
    project.mkdir(exist_ok=True)
    # Never mirror into the user's project. Keep only the publisher's Library.
    run(["/usr/bin/rsync", "-a", "--delete", "--exclude=/Library/", "--exclude=/Logs/",
         "--exclude=/Temp/", "--exclude=/UserSettings/", str(job / "source") + "/", str(project) + "/"])
    if output.exists():
        shutil.rmtree(output)  # Only an incomplete, task-owned output is removed.
    status(job, "building")
    with (job / "build-launcher.log").open("w") as launcher:
        run(["/usr/bin/nice", "-n", "10", config["unity_cli"], "run", str(project),
             "--editor-version", "6000.3.21f1", "--timeout", "1800", "--",
             "-buildTarget", "WebGL", "-executeMethod", "EchoWebBuild.BuildAutomated",
             "-buildOutput", str(output), "-logFile", str(job / "unity.log")],
            stdout=launcher, stderr=subprocess.STDOUT, timeout=1860)
    shutil.copy2(project / "Docs/CREDITS.md", output / "CREDITS.txt")
    for license_file in (project / "ArtSource/Design/Folio/Fonts").glob("*OFL.txt"):
        shutil.copy2(license_file, output / license_file.name)
    data = read(job / "job.json")
    write(output / "release.json", {"commit": data["sha"], "inputs": data["digest"]})
    validate_output(output)
    write(job / "built.json", {"completed": time.time()})
    return output


def prune():
    # Bounded local storage. Keep the two most recent terminal jobs for diagnosis.
    terminal = []
    for job in (STATE / "jobs").iterdir():
        if (job / "job.json").exists() and read(job / "job.json").get("state") in (
                "published", "unchanged", "superseded", "failed", "push_not_confirmed"):
            terminal.append(job)
    for job in sorted(terminal, reverse=True)[2:]:
        shutil.rmtree(job)


def worker(config):
    if (STATE / "paused").exists():
        return
    with (STATE / "worker.lock").open("a") as lock:
        try:
            fcntl.flock(lock, fcntl.LOCK_EX | fcntl.LOCK_NB)
        except BlockingIOError:
            return
        pending = [(j, read(j / "job.json")) for j in (STATE / "jobs").iterdir()
                   if (j / "job.json").exists()]
        pending = [(j, d) for j, d in pending if d["state"] not in (
            "published", "unchanged", "superseded", "failed", "push_not_confirmed")]
        if not pending:
            return
        try:
            head = remote_head(config)
        except subprocess.SubprocessError:
            return  # Offline; launchd retries next minute, without starting Unity.
        matches = sorted((j for j, d in pending if d["sha"] == head), reverse=True)
        for job, data in pending:
            if data["sha"] != head and time.time() - data["created"] > 1800:
                status(job, "push_not_confirmed")
        if not matches:
            prune()
            return
        job = matches[0]
        for older in matches[1:]:
            status(older, "superseded")
        data = read(job / "job.json")
        if read(STATE / "published.json").get("digest") == data["digest"]:
            status(job, "unchanged")
            prune()
            return
        if not IDENTITY.is_file():
            if data["state"] != "needs_login":
                status(job, "needs_login")
            return
        if data.get("retry_after", 0) > time.time():
            return
        try:
            output = build(config, job)
            if (STATE / "paused").exists():
                status(job, "queued")
                return
            if remote_head(config) != data["sha"]:
                status(job, "superseded")
                return
            status(job, "uploading")
            with (job / "butler.log").open("a") as upload_log:
                run([config["butler"], "-i", str(IDENTITY), "push", str(output), TARGET,
                     "--userversion", data["sha"][:12] + "-" + data["digest"][:8]],
                    stdout=upload_log, stderr=subprocess.STDOUT, timeout=900)
            write(STATE / "published.json", {"sha": data["sha"], "digest": data["digest"],
                                              "published": time.time(), "job": job.name})
            status(job, "published")
        except Exception as error:
            phase = read(job / "job.json")["state"]
            attempts = data.get("attempts", 0) + 1
            # Do not repeatedly compile a broken game. Retry only an upload failure.
            retry = phase == "uploading" and attempts < 3
            status(job, "upload_retry" if retry else "failed", attempts=attempts,
                   retry_after=time.time() + 300, error=str(error))
        finally:
            prune()


def hook(config, remote, url):
    lines = sys.stdin.read()
    backup = STATE / "pre-push.original"
    if backup.exists():
        result = subprocess.run([str(backup), remote, url], input=lines, text=True)
        if result.returncode:
            return result.returncode
    sha = pushed_main(lines) if remote == "origin" else None
    if sha:
        try:
            snapshot(config, sha)
        except Exception as error:
            print(f"Echo web publication was not queued: {error}", file=sys.stderr)
            print("Git push can continue. Run web_deploy.py enqueue to retry.", file=sys.stderr)
    return 0


def install(repo):
    if sys.platform != "darwin" or sys.version_info < (3, 12):
        raise RuntimeError("This installer needs macOS and Python 3.12 or later")
    if git(repo, "remote", "get-url", "origin") != "https://github.com/LautiGR116/EchoProtocol.git":
        raise RuntimeError("Unexpected origin; refusing to install publication hook")
    hooks = Path(git(repo, "rev-parse", "--git-path", "hooks"))
    if not hooks.is_absolute():
        hooks = repo / hooks
    hooks.mkdir(parents=True, exist_ok=True)
    target = hooks / "pre-push"
    backup = STATE / "pre-push.original"
    marker = "# Echo Protocol local web publisher"
    if target.exists() and marker not in target.read_text():
        if backup.exists():
            raise RuntimeError("An unknown hook replaced the installer; inspect it before reinstalling")
        shutil.copy2(target, backup)
    runtime = STATE / "web_deploy.py"
    if Path(__file__).resolve() != runtime.resolve():
        shutil.copy2(__file__, runtime)
    config = {"repo": str(repo), "unity_cli": str(Path.home() / ".unity/bin/unity"),
              "butler": str(STATE / "bin/butler"), "hook": str(target)}
    if not Path(config["butler"]).is_file():
        raise RuntimeError("Install the official butler executable into WebDeploy/bin first")
    write(STATE / "config.json", config)
    target.write_text("#!/bin/sh\n" + marker + "\nexec " + shlex.quote(sys.executable) + " " +
                      shlex.quote(str(runtime)) + ' hook "$@"\n')
    target.chmod(0o755)
    (STATE / "jobs").mkdir(exist_ok=True)
    agent = Path.home() / "Library/LaunchAgents" / (LABEL + ".plist")
    agent.parent.mkdir(parents=True, exist_ok=True)
    agent.write_bytes(plistlib.dumps({"Label": LABEL,
        "ProgramArguments": [sys.executable, str(runtime), "worker"],
        "StartInterval": 60, "WatchPaths": [str(STATE / "jobs")],
        "EnvironmentVariables": {"PATH": "/opt/homebrew/bin:/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin"},
        "StandardOutPath": str(STATE / "worker.log"), "StandardErrorPath": str(STATE / "worker.log")}))
    subprocess.run(["launchctl", "bootout", f"gui/{os.getuid()}/{LABEL}"], capture_output=True)
    run(["launchctl", "bootstrap", f"gui/{os.getuid()}", str(agent)])
    log("Installed local pre-push queue and launch agent; original LFS hook preserved")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("command", choices=("install", "hook", "worker", "enqueue", "status", "pause", "resume", "retry", "uninstall"))
    parser.add_argument("extra", nargs="*")
    args = parser.parse_args()
    STATE.mkdir(parents=True, exist_ok=True, mode=0o700)
    STATE.chmod(0o700)
    config = read(STATE / "config.json")
    if args.command == "install":
        install(Path(git(Path.cwd(), "rev-parse", "--show-toplevel")))
    elif args.command == "hook":
        return hook(config, *args.extra)
    elif args.command == "worker":
        worker(config)
    elif args.command == "enqueue":
        snapshot(config, git(config["repo"], "rev-parse", "HEAD"))
    elif args.command == "status":
        print(json.dumps({"paused": (STATE / "paused").exists(),
            "latest": read(STATE / "status.json"), "published": read(STATE / "published.json")}, indent=2))
    elif args.command == "pause":
        (STATE / "paused").touch()
        log("Paused: queued releases wait; a running build can finish")
    elif args.command in ("resume", "retry"):
        (STATE / "paused").unlink(missing_ok=True)
        if args.command == "retry":
            job = STATE / "jobs" / read(STATE / "status.json")["job"]
            status(job, "queued", attempts=0, retry_after=0)
        run(["launchctl", "kickstart", f"gui/{os.getuid()}/{LABEL}"])
    elif args.command == "uninstall":
        (STATE / "paused").touch()
        run(["launchctl", "bootout", f"gui/{os.getuid()}/{LABEL}"])
        target = Path(config["hook"])
        if "# Echo Protocol local web publisher" not in target.read_text():
            raise RuntimeError("The hook changed; leaving it untouched")
        backup = STATE / "pre-push.original"
        if backup.exists():
            shutil.copy2(backup, target)
        else:
            target.unlink()
        (Path.home() / "Library/LaunchAgents" / (LABEL + ".plist")).unlink(missing_ok=True)
        log("Uninstalled; builds and credentials retained locally")
    return 0


if __name__ == "__main__":
    sys.exit(main())
