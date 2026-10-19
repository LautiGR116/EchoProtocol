"""Excerpt the final, unsoloed in-game mix; never normalize its gain."""
from pathlib import Path
import hashlib
import html
import json
import wave
import numpy as np

ROOT = Path(__file__).resolve().parents[4]
EVIDENCE = ROOT / 'Docs/AudioEvidence/2026-10-08-Resonance'
SOURCE = EVIDENCE / 'final-native'
DEST = EVIDENCE / 'listening'

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

trace = json.loads((SOURCE / 'audio-route.json').read_text())
assert trace['status'] == 'PASS' and trace['dropped'] == 0
assert trace['writerJoined'] and not trace['error']
with wave.open(str(SOURCE / 'listener.wav')) as w:
    rate, channels = w.getframerate(), w.getnchannels()
    assert w.getsampwidth() == 2 and channels == 2
    pcm = np.frombuffer(w.readframes(w.getnframes()), '<i2').reshape(-1, channels)
events = {}
for state in trace['states']:
    n = state['cueCount']
    if n and n not in events:
        events[n] = state
assert set(events) == set(range(1, 9)), list(events)

spec = [
    ('01-se2', 'SE2 · reconocimiento', 1, 2, 16,
     'Cruce real del salón. Fricción del marco y motivo interno después del cambio; el Echo sigue sin voz.'),
    ('02-transferencia', 'P3 · transferencia', 2, 2, 13,
     'Después de confirmar B, el metal responde al otro lado del tabique. La ventilación pierde fuerza.'),
    ('03-archivo', 'Archivo · regreso', 3, 1.5, 13,
     'Una fricción se acerca detrás de la pared mientras el jugador vuelve por el archivo.'),
    ('04-aislamiento', 'Planta · después del drenaje', 4, 2, 17,
     'La máquina se apaga, el caño contrae y llega una única voz desde el espacio de servicio.'),
    ('05-telefono', 'Casa · la línea', 6, 2, 18,
     'La toma se escucha desde el auricular físico. En el juego se puede interrumpir colgando; no se repite al levantarlo.'),
    ('06-devolucion', '6B · reconocer y devolver', 7, 1.5, 24,
     'El motivo reaparece después de RECALL; la devolución de la llave abre su resolución y el fundido de salida.'),
]
DEST.mkdir(exist_ok=True)
rows, reel = [], []
for slug, title, cue, before, duration, description in spec:
    start = max(0, int((events[cue]['seconds'] - before) * rate))
    stop = min(len(pcm), start + int(duration * rate))
    if cue == 7:
        stop = min(len(pcm), int((events[8]['seconds'] + 10) * rate))
    excerpt = pcm[start:stop]
    path = DEST / (slug + '.wav')
    with wave.open(str(path), 'wb') as w:
        w.setnchannels(channels); w.setsampwidth(2); w.setframerate(rate)
        w.writeframes(excerpt.tobytes())
    # Individual excerpts are byte-identical slices. Only the montage has
    # 10 ms edge fades and a one-second silent separator at each edit.
    cut = excerpt.astype(np.float64)
    fade = min(int(.01 * rate), len(cut) // 2)
    cut[:fade] *= np.linspace(0, 1, fade)[:, None]
    cut[-fade:] *= np.linspace(1, 0, fade)[:, None]
    if reel: reel.append(np.zeros((rate, channels), dtype='<i2'))
    reel.append(np.rint(cut).astype('<i2'))
    rows.append(dict(file=path.name, title=title, description=description,
                     sourceStartFrame=start, sourceEndFrame=stop,
                     sourceSeconds=start/rate, seconds=(stop-start)/rate,
                     sha256=sha(path), peak=float(np.abs(excerpt.astype(float)).max()/32768)))
montage = np.concatenate(reel)
with wave.open(str(DEST/'resonance-in-game-reel.wav'), 'wb') as w:
    w.setnchannels(channels); w.setsampwidth(2); w.setframerate(rate)
    w.writeframes(montage.tobytes())
manifest = dict(source='../final-native/listener.wav', sourceSha256=sha(SOURCE/'listener.wav'),
                sampleRate=rate, channels=channels,
                method='Individual excerpts: byte-identical contiguous PCM slices, no gain or EQ processing. Reel: cuts, 10ms boundary fades, 1s silence between clips. No normalization, source soloing, or replacement audio.',
                scope=trace['scope'], clips=rows, reelSeconds=len(montage)/rate,
                reelSha256=sha(DEST/'resonance-in-game-reel.wav'))
(DEST/'samples.json').write_text(json.dumps(manifest, indent=2, ensure_ascii=False)+'\n')
cards=''.join(f'<article><h2>{html.escape(r["title"])}</h2><p>{html.escape(r["description"])}</p><audio controls preload="none" src="{r["file"]}"></audio><small>Captura original: {r["sourceSeconds"]:.2f}–{r["sourceSeconds"]+r["seconds"]:.2f} s · mezcla sin normalizar</small></article>' for r in rows)
page='''<!doctype html><html lang="es"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Echo Protocol · Resonance · escucha</title>
<style>:root{color-scheme:dark}body{margin:0 auto;max-width:900px;padding:52px 24px 80px;background:#10191e;color:#dae4df;font:17px/1.55 system-ui}header{border-bottom:1px solid #35464c;padding-bottom:28px}small,.eyebrow{color:#88b2ac}h1{font-size:46px;line-height:1.05;margin:16px 0}h2{font-size:22px}article{padding:18px 0 24px;border-bottom:1px solid #35464c}audio{display:block;width:100%;margin:16px 0}a{color:#93d5c9}p{max-width:750px}.notice{border-left:3px solid #9caa88;padding-left:18px;margin:26px 0}</style>
<header><div class="eyebrow">ECHO PROTOCOL / RESONANCE / 08.10.2026</div><h1>El edificio también responde.</h1><p>Seis momentos de la mezcla final, capturada desde el oyente real de la campaña macOS. Los recortes individuales conservan exactamente la señal del juego.</p></header>
<p class="notice">Auriculares a volumen cómodo o parlantes. No hace falta subir el volumen. Esta selección revela encuentros y el cierre; una primera partida sin anticiparlos sigue siendo la prueba importante.</p>
<h2>Recorrido sonoro</h2><p>El montaje une los seis recortes con un segundo de silencio y pequeños fundidos en los cortes. No representa una partida sin interrupciones.</p><audio controls preload="none" src="resonance-in-game-reel.wav"></audio>
'''+cards+'''
<p>La señal, las direcciones estéreo, la oclusión, la pausa y las opciones de mezcla tienen comprobaciones técnicas. La interpretación vocal, la localización percibida, la comodidad y el miedo necesitan escucha humana. No se ha declarado una aprobación por oído.</p>
<p><a href="../../../RESONANCE_CANDIDATE.md">Informe, ruta de juego y límites</a> · <a href="samples.json">Tiempos y trazabilidad de las muestras</a></p></html>'''
(DEST/'index.html').write_text(page)
print(json.dumps(dict(clips=len(rows), reelSeconds=manifest['reelSeconds'], output=str(DEST)), indent=2))
