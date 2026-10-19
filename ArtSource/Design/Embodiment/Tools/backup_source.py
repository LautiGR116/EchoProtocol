"""Private final recovery checkpoint; verify all eleven source layers and originals."""
from pathlib import Path
import datetime,hashlib,json,re,tarfile
ROOT=Path(__file__).resolve().parents[4]
BACKUPS=ROOT.parent/'EchoProtocol-backups'
FOLDER=BACKUPS/'embodiment-checkpoint-20261009'
PRIOR=BACKUPS/'coverage-checkpoint-20261009'
LICENSED=ROOT.parent/'EchoProtocol-licensed/Renderpeople-20261009'
old=json.loads((PRIOR/'manifest.json').read_text())
layers=[(Path(x['archive']),x['sha256']) for x in old['recoveryPrerequisites']]
layers.append((PRIOR/'source-overlay.tar.gz',old['overlaySha256']))
def digest(p):
    h=hashlib.sha256()
    with p.open('rb') as f:
        for b in iter(lambda:f.read(4*1024*1024),b''):h.update(b)
    return h.hexdigest()
assert not FOLDER.exists() or not any(FOLDER.iterdir()),'Preserve existing backup'
FOLDER.mkdir(mode=0o700,exist_ok=True)
previous={x['path']:x for x in old['files']}
files=[]
for name in ['.gitattributes','.gitignore','AGENTS.md','ArtSource','Assets','Docs','Packages','ProjectSettings','README.md']:
    p=ROOT/name
    for f in sorted(p.rglob('*')) if p.is_dir() else [p]:
        if not f.is_file() or f.is_symlink():continue
        rel=f.relative_to(ROOT).as_posix()
        if (f.name=='.DS_Store' or '__pycache__' in f.parts or re.search(r'\.blend\d+$',f.name)) and rel not in previous:continue
        files.append(dict(path=rel,bytes=f.stat().st_size,sha256=digest(f)))
current={x['path']:x for x in files}
removed=sorted(set(previous)-set(current));assert not removed,removed
changed=[x for x in files if x['path'] not in previous or x['sha256']!=previous[x['path']]['sha256']]
print('Private source manifest',len(files),'overlay',len(changed),'removals',len(removed),flush=True)
archive=FOLDER/'source-overlay.tar.gz'
with tarfile.open(archive,'w:gz',compresslevel=6) as t:
    for x in changed:t.add(ROOT/x['path'],arcname=x['path'],recursive=False)
archive.chmod(0o600);sha=digest(archive)
for p,expected in layers:assert digest(p)==expected,str(p)
restored={};counts=[]
for p in [x[0] for x in layers]+[archive]:
    count=0
    with tarfile.open(p,'r|gz') as t:
        for m in t:
            if not m.isfile():continue
            h=hashlib.sha256();size=0
            with t.extractfile(m) as f:
                for b in iter(lambda:f.read(4*1024*1024),b''):h.update(b);size+=len(b)
            restored[m.name.removeprefix('./')]=dict(sha256=h.hexdigest(),bytes=size);count+=1
    counts.append(dict(archive=str(p),regularFilesRead=count));print('Verified layer',p.parent.name,count,flush=True)
for x in files:assert restored.get(x['path'])==dict(sha256=x['sha256'],bytes=x['bytes']),x['path']
assert set(restored)==set(current),'Extra restored paths'
licensed=[dict(path=p.relative_to(LICENSED).as_posix(),bytes=p.stat().st_size,sha256=digest(p)) for p in sorted(LICENSED.rglob('*')) if p.is_file() and not p.is_symlink()]
originals=FOLDER/'licensed-originals.tar.gz'
with tarfile.open(originals,'w:gz',compresslevel=6) as t:
    for x in licensed:t.add(LICENSED/x['path'],arcname=x['path'],recursive=False)
originals.chmod(0o600)
with tarfile.open(originals,'r:gz') as t:
    for x in licensed:
        member=t.getmember(x['path']);h=hashlib.sha256()
        with t.extractfile(member) as f:
            for b in iter(lambda:f.read(4*1024*1024),b''):h.update(b)
        assert member.size==x['bytes'] and h.hexdigest()==x['sha256'],x['path']
manifest=dict(createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),checkpoint='Embodiment',scene='Assets/_Game/Scenes/Production/EchoProtocol_Embodiment.unity',checkpointKey='EP.CampaignCheckpoint.Embodiment.v1',recoveryPrerequisites=[dict(archive=str(p),sha256=s) for p,s in layers],overlaySha256=sha,fileCount=len(files),overlayFileCount=len(changed),removedFiles=removed,files=files,licensedOriginals=dict(archive=str(originals),sha256=digest(originals),files=licensed),scope='Private personal recovery including licensed local assets and skeleton-bearing scene/prefab. Not for public Git or raw-asset redistribution. Excludes caches, builds, Git, new bytecode, Blender autosaves and symlinks. Eleven ordered project source layers plus one original-source archive.')
(FOLDER/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
report=dict(status='PASS',method='Hashed prerequisites; streamed and compared every restored byte/path from all eleven layers. Independently read back every licensed original/metadata file. No extraction onto dirty workspace.',archive=str(archive),archiveBytes=archive.stat().st_size,overlaySha256=sha,completeManifestFilesVerified=len(files),overlayFiles=len(changed),removedFiles=removed,extraRecoveredFiles=[],layersRead=counts,licensedOriginalFilesVerified=len(licensed),licensedOriginalArchiveSha256=digest(originals))
(FOLDER/'verification.json').write_text(json.dumps(report,indent=2)+'\n')
(FOLDER/'SHA256SUMS.txt').write_text(sha+'  source-overlay.tar.gz\n'+digest(originals)+'  licensed-originals.tar.gz\n'+digest(FOLDER/'manifest.json')+'  manifest.json\n')
package=ROOT/'Builds/EchoProtocol-Embodiment-20261009.zip'
(FOLDER/'RECOVER.txt').write_text('''RECUPERACIÓN PRIVADA EMBODIMENT — 2026-10-09

Verificar SHA256SUMS.txt y todos los requisitos de manifest.json.
Crear un directorio VACÍO. Extraer recoveryPrerequisites EN SU ORDEN, seguido
de source-overlay.tar.gz. Comparar todas las rutas/bytes/hashes con el manifest.
Son once capas: las diez de Coverage y el acabado de personajes Embodiment.
Extraer licensed-originals.tar.gz en otro directorio privado para recuperar
los originales Renderpeople, sus términos, procedencia y estudios de animación.
Verificar también el manifest licensedOriginals. Abrir con Unity 6000.3.21f1.
Escena: Assets/_Game/Scenes/Production/EchoProtocol_Embodiment.unity
Guardado propio: EP.CampaignCheckpoint.Embodiment.v1

Este respaldo contiene fuentes LICENCIADAS. Es de recuperación personal:
NO subir a Git público, compartir como assets ni cargar en servicios de IA.
Consultar Docs/CREDITS.md y ArtSource/Design/Embodiment/provenance.json.
No incluye Library, Temp, Logs, Obj, UserSettings, builds, Git o credenciales.
Las builds anteriores se conservan. La app compilada se entrega separadamente.
ZIP: '''+str(package)+'\nSHA256: '+digest(package)+'\n')
for p in FOLDER.iterdir():p.chmod(0o600)
print(json.dumps(report),flush=True)
