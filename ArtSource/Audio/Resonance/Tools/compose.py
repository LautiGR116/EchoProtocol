"""Deterministic offline edit. CC0 source provenance in Sources/download-manifest.json.
All spatial masters mono, 48 kHz/24-bit. No generated or cloned voices.
"""
from pathlib import Path
import numpy as np, wave, json, hashlib, subprocess
ROOT=Path(__file__).resolve().parents[4]; D=ROOT/'ArtSource/Audio/Resonance/Decoded'; OUT=ROOT/'Assets/_Game/Audio/Clips/Resonance';OUT.mkdir(parents=True,exist_ok=True);RATE=48000;records=[]
def read(name):
 p=D/name
 with wave.open(str(p)) as w:
  c,r,b=w.getnchannels(),w.getframerate(),w.getsampwidth();raw=w.readframes(w.getnframes())
  if b==2:x=np.frombuffer(raw,'<i2').astype(float)/32768
  elif b==3:
   a=np.frombuffer(raw,np.uint8).reshape(-1,3).astype(np.int32);a=a[:,0]+(a[:,1]<<8)+(a[:,2]<<16);a=np.where(a>=8388608,a-16777216,a);x=a/8388608
  else:raise ValueError(p)
  x=x.reshape(-1,c).mean(1)
  if r!=RATE:x=np.interp(np.arange(round(len(x)*RATE/r))*r/RATE,np.arange(len(x)),x)
  return x-x.mean()
def fade(x,a=.025,b=.12):
 x=x.copy();n=min(int(a*RATE),len(x));m=min(int(b*RATE),len(x));x[:n]*=np.linspace(0,1,n)**2;x[-m:]*=np.linspace(1,0,m)**2;return x
def band(x,lo=65,hi=8500):
 n=1<<(len(x)+4095).bit_length();f=np.fft.rfftfreq(n,1/RATE);gain=1/np.sqrt(1+(lo/np.maximum(f,1))**6)/np.sqrt(1+(f/hi)**8);return np.fft.irfft(np.fft.rfft(x,n)*gain,n)[:len(x)]
def norm(x,peak=.5,rms=None):
 x=x.copy();g=peak/(np.max(np.abs(x))+1e-12)
 if rms is not None:g=min(g,rms/np.sqrt(np.mean(x*x)+1e-12))
 return x*g
def speed(x,factor):return np.interp(np.arange(round(len(x)/factor))*factor,np.arange(len(x)),x)
def mix(seconds,layers):
 y=np.zeros(int(seconds*RATE))
 for t,x,g in layers:
  a=int(t*RATE);n=min(len(x),len(y)-a)
  if n>0:y[a:a+n]+=x[:n]*g
 return y
def room(x,tail=1.2,amount=.22):
 # Authored early reflections + diffuse exponentially decaying tail. Local source
 # remains mono; game geometry and orientation own localization.
 ir=np.zeros(int(tail*RATE));ir[0]=1
 for t,g in [(.029,.19),(.047,-.15),(.073,.11),(.113,.09),(.179,-.055)]:ir[int(t*RATE)]+=g*amount/.22
 rng=np.random.default_rng(640);noise=band(rng.standard_normal(len(ir)),160,3300);t=np.arange(len(ir))/RATE;noise*=np.exp(-t*6.9/tail)*(1-np.exp(-t*40));ir+=noise*amount*.009
 n=1<<(len(x)+len(ir)-1).bit_length();return np.fft.irfft(np.fft.rfft(x,n)*np.fft.rfft(ir,n),n)[:len(x)+len(ir)-1]
def modes(seconds,freqs,decay=3):
 t=np.arange(int(seconds*RATE))/RATE;y=np.zeros_like(t)
 for i,f in enumerate(freqs):y+=np.sin(2*np.pi*f*t+.19*i)*np.exp(-t/decay)/(1+i*.8)
 return fade(y,.045,.4)
