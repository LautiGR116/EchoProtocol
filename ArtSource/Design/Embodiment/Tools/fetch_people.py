from inspect_people_archives import inner
from pathlib import Path
from urllib.request import urlopen,Request
import zipfile,hashlib,json,datetime
root=Path('/Users/lautigr116/EchoProtocol-licensed/Renderpeople-20261009')
root.mkdir(parents=True,exist_ok=True)
selections=[('rigged','rp_eric_rigged_001_FBX.zip','Eric',[
 'rp_eric_rigged_001_u3d.fbx','tex/rp_eric_rigged_001_dif.jpg','tex/rp_eric_rigged_001_norm.jpg',
 'tex/rp_eric_rigged_001_gloss.jpg','tex/rp_eric_rigged_001_mask01.jpg','tex/rp_eric_rigged_001_mask02.jpg']),
 ('animated','rp_nathan_animated_003_walking_FBX.zip','Nathan',[
 'rp_nathan_animated_003_walking.fbx','tex\\rp_nathan_animated_003_dif.jpg','tex\\rp_nathan_animated_003_norm.jpg',
 'tex\\rp_nathan_animated_003_gloss.jpg','tex\\rp_nathan_animated_003_mask01.jpg','tex\\rp_nathan_animated_003_mask02.jpg'])]
ledger=[]
for kind,model,name,names in selections:
 with zipfile.ZipFile(inner(kind,model)) as z:
  for filename in names:
   path=root/name/filename.replace('\\','/')
   path.parent.mkdir(parents=True,exist_ok=True)
   assert not path.exists(),'Preserve existing downloaded file: '+str(path)
   b=z.read(filename);path.write_bytes(b)
   record={'file':str(path),'outerURL':f'https://renderpeople.com/sample/free/renderpeople_free_{kind}_people_FBX.zip','innerArchive':model,'member':filename,'bytes':len(b),'sha256':hashlib.sha256(b).hexdigest()}
   ledger.append(record);print(name,filename,len(b),flush=True)
terms='https://renderpeople.com/general-terms-and-conditions/'
with urlopen(Request(terms,headers={'User-Agent':'Mozilla/5.0'}),timeout=40) as r:text=r.read()
(root/'terms-private.html').write_bytes(text)
report={'dateUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'provider':'Renderpeople GmbH','selectionPage':'https://renderpeople.com/free-3d-people/','terms':terms,'termsSha256':hashlib.sha256(text).hexdigest(),'termsVersion':'2024-12-13','costUSD':0,'license':'Provider free license; same terms as paid models. Not CC0. 2.5 grants free use; 4.1(b) permits computer/video games; 4.3(b) excludes public standalone raw/derived assets. No ML/CV or third-party AI upload.','distribution':'Compiled game only. Raw models, textures and Blender derivatives remain local, excluded from public Git. No purchase, account creation, plugin or package installed.','files':ledger}
(root/'provenance.json').write_text(json.dumps(report,indent=2)+'\n')
print('Downloaded selected asset members, not whole multi-model packages.',flush=True)
