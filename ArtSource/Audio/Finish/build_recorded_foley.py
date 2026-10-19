"""Finite derivatives of already licensed CC0 project sources. No speech/voice synthesis."""
from pathlib import Path
import numpy as np,wave,json,hashlib
root=Path('Assets/_Game/Audio/Clips');out=root/'Finish';out.mkdir(exist_ok=True)
def read(p):
 with wave.open(str(p),'rb') as f:
  rate=f.getframerate();channels=f.getnchannels();width=f.getsampwidth();data=f.readframes(f.getnframes())
 if width==3:
  b=np.frombuffer(data,np.uint8).reshape(-1,3);v=b[:,0].astype(np.int32)|(b[:,1].astype(np.int32)<<8)|(b[:,2].astype(np.int32)<<16);v=(v^0x800000)-0x800000;x=v/8388608
 else:x=np.frombuffer(data,'<i2')/32768
 return x.reshape(-1,channels).mean(axis=1),rate
manifest=[]
def write(name,x,rate,source,edit,peak=.25):
 x=np.asarray(x);x-=x.mean();n=min(int(.012*rate),len(x)//4);x[:n]*=np.linspace(0,1,n);x[-n:]*=np.linspace(1,0,n);x=x/(np.max(np.abs(x))+1e-10)*peak
 p=out/(name+'.wav')
 with wave.open(str(p),'wb') as f:f.setparams((1,2,rate,0,'NONE',''));f.writeframes((x*32767).astype('<i2').tobytes())
 manifest.append({'output':str(p),'source':str(source),'edit':edit,'duration':len(x)/rate,'peak':float(np.max(np.abs(x))),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
# Extract isolated real heel/sole contacts, selected by the envelope with a separation constraint.
src=root/'SD1/SFX_SD1_Footsteps.wav';x,rate=read(src)
step=480;env=np.array([np.sqrt(np.mean(x[i:i+step]**2)) for i in range(0,len(x),step)]);peaks=[]
for k in np.argsort(env)[::-1]:
 if all(abs(int(k)-v)>35 for v in peaks):peaks.append(int(k))
 if len(peaks)==3:break
for j,k in enumerate(sorted(peaks)):
 a=max(0,k*step-int(.055*rate));clip=x[a:min(len(x),a+int(.34*rate))].copy()
 write('FootVinyl'+str(j),clip.copy(),rate,src,'isolated recorded contact '+str(round(a/rate,3)),.22)
 # A low-passed body from the same contact; actual shoe texture remains.
 filtered=np.convolve(clip,np.ones(7)/7,'same');wood=clip*.58+filtered*.42
 write('FootWood'+str(j),wood,rate,src,'same real contact, mild high-frequency damping for timber',.22)
src=root/'SD1/SFX_SD1_DomesticKnock.wav';x,rate=read(src);write('WallContact',x.copy(),rate,src,'complete downmixed knock, conservative peak',.26)
src=root/'Mechanisms/SFX_DoorEndpoint.wav';x,rate=read(src);write('Relay',x[:int(.6*rate)].copy(),rate,src,'short real mechanism endpoint',.27)
src=root/'Mechanisms/SFX_DoorMovement_LP.wav';x,rate=read(src);write('CloseMovement',x[:int(1.6*rate)].copy(),rate,src,'real friction excerpt, no voice',.18)
src=root/'Ambience/AMB_DistantMachinery_LP.wav';x,rate=read(src);write('PressureBed',x.copy(),rate,src,'existing recorded machinery, loop preserved with short endpoint fades',.21)
Path('ArtSource/Audio/Finish/derivatives.json').write_text(json.dumps(manifest,indent=2));print('Recorded foley derivatives',len(manifest));print('Footstep source envelope peaks',sorted(peaks))
