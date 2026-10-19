"""Read-only serialized-section and source-byte comparison against Resonance."""
from pathlib import Path
import hashlib,json,re,collections
root=Path(__file__).resolve().parents[4]
def sections(path):
 s=path.read_text();return {m[1]:(int(m[0]),m[2]) for m in re.findall(r'^--- !u!(\d+) &(\d+)[^\n]*\n(.*?)(?=^--- !u!|\Z)',s,re.M|re.S)}
a=sections(root/'Assets/_Game/Scenes/Production/EchoProtocol_Resonance.unity');b=sections(root/'Assets/_Game/Scenes/Production/EchoProtocol_Folio.unity');allowed=[];unexpected=[];same=0
text_go={re.search(r'm_GameObject: \{fileID: (\d+)\}',s)[1]for typ,s in a.values()if typ==102}
for key,(typ,old)in a.items():
 if key not in b:unexpected.append({'id':key,'reason':'removed'});continue
 _,new=b[key]
 if old==new:same+=1;continue
 pattern=None
 if typ==102:pattern=r'^  m_(?:CharacterSize|LineSpacing|Font):[^\n]*\n'
 elif typ==23 and re.search(r'm_GameObject: \{fileID: (\d+)\}',old)[1]in text_go:pattern=r'^  m_Materials:\n(?:  - [^\n]*\n)+'
 elif typ==114:pattern=r'^  (?:checkpointKey|identity|editionLabel):[^\n]*\n'
 elif typ==1660057539:pattern=r'^  - \{fileID: 176972964\}\n'
 if pattern and re.sub(pattern,'',old,flags=re.M)==re.sub(pattern,'',new,flags=re.M):allowed.append({'id':key,'type':typ})
 else:unexpected.append({'id':key,'type':typ,'reason':'outside authorized presentation fields'})
added=[{'id':key,'type':b[key][0]}for key in b.keys()-a.keys()];assert len(added)==3 and {v['type']for v in added}=={1,4,114}
manifest=json.loads(Path('/Users/lautigr116/EchoProtocol-backups/resonance-checkpoint-20261008/manifest.json').read_text());source_changes=[];unchanged=0
for x in manifest['files']:
 if x['path'].startswith(('Assets/_Game/Scripts/','Assets/_Game/Audio/','Assets/Settings/','ProjectSettings/')):
  p=root/x['path'];sha=hashlib.sha256(p.read_bytes()).hexdigest() if p.exists()else'missing'
  if sha!=x['sha256']:source_changes.append(x['path'])
  else:unchanged+=1
expected={'Assets/_Game/Scripts/Campaign/CampaignDirector.cs','Assets/_Game/Scripts/Horror/SD1SubtitleDisplay.cs','Assets/_Game/Scripts/Presentation/ContextualHints.cs','Assets/_Game/Scripts/Presentation/GameSession.cs','Assets/_Game/Scripts/Presentation/InteractionFocusHUD.cs','ProjectSettings/EditorBuildSettings.asset'}
assert set(source_changes)==expected
report={'status':'PASS'if not unexpected else'FAIL','method':'Read-only Unity serialized sections by retained local object ID. Allowed changes are TextMesh font/advance/leading, their renderer atlas material, checkpoint edition, session identity/edition, and one new graphic root. Every other serialized byte in original objects compared exactly. Project settings/audio/runtime-source bytes compared to recovered Resonance manifest.','sceneUnchangedObjects':same,'sceneAllowedChanges':len(allowed),'changesByUnityClass':dict(collections.Counter(x['type']for x in allowed)),'newGraphicRootObjects':added,'unexpectedSceneChanges':unexpected,'unchangedScopedSourceAssets':unchanged,'changedScopedSourceAssets':source_changes}
(root/'Docs/VisualEvidence/2026-10-08-Folio/preservation-audit.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report));assert report['status']=='PASS'
