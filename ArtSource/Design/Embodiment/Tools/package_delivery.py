"""Package the finished Embodiment app, retaining prior verified deliveries."""
from pathlib import Path
import hashlib,json,stat,subprocess,tempfile,zipfile,datetime
ROOT=Path(__file__).resolve().parents[4]
FOLDER=ROOT/'Builds/EchoProtocol-Embodiment-20261009'
APP=FOLDER/'EchoProtocol.app'
ZIP=FOLDER.with_suffix('.zip')
EVIDENCE=ROOT/'Docs/VisualEvidence/2026-10-09-Embodiment'
def run(*args):return subprocess.check_output(args,stderr=subprocess.STDOUT,text=True).strip()
def digest(p):
    h=hashlib.sha256()
    with p.open('rb') as f:
        for b in iter(lambda:f.read(4*1024*1024),b''):h.update(b)
    return h.hexdigest()
assert APP.is_dir() and not ZIP.exists(),'Preserve previous delivery'
assert json.loads((EVIDENCE/'native-full/campaign-full.result.json').read_text())['status']=='PASS'
assert json.loads((EVIDENCE/'native-full/character-result.json').read_text())['status']=='PASS'
assert json.loads((EVIDENCE/'delivery-assembly.json').read_text())['status']=='PASS'
(FOLDER/'LEEME.txt').write_text('''ECHO PROTOCOL — EMBODIMENT
Personajes renovados · macOS · Intel y Apple Silicon · 2026-10-09

Abrí EchoProtocol.app. «Iniciar sesión» recorre la campaña completa.
«Jugar · La máquina que te cubre» abre directamente el encuentro del subsuelo.
Tab/flechas seleccionan; Enter/Espacio activa. WASD: moverte. Mouse: mirar.
Mantené E para accionar mandos. R: repetir lo grabado. Esc: pausa.
H: ayuda contextual pausada; J: pasos si los necesitás.
En el fallo, Enter reintenta con la misma toma; también podés grabar otra.

El Echo tiene nuevo rostro, pelo, ojos, ropa y articulación. El perseguidor
presente usa otra persona, otra silueta y otra postura. La campaña y las reglas
de la bomba, la presión, la reja y la persecución corresponden a Coverage.
La reproducción conserva sus datos registrados, incluido el contacto con el mando.

Auriculares a volumen cómodo o parlantes. No hace falta subir el volumen.
El menú conserva mezcla mono y reducción de impactos sonoros.
Campaña, grabación, pausa, persecución y reintento tienen evidencia objetiva.
Credibilidad, movimiento natural, escucha, tensión y miedo siguen EN PRUEBA humana.
La claridad de las instrucciones y el ajuste de la manivela quedan para otro goal.

Modelos Renderpeople con licencia para el juego compilado; fuentes excluidas
del Git público. Créditos: CREDITS.txt. Fuentes tipográficas: FontLicenses/.
Firma local ad hoc; no notarizada por Apple. No contiene código de pruebas.
Informe y comparación: Docs/EMBODIMENT_CANDIDATE.md en el proyecto.
''')
(FOLDER/'CREDITS.txt').write_bytes((ROOT/'Docs/CREDITS.md').read_bytes())
(FOLDER/'FontLicenses').mkdir(exist_ok=True)
for name in ['Cormorant-OFL.txt','plex-OFL.txt']:(FOLDER/'FontLicenses'/name).write_bytes((ROOT/'ArtSource/Design/Folio/Fonts'/name).read_bytes())
signature=run('/usr/bin/codesign','--verify','--deep','--strict','--verbose=2',str(APP))
binary=APP/'Contents/MacOS/EchoProtocol'
architectures=run('/usr/bin/lipo','-archs',str(binary)).split()
assert set(architectures)=={'x86_64','arm64'}
assert set(run('/usr/bin/lipo','-archs',str(APP/'Contents/Frameworks/UnityPlayer.dylib')).split())==set(architectures)
run('/usr/bin/ditto','-c','-k','--sequesterRsrc','--keepParent',str(FOLDER),str(ZIP))
files=[p for p in FOLDER.rglob('*') if p.is_file() and not p.is_symlink()]
with zipfile.ZipFile(ZIP) as z:
    assert z.testzip() is None
    for p in files:
        with z.open(p.relative_to(FOLDER.parent).as_posix()) as f:
            h=hashlib.sha256()
            for b in iter(lambda:f.read(4*1024*1024),b''):h.update(b)
        assert h.hexdigest()==digest(p),str(p)
    expected={p.relative_to(FOLDER.parent).as_posix() for p in files}
    actual={i.filename for i in z.infolist() if not i.is_dir() and not i.filename.startswith('__MACOSX/') and not stat.S_ISLNK(i.external_attr>>16)}
    assert actual==expected
    assert z.getinfo(binary.relative_to(FOLDER.parent).as_posix()).external_attr>>16 & stat.S_IXUSR
with tempfile.TemporaryDirectory(prefix='ep-embodiment-package-') as tmp:
    run('/usr/bin/ditto','-x','-k',str(ZIP),tmp)
    run('/usr/bin/codesign','--verify','--deep','--strict','--verbose=2',str(Path(tmp)/FOLDER.name/'EchoProtocol.app'))
preserved={}
for name,expected in [('Coverage-20261009','5e36228a8457253ef66abc22ec5dd292dc9615b2dd7b968f008b52ccb2025f88'),('Folio-20261008','ca6b0676b3a1ecafd765d290f767e450444e3b065290b9693e3e1aa2d979d12f'),('Resonance-20261008','37211b901446a162419b9ca9b934149cea5d107625bb6ea9b941a679f865de3f')]:
    value=digest(ROOT/('Builds/EchoProtocol-'+name+'.zip'));assert value==expected;preserved[name]=value
report=dict(status='PASS',dateUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),zip=str(ZIP),sha256=digest(ZIP),bytes=ZIP.stat().st_size,regularFilesMatched=len(files),zipCRC='all entries pass',executablePermissionRetained=True,strictAdHocSignature='PASS original and ZIP-extracted app',architectures=architectures,notarized=False,preserved=preserved)
(EVIDENCE/'package.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
