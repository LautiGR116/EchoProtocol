from pathlib import Path
import wave, numpy as np, json, hashlib, shutil
root=Path(__file__).resolve().parents[3]; base=root/'ArtSource/Audio/Intruder'; out=root/'Assets/_Game/Audio/Clips/Intruder'; R=48000; ledger=[]
def read(id):
 with wave.open(str(base/'Decoded'/f'{id}-hq.wav')) as w:return np.frombuffer(w.readframes(w.getnframes()),'<i2').astype(float)/32768

def master(name,id,a,b,peak,hi=10000,lo=85,fadeout=.05):
 x=read(id)[int(a*R):int(b*R)].copy();x-=x.mean();n=1<<(len(x)+4095).bit_length();f=np.fft.rfftfreq(n,1/R)
 gain=1/np.sqrt(1+(lo/np.maximum(f,1))**6)/np.sqrt(1+(f/hi)**8);x=np.fft.irfft(np.fft.rfft(x,n)*gain,n)[:len(x)]
 attack=min(int(.004*R),len(x));tail=min(int(fadeout*R),len(x));x[:attack]*=np.linspace(0,1,attack);x[-tail:]*=np.linspace(1,0,tail)**2;x*=peak/max(abs(x).max(),1e-9)
 target=out/(name+'.wav')
 with wave.open(str(target),'wb') as w:w.setparams((1,2,R,0,'NONE','not compressed'));w.writeframes(np.round(x*32767).astype('<i2').tobytes())
 ledger.append(dict(file=name+'.wav',source=id,region=[a,b],peak=float(abs(x).max()),seconds=len(x)/R,sha256=hashlib.sha256(target.read_bytes()).hexdigest(),edit=f'Mono 48 kHz PCM16, DC removal, {lo}–{hi} Hz gentle band limits, 4 ms attack and {fadeout} s tail; original pitch, no generated layer.'))
master('Intruder_BreachVoice',173944,.30,1.46,.59,10500,95,.08)
master('Intruder_ChargeEffort',272023,.025,.61,.39,9500,110,.07)
master('Intruder_MetalBreach',556693,.11,.96,.54,7000,65,.16)
master('Intruder_DoorHandle',364682,0,.51,.28,9000,130,.045)
master('Intruder_DoorPressure',364682,1.03,1.68,.26,5200,70,.07)
# A separate unvoiced intake from the new performance; intermittent breathing is
# spaced in the master, not a continuous voiced scream under the exposed exit.
x=read(173944)[int(6.0*R):int(7.02*R)]; silence=np.zeros(int(1.45*R)); y=np.concatenate([x,silence]);y-=y.mean();y[:500]*=np.linspace(0,1,500);y[len(x)-4000:len(x)]*=np.linspace(1,0,4000);y[len(x):]=0;y*=.13/max(abs(y).max(),1e-9)
target=out/'Intruder_CloseBreath.wav'
with wave.open(str(target),'wb') as w:w.setparams((1,2,R,0,'NONE','not compressed'));w.writeframes(np.round(y*32767).astype('<i2').tobytes())
ledger.append(dict(file=target.name,source=173944,region=[6,7.02],seconds=len(y)/R,peak=.13,sha256=hashlib.sha256(target.read_bytes()).hexdigest(),edit='Original intake, 10 ms in / 83 ms out, 1.45 s silence before next breath; mono 48 kHz PCM16; no pitch alteration. Casting is not human-approved.'))
(base/'masters.json').write_text(json.dumps(ledger,indent=2))
print(json.dumps([dict(file=r['file'],seconds=r['seconds']) for r in ledger]))
