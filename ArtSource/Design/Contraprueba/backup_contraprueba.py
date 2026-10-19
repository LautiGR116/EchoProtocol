from pathlib import Path
import json, hashlib, tarfile, datetime, re
root=Path('/Users/lautigr116/EchoProtocol')
folder=Path('/Users/lautigr116/EchoProtocol-backups/contraprueba-checkpoint-20261008')
prior=folder.parent/'reconstruction-checkpoint-20261002'
old=json.loads((prior/'manifest.json').read_text())
layers=[
 (folder.parent/'candidate-checkpoint-20261001/source.tar.gz','2ca16801d5c947fa79bcab5860fc23ad0627570731c84176a92affb564c16063'),
 (folder.parent/'legibility-checkpoint-20261002/source-overlay.tar.gz','7d5842e1e502ccfd7e2f12eecb68ca0e4d7726fc9d86d92c136469830c47be89'),
 (folder.parent/'encounter-art-checkpoint-20261002/source-overlay.tar.gz','bc2a31da9ecb05e3746bee65ad2acd3cc0192afc6d97c3067a237c8526291ac9'),
 (prior/'source-overlay.tar.gz',old['overlaySha256'])]
def digest(path):
 h=hashlib.sha256()
 with path.open('rb') as f:
  for b in iter(lambda:f.read(4*1024*1024),b''):h.update(b)
 return h.hexdigest()
assert not folder.exists() or not any(folder.iterdir()),'Preserve any existing backup'
folder.mkdir(exist_ok=True)
previous={x['path']:x for x in old['files']};files=[]
for name in ['.gitattributes','.gitignore','AGENTS.md','ArtSource','Assets','Docs','Packages','ProjectSettings','README.md']:
 p=root/name
 for f in sorted(p.rglob('*')) if p.is_dir() else [p]:
  if not f.is_file() or f.is_symlink():continue
  rel=f.relative_to(root).as_posix()
  if (f.name=='.DS_Store' or '__pycache__' in f.parts or re.search(r'\.blend\d+$',f.name)) and rel not in previous:continue
  files.append({'path':rel,'bytes':f.stat().st_size,'sha256':digest(f)})
current={x['path']:x for x in files};removed=sorted(set(previous)-set(current));assert not removed,removed
changed=[x for x in files if x['path'] not in previous or x['sha256']!=previous[x['path']]['sha256']]
print('Complete source manifest',len(files),'overlay',len(changed),'removals',len(removed),flush=True)
archive=folder/'source-overlay.tar.gz'
with tarfile.open(archive,'w:gz',compresslevel=6) as t:
 for x in changed:t.add(root/x['path'],arcname=x['path'],recursive=False)
sha=digest(archive)
for path,expected in layers:assert digest(path)==expected,str(path)
print('Source overlay written; four preserved layers checked',flush=True)
restored={};counts=[]
for path in [p for p,_ in layers]+[archive]:
 count=0
 with tarfile.open(path,'r|gz') as t:
  for m in t:
   if not m.isfile():continue
   name=m.name.removeprefix('./');h=hashlib.sha256();size=0
   with t.extractfile(m) as f:
    for b in iter(lambda:f.read(4*1024*1024),b''):h.update(b);size+=len(b)
   restored[name]={'sha256':h.hexdigest(),'bytes':size};count+=1
 counts.append({'archive':str(path),'regularFilesRead':count})
 print('Read back',path.parent.name,count,flush=True)
for x in files:assert restored.get(x['path'])=={'sha256':x['sha256'],'bytes':x['bytes']},x['path']
assert set(restored)==set(current),'Extra restored paths'
manifest={'createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'checkpoint':'Contraprueba','recoveryPrerequisites':[{'archive':str(p),'sha256':s}for p,s in layers],'overlaySha256':sha,'fileCount':len(files),'overlayFileCount':len(changed),'removedFiles':removed,'scope':'Project-owned sources/settings/docs/evidence. Excludes caches/builds/Git/new bytecode/Blender autosaves/symlinks. One inherited bytecode file retained for earlier manifest identity. Restore all five layers in order into an empty directory.','files':files}
(folder/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
(folder/'SHA256SUMS.txt').write_text(sha+'  source-overlay.tar.gz\n'+digest(folder/'manifest.json')+'  manifest.json\n')
zip_path=root/'Builds/EchoProtocol-Contraprueba-20261008.zip'
(folder/'RECOVER.txt').write_text('RECUPERACIÓN CONTRAPRUEBA — 2026-10-08\n\nVerificar SHA256SUMS.txt y los cuatro requisitos del manifest.\nCrear un directorio VACÍO; nunca extraer sobre trabajo sucio.\nExtraer Candidate, Legibility, EncounterArt, Reconstruction y por último este\nsource-overlay.tar.gz. Comparar todas las rutas/bytes/SHA256 con manifest.json.\nAbrir Unity 6000.3.21f1 y seleccionar:\nAssets/_Game/Scenes/Production/EchoProtocol_Contraprueba.unity\n\nNo hubo eliminaciones. Se leyeron enteras las cinco capas en orden y todos los\narchivos recuperados coinciden con el manifest. No incluye Library, Temp, Logs,\nObj, UserSettings, builds o Git. La app/ZIP permanece en EchoProtocol/Builds.\nZIP: '+str(zip_path)+'\nSHA256: '+digest(zip_path)+'\n\nLos guardados de máquina no se archivan. La nueva clave es\nEP.CampaignCheckpoint.Contraprueba.v1. Las ediciones anteriores conservan\nsu build y capas fuente recuperables.\n')
verification={'status':'PASS','method':'Checked all prerequisite archive hashes; streamed every regular-file byte from Candidate + Legibility + EncounterArt + Reconstruction + Contraprueba in order; compared complete restored manifest. No extraction onto dirty workspace.','archive':str(archive),'archiveBytes':archive.stat().st_size,'overlaySha256':sha,'completeManifestFilesVerified':len(files),'overlayFiles':len(changed),'removedFiles':removed,'extraRecoveredFiles':[],'layersRead':counts}
(folder/'verification.json').write_text(json.dumps(verification,indent=2)+'\n')
print(json.dumps(verification),flush=True)