def save(name,x,sources,edit,peak=.5,rms=.15,loop=False):
 x=norm(band(x,40,14500),peak,rms);x=fade(x,.005,.045) if not loop else x
 # Crossfade the tail into the head. The seam then joins consecutive original
 # samples; do not force arbitrary endpoints to zero (that introduces DC).
 if loop:
  n=int(.4*RATE);tail=x[-n:].copy();head=x[:n].copy();curve=np.linspace(0,1,n);join=tail*(1-curve)+head*curve;x=np.concatenate([x[n:-n],join])
  x-=np.mean(x);x=norm(x,peak,rms)
 q=np.clip(np.rint(x*8388607),-8388608,8388607).astype(np.int32);b=np.column_stack([q&255,(q>>8)&255,(q>>16)&255]).astype(np.uint8)
 p=OUT/(name+'.wav')
 with wave.open(str(p),'wb') as w:w.setnchannels(1);w.setsampwidth(3);w.setframerate(RATE);w.writeframes(b.tobytes())
 records.append(dict(file=str(p.relative_to(ROOT)),sources=sources,edit=edit,seconds=len(x)/RATE,peakDb=round(20*np.log10(max(np.max(np.abs(x)),1e-10)),2),rmsDb=round(20*np.log10(np.sqrt(np.mean(x*x))+1e-10),2),loop=loop,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
clank=norm(band(read('mechanical-clank1.wav'),75,6200));latch=norm(read('mechanical-lightclunk1.wav'));rattle=norm(read('mechanical-rattle1.wav'));swing=norm(read('metal_interactions/metal_swing1.wav'))
s1=norm(band(read('scrape-1.wav'),150,6100));s2=norm(band(read('scrape-2.wav'),150,6100));s6=norm(band(read('scrape-6.wav'),130,5300))
save('Contact_Closed',room(latch,.26,.07),['mechanical-lightclunk1.wav'],'Mono; 75 Hz high pass, short room, fades',peak=.48,rms=.14)
save('Relay_Strain',mix(.70,[(0,clank,.8),(.03,speed(swing,.85),.24)]),['mechanical-clank1.wav','metal_interactions/metal_swing1.wav'],'Physical metal onset and friction, fits 0.82 s lane stage',peak=.33,rms=.12)
save('Transfer_Seat',mix(1.7,[(0,latch,.65),(.13,clank,.4),(.07,speed(swing,.76),.28)]),['mechanical-lightclunk1.wav','mechanical-clank1.wav','metal_interactions/metal_swing1.wav'],'Two causal contacts; tail; no voice',peak=.44,rms=.14)
save('Latch_Release',mix(.65,[(0,norm(read('mechanical-mechanical2.wav')),.7),(.21,latch,.24)]),['mechanical-mechanical2.wav','mechanical-lightclunk1.wav'],'Lever release, short fade',peak=.44,rms=.12)
# Three contacts / falling metal modes become the recognisable nonverbal motif.
strain=mix(8.8,[(.12,speed(s1,.78),.5),(2.15,speed(s6,.82),.34),(4.8,speed(s2,.64),.46),(.3,modes(7,[111,226.4,339.6],2.1),.07),(2.35,modes(5,[104.8,214.5,319],1.7),.05),(5.0,modes(3.8,[98.8,202,299],1.4),.07)])
save('Wall_ThreeStrains',room(strain,1.15,.17),['scrape-1.wav','scrape-2.wav','scrape-6.wav','original modal composition'],'Three uneven real friction gestures, descending 111/104.8/98.8 Hz modes, room decay',peak=.46,rms=.095)
save('Passage_RearContact',room(mix(4.6,[(0,speed(s6,.84),.8),(1.7,speed(s1,.90),.34),(2.9,rattle,.32)]),.75,.11),['scrape-6.wav','scrape-1.wav','mechanical-rattle1.wav'],'Concrete drag behind rear partition, no vocal ownership or footfall',peak=.48,rms=.10)
save('Archive_Approach',room(mix(6.7,[(0,speed(s2,.87),.34),(2.0,speed(s1,.76),.55),(4.2,speed(s6,.88),.72),(5.6,latch,.38)]),1.1,.16),['scrape-1.wav','scrape-2.wav','scrape-6.wav','mechanical-lightclunk1.wav'],'Deliberately approaching sequence, increasing friction density, no footstep pattern',peak=.48,rms=.10)
# Keep actual adult performance recognizable. No formant shift or monster pitch.
cry=read('cry-776565_16019975-hq.wav')[int(17.5*RATE):int(29.5*RATE)]
cry=band(cry,330,2950);cry=np.tanh(cry*2)/2 # receiver bandwidth and gentle crest reduction
save('Line_Unanswered',mix(15.2,[(1.45,fade(norm(cry,.5,.16),.22,.65),.9),(14.1,latch,.11)]),['SPAudiobooks 776565, excerpt 17.5–29.5 s'],'Native pitch, mono telephone 330–2950 Hz, 1.45 s anticipation, twelve-second vocal excerpt, line end',peak=.47,rms=.09)
scream=read('scream-325548_2826150-hq.wav')[int(.82*RATE):int(3.2*RATE)]
save('Shaft_DistantCry',room(fade(band(scream,220,3200),.12,.35),2.05,.3),['Archeos 325548, excerpt 0.82–3.20 s'],'Adult actor original pitch, distant shaft bandwidth and 2.05 s decay; one live shutdown cry only',peak=.48,rms=.12)
# Score is explicitly subjective, never a purported voice or a sound from Echo.
def score(sec):
 y=np.zeros(int(sec*RATE));t=np.arange(len(y))/RATE
 for at,f,vol in [(0,111,.15),(4.9,104.8,.13),(10.4,98.8,.10)]:
  x=modes(max(1,sec-at),[f,f*2.014,f*3.053,f*4.01],4.3);a=int(at*RATE);y[a:a+len(x)]+=x*vol
 # Recorded material provides bow grain underneath tonal modes.
 grain=band(s1,650,2300);grain=np.resize(grain,len(y));grain*=np.sin(np.pi*np.clip(t/sec,0,1))**2*.016
 return fade(y+grain,1.2,3)
save('Score_Recognition',score(19),['original modal composition','scrape-1.wav'],'Internal nonverbal cue; descending structural motif, 1.2 s entrance/3 s release',peak=.38,rms=.075)
save('Score_Release',fade(speed(score(19),.84),1.8,5),['original modal composition','scrape-1.wav'],'Lowered final reminiscence; no vocal, five-second release',peak=.3,rms=.06)
save('Key_Tray',room(mix(.85,[(0,norm(read('metal_interactions/metal_button_press2.wav')),.5),(.14,latch,.4)]),.35,.08),['metal_interactions/metal_button_press2.wav','mechanical-lightclunk1.wav'],'Small metal contact and restrained tray resonance',peak=.45,rms=.12)
# New looping texture derives from existing accepted real air, no synthetic noise floor.
air_path=ROOT/'Assets/_Game/Audio/Clips/Ambience/AMB_FacilityAir_LP.wav'
subprocess.run(['/usr/bin/afconvert','-f','WAVE','-d','LEI16@48000',str(air_path),str(D/'licensed-facility-air.wav')],check=True,capture_output=True)
air=read('licensed-facility-air.wav');t=np.arange(len(air))/RATE
# Windowed spectral emphasis: tonal pressure, not a bass-only clue.
air=band(air,80,3800);air*=.79+.14*np.cos(2*np.pi*t/(len(air)/RATE))+.07*np.sin(2*np.pi*t/(len(air)/RATE)*3)
save('Pipe_Air_LP',air,['HECKFRICKER 753542 via existing FacilityAir derivative'],'Slow physical air modulation, 80–3800 Hz, seamless overlap, no musical pulse',peak=.45,rms=.10,loop=True)
(OUT/'../ResonanceManifest.json').resolve().write_text(json.dumps(records,indent=2))
(ROOT/'ArtSource/Audio/Resonance/masters.json').write_text(json.dumps(records,indent=2));print(json.dumps([{'file':r['file'].split('/')[-1],'s':round(r['seconds'],2),'peak':r['peakDb'],'rms':r['rmsDb']} for r in records],indent=2))
