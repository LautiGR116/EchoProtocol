"""Deterministic original synthesis; not recordings of voices or of Echo channels."""
import math,random,wave,array,os
out='Assets/_Game/Audio/Clips/Campaign';os.makedirs(out,exist_ok=True);rate=48000
random.seed(613)
def write(name,duration,fn,peak):
 buf=array.array('f');state=[0,0]
 for i in range(int(duration*rate)):
  t=i/rate;n=random.uniform(-1,1);state[0]=.965*state[0]+.035*n;state[1]=.75*state[1]+.25*n
  buf.append(fn(t,n,state))
 maximum=max(abs(x) for x in buf) or 1
 pcm=array.array('h',(int(max(-1,min(1,x/maximum*peak))*32767) for x in buf))
 with wave.open(out+'/'+name+'.wav','wb') as f:f.setparams((1,2,rate,0,'NONE','not compressed'));f.writeframes(pcm.tobytes())
 print(name,len(buf)/rate,round(20*math.log10(peak),1),'dBFS peak')
write('Relay',.8,lambda t,n,s: (n*.3+math.sin(t*2*math.pi*180)*.2)*math.exp(-t*25)+(s[0]*.2)*math.exp(-max(0,t-.13)*12)*(t>.13),.32)
write('Telephone',2.4,lambda t,n,s: (math.sin(t*2*math.pi*880)+.4*math.sin(t*2*math.pi*1321))*(.6+.4*math.sin(t*2*math.pi*21))*min(1,t*40)*min(1,(2.4-t)*25)*(.5+.5*math.sin(t*2*math.pi*2)),.24)
write('Wire',2.6,lambda t,n,s: (s[1]*.5+math.sin(t*2*math.pi*(116+9*math.sin(t*3)))*.16)*math.sin(math.pi*t/2.6)**2,.25)
write('WallContact',3.5,lambda t,n,s: (s[0]*.8+math.sin(t*2*math.pi*61)*.1)*sum(math.exp(-max(0,t-d)*16)*(t>=d) for d in [.1,.82,2.1]),.28)
write('CloseMovement',2.8,lambda t,n,s: (s[0]*.8+s[1]*.18)*math.sin(math.pi*t/2.8)**2*(.65+.35*math.sin(t*17)),.22)
write('Archive',4,lambda t,n,s: (math.sin(t*2*math.pi*146.83)*.3+math.sin(t*2*math.pi*220)*.13+s[0]*.2)*math.sin(math.pi*t/4)**2,.22)
# Exactly periodic 8 s machinery loop; start/end window avoids discontinuity.
write('PressureBed',8,lambda t,n,s: (math.sin(t*2*math.pi*48)*.26+math.sin(t*2*math.pi*96)*.1+s[0]*.45)*(.8+.2*math.sin(t*2*math.pi*.5))*min(1,t*8)*min(1,(8-t)*8),.24)
# Present-tense Player-only contacts. They are deliberately never attached to an Echo.
for surface in ['Vinyl','Wood']:
 for variant in range(3):
  frequency=(92 if surface=='Wood' else 125)+variant*7
  decay=22 if surface=='Wood' else 31
  write('Step'+surface+str(variant),.28,lambda t,n,s,f=frequency,d=decay: (math.sin(t*2*math.pi*f)*.22+s[1]*.75)*min(1,t*600)*math.exp(-t*d)+s[0]*.12*math.exp(-max(0,t-.10)*30)*(t>.10),.23)
