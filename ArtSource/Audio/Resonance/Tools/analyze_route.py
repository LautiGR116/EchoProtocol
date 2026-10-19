from pathlib import Path
import numpy as np,wave,json,sys
p=Path(sys.argv[1]);j=json.load(open(p/'audio-route.json'));states=j['states']
with wave.open(str(p/'listener.wav')) as w:r=w.getframerate();c=w.getnchannels();x=np.frombuffer(w.readframes(w.getnframes()),'<i2').astype(float).reshape(-1,c)/32768
rows=[];windows=[]
for a in range(0,len(x),r//4):
 v=x[a:a+r//4];t=a/r;state=next((s for s in reversed(states) if s['seconds']<=t),states[0]);energy=np.mean(v*v,axis=0);rms=np.sqrt(energy.mean());windows.append(dict(seconds=t,rmsDb=float(20*np.log10(rms+1e-10)),peakDb=float(20*np.log10(np.abs(v).max()+1e-10)),balanceDb=float(10*np.log10((energy[-1]+1e-12)/(energy[0]+1e-12))),paused=state['paused'],step=state['step'],cue=state['cue']))
last=0
for s in states:
 if s['cueCount']!=last:
  last=s['cueCount'];t=s['seconds'];active=[e for e in s['emitters'] if e['playing'] and e['gain']>0];v=x[max(0,int((t-.25)*r)):min(len(x),int((t+9)*r))]
  rows.append(dict(seconds=t,cue=s['cue'],count=last,peaks=float(np.abs(v).max()),rms=float(np.sqrt(np.mean(v*v))),active=active))
report=dict(status=j['status'],duration=len(x)/r,peak=float(np.abs(x).max()),rms=float(np.sqrt(np.mean(x*x))),finite=bool(np.isfinite(x).all()),hardClippedSamples=int((np.abs(x)>=.999).sum()),dropped=j['dropped'],writerJoined=j['writerJoined'],error=j['error'],events=rows,windows=windows)
(p/'mix-analysis.json').write_text(json.dumps(report,indent=2));print(json.dumps({k:v for k,v in report.items() if k not in ('events','windows')},indent=2));print('Events:',[(round(e['seconds'],2),e['cue']) for e in rows])
