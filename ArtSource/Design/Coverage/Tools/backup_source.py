"""Final Coverage source checkpoint: preserve layers and verify every restored byte."""
from pathlib import Path
import datetime
import hashlib
import json
import re
import tarfile

ROOT = Path(__file__).resolve().parents[4]
BACKUPS = ROOT.parent / "EchoProtocol-backups"
FOLDER = BACKUPS / "coverage-checkpoint-20261009"
PRIOR = BACKUPS / "coverage-baseline-20261009"
old = json.loads((PRIOR / "manifest.json").read_text())
layers = [(Path(x["archive"]), x["sha256"]) for x in old["recoveryPrerequisites"]]
layers.append((PRIOR / "source-overlay.tar.gz", old["overlaySha256"]))

def digest(path):
    h = hashlib.sha256()
    with path.open("rb") as f:
        for b in iter(lambda: f.read(4 * 1024 * 1024), b""):
            h.update(b)
    return h.hexdigest()

assert not FOLDER.exists() or not any(FOLDER.iterdir()), "Preserve any existing backup"
FOLDER.mkdir(exist_ok=True)
previous = {x["path"]: x for x in old["files"]}
files = []
for name in [".gitattributes", ".gitignore", "AGENTS.md", "ArtSource", "Assets", "Docs", "Packages", "ProjectSettings", "README.md"]:
    p = ROOT / name
    for f in sorted(p.rglob("*")) if p.is_dir() else [p]:
        if not f.is_file() or f.is_symlink():
            continue
        rel = f.relative_to(ROOT).as_posix()
        if (f.name == ".DS_Store" or "__pycache__" in f.parts or re.search(r"\.blend\d+$", f.name)) and rel not in previous:
            continue
        files.append({"path": rel, "bytes": f.stat().st_size, "sha256": digest(f)})
current = {x["path"]: x for x in files}
removed = sorted(set(previous) - set(current))
assert not removed, removed
changed = [x for x in files if x["path"] not in previous or x["sha256"] != previous[x["path"]]["sha256"]]
print("Complete source manifest", len(files), "overlay", len(changed), "removals", len(removed), flush=True)
archive = FOLDER / "source-overlay.tar.gz"
with tarfile.open(archive, "w:gz", compresslevel=6) as t:
    for x in changed:
        t.add(ROOT / x["path"], arcname=x["path"], recursive=False)
sha = digest(archive)
for path, expected in layers:
    assert digest(path) == expected, str(path)
print("Source overlay written;", len(layers), "preserved layer hashes checked", flush=True)
restored = {}
counts = []
for path in [p for p, _ in layers] + [archive]:
    count = 0
    with tarfile.open(path, "r|gz") as t:
        for m in t:
            if not m.isfile():
                continue
            name = m.name.removeprefix("./")
            h, size = hashlib.sha256(), 0
            with t.extractfile(m) as f:
                for b in iter(lambda: f.read(4 * 1024 * 1024), b""):
                    h.update(b)
                    size += len(b)
            restored[name] = {"sha256": h.hexdigest(), "bytes": size}
            count += 1
    counts.append({"archive": str(path), "regularFilesRead": count})
    print("Read back", path.parent.name, count, flush=True)
for x in files:
    assert restored.get(x["path"]) == {"sha256": x["sha256"], "bytes": x["bytes"]}, x["path"]
assert set(restored) == set(current), "Extra restored paths"
manifest = {
    "createdUTC": datetime.datetime.now(datetime.timezone.utc).isoformat(),
    "checkpoint": "Coverage", "scene": "Assets/_Game/Scenes/Production/EchoProtocol_Coverage.unity",
    "checkpointKey": "EP.CampaignCheckpoint.Coverage.v1",
    "recoveryPrerequisites": [{"archive": str(p), "sha256": s} for p, s in layers],
    "overlaySha256": sha, "fileCount": len(files), "overlayFileCount": len(changed), "removedFiles": removed,
    "scope": "Project-owned sources/settings/docs/evidence. Excludes caches/builds/Git/new bytecode/Blender autosaves/symlinks. One inherited bytecode file retained for earlier manifest identity. Restore all ten layers in order into an empty directory.",
    "files": files,
}
(FOLDER / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
(FOLDER / "SHA256SUMS.txt").write_text(sha + "  source-overlay.tar.gz\n" + digest(FOLDER / "manifest.json") + "  manifest.json\n")
zip_path = ROOT / "Builds/EchoProtocol-Coverage-20261009.zip"
recover = """RECUPERACIÓN COVERAGE — 2026-10-09

Verificar SHA256SUMS.txt y los nueve requisitos del manifest.
Crear un directorio VACÍO; nunca extraer sobre trabajo sucio.
Extraer cada archivo recoveryPrerequisites del manifest EN ESE ORDEN y por último
este source-overlay.tar.gz. Las capas son Candidate, Legibility, EncounterArt,
Reconstruction, Contraprueba, Lighting, Resonance, Folio, Coverage baseline y
Coverage final. Comparar TODAS las rutas/bytes/SHA256 contra manifest.json.
Abrir Unity 6000.3.21f1 y seleccionar:
Assets/_Game/Scenes/Production/EchoProtocol_Coverage.unity

No hubo eliminaciones. Se leyeron enteras las diez capas en orden y todos los
archivos recuperados coinciden con el manifest. No incluye Library, Temp, Logs,
Obj, UserSettings, builds ni Git. La app/ZIP permanece en EchoProtocol/Builds.
ZIP: ZIP_PATH
SHA256: ZIP_SHA

Los guardados de máquina no se archivan. La nueva clave es
EP.CampaignCheckpoint.Coverage.v1. Las ediciones anteriores conservan
su build y capas fuente recuperables.
""".replace("ZIP_PATH", str(zip_path)).replace("ZIP_SHA", digest(zip_path))
(FOLDER / "RECOVER.txt").write_text(recover)
verification = {
    "status": "PASS", "method": "Verified every prerequisite archive hash; streamed every regular-file byte from all ten layers in order; compared complete restored manifest. No extraction onto dirty workspace.",
    "archive": str(archive), "archiveBytes": archive.stat().st_size, "overlaySha256": sha,
    "completeManifestFilesVerified": len(files), "overlayFiles": len(changed), "removedFiles": removed,
    "extraRecoveredFiles": [], "layersRead": counts,
}
(FOLDER / "verification.json").write_text(json.dumps(verification, indent=2) + "\n")
print(json.dumps(verification), flush=True)
