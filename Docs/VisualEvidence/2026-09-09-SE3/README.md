# VS1-SE3 Omitted Passage — EN PRUEBA

Implemented and objectively verified in Unity 6000.3.21f1 / URP on September 9,
2026. Human approval, spatial-impossibility reading and emotional effect remain
pending. No new audio, figure, package, Echo rule or puzzle was added.

## Method

A temporary Editor-only input harness prepared the configured domestic checkpoint
at `(-2.75,0.03,22.05)` once and simulated the existing post-SE1 lighting state.
It disabled the checkpoint trigger after that seed to avoid a second recording
boundary. Subsequent movement, body yaw and R used virtual keyboard/mouse input
through the actual Input System, FirstPersonController, CharacterController,
recorder, Echo playback, P2 reader and gate. There were no subsequent harness
teleports or forced SE2 states. P1, SE1 and the portazo were not replayed end to
end. The harness was removed after testing.

The Player recorded a 30-second reader hold, pressed R and followed the real
Echo-held gate. The original recording remained the Echo's immutable snapshot.
Instrumented armed-recall, short-return and Echo-only runs checked every active
Echo Rigidbody sample against recorded position/yaw (`<0.01 m / <0.1°`).
CSV files retain state/step changes and approximately 10 Hz position samples.

## Results

| Check | Result / trace |
| --- | --- |
| Complete SE2, return, traverse both ways | Passed; one SE3 reveal, no floor fall or blocked traversal; `positive.csv` |
| Return before complete, then R | Passed at maximum `0.7148 s` SE2 reading; zero SE3 reveals, cold reset; `incomplete.csv` |
| Complete SE2, R before return | Stayed armed and clinical; outbound crossing on the next attempt did not reveal; subsequent return did; `armed_recall.csv` |
| Complete SE2 near the corner | Passed while stopped at `x=-0.860`, without approaching the glass; return, two-way traversal, disable/re-enable and completed R passed; `short_return.csv` |
| Echo traverses the return alone | Passed with genuine SE2 Complete: temporarily disable SE3, record the Player's return, R and re-enable; that truthful Echo crosses west while the Player stays at the checkpoint; zero reveals; `echo_only.csv` |
| Fresh no-Echo route | Closed gate stopped Player at `z=24.55`; SE3 clinical; `no_echo.csv` |
| Completed R | Revealed passage and SE2 persisted, checkpoint unchanged; gate still blocked without a live reader hold |
| Optional disable | Disabling the component restored baseline; re-enabling restored the same revealed state without a second reveal |
| Physical authority | Original colliders retained; `420/420` floor rays and `36/36` side-wall rays passed; SE3 has zero solid colliders |
| Dependencies | All five SE3 references assigned; existing SE1 48 and checkpoint 5 references assigned; original gameplay scripts unchanged by this feature |
| Compile / Console | Compilation passed; successful runs had zero gameplay errors. Unity AI Account API emitted its existing 30-second Editor timeout warning |
| Harness removed, normal startup | Fresh Play: SE3 `AwaitingSE2`, clinical on/overlay off, SE2 `AwaitingCrossing`, checkpoint revision `0`; Console zero errors. Stopped Play afterward; saved scene remains clinical |

The first positive, incomplete and armed-recall runs used a return plane at
`x=4.4`. Inspection identified that it could miss a Player who reads SE2 near
the corner. The final plane is `x=-1.55`; the short-return, Echo-only and fresh
no-Echo runs verify that saved version. The light-only refinement retains a
cool clinical ceiling reflection alongside two warm sources.

Four harness attempts are **not** counted as passes: three stopped at the initial
reader walk because synthetic movement/look did not reach the controller (runs
`172532`, `173056`, `173340` UTC); unchanged fresh-session retries progressed.
The exact input failure cause was not established and gameplay input code was
not changed. Run `173158` stopped before SE2 acceptance because its test target
was outside the actual observation volume. The successful short-return target
was placed inside that existing volume; SE2 was not modified to pass the test.

## Unedited Player-camera evidence

1280×720 URP renders, configured Player camera FOV `60°`, existing post-processing.
The comparison uses the same southward yaw (`180°`) and height (`1.63 m`). Before
position is `(-2.75,1.63,29.03)`; after is `(-2.64,1.63,29.03)` (about 11 cm apart).
The blue capsule at the far end is the existing, truthful P2 Echo placeholder,
not another SE3 figure.

![Clinical return before SE3](SE3_01_before.png)

![Domestic overlap after the return](SE3_02_after.png)

- [Oblique transition frame](SE3_03_oblique_transition.png): a near-corner return
  can catch the domestic fragments in the left part of the view before turning
  fully south. The whole overlap switches together; full occlusion is not
  guaranteed for arbitrary looks. Judge whether this feels like a spatial change
  or visible set activation during the human pass.
- [Reverse traversal](SE3_04_reverse.png): same physical clinical route remains.

## Human playtest

1. Open `Assets/_Game/Scenes/Development/VS1_ParallelReturn_Blockout.unity` and
   enter Play. Reach the domestic checkpoint through the existing P1/SE1 route.
2. Face the P2 wall lens until cyan, hold for about 30 seconds, then press R.
   Let the Echo open the gate; walk north, turn east and observe SE2's domestic
   chamber for at least four uninterrupted seconds after it appears.
3. Turn back naturally, follow the corridor west and then south. Judge whether
   the four frames/rug are obvious without searching, whether clinical and
   domestic architecture seem to occupy the same space, and whether the edge
   transition feels authored rather than like objects popping into view.
4. Walk the passage both ways while the Echo still holds the gate, then press R:
   the passage stays changed and the original gate keeps its normal rule. A
   short recording can end the hold; this feature does not latch P2 open.
5. Start a fresh Play session and press R before SE2's reading completes:
   the return must remain clinical. Also try reading SE2 near the turn instead
   of at the glass. Check free looking, side clearance, and the exit legibility.

Report detection, spatial interpretation and discomfort separately. Do not
infer fear from successful activation. Uncheck `studyEnabled` or disable
`VS1OmittedPassageEvent` on `SE3 Return Crossing (+Z toward return)` to restore
the clinical presentation. The study is optional and not final approved content.
