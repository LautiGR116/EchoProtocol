"""Package and verify the already-built Resonance delivery without touching QA."""
from pathlib import Path
import datetime
import hashlib
import json
import stat
import subprocess
import tempfile
import zipfile

ROOT = Path(__file__).resolve().parents[4]
FOLDER = ROOT/'Builds/EchoProtocol-Resonance-20261008'
APP = FOLDER/'EchoProtocol.app'
ZIP = FOLDER.with_suffix('.zip')
EVIDENCE = ROOT/'Docs/AudioEvidence/2026-10-08-Resonance'

def run(*args):
    return subprocess.check_output(args, stderr=subprocess.STDOUT, text=True).strip()

def digest(path):
    h = hashlib.sha256()
    with path.open('rb') as f:
        for b in iter(lambda:f.read(4*1024*1024), b''): h.update(b)
    return h.hexdigest()

assert APP.is_dir()
assert not ZIP.exists(), 'Preserve any previous delivery ZIP'
(FOLDER/'LEEME.txt').write_text('''ECHO PROTOCOL — RESONANCE
macOS · 2026-10-08 · Intel y Apple Silicon

Abrí EchoProtocol.app. Enter inicia; Continuar conserva los hitos guardados.
WASD: moverte. Mouse: mirar. E: interactuar. R: RECALL.
Esc: pausa y opciones. H: ayuda contextual pausada. J: pasos explícitos.

Auriculares a volumen cómodo o parlantes. No hace falta subir el volumen.
El menú permite mezcla mono y reducir impactos sonoros. Volumen inicial: 70%.

Esta candidata conserva los puzzles y la iluminación de Lighting. El sonido
pertenece al edificio, los aparatos o la música interna. El Echo no reproduce
voces ni pasos que no hayan sido grabados. La nueva edición usa sus propios
guardados; no modifica los de Lighting.

La campaña completa, las cargas y las opciones tienen evidencia técnica.
La interpretación vocal, localización percibida, comodidad, primera partida
y miedo siguen pendientes de evaluación humana. No se afirma una escucha
humana de esta candidata.

Firma local ad hoc; no notarizada por Apple. No incluye código de pruebas.
Fuentes, créditos y modificaciones: CREDITS.txt.
Informe y muestras capturadas: Docs/RESONANCE_CANDIDATE.md y
Docs/AudioEvidence/2026-10-08-Resonance/listening/index.html en el proyecto.
''')
(FOLDER/'CREDITS.txt').write_bytes((ROOT/'Docs/CREDITS.md').read_bytes())
signature = run('/usr/bin/codesign','--verify','--deep','--strict','--verbose=2',str(APP))
binary = APP/'Contents/MacOS/EchoProtocol'
architectures = run('/usr/bin/lipo','-archs',str(binary)).split()
assert set(architectures) == {'x86_64','arm64'}
framework = APP/'Contents/Frameworks/UnityPlayer.dylib'
assert set(run('/usr/bin/lipo','-archs',str(framework)).split()) == set(architectures)
run('/usr/bin/ditto','-c','-k','--sequesterRsrc','--keepParent',str(FOLDER),str(ZIP))
files = [p for p in FOLDER.rglob('*') if p.is_file() and not p.is_symlink()]
with zipfile.ZipFile(ZIP) as z:
    assert z.testzip() is None
    for p in files:
        member = p.relative_to(FOLDER.parent).as_posix()
        with z.open(member) as f:
            h = hashlib.sha256()
            for b in iter(lambda:f.read(4*1024*1024), b''): h.update(b)
        assert h.hexdigest() == digest(p), member
    expected = {p.relative_to(FOLDER.parent).as_posix() for p in files}
    actual = {i.filename for i in z.infolist() if not i.is_dir() and not i.filename.startswith('__MACOSX/') and not stat.S_ISLNK(i.external_attr>>16)}
    assert actual == expected, actual ^ expected
    assert z.getinfo(binary.relative_to(FOLDER.parent).as_posix()).external_attr>>16 & stat.S_IXUSR
with tempfile.TemporaryDirectory(prefix='ep-resonance-package-') as tmp:
    run('/usr/bin/ditto','-x','-k',str(ZIP),tmp)
    extracted_signature = run('/usr/bin/codesign','--verify','--deep','--strict','--verbose=2',str(Path(tmp)/FOLDER.name/'EchoProtocol.app'))
lighting = ROOT/'Builds/EchoProtocol-Lighting-20261008.zip'
lighting_hash = digest(lighting)
assert lighting_hash == '7410178f7f72eab540f2bcb972d3c0d922885aed92c84b3d1e467075b9f9fecf'
report = dict(status='PASS',dateUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),
              zip=str(ZIP),sha256=digest(ZIP),bytes=ZIP.stat().st_size,
              regularFilesMatched=len(files),zipCRC='all entries pass',
              executablePermissionRetained=True,
              strictAdHocSignature='PASS original and ZIP-extracted app',
              architectures=architectures,notarized=False,
              preservedLightingSha256=lighting_hash)
(EVIDENCE/'package.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
