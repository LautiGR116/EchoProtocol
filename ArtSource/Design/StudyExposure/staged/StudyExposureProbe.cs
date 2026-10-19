#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using EchoProtocol.Campaign;
using EchoProtocol.Horror;
using EchoProtocol.Interaction;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Selection evidence, not a gaze gate or a fear test. One initial fixture per
// fresh Play; all later movement, camera turns, reserve/key and door actions use input.
[DefaultExecutionOrder(500)]
public sealed class StudyExposureProbe : MonoBehaviour
{
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    readonly List<string> checks = new List<string>();
    readonly List<FrameCopy> frames = new List<FrameCopy>();
    readonly float[] cameraZ = { -54.5f, -55.5f, -56.5f, -57.5f };
    readonly Plane[] planes = new Plane[6];
    FirstPersonController player; Camera view; PlayerInput input; PlayerInteractor interactor;
    GameSession session; CampaignDirector campaign; HouseMemory house; HouseSwingDoor door;
    GameObject body; BoxCollider safety; AudioSource weight; Collider[] transparent;
    MeshRenderer[] bodyMeshes;
    Keyboard keyboard; Mouse mouse; InputDevice[] previousDevices; string previousScheme;
    InputSettings.EditorInputBehaviorInPlayMode oldBehavior;
    bool answered, configured, running, finished, fixtureStarted, previousAutoSwitch, oldSuppress, measureReturn;
    Key[] keys = Array.Empty<Key>();
    float yaw = 180, pitch, sensitivity, started, returnAt = -1, activatedAt = -1, cueAt = -1, firstVisibleAt = -1;
    float centreSeconds, headSeconds, eitherSeconds, currentRun, longestRun, maximumReturnDelta;
    int resets, nextCamera, pendingFrames, fixtureCount; string output, step = "fixture", runtimeError;
    Vector3 weightAnchor; StreamWriter csv;
    public string Status { get; private set; } = "starting";
    sealed class FrameCopy { public string label, metadata; public RenderTexture texture; }

    public static void RunAnswered() { Run(true); }
    public static void RunUnanswered() { Run(false); }
    static void Run(bool answered)
    {
        if (!Application.isPlaying || FindFirstObjectByType<StudyExposureProbe>()) throw new Exception("Use fresh Play in Study for each branch");
        var p = new GameObject("Temporary natural study return").AddComponent<StudyExposureProbe>(); p.answered = answered;
        p.StartCoroutine(p.Guarded(p.Execute()));
    }
    static T Field<T>(object o, string n) => (T)(o.GetType().GetField(n, Private) ?? throw new MissingFieldException(o.GetType().Name, n)).GetValue(o);
    void Check(bool ok, string message) { if (!ok) throw new Exception(message); checks.Add("PASS " + message); }
    static string F(float n) => n.ToString("F4", Inv);
    IEnumerator Guarded(IEnumerator r)
    {
        var stack = new Stack<IEnumerator>(); stack.Push(r);
        while (stack.Count > 0)
        {
            object next;
            try { if (!stack.Peek().MoveNext()) { stack.Pop(); continue; } next = stack.Peek().Current; }
            catch (Exception e) { Finish(false, e.ToString()); yield break; }
            if (next is IEnumerator child) stack.Push(child); else yield return next;
        }
    }

