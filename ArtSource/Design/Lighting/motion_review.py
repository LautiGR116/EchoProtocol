from pathlib import Path
from PIL import Image,ImageDraw
r=Path('/Users/lautigr116/EchoProtocol/Docs/VisualEvidence/2026-10-08-Lighting/final2-native/motion');fs=sorted(r.glob('*.png'))
for k in range((len(fs)+63)//64):
 s=Image.new('RGB',(1920,1600),(12,12,12));d=ImageDraw.Draw(s)
 for i,f in enumerate(fs[k*64:(k+1)*64]):
  x=(i%8)*240;y=(i//8)*200;im=Image.open(f);im.thumbnail((238,174));s.paste(im,(x,y));d.text((x+4,y+176),f.stem,fill='white')
 s.save('/tmp/final2-lighting-motion-'+str(k)+'.jpg')
print(len(fs))
