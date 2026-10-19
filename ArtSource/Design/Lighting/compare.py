from pathlib import Path
from PIL import Image, ImageDraw
import sys,json,re
root=Path('/Users/lautigr116/EchoProtocol')
b=root/'Docs/VisualEvidence/2026-10-08-Lighting/baseline-native/paired'
a=root/('Docs/VisualEvidence/2026-10-08-Lighting/'+(sys.argv[1] if len(sys.argv)>1 else 'pass1-native')+'/paired')
fs=sorted([f for f in a.glob('*.png') if (b/f.name).exists() and not re.search(r'-turn(?:-90|90|180)\.png$',f.name)])
for k in range((len(fs)+3)//4):
 s=Image.new('RGB',(1280,1540),(16,16,16));d=ImageDraw.Draw(s)
 for i,f in enumerate(fs[k*4:(k+1)*4]):
  y=i*385
  for j,folder in enumerate([b,a]):
   im=Image.open(folder/f.name);im.thumbnail((638,359));s.paste(im,(j*640,y))
  d.text((5,y+362),f.stem,fill='white')
 s.save('/tmp/lighting-comparison-'+str(k)+'.jpg')
print(len(fs),'paired captures')
