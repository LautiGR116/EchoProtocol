from pathlib import Path
import wave,json,numpy as np
root=Path(__file__).resolve().parents[4];rows=[]
for p in (root/'Assets/_Game/Audio/Clips/Resonance').glob('*.wav'):
 with wave.open(str(p)) as w:
  rate=w.getframerate();a=np.frombuffer(w.readframes(w.getnframes()),np.uint8).reshape(-1,3).astype(np.int32);x=a[:,0]+(a[:,1]<<8)+(a[:,2]<<16);x=np.where(x>=8388608,x-16777216,x)/8388608
 sp=abs(np.fft.rfft(x))**2;f=np.fft.rfftfreq(len(x),1/rate);frac=float(sp[(f>=200)&(f<=8000)].sum()/sp.sum());delta=np.abs(np.diff(x));rows.append(dict(clip=p.name,channels=1,rate=rate,seconds=len(x)/rate,peak=float(max(abs(x))),rms=float(np.sqrt(np.mean(x*x))),dc=float(x.mean()),finite=bool(np.isfinite(x).all()),speakerBandEnergy=frac,seam=float(abs(x[0]-x[-1])),sampleDelta99=float(np.quantile(delta,.99))))
(root/'Docs/AudioEvidence/2026-10-08-Resonance/master-analysis.json').write_text(json.dumps(rows,indent=2));print('Masters',len(rows),'finite',all(x['finite'] for x in rows),'maxpeak',max(x['peak'] for x in rows))
