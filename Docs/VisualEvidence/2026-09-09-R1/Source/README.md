# Reproducing the R1 checks

This archived temporary harness is outside Assets and does not ship with the game.
To repeat locally, copy `R1ValidationProbe.cs` into a temporary runtime script
folder under Assets, let Unity compile, open the VS1 scene and enter Play.
Call `R1ValidationProbe.Run("positive")` from Unity MCP RunCommand. Use a fresh
Play session for each of `positive`, `incomplete`, `armed`, and `no_echo`.
Its output directory is `/tmp/echo-vs1-r1/`; create it first. Check the final
`.result.txt` and `.checks.txt`, CSV, camera captures and Console. Delete old
result files before re-running to avoid mistaking an earlier pass for a new one.

The harness changes only Play-session test setup, including Editor input routing;
do not save project settings or the scene during testing. It seeds the domestic
checkpoint once, models post-SE1 lighting and uses actual input thereafter.
Stop Play and remove the temporary script and meta afterward. The normal saved
scene must start with hub inactive, baseline wall active, SE3 AwaitingSE2, and
checkpoint revision 0. A harness exception/timeout is not a gameplay pass.
