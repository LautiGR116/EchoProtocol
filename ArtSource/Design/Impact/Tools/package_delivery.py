"""Package the finished Impact app, retaining prior verified deliveries."""
from pathlib import Path
import hashlib,json,stat,subprocess,tempfile,zipfile,datetime
ROOT=Path(__file__).resolve().parents[4]
FOLDER=ROOT/'Builds/EchoProtocol-Impact-20261011'
APP=FOLDER/'EchoProtocol.app'
ZIP=FOLDER.with_suffix('.zip')
EVIDENCE=ROOT/'Docs/VisualEvidence/2026-10-11-Impact'
def run(*args):return subprocess.check_output(args,stderr=subprocess.STDOUT,text=True).strip()
def digest(p):
    h=hashlib.sha256()
    with p.open('rb') as f:
        for b in iter(lambda:f.read(4*1024*1024),b''):h.update(b)
    return h.hexdigest()
assert APP.is_dir() and not ZIP.exists(),'Preserve previous delivery'
assert json.loads((EVIDENCE/'native-segment/campaign-full.result.json').read_text())['status']=='PASS'
assert json.loads((EVIDENCE/'native-segment/character-result.json').read_text())['status']=='PASS'
assert json.loads((EVIDENCE/'delivery-assembly.json').read_text())['status']=='PASS'
(FOLDER/'LEEME.txt').write_text('ECHO PROTOCOL — IMPACT\nmacOS · Intel y Apple Silicon · 2026-10-11\n\nAbrí EchoProtocol.app y elegí «Jugar · La máquina que te cubre».\nWASD: moverte. Mouse: mirar. Mantené E para accionar los mandos.\nR: RECALL. Esc: pausa. H: notas y ayuda. Enter: reintentar tras fallar.\nLa misma toma conserva movimientos, cuerpo y contactos grabados.\n\nLa mezcla tiene contacto previo, irrupción, pasos próximos, fricción y pulso\ninterno. El rodeo lateral rompe la visión y permite usar el ruido de la bomba.\nEl HUD separa aviso de RECALL, objetivo y mensajes inmediatos.\nAuriculares a volumen cómodo; no hace falta subirlos. Mono y reducción\nde impactos permanecen en Opciones. Tu prueba define comodidad, orientación,\ntensión y miedo; siguen EN PRUEBA humana.\n\nLa verificación nueva se concentra en el encuentro y sus cambios.\nLa campaña anterior completa tiene evidencia de Embodiment; no se repitió\npor rutina. Informe: Docs/IMPACT_CANDIDATE.md. Comparación audiovisual:\nDocs/VisualEvidence/2026-10-11-Impact/index.html.\n\nFuentes Renderpeople privadas con licencia para el juego compilado.\nCREDITS.txt y FontLicenses/ conservan atribuciones. Firma local ad hoc;\nno notarizada por Apple. La entrega excluye el código de pruebas.\n')
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
with tempfile.TemporaryDirectory(prefix='ep-impact-package-') as tmp:
    run('/usr/bin/ditto','-x','-k',str(ZIP),tmp)
    run('/usr/bin/codesign','--verify','--deep','--strict','--verbose=2',str(Path(tmp)/FOLDER.name/'EchoProtocol.app'))
preserved={}
for name,expected in [('Embodiment-20261009','0b5971ea4a0329ca5f6480d6bb37ec1b074e886db5d37ad1176aefa8a521b1f5'),('Coverage-20261009','5e36228a8457253ef66abc22ec5dd292dc9615b2dd7b968f008b52ccb2025f88')]:
    value=digest(ROOT/('Builds/EchoProtocol-'+name+'.zip'));assert value==expected;preserved[name]=value
report=dict(status='PASS',dateUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),zip=str(ZIP),sha256=digest(ZIP),bytes=ZIP.stat().st_size,regularFilesMatched=len(files),zipCRC='all entries pass',executablePermissionRetained=True,strictAdHocSignature='PASS original and ZIP-extracted app',architectures=architectures,notarized=False,preserved=preserved)
(EVIDENCE/'package.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
