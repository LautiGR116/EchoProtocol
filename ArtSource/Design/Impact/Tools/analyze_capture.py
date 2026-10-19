from pathlib import Path
import json,wave,sys,numpy as np
p=Path(sys.argv[1]);m=json.loads((p/'capture.json').read_text())
with wave.open(str(p/'listener.wav')) as w:x=np.frombuffer(w.readframes(w.getnframes()),'<i2').reshape(-1,w.getnchannels())/32768;r=w.getframerate()
def db(a):return round(float(20*np.log10(max(float(a),1e-9))),2)
rows=[]
for i,q in enumerate(m['marks']):
 a=int(q['seconds']*r);b=int((m['marks'][i+1]['seconds'] if i+1<len(m['marks']) else len(x)/r)*r);y=x[a:b]
 if not len(y):continue
 if 'coverage_' not in q['step']:continue
 z=y[:len(y)//480*480].reshape(-1,480,y.shape[1])
 rows.append(dict(step=q['step'],start=q['seconds'],end=b/r,seconds=round(len(y)/r,2),peakDb=db(np.max(np.abs(y))),rmsDb=db(np.sqrt(np.mean(y*y))),shortRms95Db=db(np.percentile(np.sqrt(np.mean(z*z,axis=(1,2))),95)) if len(z) else -180,stereoRms=[db(np.sqrt(np.mean(y[:,c]**2))) for c in range(y.shape[1])]))
 rows[-1].update(listenerVolume=q.get('listenerVolume',m['listenerVolume']),mono=q.get('mono',m['mono']),reduced=q.get('reduced',m['reduced']),maxChannelDifference=float(np.max(np.abs(y[:,0]-y[:,1]))) if y.shape[1]==2 else 0)
z=x[:len(x)//480*480].reshape(-1,480,x.shape[1]);peaks=np.max(np.abs(z),axis=(1,2))
report=dict(scope='Actual PCM after SessionAudio. Each stage names its listener volume and mono/reduced mode; compare only matched settings. RMS/peaks measure digital presence, not ear comfort or fear.',rate=r,channels=x.shape[1],seconds=len(x)/r,peakDb=db(np.max(np.abs(x))),rmsDb=db(np.sqrt(np.mean(x*x))),samplesAtFullScale=int(np.sum(np.abs(x)>=32767/32768)),tenMsBlocksAtCeiling=int(np.sum(peaks>=.498)),totalBlocks=len(peaks),rows=rows)
(p/'signal-summary.json').write_text(json.dumps(report,indent=2))
print(json.dumps({k:v for k,v in report.items() if k!='rows'},indent=2));print(json.dumps([q for q in rows if q['step'] in ['coverage_deliberate_failure','coverage_hold_release','coverage_prepared_return','coverage_record_first_noise']],indent=2))
