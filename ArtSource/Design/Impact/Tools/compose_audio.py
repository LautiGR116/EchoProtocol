"""Offline edits of existing CC0 recordings; original pitch, no generated voice.
Stages masters outside Assets until their rights/provenance are checked.
"""
from pathlib import Path
import numpy as np, wave, json, hashlib
ROOT=Path(__file__).resolve().parents[4]
OUT=ROOT/'ArtSource/Audio/Impact/Masters';OUT.mkdir(parents=True,exist_ok=True)
RATE=48000; records=[]
def read(p):
 with wave.open(str(ROOT/p)) as w:
  c,r,b=w.getnchannels(),w.getframerate(),w.getsampwidth();raw=w.readframes(w.getnframes())
 if b==2:x=np.frombuffer(raw,'<i2').astype(float)/32768
 elif b==3:
  q=np.frombuffer(raw,np.uint8).reshape(-1,3).astype(np.int32);q=q[:,0]+(q[:,1]<<8)+(q[:,2]<<16);x=np.where(q>=8388608,q-16777216,q)/8388608
 else:raise ValueError((p,b))
 x=x.reshape(-1,c).mean(1)
 if r!=RATE:x=np.interp(np.arange(round(len(x)*RATE/r))*r/RATE,np.arange(len(x)),x)
 return x-x.mean()
def band(x,lo,hi):
 n=1<<(len(x)+4095).bit_length();f=np.fft.rfftfreq(n,1/RATE);g=1/np.sqrt(1+(lo/np.maximum(f,1))**6)/np.sqrt(1+(f/hi)**8)
 return np.fft.irfft(np.fft.rfft(x,n)*g,n)[:len(x)]
def fade(x,a=.008,b=.08):
 x=x.copy();n=min(int(a*RATE),len(x));m=min(int(b*RATE),len(x));x[:n]*=np.linspace(0,1,n);x[-m:]*=np.linspace(1,0,m);return x
def normalize(x,p=.58):return x*p/(np.max(np.abs(x))+1e-10)
def save(name,x,sources,edit,loop=False):
 x=fade(x,.003,.035) if not loop else x
 q=np.clip(np.rint(x*8388607),-8388608,8388607).astype(np.int32);b=np.column_stack([q&255,(q>>8)&255,(q>>16)&255]).astype(np.uint8)
 p=OUT/(name+'.wav')
 with wave.open(str(p),'wb') as w:w.setnchannels(1);w.setsampwidth(3);w.setframerate(RATE);w.writeframes(b.tobytes())
 records.append(dict(file=name+'.wav',sources=sources,license='CC0 1.0; existing repository source trail',date='2026-10-11',model=None,voice='Archeos original adult performance' if 'Cry' in name else None,edit=edit,seconds=len(x)/RATE,peakDb=float(20*np.log10(np.max(np.abs(x))+1e-10)),rmsDb=float(20*np.log10(np.sqrt(np.mean(x*x))+1e-10)),loop=loop,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
D=Path('ArtSource/Audio/Resonance/Decoded')
x=read(D/'scream-325548_2826150-hq.wav')[int(.82*RATE):int(3.2*RATE)]
x=band(x,115,9500);x=normalize(x,.85);x=np.tanh(x*1.8)/1.8
# Short physical early reflections retain the direct onset instead of the shaft tail.
y=np.pad(fade(x,.018,.18),(0,int(.18*RATE)))
for delay,gain in [(.037,.09),(.073,.055),(.129,.03)]:
 n=int(delay*RATE);y[n:n+len(x)]+=x*gain
save('Threat_DirectCry',normalize(y,.58),['Archeos 325548, original excerpt 0.82–3.20 s; Resonance/Sources/download-manifest.json'],'Original adult pitch; 115–9500 Hz; 18 ms attack; gentle fixed crest reduction; short 180 ms physical reflections. Replaces distant shaft treatment for present threat.')
clank=normalize(band(read(D/'mechanical-clank1.wav'),85,7600));rattle=normalize(band(read(D/'mechanical-rattle1.wav'),170,8500))
y=np.zeros(int(.78*RATE));n=min(len(clank),len(y));y[:n]+=clank[:n]*.8;offset=int(.26*RATE);n=min(len(rattle),len(y)-offset);y[offset:offset+n]+=rattle[:n]*.46
save('Threat_FrameContact',normalize(y,.48),['Brian MacIntosh mechanical-clank1.wav / mechanical-rattle1.wav; existing CC0 manifest'],'Dry door-frame contact and short rattle. One warning during held exit release; one visible frame encounter during rush.')
for i in range(3):
 p=Path(f'Assets/_Game/Audio/Clips/Finish/FootVinyl{i}.wav');x=read(p);x=band(x,65,8000);x=normalize(x,.72);x=np.tanh(x*1.4)/1.4
 save(f'Threat_Step{i}',normalize(x,.55),[str(p),'Overhaul/masters.json and source trail'],'Original physical floor contact; 65–8000 Hz; fixed crest reduction; deterministic left/right order. No independent recorded Echo sound.')
scrape=read(D/'scrape-6.wav');scrape=band(scrape,270,6800);scrape=np.tile(scrape,3)[:int(2.4*RATE)];t=np.arange(len(scrape))/RATE;scrape*=.55+.45*np.sin(2*np.pi*2*t)**2
n=int(.24*RATE);join=scrape[-n:]*(1-np.linspace(0,1,n))+scrape[:n]*np.linspace(0,1,n);scrape=np.concatenate([scrape[n:-n],join]);scrape-=scrape.mean()
save('Threat_MovingFriction',normalize(scrape,.22),['AntumDeluge scrape-6.wav; existing CC0 manifest'],'Band-limited physical scrape/grain. Level follows actual present movement, spatialized from torso; seamless 240 ms overlap.',True)
# Original internal music: discrete irregular accelerations with gaps for steps.
t=np.arange(int(8*RATE))/RATE;y=np.zeros_like(t)
for onset in [0,.72,1.44,2.02,2.62,3.16,3.72,4.23,4.78,5.30,5.84,6.38,6.92,7.48]:
 u=t-onset;env=np.where(u>=0,np.exp(-np.maximum(u,0)*16)*(1-np.exp(-np.maximum(u,0)*120)),0)
 y+=env*(np.sin(2*np.pi*98.8*u)+.22*np.sin(2*np.pi*202*u)+.1*np.sin(2*np.pi*610*u))
y=normalize(y,.26)
save('Pursuit_Pulse',y,['Original nonverbal modal composition, no licensed input'],'8 s internal nonverbal pursuit pulse; sub/broad mid partials, gaps for spatial contacts; onset/offset fades owned by present threat.',True)
(OUT.parent/'masters.json').write_text(json.dumps(records,indent=2))
print(json.dumps([{k:r[k] for k in ('file','seconds','peakDb','rmsDb','sha256')} for r in records],indent=2))
