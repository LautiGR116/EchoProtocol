"""Provisional exterior ambience, derived only from the traced CC0 public HQ preview.
No auditory selection is claimed. Requires macOS afconvert and Python NumPy.
"""
from pathlib import Path
import subprocess, tempfile, wave, hashlib, json
import numpy as np
base=Path(__file__).resolve().parent
source=base/'481964_6657854-hq.mp3'
assert hashlib.sha256(source.read_bytes()).hexdigest()=='78da966dd6a876d9b53f148cdf9d443454f732bb6b9f82b8ffb59a8d22283e42'
with tempfile.TemporaryDirectory() as tmp:
    decoded=Path(tmp)/'preview.wav'
    subprocess.run(['/usr/bin/afconvert','-f','WAVE','-d','LEI16',str(source),str(decoded)],check=True)
    with wave.open(str(decoded),'rb') as w:
        assert w.getnchannels()==1 and w.getsampwidth()==2 and w.getframerate()==48000
        x=np.frombuffer(w.readframes(w.getnframes()),dtype='<i2').astype(np.float64)/32768
fade=48000
t=np.linspace(0,1,fade)
y=np.concatenate([x[fade:-fade],x[-fade:]*(1-t)+x[:fade]*t])*10**(18/20)
assert np.isfinite(y).all() and np.max(np.abs(y))<.2
pcm=np.round(y*32767).astype('<i2')
out=base/'AMB_CourtyardNight_LP.wav'
with wave.open(str(out),'wb') as w:
    w.setnchannels(1);w.setsampwidth(2);w.setframerate(48000);w.writeframes(pcm.tobytes())
z=pcm.astype(np.float64)/32768
report={'date':'2026-10-01','source':source.name,'sourceSHA256':hashlib.sha256(source.read_bytes()).hexdigest(),'sourceRights':'CC0 1.0; see source.json. Public MP3 HQ preview, not original WAV.','output':out.name,'sha256':hashlib.sha256(out.read_bytes()).hexdigest(),'sampleRate':48000,'channels':1,'frames':len(z),'seconds':len(z)/48000,'constantGainDB':18,'loopCrossfadeSeconds':1,'processing':'48k mono16 PCM decode; first/last1s linear overlap, steady gain+18dB; no EQ, compression or generated layer','peakDBFS':float(20*np.log10(np.max(np.abs(z)))),'rmsDBFS':float(20*np.log10(np.sqrt(np.mean(z*z)))),'loopBoundaryStep':float(abs(z[-1]-z[0])),'maxAdjacentStep':float(np.max(abs(np.diff(z)))),'humanAudition':False,'status':'Provisional integration candidate; listening, timbre, audibility and comfort EN PRUEBA'}
(base/'master.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report,indent=2))
