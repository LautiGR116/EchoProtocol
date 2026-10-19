# R3 → Hub orientation correction

The creator reported that the demo build was playable but the return to the
original Hub did not make the continuation toward D1 clear. This is one human
observation; readability for a new player remains **EN PRUEBA**.

The [before](before-R3-exit.png) and [after](after-R3-exit.png) images are direct
1280 × 720 renders from the actual Player Camera after the same physical
P3 → R3 → Hub traversal. The Player finished at approximately
`(-3.56, 0.03, -9.48)`, facing east at yaw `90°` and FOV `60°`. No camera was
placed or rotated for either image; [camera-pose.txt](camera-pose.txt) records
both exact poses. The original Hub shell and fixtures remain visible. The only
added signal is a physical `REVISION` label on the east wall near the service
access. It belongs to the existing D1 presentation, which appears only after
the Player passes both real R3 arrival volumes. The arrival does not show D1's
pause or recording instructions; `[E] Fijar origen` remains at the station.

The guided Input System regression passed [58 checks](final.checks.txt): the
sign is hidden in a fresh scene and at P3 exit unlock, appears after the actual
Hub arrival, and the Player reaches and activates the station with E. It also
replays a genuine recorded D1 pause and reaches the demo closing screen.
[Result](final.result.txt) is `PASS`. The temporary
[probe source](Source/OrientationProbe.cs) was removed from `Assets/` before
building. As in the earlier D1 route test, the P1/SE1 opening was not replayed;
the prepared domestic checkpoint and approximated completed SE1 lighting are
test setup. These checks establish route and state behavior, not spontaneous
human detection, clarity, or emotional effect.

After removing the probe, Unity recompilation passed. A fresh Play Mode start
showed D1 and the sign inactive, no sign collider, and an empty Console. The
new macOS build is `Builds/Demo-20260928-02/`; its report records success,
zero errors and 346 package shader warnings, the same count as the preserved
`Demo-20260928-01` build. A headless launch exited with code 0 and no runtime
error in its log. Neither build has been used for a new human clarity test.
