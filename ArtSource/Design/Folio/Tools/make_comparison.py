"""Present real native captures; resizing only the contact-sheet thumbnails."""
from pathlib import Path
import html,json
from PIL import Image,ImageDraw,ImageFont
root=Path(__file__).resolve().parents[4];out=root/'Docs/VisualEvidence/2026-10-08-Folio';ui=out/'native-ui-07'
pages=[('title','Inicio'),('pause','Pausa'),('letter','Carta'),('settings','Ajustes'),('hud','HUD en contexto')]
font=ImageFont.truetype(str(root/'Assets/_Game/Art/Folio/Fonts/IBMPlexSans-Medium.ttf'),24)
canvas=Image.new('RGB',(1600,5*505+90),'#172b2d');draw=ImageDraw.Draw(canvas);draw.text((25,20),'ECHO PROTOCOL / FOLIO — capturas nativas 1280×720',font=font,fill='#e8e2d1');sections=[]
for index,(key,label)in enumerate(pages):
 y=90+index*505;draw.text((25,y),label+' / anterior',font=font,fill='#ccd8d2');draw.text((825,y),label+' / Folio',font=font,fill='#ccd8d2')
 before=ui/f'before-{key}.png';after=ui/f'1280x720-{key}.png'
 for x,path in [(0,before),(800,after)]:
  im=Image.open(path);assert im.size==(1280,720);canvas.paste(im.resize((800,450),Image.Resampling.LANCZOS),(x,y+43))
 sections.append(f'<section><h2>{label}</h2><div class="pair"><figure><figcaption>Anterior · interfaz de Resonance</figcaption><a href="native-ui-07/{before.name}"><img src="native-ui-07/{before.name}" alt="{label}, anterior"></a></figure><figure><figcaption>Folio</figcaption><a href="native-ui-07/{after.name}"><img src="native-ui-07/{after.name}" alt="{label}, Folio"></a></figure></div></section>')
canvas.save(out/'comparison.png')
types=''.join(f'<figure><figcaption>{label}</figcaption><img src="{name}" alt="{label}"></figure>'for name,label in [('type-cormorant-title.png','Cormorant Garamond · seleccionada'),('type-barlow-title.png','Barlow Condensed · comparación'),('type-plex-title.png','IBM Plex Serif · comparación')])
extra=''.join(f'<a href="native-ui-07/{name}">{label}</a> · 'for name,label in [('1280x800-title.png','16:10'),('1440x810-settings.png','16:9'),('1280x720-stress-end.png','Documento largo / final'),('1280x720-notes.png','Ayuda'),('1280x720-credits.png','Créditos'),('1280x720-ending.png','Final / fixture'),('title-saved-checkpoint.png','Continuación guardada')])
(out/'index.html').write_text('''<!doctype html><html lang="es"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Echo Protocol — Folio</title><style>body{margin:0;background:#172b2d;color:#eee7d7;font:18px system-ui;line-height:1.5}main{max-width:1600px;margin:auto;padding:30px}h1{font-size:36px}h2{font-size:25px;margin-top:48px}a{color:#b2d5c6}.pair{display:grid;grid-template-columns:1fr 1fr;gap:18px}figure{margin:0}figcaption{padding:10px 0;color:#c4d1c9}img{width:100%;display:block}p{max-width:980px}details{margin:30px 0}@media(max-width:800px){.pair{grid-template-columns:1fr}}</style><main><h1>Echo Protocol / Folio</h1><p>Archivo preciso y huella personal. Comparación sobre ventanas reales de 1280×720; imágenes completas al hacer clic. Cartas, textos y estados provienen del juego. La captura anterior usa el renderer legado en un estado equivalente de la misma escena; las señales del mundo ya usan Plex. Las pantallas de documentos/HUD/final de esta prueba emplean fixtures explícitos y no se presentan como una partida continua.</p>'''+''.join(sections)+f'<h2>Otras superficies y resoluciones</h2><p>{extra}</p><details><summary>Comparación tipográfica inicial</summary><p>Game View en ejecución a 2940×1678, redimensionada a 1280×720. Juicio del agente; no aprobación humana.</p>{types}</details><p>130 comprobaciones de interfaz. <a href="native-ui-07/result.json">Resultado, medidas y fotogramas</a> · <a href="../../FOLIO_IDENTITY.md">Guía de identidad</a></p></main>')
print('Real native comparison: 5 pairs, 1600×2615 contact sheet, standalone HTML')
