"""Read Unicode cmap directly; verify packaged font glyphs without OS fallback."""
from pathlib import Path
import struct,json,re
root=Path(__file__).resolve().parents[4]
fonts=root/'Assets/_Game/Art/Folio/Fonts'
def cmap(path):
 b=path.read_bytes(); u16=lambda o:struct.unpack_from('>H',b,o)[0];u32=lambda o:struct.unpack_from('>I',b,o)[0]
 tables={b[12+i*16:16+i*16].decode():u32(20+i*16) for i in range(u16(4))};c=tables['cmap'];result=set()
 for i in range(u16(c+2)):
  rec=c+4+i*8;platform=u16(rec);encoding=u16(rec+2)
  if platform!=0 and not(platform==3 and encoding in(1,10)):continue
  s=c+u32(rec+4);form=u16(s)
  if form==12:
   for j in range(u32(s+12)):
    a,z,g=struct.unpack_from('>III',b,s+16+j*12);result.update(range(a+(g==0),z+1))
  elif form==4:
   n=u16(s+6)//2;end=s+14;start=end+2*n+2;delta=start+2*n;offset=delta+2*n
   for j in range(n):
    a,z=u16(start+j*2),u16(end+j*2);d=u16(delta+j*2);r=u16(offset+j*2)
    for ch in range(a,z+1):
     if ch==65535:continue
     glyph=(ch+d)&65535 if r==0 else u16(offset+j*2+r+2*(ch-a))
     if r and glyph:glyph=(glyph+d)&65535
     if glyph:result.add(ch)
 return result
# A conservative superset of functional and world-sign text, including every Unicode
# codepoint in runtime C# strings. Display/editorial samples reflect their actual roles.
strings=[]
for p in (root/'Assets/_Game/Scripts').rglob('*.cs'):
 strings.extend(re.findall(r'"((?:[^"\\]|\\.)*)"',p.read_text()))
functional=''.join(strings)
functional=functional.replace('\\n','').replace('\\r','').replace('\\t','')
display='ECHO PROTOCOL Original conservado. La sesión está detenida. Escucha y ajustes Créditos Notas de la sesión La llave de 6B La luz del recibidor 03:17 / Llegada La casa sin corriente Consentimiento / última sesión Una carta fuera del registro Archivo / prueba explícita de lectura'
italic='Una llave. Dos declaraciones. Tu registro continúa cuando vuelvas. La toma quedó entera. La llave quedó en portería. La dejé en portería. Mara El original permanece.'
# Personal documents use only alphabet, numbers and editorial punctuation below.
editorial=''.join(chr(i) for i in range(32,127))+'ÁÉÍÓÚÜÑáéíóúüñ¿¡«»·—–…'
roles={'IBMPlexSans-Regular.ttf':functional,'IBMPlexSans-Medium.ttf':functional,'CormorantGaramond-SemiBold.ttf':display,'CormorantGaramond-Italic.ttf':italic,'IBMPlexSerif-Regular.ttf':editorial,'IBMPlexSerif-Italic.ttf':editorial}
reports=[]
for name,text in roles.items():
 required={ord(c) for c in text if c.isprintable()};missing=required-cmap(fonts/name);reports.append({'font':name,'codepointsChecked':len(required),'missing':[{'codepoint':hex(c),'character':chr(c)}for c in sorted(missing)]})
report={'status':'PASS' if not any(r['missing']for r in reports)else'FAIL','method':'Direct Unicode cmap format 4/12, packaged static fonts; no system-font fallback. Functional roles use conservative runtime-string superset; display uses all actual headings; personal editorial uses Latin/Spanish punctuation superset.','roles':reports}
(root/'Docs/VisualEvidence/2026-10-08-Folio/glyph-audit.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n');print(json.dumps(report,ensure_ascii=False));assert report['status']=='PASS'
