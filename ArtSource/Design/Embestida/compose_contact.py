from pathlib import Path
import numpy as np, wave, json, hashlib
from scipy.signal import butter,sosfilt
root=Path(__file__).resolve().parents[3];out=root/'Assets/_Game/Audio/Clips/Embestida';R=48000;ledger=[]
def read(id,a,b):
 p=root/f'ArtSource/Audio/Intruder/Decoded/{id}-hq.wav'
 with wave.open(str(p)) as w:
  c=w.getnchannels();r=w.getframerate();x=np.frombuffer(w.readframes(w.getnframes()),'<i2').astype(float).reshape(-1,c).mean(1)/32768
 assert r==R
 return x[int(a*R):int(b*R)]
def band(x,lo,hi):return sosfilt(butter(2,[lo,hi],fs=R,btype='band',output='sos'),x)
def write(name,x,peak,source):
 x-=np.mean(x);n=min(len(x),480);x[:n]*=np.linspace(0,1,n);x[-min(3800,len(x)):]*=np.linspace(1,0,min(3800,len(x)))**2
 x*=peak/max(abs(x).max(),1e-8);p=out/(name+'.wav')
 with wave.open(str(p),'wb') as w:w.setparams((1,2,R,0,'NONE','not compressed'));w.writeframes(np.round(x*32767).astype('<i2').tobytes())
 ledger.append(dict(file=str(p.relative_to(root)),source=source,sha256=hashlib.sha256(p.read_bytes()).hexdigest(),seconds=len(x)/R,peak=peak))
# Contact begins with the real metal attack; no music/voice covers it. Original
# short damped low resonances supply housing mass without an infrasonic layer.
x=band(read(556693,.11,.69),80,6500);t=np.arange(len(x))/R
thump=(np.sin(2*np.pi*(125*t-27*t*t))+.40*np.sin(2*np.pi*213*t))*np.exp(-t*24)
x=x*.72+thump*.11
write('Embestida_HousingContact',x,.56,'NachtmahrTV 556693 CC0, 0.11–0.69 s; original authored damped 125/213 Hz housing modes, 80–6500 Hz band. No pitch/time change.')
x=band(read(272023,.045,.39),130,5800)
write('Embestida_ExpelledEffort',x,.34,'aldenroth2 272023 CC0, 0.045–0.39 s. One compressed adult effort, no pitch/time change, 130–5800 Hz. Plays 75 ms after contact.')
x=band(read(173944,6.0,7.02),180,6200);x=np.concatenate([x,np.zeros(int(.44*R)),x[:int(.51*R)]*.70,np.zeros(int(.65*R))])
write('Embestida_BodyBreath',x,.15,'JohnsonBrandEditing 173944 CC0, 6.00–7.02 s intake with shorter repeated release and authored silence. No pitch/time change. Body source after impact.')
(root/'ArtSource/Design/Embestida/audio-masters.json').write_text(json.dumps(dict(date='2026-10-12',generation='No AI voice generation/cloning. Existing locally traced CC0 recordings plus original DSP.',masters=ledger),indent=2))
print([(a['file'],a['seconds']) for a in ledger])
