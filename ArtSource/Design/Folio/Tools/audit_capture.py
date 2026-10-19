"""Signal/integrity check for the passive final native mix; no listening claim."""
from pathlib import Path
import json,wave,array,math,sys
root=Path(__file__).resolve().parents[4];folder=root/'Docs/VisualEvidence/2026-10-08-Folio/continuous-final-02'
a=json.loads((folder/'audio-route.json').read_text());peak=0;clipped=0;samples=0;square=0
with wave.open(str(folder/'listener.wav'),'rb')as w:
 assert w.getnchannels()==2 and w.getframerate()==48000 and w.getsampwidth()==2
 while b:=w.readframes(32768):
  values=array.array('h',b)
  if sys.byteorder!='little':values.byteswap()
  for v in values:peak=max(peak,abs(v));clipped+=abs(v)>=32766;samples+=1;square+=v*v
states=a['states'];last=states[-1]
report={'status':'PASS'if a['dropped']==0 and not a['error'] and a['writerJoined'] and samples==a['sampleCount'] and clipped==0 else'LIMITED','gameplayRouteStatus':a['status'],'scope':'Actual post-session mix, no normalization or source soloing. Checks capture completeness and digital samples only; not voice/localization/comfort approval or proof of OS/hardware output.','rate':a['rate'],'channels':a['channels'],'dropped':a['dropped'],'shapeMismatch':a.get('shapeMismatch'),'bufferStarved':a.get('bufferStarved'),'writerJoined':a['writerJoined'],'sampleCountMatches':samples==a['sampleCount'],'clippedSamples':clipped,'peakDbFS':20*math.log10(max(1,peak)/32767),'rmsDbFS':20*math.log10(max(1,math.sqrt(square/max(1,samples)))/32767),'cueCount':last['cueCount'],'recalls':last['recalls'],'sourcePlays':{e['name']:e['plays']for e in last['emitters']},'frameMeanMs':a['meanMs'],'frameP95Ms':a['p95Ms']}
(folder/'capture-audit.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report,indent=2))
