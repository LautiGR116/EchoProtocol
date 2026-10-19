"""Publish existing final Player captures and exact PCM excerpts; never normalize."""
from pathlib import Path
import datetime
import hashlib
import html
import json
import wave
from collections import Counter

ROOT = Path(__file__).resolve().parents[4]
OUT = ROOT / "Docs/VisualEvidence/2026-10-09-Coverage"

def sha(path):
    h = hashlib.sha256()
    with path.open("rb") as f:
        for block in iter(lambda: f.read(4 * 1024 * 1024), b""):
            h.update(block)
    return h.hexdigest()

clips = [
    ("regla-estereo.wav", "native-campaign-final", 252.0, 272.0,
     "Prueba de la bomba: marcha, descarga y otra marcha", "estéreo"),
    ("regreso-estereo.wav", "native-campaign-final", 311.0, 322.0,
     "Reja, apertura, embestida y regreso", "estéreo"),
    ("captura-mono-reducida.wav", "native-final-02", 68.0, 75.0,
     "Aproximación y captura durante el intento fallido", "mono · dinámica reducida"),
]
audio_dir = OUT / "listening"
audio_dir.mkdir(exist_ok=True)
manifest = []
for name, route, start, end, title, mode in clips:
    source = OUT / route / "listener.wav"
    with wave.open(str(source), "rb") as w:
        params = w.getparams()
        first = int(start * w.getframerate())
        count = int((end - start) * w.getframerate())
        assert first + count <= w.getnframes()
        w.setpos(first)
        pcm = w.readframes(count)
    target = audio_dir / name
    with wave.open(str(target), "wb") as w:
        w.setparams(params)
        w.writeframes(pcm)
    with wave.open(str(target), "rb") as w:
        assert w.getnframes() == count and w.readframes(count) == pcm
    manifest.append({"file": str(target.relative_to(OUT)), "source": str(source.relative_to(OUT)),
                     "sourceSha256": sha(source), "sha256": sha(target),
                     "startSeconds": start, "endSeconds": end, "mode": mode,
                     "method": "Exact consecutive captured PCM frames; no gain, normalization, fades or resampling."})
