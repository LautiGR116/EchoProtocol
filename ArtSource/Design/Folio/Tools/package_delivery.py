"""Package and verify the already-built Folio delivery without touching QA."""
from pathlib import Path
import datetime
import hashlib
import json
import stat
import subprocess
import tempfile
import zipfile

ROOT = Path(__file__).resolve().parents[4]
FOLDER = ROOT/'Builds/EchoProtocol-Folio-20261008'
APP = FOLDER/'EchoProtocol.app'
ZIP = FOLDER.with_suffix('.zip')
EVIDENCE = ROOT/'Docs/VisualEvidence/2026-10-08-Folio'

def run(*args):
    return subprocess.check_output(args, stderr=subprocess.STDOUT, text=True).strip()

def digest(path):
    h = hashlib.sha256()
    with path.open('rb') as f:
        for b in iter(lambda:f.read(4*1024*1024), b''): h.update(b)
    return h.hexdigest()

assert APP.is_dir()
assert not ZIP.exists(), 'Preserve any previous delivery ZIP'
(FOLDER/'LEEME.txt').write_text("""ECHO PROTOCOL — FOLIO
macOS · 2026-10-08 · Intel y Apple Silicon

Abrí EchoProtocol.app. Tab/flechas seleccionan; Enter/Espacio activa.
WASD: moverte. Mouse: mirar. E: interactuar. R: RECALL.
Esc: pausa. H: ayuda contextual pausada. J: pasos explícitos.
Ajustes: izquierda/derecha cambia volumen; Enter activa opciones.
Documentos: rueda/flechas, PageUp/PageDown, Home/End; Esc vuelve.

Auriculares a volumen cómodo o parlantes. No hace falta subir el volumen.
El menú ofrece mezcla mono y reducir impactos sonoros.

Folio añade identidad tipográfica, documentos con papel y lectura completa,
menús coherentes y HUD compartido. Conserva audio, luces, puzzles y Echo de
Resonance. Usa guardados propios; las ediciones anteriores permanecen intactas.

Campaña previa completa; interfaz final y controles con evidencia técnica.
Los intentos de campaña final se interrumpieron por pérdida de foco. Primera partida,
comodidad de lectura/escucha, interpretación vocal y miedo requieren evaluación
humana. La revisión visual del agente no equivale a aprobación humana.

Firma local ad hoc; no notarizada por Apple. No incluye código de pruebas.
Créditos: CREDITS.txt. Avisos completos: FontLicenses/.
Guía y comparación en el proyecto: Docs/FOLIO_IDENTITY.md,
Docs/FOLIO_CANDIDATE.md y Docs/VisualEvidence/2026-10-08-Folio/index.html.
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
report = dict(status='PASS',dateUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),
              zip=str(ZIP),sha256=digest(ZIP),bytes=ZIP.stat().st_size,
              regularFilesMatched=len(files),zipCRC='all entries pass',
              executablePermissionRetained=True,
              strictAdHocSignature='PASS original and ZIP-extracted app',
              architectures=architectures,notarized=False,
              preservedResonanceSha256=lighting_hash)
(EVIDENCE/'package.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