    IEnumerator Execute()
    {
        started = Time.realtimeSinceStartup;
        output = "Docs/VisualEvidence/2026-10-01-Study/natural-return/" + (answered ? "answered" : "unanswered");
        Directory.CreateDirectory(output); File.Delete(output + "/result.txt");
        foreach (var path in Directory.GetFiles(output, "study-*.png")) File.Delete(path);
        File.WriteAllText(output + "/cameras.txt", "Actual Game view copied into retained RenderTextures at frame end while W remains held. No extra Camera.Render, PNG encoding or ReadPixels during return. UI may appear. CPU readback/encoding occurs after the exit. GPU copy overhead remains possible; deltaTime is logged.\n");
        Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path.EndsWith("EchoProtocol_Study.unity"), "fresh Study scene selected");
        player = FindFirstObjectByType<FirstPersonController>(); view = player.GetComponentInChildren<Camera>(); input = player.GetComponent<PlayerInput>(); interactor = player.GetComponent<PlayerInteractor>();
        session = FindFirstObjectByType<GameSession>(); campaign = FindFirstObjectByType<CampaignDirector>(); house = FindFirstObjectByType<HouseMemory>();
        body = Field<GameObject>(house, "presenceRoot"); safety = Field<BoxCollider>(house, "presenceVisibilityVolume");
        weight = Field<AudioSource>(house, "presenceAudio"); transparent = Field<Collider[]>(house, "transparentSightColliders");
        door = Field<HouseSwingDoor>(house, "exitDoor");
        Check(!house.Active && !house.HasKey && !house.HasAnswered && !body.activeSelf, "house and presence start without fabricated progress");
        bodyMeshes = body.GetComponentsInChildren<MeshRenderer>(true).Where(r => r.enabled && ActiveWithinBody(r.transform) && !r.forceRenderingOff).ToArray();
        Check(bodyMeshes.Length > 0, "enabled original adult meshes exist separately from disabled legacy renderers");
        AuditSafety(); weightAnchor = weight.transform.position;
        Check(Mathf.Abs(weightAnchor.x - body.transform.position.x) < .01f && Mathf.Abs(weightAnchor.z - body.transform.position.z) < .01f,
            "existing floorboard source follows the physical figure horizontally");

        oldSuppress = campaign.SuppressSaves; campaign.SuppressSaves = true; fixtureStarted = true;
        // SOLE INITIAL FIXTURE: completed prior chapter/lighting and entry pose.
        // No house progress, light reserve, presence, gaze, cue or audio state is set.
        session.BeginOrResume(); campaign.PrepareHouseProbe();
        var se1 = FindFirstObjectByType<VS1StrongEventOne>();
        Check(se1.RestoreCompletedLighting() && se1.FluorescentPowerFraction == 0, "initial fixture restores completed first-act lighting");
        player.ResetPose(new Vector3(0, .03f, -49.5f), Quaternion.Euler(0, 180, 0)); fixtureCount++;
        Configure(); player.PoseReset += ObserveReset; Application.logMessageReceived += ReceiveLog;
        running = true; Status = "running";
        csv = new StreamWriter(output + "/exposure.csv");
        csv.WriteLine("frame,time,delta,step,return,eye_x,eye_y,eye_z,yaw,pitch,active,triggered,cue_committed,cue_playing,safety_frustum,centre_in_frame,centre_clear,head_in_frame,head_clear,renderable,bounds_height_px,house_seen,retired");
        yield return Wait(.2f);
        yield return UseTarget(FindObjectsByType<CampaignStation>(FindObjectsSortMode.None).Single(s => s.Kind == StationKind.HouseOrigin));
        Check(house.Active && campaign.Chapter == CampaignChapter.House, "actual E commits house entry");

        if (answered)
        {
            step = "answer_and_move_reserve";
            yield return Go(0, -52.3f, 180); yield return Go(-2, -52.3f, 270); yield return Go(-3.45f, -51.9f, 270);
            yield return Use(HouseInteractionKind.Telephone); yield return Wait(.8f);
            Check(house.HasAnswered, "actual E answers the optional telephone");
            yield return Go(-2, -52.3f, 90); yield return Go(0, -52.3f, 90); yield return Go(0, -50.8f, 0);
            yield return Use(HouseInteractionKind.HallSocket);
            Check(house.PowerLocation == HousePowerLocation.Carried, "actual E removes the sole hall reserve");
        }
        step = "bedroom_and_key";
        yield return Go(0, -53.3f, 180); yield return Go(1.8f, -53.3f, 90); yield return Go(1.8f, -50.8f, 0);
        if (answered)
        {
            yield return Go(4.6f, -51.25f, 90); yield return Go(4.6f, -53, 180); yield return Use(HouseInteractionKind.BedroomSocket);
            Check(house.PowerLocation == HousePowerLocation.Bedroom, "actual E powers the bedroom for the key approach");
            yield return Go(4.6f, -51.25f, 0);
        }
        yield return Go(3.2f, -50.8f, 270); yield return Use(HouseInteractionKind.Key);
        Check(house.HasKey && house.HasAnswered == answered, "real key interaction preserves the selected telephone branch");
        if (answered)
        {
            yield return Go(4.6f, -51.25f, 90); yield return Go(4.6f, -53, 180); yield return Use(HouseInteractionKind.BedroomSocket);
            Check(house.PowerLocation == HousePowerLocation.Carried, "actual E removes bedroom power for the dark carried return");
            yield return Go(4.6f, -51.25f, 0);
        }
        yield return Go(1.8f, -50.8f, 270); yield return Go(1.8f, -53.3f, 180); yield return Go(0, -53.3f, 270);
        step = "natural_forward_return"; yield return Face(180);
        returnAt = Time.time; measureReturn = true;
        // No Aim/body-derived steering, stops or presentation delays in this segment.
        yield return Walk(0, -61.1f); measureReturn = false;
        Check(nextCamera == cameraZ.Length, "physical march crosses all four requested camera depths");
        Check(fixtureCount == 1 && resets == 0 && player.transform.position.z < -61f, "forward return uses one initial fixture and reaches the door without any later pose reset");

