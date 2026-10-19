"""Package and verify the already-built Coverage delivery without touching QA."""
from pathlib import Path
import datetime
import hashlib
import json
import stat
import subprocess
import tempfile
import zipfile

ROOT = Path(__file__).resolve().parents[4]
FOLDER = ROOT/'Builds/EchoProtocol-Coverage-20261009'
APP = FOLDER/'EchoProtocol.app'
ZIP = FOLDER.with_suffix('.zip')
EVIDENCE = ROOT/'Docs/VisualEvidence/2026-10-09-Coverage'

def run(*args):
    return subprocess.check_output(args, stderr=subprocess.STDOUT, text=True).strip()

def digest(path):
    h = hashlib.sha256()
    with path.open('rb') as f:
        for b in iter(lambda:f.read(4*1024*1024), b''): h.update(b)
    return h.hexdigest()

assert APP.is_dir()
assert not ZIP.exists(), 'Preserve any previous delivery ZIP'
(FOLDER/'LEEME.txt').write_text("""ECHO PROTOCOL — LA MÁQUINA QUE TE CUBRE
macOS · 2026-10-09 · Intel y Apple Silicon

Abrí EchoProtocol.app. Elegí «Jugar · La máquina que te cubre» para ir al tramo.
«Nueva sesión» recorre la campaña completa, con el encuentro en el subsuelo.
Tab/flechas seleccionan; Enter/Espacio activa. WASD: moverte. Mouse: mirar.
Mantené E para accionar mandos. R: repetir lo grabado. Esc: pausa.
H: ayuda contextual pausada; J: pasos explícitos sólo si los necesitás.
En el fallo, Enter reintenta con la misma toma; también podés grabar otra.

La bomba produce ruido y presión. Prepará una grabación que te permita trabajar
junto a la reja y después cruzarla. La amenaza pertenece al presente.
Una carga parcial vuelve a un comienzo seguro; una salida ganada permanece abierta.

Auriculares a volumen cómodo o parlantes. No hace falta subir el volumen.
El menú conserva mezcla mono y reducción de impactos sonoros.
Los controles, la grabación y sus consecuencias tienen evidencia objetiva.
Comprensión espontánea, credibilidad, escucha, tensión y miedo siguen EN PRUEBA.
El personaje se reutiliza de la candidata anterior; su renovación completa queda
para el siguiente goal. No se pagó ni se agregó un servicio.

Firma local ad hoc; no notarizada por Apple. No incluye código de pruebas.
Créditos: CREDITS.txt. Licencias de las fuentes: FontLicenses/.
Informe: Docs/COVERAGE_CANDIDATE.md en el proyecto.
""")
(FOLDER/'FontLicenses').mkdir(exist_ok=True)
for name in ['Cormorant-OFL.txt','plex-OFL.txt']:
    (FOLDER/'FontLicenses'/name).write_bytes((ROOT/'ArtSource/Design/Folio/Fonts'/name).read_bytes())
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
with tempfile.TemporaryDirectory(prefix='ep-folio-package-') as tmp:
    run('/usr/bin/ditto','-x','-k',str(ZIP),tmp)
    extracted_signature = run('/usr/bin/codesign','--verify','--deep','--strict','--verbose=2',str(Path(tmp)/FOLDER.name/'EchoProtocol.app'))
lighting = ROOT/'Builds/EchoProtocol-Resonance-20261008.zip'
lighting_hash = digest(lighting)
assert lighting_hash == '37211b901446a162419b9ca9b934149cea5d107625bb6ea9b941a679f865de3f'
folio_hash = digest(ROOT/'Builds/EchoProtocol-Folio-20261008.zip')
assert folio_hash == 'ca6b0676b3a1ecafd765d290f767e450444e3b065290b9693e3e1aa2d979d12f'
report = dict(preservedFolioSha256=folio_hash,status='PASS',dateUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),
              zip=str(ZIP),sha256=digest(ZIP),bytes=ZIP.stat().st_size,
              regularFilesMatched=len(files),zipCRC='all entries pass',
              executablePermissionRetained=True,
              strictAdHocSignature='PASS original and ZIP-extracted app',
              architectures=architectures,notarized=False,
              preservedResonanceSha256=lighting_hash)
(EVIDENCE/'package.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
