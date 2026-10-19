from pathlib import Path
import json,wave,math,numpy as np
root=Path('/Users/lautigr116/EchoProtocol/Docs/VisualEvidence/2026-10-09-Coverage')
reports=[]
for name in ['native-final-02','native-campaign-final']:
 p=root/name
 if not (p/'audio-route.json').exists():continue
 r=json.loads((p/'audio-route.json').read_text())
 with wave.open(str(p/'listener.wav')) as w:
  channels=w.getnchannels();rate=w.getframerate();peak=0;sum_sq=0;n=0;clipped=0;max_diff=0
  while True:
   b=w.readframes(48000*5)
   if not b:break
   x=np.frombuffer(b,dtype='<i2').astype(np.float64)/32767
   peak=max(peak,float(np.max(np.abs(x))));sum_sq+=float(np.dot(x,x));n+=len(x);clipped+=int(np.sum(np.abs(x)>=.999))
   if channels==2:max_diff=max(max_diff,float(np.max(np.abs(x.reshape(-1,2)[:,0]-x.reshape(-1,2)[:,1]))))
 reports.append(dict(route=name,status=r['status'],rate=rate,channels=channels,peak=peak,peakDBFS=20*math.log10(peak) if peak else None,rms=math.sqrt(sum_sq/n),clippedSamples=clipped,maxLRDifference=max_diff,dropped=r['dropped'],shapeMismatch=r['shapeMismatch'],bufferStarved=r['bufferStarved'],meanFrameMs=r['meanMs'],p95FrameMs=r['p95Ms'],allStatesMono=all(s['mono'] for s in r['states']),allStatesReduced=all(s['reduced'] for s in r['states'])))
(root/'audio-summary.json').write_text(json.dumps({'scope':'Actual post-SessionAudio native PCM. No normalization; neither hardware SPL, comfort nor human fear is proved.', 'routes':reports},indent=2)+'\n')
print(json.dumps(reports,indent=2))