        step = "free_exit"; yield return Use(HouseInteractionKind.ExitDoor);
        Check(house.HasExited && door.IsOpenRequested, "key alone opens the real exit irrespective of measured presence visibility");
        yield return Until(() => door.IsFullyOpen, 8f, "Physical exit did not open");
        yield return Go(0, -63.6f, 180);
        Check(player.transform.position.z < -63.3f && house.HasAnswered == answered
            && house.PowerLocation == (answered ? HousePowerLocation.Carried : HousePowerLocation.Hall),
            "actual departure preserves answered/carried or unanswered/hall choice");
        // Retirement remains an observation, not an exit prerequisite. This route
        // is allowed to finish even when the figure was never geometrically seen.
        yield return Until(() => pendingFrames == 0, 3f, "Game-view frame capture did not complete; keep Game view visible");
        SaveFrames(); Finish(true, "");
    }

    bool ActiveWithinBody(Transform t)
    {
        while (t && t != body.transform) { if (!t.gameObject.activeSelf) return false; t = t.parent; }
        return t == body.transform;
    }
    static IEnumerable<Vector3> Corners(Bounds bounds)
    {
        for (int x = -1; x <= 1; x += 2) for (int y = -1; y <= 1; y += 2) for (int z = -1; z <= 1; z += 2)
            yield return bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z));
    }
    bool SafetyContains(Vector3 world) => new Bounds(safety.center, safety.size + Vector3.one * .002f).Contains(safety.transform.InverseTransformPoint(world));
    void AuditSafety()
    {
        var lines = new List<string>();
        foreach (var mesh in bodyMeshes)
        {
            // Transform local renderer bounds explicitly: the body is correctly
            // inactive here, so no rendered-frame bounds cache is required.
            var points = Corners(mesh.localBounds).Select(mesh.transform.TransformPoint).ToArray();
            bool contained = points.All(SafetyContains);
            lines.Add(mesh.name + " localBounds=" + mesh.localBounds + " containsAllCorners=" + contained + " shadows=" + mesh.shadowCastingMode);
            Check(contained, "safety contains every transformed bounds corner of " + mesh.name);
        }
        foreach (var light in body.GetComponentsInChildren<Light>(true).Where(l => l.enabled && l.intensity > 0 && ActiveWithinBody(l.transform)))
        {
            // Conservative influence envelope. A point/rim that illuminates walls
            // outside safety must become permanent or get a correctly sized safety
            // envelope; do not certify only the emitter's small centre point.
            bool contained = light.type != LightType.Directional
                && Corners(new Bounds(light.transform.position, Vector3.one * light.range * 2f)).All(SafetyContains);
            lines.Add("GATED LIGHT " + light.name + " type=" + light.type + " range=" + light.range + " conservativeInfluenceContained=" + contained);
            Check(contained, "safety conservatively contains the influence of body-child light " + light.name);
        }
        var permanent = FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l => l.name == "Study window residual");
        foreach (var light in permanent)
        {
            Check(!light.transform.IsChildOf(body.transform), "permanent study residual is not switched with the figure");
            lines.Add("PERMANENT SIBLING " + light.name + " position=" + light.transform.position + " range=" + light.range + "; not attributed to safety");
        }
        lines.Add("Containment covers switched renderer geometry and conservative child-light influence. It does not prove containment of all cast shadows or reflections from permanent illumination.");
        File.WriteAllLines(output + "/safety.txt", lines);
    }
    bool InFrame(Vector3 target)
    {
        var p = view.WorldToViewportPoint(target);
        return p.z > view.nearClipPlane && p.z < view.farClipPlane && p.x >= 0 && p.x <= 1 && p.y >= 0 && p.y <= 1;
    }
    bool ClearRay(Vector3 target)
    {
        Vector3 d = target - view.transform.position; float distance = d.magnitude;
        if (distance < .01f) return true;
        foreach (var hit in Physics.RaycastAll(view.transform.position, d / distance, distance - .01f, ~0, QueryTriggerInteraction.Ignore))
        {
            var c = hit.collider;
            if (c.transform.IsChildOf(player.transform) || c.transform.IsChildOf(body.transform) || c == safety || transparent.Contains(c)) continue;
            return false;
        }
        return true;
    }
    void ObserveReset() { resets++; }
    void ReceiveLog(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) runtimeError = message + "\n" + stack; }
    void LateUpdate()
    {
        if (!running) return;
        try
        {
            if (session.IsPaused || !string.IsNullOrEmpty(runtimeError) || player.transform.position.y < -.2f || resets != 0)
                throw new Exception("Unexpected pause, runtime error or physical continuity failure. No hidden recovery. " + runtimeError);
            if (Time.realtimeSinceStartup - started > 120f) throw new Exception("Study probe exceeded 120 seconds");
            if (Vector3.Distance(weightAnchor, weight.transform.position) > .0001f) throw new Exception("Study floorboard source moved during play");
            bool triggered = Field<bool>(house, "presenceTriggered"), cue = Field<bool>(house, "presenceArrivalPlayed");
            if (triggered && activatedAt < 0) activatedAt = Time.time;
            if (cue && cueAt < 0) cueAt = Time.time;
            Vector3 centre = safety.transform.TransformPoint(safety.center), head = body.transform.position + Vector3.up * 1.64f;
            bool centreFrame = InFrame(centre), headFrame = InFrame(head), centreClear = ClearRay(centre), headClear = ClearRay(head);
            bool renderable = body.activeInHierarchy && bodyMeshes.Any(r => r.enabled && r.gameObject.activeInHierarchy && !r.forceRenderingOff && (view.cullingMask & (1 << r.gameObject.layer)) != 0);
            GeometryUtility.CalculateFrustumPlanes(view, planes); bool boundsFrame = GeometryUtility.TestPlanesAABB(planes, safety.bounds);
            float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
            foreach (var mesh in bodyMeshes) foreach (var corner in Corners(mesh.localBounds))
            { var p = view.WorldToViewportPoint(mesh.transform.TransformPoint(corner)); if (p.z > view.nearClipPlane) { minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y); } }
            float projectedHeight = float.IsInfinity(minY) ? 0 : Mathf.Max(0, Mathf.Min(1, maxY) - Mathf.Max(0, minY)) * view.pixelHeight;
            if (measureReturn)
            {
                if (Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y, 180)) > 1f || Mathf.Abs(Field<float>(player, "pitch")) > 1f)
                    throw new Exception("Measured march deviated from natural yaw180/pitch0");
                bool cv = renderable && centreFrame && centreClear, hv = renderable && headFrame && headClear;
                if (cv) centreSeconds += Time.deltaTime; if (hv) headSeconds += Time.deltaTime;
                if (cv || hv) { eitherSeconds += Time.deltaTime; currentRun += Time.deltaTime; longestRun = Mathf.Max(longestRun, currentRun); if (firstVisibleAt < 0) firstVisibleAt = Time.time; }
                else currentRun = 0;
                maximumReturnDelta = Mathf.Max(maximumReturnDelta, Time.deltaTime);
                while (nextCamera < cameraZ.Length && player.transform.position.z <= cameraZ[nextCamera])
                { string label = "z" + (-cameraZ[nextCamera]).ToString("F1", Inv); nextCamera++; pendingFrames++; StartCoroutine(CopyFrame(label)); }
            }
            var eye = view.transform.position;
            csv.WriteLine(string.Join(",", Time.frameCount, F(Time.time), F(Time.deltaTime), step, measureReturn, F(eye.x), F(eye.y), F(eye.z),
                F(player.transform.eulerAngles.y), F(Field<float>(player, "pitch")), body.activeInHierarchy, triggered, cue, weight.isPlaying,
                boundsFrame, centreFrame, centreClear, headFrame, headClear, renderable, F(projectedHeight), house.PresenceSeen, house.PresenceRetired));
        }
        catch (Exception e) { Finish(false, e.ToString()); StopAllCoroutines(); }
    }
    IEnumerator CopyFrame(string label)
    {
        yield return new WaitForEndOfFrame();
        if (!running) { pendingFrames--; yield break; }
        var rt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        rt.Create();
        ScreenCapture.CaptureScreenshotIntoRenderTexture(rt);
        frames.Add(new FrameCopy { label = label, texture = rt, metadata = label + " actualFrame=" + Time.frameCount + " time=" + F(Time.time)
            + " eye=" + view.transform.position.ToString("F3") + " euler=" + view.transform.eulerAngles.ToString("F2") + " delta=" + F(Time.deltaTime) });
        pendingFrames--;
    }
    void SaveFrames()
    {
        foreach (var frame in frames)
        {
            var before = RenderTexture.active; var t = new Texture2D(frame.texture.width, frame.texture.height, TextureFormat.RGB24, false);
            try { RenderTexture.active = frame.texture; t.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0); t.Apply(); File.WriteAllBytes(output + "/study-" + frame.label + ".png", t.EncodeToPNG()); File.AppendAllText(output + "/cameras.txt", frame.metadata + "\n"); }
            finally { RenderTexture.active = before; Destroy(t); }
        }
    }

    IEnumerator Go(float x, float z, float facing) { yield return Face(facing); yield return Walk(x, z); yield return Wait(.10f); }
    IEnumerator Walk(float x, float z)
    {
        float deadline = Time.realtimeSinceStartup + 15f;
        while (Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(x, z)) > .1f)
        {
            if (Time.realtimeSinceStartup > deadline) throw new Exception("Physical route blocked at " + player.transform.position + " toward " + x + "," + z);
            Vector3 local = player.transform.InverseTransformDirection(new Vector3(x, player.transform.position.y, z) - player.transform.position);
            var held = new List<Key>(); if (local.z > .06f) held.Add(Key.W); else if (local.z < -.06f) held.Add(Key.S); if (local.x > .06f) held.Add(Key.D); else if (local.x < -.06f) held.Add(Key.A);
            keys = held.ToArray(); yield return null;
        }
        keys = Array.Empty<Key>();
    }
    IEnumerator Face(float targetYaw, float targetPitch = 0)
    {
        yaw = targetYaw; pitch = Mathf.Clamp(targetPitch, -84, 84); keys = Array.Empty<Key>(); float deadline = Time.realtimeSinceStartup + 5f;
        while (Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y, yaw)) > .6f || Mathf.Abs(Field<float>(player, "pitch") - pitch) > .6f)
        { if (Time.realtimeSinceStartup > deadline) throw new Exception("Mouse aim failed; no cursor override or auto-resume"); yield return null; }
        yield return Wait(.08f);
    }
    IEnumerator Aim(Vector3 target) { Vector3 d = target - view.transform.position; yield return Face(Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg, -Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg); }
    IEnumerator Use(HouseInteractionKind kind) { yield return UseTarget(FindObjectsByType<HouseInteraction>(FindObjectsSortMode.None).Single(i => i.Kind == kind)); }
    IEnumerator UseTarget(Component component)
    {
        var c = component.GetComponentsInChildren<Collider>().Where(v => v.enabled && v.gameObject.activeInHierarchy && !v.isTrigger).OrderBy(v => Vector3.Distance(view.transform.position, v.bounds.center)).FirstOrDefault();
        yield return Aim(c ? c.bounds.center : component.transform.position);
        var expected = component as IInteractable;
        Check(expected != null && interactor.TryGetTarget(out var target) && ReferenceEquals(target, expected) && expected.CanInteract(player.gameObject), "actual ray authorizes " + component.name);
        keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(.06f); keys = new[] { Key.E }; yield return new WaitForSecondsRealtime(.10f); yield return Wait(.12f);
    }
    IEnumerator Wait(float seconds) { keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(seconds); }
    IEnumerator Until(Func<bool> condition, float seconds, string error) { keys = Array.Empty<Key>(); float end = Time.realtimeSinceStartup + seconds; while (!condition()) { if (Time.realtimeSinceStartup > end) throw new Exception(error); yield return null; } }
    void Configure()
    {
        previousDevices = input.devices.ToArray(); previousScheme = input.currentControlScheme; previousAutoSwitch = input.neverAutoSwitchControlSchemes; oldBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
        sensitivity = Field<float>(player, "lookSensitivity"); keyboard = InputSystem.AddDevice<Keyboard>("StudyRouteKeyboard"); mouse = InputSystem.AddDevice<Mouse>("StudyRouteMouse"); configured = true;
        input.neverAutoSwitchControlSchemes = true; input.SwitchCurrentControlScheme("Keyboard&Mouse", keyboard, mouse); InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.onBeforeUpdate += InputUpdate;
    }
    void InputUpdate()
    {
        if (!running || InputState.currentUpdateType != InputUpdateType.Dynamic) return;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        InputSystem.QueueStateEvent(mouse, new MouseState { delta = session.IsPaused ? Vector2.zero : new Vector2(Mathf.Clamp(Mathf.DeltaAngle(player.transform.eulerAngles.y, yaw), -12, 12) / sensitivity, -Mathf.Clamp(Mathf.DeltaAngle(Field<float>(player, "pitch"), pitch), -10, 10) / sensitivity) });
    }
    void Finish(bool pass, string error)
    {
        if (finished) return; finished = true; Status = pass ? "PASS" : "FAIL " + error;
        bool seen = house && house.PresenceSeen, retired = house && house.PresenceRetired;
        Cleanup();
        File.WriteAllText(output + "/checks.txt", "One initial prior-chapter/lighting/pose fixture. Input-only house interactions and route; measured return fixes yaw180/pitch0 via mouse, never Aim at figure. PASS covers geometry/controls/safety contract, not visibility duration, human discovery, listening or fear.\n" + string.Join("\n", checks) + "\n" + Status);
        File.WriteAllText(output + "/result.txt", Status + "\nbranch=" + (answered ? "answered-carried-dark" : "unanswered-hall-general")
            + " checks=" + checks.Count + " fixtures=" + fixtureCount + " laterPoseResets=" + resets + " capturedFrames=" + frames.Count
            + "\ncentreSeconds=" + F(centreSeconds) + " headSeconds=" + F(headSeconds) + " eitherSeconds=" + F(eitherSeconds) + " longestContinuousSeconds=" + F(longestRun)
            + "\nreturnAt=" + F(returnAt) + " activationAt=" + F(activatedAt) + " cueCommittedAt=" + F(cueAt) + " firstGeometricExposureAt=" + F(firstVisibleAt)
            + "\nmaxReturnDelta=" + F(maximumReturnDelta) + " PresenceSeen=" + seen + " PresenceRetired=" + retired + "\nAll exposure durations use actual Time.deltaTime; centre/head rays and projected bounds are geometric opportunity, not measured visible pixels or spontaneous recognition.");
        Debug.Log("Study natural return " + Status);
    }
    void Cleanup()
    {
        running = false; keys = Array.Empty<Key>(); InputSystem.onBeforeUpdate -= InputUpdate; Application.logMessageReceived -= ReceiveLog; if (player) player.PoseReset -= ObserveReset;
        csv?.Dispose(); csv = null;
        if (fixtureStarted) { fixtureStarted = false; if (campaign) campaign.SuppressSaves = oldSuppress; if (session) session.SetPaused(true); }
        foreach (var frame in frames) if (frame.texture) { frame.texture.Release(); Destroy(frame.texture); }
        if (!configured) return; configured = false; InputSystem.settings.editorInputBehaviorInPlayMode = oldBehavior;
        if (input) { input.neverAutoSwitchControlSchemes = previousAutoSwitch; if (previousDevices.Length > 0 && !string.IsNullOrEmpty(previousScheme)) input.SwitchCurrentControlScheme(previousScheme, previousDevices); }
        if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard); if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
    }
    void OnDestroy() { Cleanup(); }
}
#endif