(audio_dir / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")

cards = [
    ("native-final-02/campaign-coverage-control-Game.png", "El mando y la presión",
     "Palanca y manómetro de la bomba real. La toma captura sostener y soltar el mando."),
    ("native-final-02/campaign-coverage-work-before-release-Game.png", "Trabajar lejos del Echo",
     "El ruido permite aflojar el cierre. La presión todavía sostiene la reja cerrada."),
    ("native-final-02/campaign-coverage-grille-open-Game.png", "Aprovechar la pausa",
     "La descarga baja la aguja y deja pasar. El silencio modifica el riesgo."),
    ("native-final-02/campaign-coverage-present-rush-Game.png", "La amenaza pertenece al presente",
     "Aproximación observada con movimiento real del jugador. El personaje existente sigue EN PRUEBA."),
    ("native-final-02/campaign-coverage-failure-Game.png", "Una consecuencia concreta",
     "La captura detiene la sesión. El menú permite volver a reproducir la misma toma."),
    ("native-final-02/campaign-coverage-return-covered-Game.png", "El regreso preparado",
     "Un quiebre físico corta la vista; el ruido de la bomba puede atraer al perseguidor."),
    ("native-final-02/campaign-coverage-natural-silence-Game.png", "La toma termina por sí misma",
     "Tras escapar, el mando sigue toda su grabación. Al terminar caen ruido y presión."),
    ("native-campaign-final/campaign-08-house-entry-Game.png", "Conexión con la campaña",
     "El recorrido completo continúa hacia la casa, 6B y el final, sin forzar estados."),
]
for path, *_ in cards:
    assert (OUT / path).is_file(), path

figures = "\n".join(
    f'<figure><a href="{html.escape(path)}"><img loading="lazy" src="{html.escape(path)}" alt="{html.escape(title)}"></a>'
    f'<figcaption><h3>{html.escape(title)}</h3><p>{html.escape(caption)}</p></figcaption></figure>'
    for path, title, caption in cards
)
players = "\n".join(
    f'<article><h3>{html.escape(title)}</h3><p>{html.escape(mode)} · {end-start:g} s · sin normalización</p>'
    f'<audio controls preload="none" src="listening/{html.escape(name)}"></audio></article>'
    for name, _, start, end, title, mode in clips
)
page = """<!doctype html><html lang="es"><meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>Echo Protocol · La máquina que te cubre</title>
<style>
:root{color-scheme:dark;--ink:#eee7d5;--muted:#b7c8c5;--line:#576b68}
*{box-sizing:border-box}body{margin:0;background:#243332;color:var(--ink);font:18px/1.6 system-ui,sans-serif}
main{max-width:1220px;margin:auto;padding:45px 28px 70px}header{border-bottom:1px solid var(--line);padding-bottom:30px}
.eyebrow{letter-spacing:.14em;color:var(--muted);font-size:13px}h1{font:clamp(38px,5vw,68px)/1.1 Georgia,serif;margin:18px 0}
h2{font:32px/1.2 Georgia,serif;margin-top:36px}h3{font-size:20px;margin:0 0 8px}p{max-width:880px;margin:12px 0}
a{color:#d3e6df;text-underline-offset:4px}nav{display:flex;gap:22px;flex-wrap:wrap;margin-top:22px}
.status{padding:18px 22px;border:1px solid var(--line);margin:28px 0}.status strong{color:#dbe6cd}
.grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:24px}figure{margin:0;background:#192827;border:1px solid var(--line)}
img{width:100%;display:block;height:auto}figcaption{padding:18px 20px}figcaption p,article p{font-size:16px;color:var(--muted)}
.sound{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:20px}article{border-top:1px solid var(--line);padding-top:18px}
audio{width:100%;margin:10px 0}details{border-top:1px solid var(--line);padding:20px 0;margin-top:26px}summary{cursor:pointer;font-size:21px}
footer{margin-top:35px;font-size:15px;color:var(--muted)}@media(max-width:800px){.grid,.sound{grid-template-columns:1fr}main{padding:25px 18px}}
</style><main><header><div class="eyebrow">ECHO / EVIDENCIA DEL JUEGO · 09 OCT 2026</div>
<h1>La máquina que te cubre</h1>
<p>Un plan que te permite trabajar lejos de tu Echo y sobrevivir al regreso. Abrí la nueva app y elegí
<strong>Jugar · La máquina que te cubre</strong>.</p>
<nav><a href="../../COVERAGE_CANDIDATE.md">Informe y límites</a><a href="#capturas">Capturas reales</a>
<a href="#escucha">Escuchar el encuentro</a></nav></header>
<div class="status"><strong>Comprobado: tramo, captura, reintento y campaña completa.</strong>
<p>La prueba final del tramo pasó 43 comprobaciones; la campaña pasó 541. Las grabaciones permanecieron inmutables.
Claridad, credibilidad, miedo y comodidad siguen <strong>EN PRUEBA</strong> hasta una partida humana.</p></div>
<h2 id="capturas">El encuentro en el Player final</h2>
<p>Estas imágenes muestran la solución y la amenaza. Conviene jugar antes de revisarlas si querés probar el descubrimiento.</p>
<div class="grid">FIGURES</div>
<h2 id="escucha">Sonido capturado durante la partida</h2>
<p>Extractos consecutivos de la salida real del juego, con el volumen de esa prueba. Escuchá a volumen cómodo.
La máquina, los mandos, las rejas y el cuerpo presente producen sus propios sonidos.</p><div class="sound">PLAYERS</div>
<details><summary>Alcance de las comprobaciones</summary>
<p>Los recorridos usan entradas normales del Input System en una app nativa de verificación, sin colocaciones de prueba
ni reanudación oculta. Guardado/carga usa estados explícitos separados. La app entregada tiene el mismo juego y excluye
los componentes de verificación. Los controles de su menú, movimiento, R, ayuda, pausa y salida se comprobaron con teclado y ratón reales.</p>
<p>Los extractos conservan el PCM original, sin cambios de ganancia, fades ni normalización. La captura no prueba el
volumen del dispositivo, la actuación percibida o el miedo. El personaje reutiliza el adulto existente.</p>
<p><a href="native-final-02/campaign-full.result.json">Resultado del tramo</a> ·
<a href="native-campaign-final/campaign-full.result.json">Resultado de campaña</a> ·
<a href="checkpoint-final.json">Guardado/carga</a> · <a href="delivery-assembly.json">Contenido de la build</a> ·
<a href="package.json">Paquete verificado</a> · <a href="listening/manifest.json">Origen exacto de los extractos</a></p></details>
<footer>Folio y las ediciones anteriores permanecen recuperables. Los intentos fallidos se conservan como evidencia de sus propios fallos.</footer>
</main></html>""".replace("FIGURES", figures).replace("PLAYERS", players)
(OUT / "index.html").write_text(page)
print("Published", len(cards), "unaltered screenshots and", len(clips), "exact PCM excerpts")
