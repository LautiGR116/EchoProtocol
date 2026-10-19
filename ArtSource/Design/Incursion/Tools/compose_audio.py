"""Present adult performance edit; same existing CC0 sources, no generation."""
from pathlib import Path
import runpy,json,shutil
ROOT=Path(__file__).resolve().parents[4]
# Reuse pure signal helpers without executing the earlier asset recipe.
old=(ROOT/'ArtSource/Design/Impact/Tools/compose_audio.py').read_text().split("D=Path(")[0]
ns={'__file__':str(ROOT/'ArtSource/Design/Impact/Tools/compose_audio.py')};exec(old,ns)
ns['OUT']=ROOT/'ArtSource/Audio/Incursion/Masters';ns['OUT'].mkdir(parents=True,exist_ok=True);ns['records']=[]
read,band,fade,norm,save=[ns[n] for n in ['read','band','fade','normalize','save']];R=48000
source='ArtSource/Audio/Resonance/Decoded/scream-325548_2826150-hq.wav'
x=band(read(Path(source)),105,8800)
# Compress the dramatic phrase through editing, retaining original pitch/formants.
# An abrupt voiced attack, a broken second effort, then a short strained fall.
y=ns['np'].concatenate([fade(x[int(.86*R):int(1.39*R)],.006,.035),ns['np'].zeros(int(.045*R)),fade(x[int(1.72*R):int(2.04*R)],.008,.09)*.78])
save('Threat_AttackVoice',norm(y,.56),[source,'Archeos 325548; CC0 existing Resonance source manifest'],'Original adult pitch. Two dry efforts cut from 0.86–1.39 and 1.72–2.04 s, 45 ms separation, 6 ms attack, no room wash. Attached to recognized-player launch; human performance approval pending.')
y=fade(band(read(Path(source))[int(.32*R):int(.82*R)],180,5300),.025,.065)
save('Threat_RecognitionVoice',norm(y,.23),[source,'Archeos 325548; CC0'],'Original early adult strain, 0.32–0.82 s. Recognition and pump-listening change, lower level; no speech or fabricated Echo channel.')
y=fade(x[int(2.2*R):int(2.65*R)],.012,.08)
save('Threat_PursuitVoice',norm(y,.33),[source,'Archeos 325548; CC0'],'Original late voiced effort, 2.20–2.65 s. Deterministic short pursuit efforts, stopped during pump diversion; no pitch shift.')
# Close body/floor contact at capture, without a full-screen sonic slam.
clank=read(Path('ArtSource/Audio/Resonance/Decoded/mechanical-clank1.wav'))
y=fade(band(clank[:int(.29*R)],70,3800),.004,.07)
save('Threat_CaptureContact',norm(y,.35),['Brian MacIntosh mechanical-clank1.wav; existing CC0 manifest'],'Short low-mid physical capture contact, 290 ms; no long metallic tail into retry.')
for rec in ns['records']:
 if 'Voice' in rec['file']:rec['voice']='Archeos original adult performance'
 rec['generation']=None
(ns['OUT'].parent/'masters.json').write_text(json.dumps(ns['records'],indent=2))
a=ROOT/'Assets/_Game/Audio/Clips/Incursion';a.mkdir(parents=True,exist_ok=True)
for p in ns['OUT'].glob('*.wav'):shutil.copy2(p,a/p.name)
print(json.dumps([{k:r[k] for k in ['file','seconds','peakDb','rmsDb']} for r in ns['records']],indent=2))
