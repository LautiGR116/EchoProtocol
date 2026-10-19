from pathlib import Path
import json,wave,numpy as np
capture=Path('/Users/lautigr116/EchoProtocol-backups/incursion-evidence-20261011/native/audiovisual')
with wave.open(str(capture/'listener.wav')) as w:
 rate=w.getframerate();ch=w.getnchannels();width=w.getsampwidth();x=np.frombuffer(w.readframes(w.getnframes()),'<i2').reshape(-1,ch).astype(float)/32768
assert width==2 and ch==2
rows=[json.loads(r) for r in (capture/'frames.jsonl').read_text().splitlines()]
plan=json.loads(Path('/Users/lautigr116/EchoProtocol-backups/incursion-evidence-20261011/video-plan.json').read_text())
def measure(start,end):
 y=x[round(start*rate):round(end*rate)];return dict(start=start,end=end,rmsDb=float(20*np.log10(np.sqrt(np.mean(y*y))+1e-12)),peakDb=float(20*np.log10(np.max(np.abs(y))+1e-12)),fullScaleSamples=int(np.count_nonzero(np.abs(y)>=32767/32768)),maxChannelDifference=float(np.max(np.abs(y[:,0]-y[:,1]))))
result=dict(scope='Actual post-SessionAudio native PCM; first cut saved stereo/normal, second cut nonpersistent mono/reduced retry. Same saved volume .6984127. Not equal-settings A/B, device SPL, comfort or human casting approval.',rate=rate,channels=ch,cuts=[dict(label=c['label'],**measure(c['start'],c['end'])) for c in plan['cuts']],whole=measure(0,len(x)/rate))
assert result['whole']['fullScaleSamples']==0
assert result['cuts'][1]['maxChannelDifference']==0
assert result['cuts'][1]['peakDb']<=20*np.log10(.181)
Path('Docs/VisualEvidence/2026-10-11-Incursion/audio-signal.json').write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2))
