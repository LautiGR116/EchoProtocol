#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using EchoProtocol.Interaction;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// An isolated house fixture, followed exclusively by synthetic device input.
/// It verifies physical routes and state, not human comprehension, fear or listening.
/// Run each mode in a fresh Play Mode session; this never resets a used house silently.
/// </summary>
public sealed class ResidenceRouteProbe : MonoBehaviour
{
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    private FirstPersonController player;
    private PlayerInteractor interactor;
    private PlayerInput playerInput;
    private EchoRecorder recorder;
    private LoopController loop;
    private GameSession session;
    private CampaignDirector campaign;
    private HouseMemory house;
    private HouseSwingDoor door;
    private Camera view;
    private Keyboard keyboard;
    private Mouse mouse;
    private InputDevice[] originalDevices;
    private string originalScheme;
    private bool originalAutoSwitch, oldSuppressSaves, saveSuppressionCaptured, inputConfigured, cleanedUp, running, expectedPause;
    private InputSettings.EditorInputBehaviorInPlayMode oldBehavior;
    private Key[] keys = Array.Empty<Key>();
    private float yaw, pitch, sensitivity, nextLog, startedAt;
    private string mode, step = "fixture", output;
    private int checks, focusResumeCount, fixtureCount;
    private bool focusResumeKeyDown;
    private StreamWriter log;
    private readonly List<string> screenshots = new List<string>();
    public string Status { get; private set; } = "starting";
    public string CurrentStep => step;

    [Serializable]
    private sealed class Result
    {
        public string mode, status, scene, lastStep, evidenceScope, error;
        public int checks, initialPoseFixtures, unexpectedPauseResumes;
        public float elapsedRealtime;
        public bool hasKey, answeredPhone, unlockedExit, presenceSeen, presenceRetired;
        public string powerLocation;
        public string[] screenshots;
    }

    public static void Run(string mode)
    {
        if (mode != "visual" && mode != "house" && mode != "alternate")
            throw new ArgumentException("Use ResidenceRouteProbe.Run(\"visual\"), Run(\"house\") or Run(\"alternate\").");
        var old = FindObjectsByType<ResidenceRouteProbe>(FindObjectsSortMode.None);
        if (old.Any(p => p.running)) throw new InvalidOperationException("A residence route is already running.");
        foreach (var probe in old) Destroy(probe.gameObject);
        var instance = new GameObject("Temporary residence input verification").AddComponent<ResidenceRouteProbe>();
        instance.mode = mode;
        instance.output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/VisualEvidence/2026-10-01-Residence/"));
        Directory.CreateDirectory(instance.output);
        instance.StartCoroutine(instance.Guarded(instance.Execute()));
    }

    private static T Field<T>(object owner, string field)
    {
        var info = owner.GetType().GetField(field, Private);
        if (info == null) throw new MissingFieldException(owner.GetType().Name, field);
        return (T)info.GetValue(owner);
    }

    // Flatten nested enumerators so every route failure produces a terminal JSON result.
    private IEnumerator Guarded(IEnumerator routine)
    {
        var stack = new Stack<IEnumerator>();
        stack.Push(routine);
        while (stack.Count > 0)
        {
            object yielded = null;
            bool more = false;
            Exception error = null;
            try { more = stack.Peek().MoveNext(); if (more) yielded = stack.Peek().Current; }
            catch (Exception exception) { error = exception; }
            if (error != null) { Finish(false, error.Message); yield break; }
            if (!more) { stack.Pop(); continue; }
            if (yielded is IEnumerator nested) { stack.Push(nested); continue; }
            yield return yielded;
        }
    }

    private IEnumerator Execute()
    {
        yield return null;
        startedAt = Time.realtimeSinceStartup;
        File.WriteAllText(PathFor("checks.txt"), "Synthetic input route; one explicit initial pose fixture. No human evidence.\n");
        File.WriteAllText(PathFor("cameras.txt"), "Actual Player camera after the one initial fixture; Camera renders omit OnGUI.\n");
        File.Delete(PathFor("result.json"));
        File.Delete(PathFor("result.txt"));
        log = new StreamWriter(PathFor("csv"));
        log.WriteLine("realtime,time,step,x,y,z,yaw,pitch,chapter,active,phone,key,exit,power,presenceSeen,presenceRetired,paused,recalls");
        player = FindFirstObjectByType<FirstPersonController>();
        campaign = FindFirstObjectByType<CampaignDirector>();
        house = FindFirstObjectByType<HouseMemory>();
        session = FindFirstObjectByType<GameSession>();
        loop = FindFirstObjectByType<LoopController>();
        Check(player && campaign && house && session && loop, "explicit campaign and residence systems exist");
        interactor = player.GetComponent<PlayerInteractor>();
        playerInput = player.GetComponent<PlayerInput>();
        recorder = player.GetComponent<EchoRecorder>();
        view = Field<Camera>(house, "view");
        door = Field<HouseSwingDoor>(house, "exitDoor");
        Check(interactor && playerInput && recorder && view && door, "Player input, camera and solid door references exist");
        Check(!house.Active && !house.HasKey && !house.HasAnswered && !house.HasExited, "fresh house has not been played");

        // The only permitted gameplay preparation: skip prior chapters and place at the house threshold.
        oldSuppressSaves = campaign.SuppressSaves;
        saveSuppressionCaptured = true;
        campaign.SuppressSaves = true;
        session.BeginOrResume();
        campaign.PrepareHouseProbe();
        player.ResetPose(new Vector3(0, .03f, -49.5f), Quaternion.Euler(0, 180, 0));
        fixtureCount++;
        ConfigureDevices();
        running = true;
        Status = "running";
        yaw = 180;
        pitch = 0;
        yield return Wait(.2f);
        Check(campaign.Chapter == CampaignChapter.Pressure, "isolated fixture prepares only the previous chapter");

        step = "enter_house";
        var origin = FindObjectsByType<CampaignStation>(FindObjectsSortMode.None).Single(s => s.Kind == StationKind.HouseOrigin);
        yield return UseTarget(origin);
        Check(campaign.Chapter == CampaignChapter.House && house.Active, "real E ray commits the house chapter");
        Check(!loop.enabled && !recorder.IsRecording && loop.ActiveEcho == null, "house suspends recording and playback");
        Check(house.PowerLocation == HousePowerLocation.Hall && !house.HasAnswered, "reserve begins in the hall before the phone");
        CheckFuseState();
        yield return Capture("00-entry");
        yield return RecallDisabled();

        if (mode == "alternate") yield return AlternateRoute();
        else yield return NormalRoute();

        step = "done";
        yield return Wait(.2f);
        foreach (string screenshot in screenshots)
            Check(File.Exists(Path.Combine(output, screenshot)), "capture written: " + screenshot);
        Finish(true, "");
    }

    private void ConfigureDevices()
    {
        originalScheme = playerInput.currentControlScheme;
        originalDevices = playerInput.devices.ToArray();
        originalAutoSwitch = playerInput.neverAutoSwitchControlSchemes;
        oldBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
        sensitivity = Field<float>(player, "lookSensitivity");
        if (sensitivity <= 0) throw new InvalidOperationException("A nonzero look sensitivity is required.");
        keyboard = InputSystem.AddDevice<Keyboard>("ResidenceTestKeyboard");
        mouse = InputSystem.AddDevice<Mouse>("ResidenceTestMouse");
        inputConfigured = true;
        playerInput.neverAutoSwitchControlSchemes = true;
        playerInput.SwitchCurrentControlScheme("Keyboard&Mouse", keyboard, mouse);
        playerInput.ActivateInput();
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.onBeforeUpdate += InputUpdate;
    }

    private void InputUpdate()
    {
        if (!running || InputState.currentUpdateType != InputUpdateType.Dynamic) return;
        Key[] frameKeys = keys;
        if (session.IsPaused && !expectedPause && !session.IsEnding)
        {
            // Focus-related Editor pauses are recovered with a real Enter edge, never a session method.
            if (!focusResumeKeyDown) focusResumeCount++;
            focusResumeKeyDown = !focusResumeKeyDown;
            frameKeys = focusResumeKeyDown ? new[] { Key.Enter } : Array.Empty<Key>();
        }
        else focusResumeKeyDown = false;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(frameKeys));
        float horizontal = Mathf.Clamp(Mathf.DeltaAngle(player.transform.eulerAngles.y, yaw), -12, 12) / sensitivity;
        float vertical = -Mathf.Clamp(Mathf.DeltaAngle(Field<float>(player, "pitch"), pitch), -10, 10) / sensitivity;
        InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(horizontal, vertical) });
        if (player.enabled && !session.IsPaused) player.GetType().GetField("cursorIsLocked", Private).SetValue(player, true);
    }

    private void LateUpdate()
    {
        if (!running) return;
        if (player.transform.position.y < -.2f) { Finish(false, "Player fell below the authored floor."); StopAllCoroutines(); return; }
        if (Time.realtimeSinceStartup < nextLog) return;
        nextLog = Time.realtimeSinceStartup + .1f;
        var p = player.transform.position;
        log.WriteLine(string.Join(",", Time.realtimeSinceStartup.ToString("F3", Invariant), Time.time.ToString("F3", Invariant), step,
            p.x.ToString("F3", Invariant), p.y.ToString("F3", Invariant), p.z.ToString("F3", Invariant),
            player.transform.eulerAngles.y.ToString("F2", Invariant), Field<float>(player, "pitch").ToString("F2", Invariant),
            campaign.Chapter, house.Active, house.HasAnswered, house.HasKey, house.HasExited, house.PowerLocation,
            house.PresenceSeen, house.PresenceRetired, session.IsPaused, loop.CompletedLoopCount));
        log.Flush();
    }

    private IEnumerator NormalRoute()
    {
        step = "parlor_document";
        yield return Go(0, -52.3f, 180);
        yield return Go(-2f, -52.3f, 270);
        yield return Go(-2.4f, -50.5f, 0);
        var document = FindObjectsByType<HouseInteraction>(FindObjectsSortMode.None)
            .Where(s => s.Kind == HouseInteractionKind.Document)
            .OrderBy(s => Vector3.Distance(TargetPoint(s), new Vector3(-2.4f, 1.5f, -49.15f))).FirstOrDefault();
        Check(document, "an optional physical house document exists");
        expectedPause = true;
        yield return UseTarget(document);
        Check(session.IsPaused && !string.IsNullOrEmpty(Field<string>(session, "documentTitle")), "E opens a real document and pauses simulation");
        float documentTime = Time.time;
        Vector3 documentPose = player.transform.position;
        yield return Capture("01-document-game");
        yield return Press(Key.R);
        Check(Mathf.Approximately(documentTime, Time.time) && Vector3.Distance(documentPose, player.transform.position) < .01f,
            "document pause preserves clock and pose despite R input");
        yield return Press(Key.Escape);
        Check(!session.IsPaused, "Escape closes the document through keyboard input");
        expectedPause = false;
        Check(!house.HasAnswered && !house.HasKey && !house.HasExited, "document reading changes no progression flags");

        step = "phone_and_reserve";
        yield return Go(-2f, -52.3f, 180);
        yield return Go(-3.45f, -51.9f, 270);
        yield return Aim(TargetPoint(Interaction(HouseInteractionKind.Telephone)));
        yield return Capture("02-phone-before");
        yield return Use(HouseInteractionKind.Telephone);
        Check(house.HasAnswered && house.PowerLocation == HousePowerLocation.Hall, "phone removes mains and retains the hall reserve");
        CheckFuseState(); CheckPowerState();
        yield return Capture("03-phone-reserve");
        yield return MenuPause();

        step = "remove_hall_reserve";
        yield return Go(-2f, -52.3f, 90);
        yield return Go(0, -52.3f, 90);
        yield return Go(0, -50.8f, 0);
        yield return Use(HouseInteractionKind.HallSocket);
        Check(house.PowerLocation == HousePowerLocation.Carried, "E physically removes the sole hall reserve");
        CheckFuseState(); CheckPowerState();
        yield return Capture("04-hall-without-reserve");

        step = "reserve_to_kitchen";
        yield return Go(0, -58.2f, 180);
        yield return Go(-2f, -58.2f, 270);
        yield return Go(-4.2f, -57.5f, 270);
        yield return Use(HouseInteractionKind.KitchenSocket);
        Check(house.PowerLocation == HousePowerLocation.Kitchen, "reserve can be placed directly in the kitchen");
        CheckFuseState(); CheckPowerState();
        yield return Capture("05-kitchen-powered");
        yield return Use(HouseInteractionKind.KitchenSocket);
        Check(house.PowerLocation == HousePowerLocation.Carried, "kitchen reserve is removable without a completion sequence");
        CheckFuseState(); CheckPowerState();
        yield return Go(-2f, -58.2f, 90);
        yield return Go(0, -58.2f, 90);

        step = "locked_door_without_key";
        yield return Go(0, -61.1f, 180);
        yield return Use(HouseInteractionKind.ExitDoor);
        Check(!house.HasExited && !door.IsOpenRequested, "a real E attempt without the key cannot open the mechanical exit");

        step = "reserve_to_bedroom";
        yield return Go(0, -53.3f, 0);
        yield return EnterBedroom();
        yield return Go(4.6f, -51.25f, 90);
        yield return Go(4.6f, -53f, 180);
        yield return Use(HouseInteractionKind.BedroomSocket);
        Check(house.PowerLocation == HousePowerLocation.Bedroom, "reserve transfers from kitchen to bedroom through actual carried state");
        CheckFuseState(); CheckPowerState();
        yield return Capture("06-bedroom-powered");

        step = "physical_key";
        yield return Go(4.6f, -51.25f, 0);
        yield return Go(3.2f, -50.8f, 270);
        yield return Use(HouseInteractionKind.Key);
        Check(house.HasKey && !Field<GameObject>(house, "keyWorld").activeInHierarchy, "E collects the physical key and removes its world representation");
        yield return Capture("07-key-taken");
        yield return RecallDisabled();

        step = "remove_bedroom_reserve";
        yield return Go(4.6f, -51.25f, 90);
        yield return Go(4.6f, -53f, 180);
        yield return Use(HouseInteractionKind.BedroomSocket);
        Check(house.PowerLocation == HousePowerLocation.Carried, "the key does not lock the reserve in the bedroom");
        CheckFuseState(); CheckPowerState();
        yield return Go(4.6f, -51.25f, 0);
        yield return Go(1.8f, -50.8f, 270);
        yield return Go(1.8f, -53.3f, 180);
        yield return Go(0, -53.3f, 270);

        step = "presence_through_actual_glass";
        yield return Go(0, -57.7f, 180);
        var presence = Field<GameObject>(house, "presenceRoot");
        var volume = Field<BoxCollider>(house, "presenceVisibilityVolume");
        Check(presence && volume, "the house has a configured contained presence");
        Vector3 torso = volume.transform.TransformPoint(volume.center);
        yield return Aim(torso);
        yield return Wait(.2f);
        Check(house.PresenceSeen && presence.activeInHierarchy, "unobstructed Player view actually sees the persistent presence");
        yield return Capture("08-presence-held-in-view");
        yield return Face(yaw + 180, 0);
        yield return Wait(.2f);
        Check(presence.activeInHierarchy && !house.PresenceRetired, "turning away without physical occlusion does not remove the presence");
        yield return Go(0, -50.8f, 0);
        yield return Wait(.2f);
        Check(house.PresenceRetired && !presence.activeInHierarchy, "a genuine opaque return corner retires the already seen presence");

        step = "leave_hall_lit";
        yield return Use(HouseInteractionKind.HallSocket);
        Check(house.HallLightLeftOn, "the Player can answer Mara's request by restoring the hall light");
        CheckFuseState(); CheckPowerState();
        yield return Capture("09-hall-light-returned");
        yield return ExitThroughDoor();
        Check(house.HasAnswered && house.HallLightLeftOn, "normal exit preserves the chosen hall light");
    }

    private IEnumerator AlternateRoute()
    {
        step = "alternate_key_without_phone";
        yield return Go(0, -53.3f, 180);
        yield return EnterBedroom();
        yield return Go(3.2f, -50.8f, 90);
        yield return Use(HouseInteractionKind.Key);
        Check(house.HasKey && !house.HasAnswered, "the physical key is available before answering the phone");
        Check(house.PowerLocation == HousePowerLocation.Hall, "alternate route leaves the reserve where it began");
        CheckFuseState();
        yield return Capture("alternate-key-before-phone");
        yield return RecallDisabled();
        yield return Go(1.8f, -50.8f, 270);
        yield return Go(1.8f, -53.3f, 180);
        yield return Go(0, -53.3f, 270);
        yield return ExitThroughDoor();
        Check(house.HasExited && !house.HasAnswered, "mechanical exit permits completion without phone, circuit order or documents");
    }

    private IEnumerator EnterBedroom()
    {
        yield return Go(1.8f, -53.3f, 90);
        yield return Go(1.8f, -50.8f, 0);
    }

    private IEnumerator ExitThroughDoor()
    {
        step = "unlock_and_cross_exit";
        yield return Go(0, -61.1f, 180);
        yield return Use(HouseInteractionKind.ExitDoor);
        Check(house.HasExited && door.IsOpenRequested, "the key unlocks the physical hinged door with real E input");
        float deadline = Time.realtimeSinceStartup + 8f;
        while (!door.IsFullyOpen)
        {
            if (Time.realtimeSinceStartup > deadline) throw new Exception("Exit leaf did not finish opening; obstructed=" + door.IsObstructed);
            yield return null;
        }
        Check(door.IsFullyOpen, "hinged leaf reaches its authored open rotation");
        yield return Capture("10-mechanical-door-open");
        yield return Go(0, -63.6f, 180);
        Check(player.transform.position.z < -63.3f, "the real CharacterController crosses the opened exit");
        yield return Capture("11-outside-house");
    }

    private IEnumerator MenuPause()
    {
        step = "keyboard_pause";
        expectedPause = true;
        yield return Press(Key.Escape);
        Check(session.IsPaused, "Escape opens the actual pause menu");
        float at = Time.time;
        Vector3 position = player.transform.position;
        int recalls = loop.CompletedLoopCount;
        keys = new[] { Key.W, Key.R };
        yield return new WaitForSecondsRealtime(.25f);
        keys = Array.Empty<Key>();
        Check(Mathf.Approximately(at, Time.time) && Vector3.Distance(position, player.transform.position) < .01f && recalls == loop.CompletedLoopCount,
            "real paused movement and R input leave clock, Player and loop unchanged");
        yield return Capture("pause-menu");
        yield return Press(Key.Escape);
        Check(!session.IsPaused && player.enabled, "Escape restores actual Player control");
        expectedPause = false;
    }

    private IEnumerator RecallDisabled()
    {
        int recalls = loop.CompletedLoopCount;
        Vector3 position = player.transform.position;
        var reserve = house.PowerLocation;
        bool key = house.HasKey;
        yield return Press(Key.R);
        Check(loop.CompletedLoopCount == recalls && Vector3.Distance(position, player.transform.position) < .025f &&
            loop.ActiveEcho == null && !recorder.IsRecording, "real R input cannot reset or record the house");
        Check(house.PowerLocation == reserve && house.HasKey == key, "R leaves the reserve and key unchanged");
    }

    private void CheckFuseState()
    {
        string[] names = { "hallFuse", "kitchenFuse", "bedroomFuse" };
        int represented = 0;
        for (int i = 0; i < names.Length; i++)
        {
            var representation = Field<GameObject>(house, names[i]);
            Check(representation, "authored fuse representation exists: " + names[i]);
            bool expected = (int)house.PowerLocation == i;
            Check(representation.activeInHierarchy == expected, names[i] + " agrees with the single reserve state");
            if (representation.activeInHierarchy) represented++;
        }
        var carried = Field<GameObject>(house, "carriedFuse");
        if (carried)
        {
            Check(carried.activeInHierarchy == (house.PowerLocation == HousePowerLocation.Carried), "carried mesh agrees with reserve state");
            if (carried.activeInHierarchy) represented++;
        }
        int expectedCount = house.PowerLocation == HousePowerLocation.Carried && !carried ? 0 : 1;
        Check(represented == expectedCount, "reserve never has duplicate physical representations");
    }

    private void CheckPowerState()
    {
        Check(house.HasAnswered, "reserve lighting is checked after mains removal");
        string[] groups = { "hallLights", "kitchenLights", "bedroomLights" };
        var reserveLights = new HashSet<Light>();
        for (int i = 0; i < groups.Length; i++)
        {
            var lights = Field<Light[]>(house, groups[i]);
            Check(lights.Length > 0 && lights.All(light => light), groups[i] + " has explicit live references");
            bool expected = (int)house.PowerLocation == i;
            foreach (var light in lights)
            {
                reserveLights.Add(light);
                Check(light.enabled == expected, light.name + " follows its actual reserve circuit");
            }
        }
        foreach (var light in Field<Light[]>(house, "generalLights"))
            if (light && !reserveLights.Contains(light)) Check(!light.enabled, "general supply removed from " + light.name);
        var appliance = Field<AudioSource>(house, "kitchenApplianceAudio");
        var clip = Field<AudioClip>(house, "kitchenApplianceClip");
        if (appliance && clip && appliance.isActiveAndEnabled)
            Check(appliance.isPlaying == (house.PowerLocation == HousePowerLocation.Kitchen), "appliance playback state follows kitchen power; this is not listening evidence");
    }

    private HouseInteraction Interaction(HouseInteractionKind kind)
    {
        return FindObjectsByType<HouseInteraction>(FindObjectsSortMode.None).Single(s => s.Kind == kind);
    }

    private IEnumerator Use(HouseInteractionKind kind) { yield return UseTarget(Interaction(kind)); }

    private IEnumerator UseTarget(Component component)
    {
        Vector3 point = TargetPoint(component);
        Check(Vector3.Distance(view.transform.position, point) <= interactor.InteractionDistance + .05f,
            component.name + " is within the actual interaction range");
        yield return Aim(point);
        var expected = component as IInteractable;
        Check(expected != null && interactor.TryGetTarget(out var target) && ReferenceEquals(target, expected),
            "center-camera ray reaches " + component.name);
        Check(expected.CanInteract(player.gameObject), component.name + " accepts the actual Player in the current chapter");
        yield return Press(Key.E);
    }

    private Vector3 TargetPoint(Component component)
    {
        var collider = component.GetComponentsInChildren<Collider>()
            .Where(c => c.enabled && c.gameObject.activeInHierarchy && !c.isTrigger)
            .OrderBy(c => Vector3.Distance(c.bounds.center, view.transform.position)).FirstOrDefault();
        return collider ? collider.bounds.center : component.transform.position;
    }

    private IEnumerator Go(float x, float z, float facing)
    {
        yield return Face(facing, 0);
        float deadline = Time.realtimeSinceStartup + 22f;
        while (Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(x, z)) > .1f)
        {
            if (Time.realtimeSinceStartup > deadline)
                throw new Exception("Physical route blocked at " + player.transform.position.ToString("F2") + " toward " + x + "," + z);
            Vector3 local = player.transform.InverseTransformDirection(new Vector3(x, player.transform.position.y, z) - player.transform.position);
            var movement = new List<Key>();
            if (local.z > .06f) movement.Add(Key.W); else if (local.z < -.06f) movement.Add(Key.S);
            if (local.x > .06f) movement.Add(Key.D); else if (local.x < -.06f) movement.Add(Key.A);
            keys = movement.ToArray();
            yield return null;
        }
        keys = Array.Empty<Key>();
        yield return Wait(.12f);
    }

    private IEnumerator Aim(Vector3 point)
    {
        Vector3 direction = point - view.transform.position;
        yield return Face(Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg,
            -Mathf.Atan2(direction.y, new Vector2(direction.x, direction.z).magnitude) * Mathf.Rad2Deg);
    }

    private IEnumerator Face(float targetYaw, float targetPitch)
    {
        yaw = targetYaw; pitch = Mathf.Clamp(targetPitch, -84, 84); keys = Array.Empty<Key>();
        float deadline = Time.realtimeSinceStartup + 5f;
        while (Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y, yaw)) > .6f ||
            Mathf.Abs(Mathf.DeltaAngle(Field<float>(player, "pitch"), pitch)) > .6f)
        {
            if (Time.realtimeSinceStartup > deadline) throw new Exception("Synthetic mouse look did not reach the requested real-camera angle.");
            yield return null;
        }
        yield return Wait(.12f);
    }

    private IEnumerator Press(Key key)
    {
        keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(.08f);
        keys = new[] { key }; yield return new WaitForSecondsRealtime(.12f);
        keys = Array.Empty<Key>(); yield return new WaitForSecondsRealtime(.12f);
    }

    private IEnumerator Wait(float seconds)
    {
        keys = Array.Empty<Key>();
        yield return new WaitForSecondsRealtime(seconds);
    }

    private IEnumerator Capture(string label)
    {
        keys = Array.Empty<Key>();
        yield return null;
        string name = mode + "-" + label;
        string gameName = name + "-Game.png";
        File.Delete(Path.Combine(output, gameName));
        ScreenCapture.CaptureScreenshot(Path.Combine(output, gameName));
        screenshots.Add(gameName);
        var oldTarget = view.targetTexture;
        var oldActive = RenderTexture.active;
        var texture = new RenderTexture(1280, 720, 24);
        var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        try
        {
            view.targetTexture = texture;
            view.Render();
            RenderTexture.active = texture;
            pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            pixels.Apply();
            string cameraName = name + "-Camera.png";
            File.WriteAllBytes(Path.Combine(output, cameraName), pixels.EncodeToPNG());
            screenshots.Add(cameraName);
            File.AppendAllText(PathFor("cameras.txt"), name + " actual camera position=" + view.transform.position.ToString("F3") +
                " rotation=" + view.transform.eulerAngles.ToString("F2") + " FOV=" + view.fieldOfView.ToString("F1", Invariant) +
                " paused=" + session.IsPaused + " power=" + house.PowerLocation + " step=" + step + "\n");
        }
        finally
        {
            view.targetTexture = oldTarget;
            RenderTexture.active = oldActive;
            Destroy(texture); Destroy(pixels);
        }
        yield return new WaitForSecondsRealtime(.18f);
    }

    private string PathFor(string suffix) => Path.Combine(output, mode + "." + suffix);

    private void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(step + ": " + message);
        checks++;
        File.AppendAllText(PathFor("checks.txt"), "PASS " + message + "\n");
    }

    private void Finish(bool success, string error)
    {
        if (Status.StartsWith("PASS") || Status.StartsWith("FAIL")) return;
        Status = success ? "PASS" : "FAIL " + step + ": " + error;
        running = false;
        var result = new Result
        {
            mode = mode, status = success ? "PASS" : "FAIL", error = error, lastStep = step,
            scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path, checks = checks,
            initialPoseFixtures = fixtureCount, unexpectedPauseResumes = focusResumeCount,
            elapsedRealtime = Time.realtimeSinceStartup - startedAt,
            evidenceScope = "Isolated previous-chapter fixture, then real CharacterController/raycast/keyboard/mouse through Input System. Game captures include UI; Camera captures omit it. No human play, listening, fear or full-campaign claim.",
            hasKey = house && house.HasKey, answeredPhone = house && house.HasAnswered,
            unlockedExit = house && house.HasExited, presenceSeen = house && house.PresenceSeen,
            presenceRetired = house && house.PresenceRetired, powerLocation = house ? house.PowerLocation.ToString() : "unavailable",
            screenshots = screenshots.ToArray()
        };
        File.WriteAllText(PathFor("result.json"), JsonUtility.ToJson(result, true));
        File.WriteAllText(PathFor("result.txt"), Status + "\n");
        if (!success) File.AppendAllText(PathFor("checks.txt"), Status + "\n");
        Cleanup();
        if (success) Debug.Log("Residence " + mode + " PASS"); else Debug.LogWarning("Residence " + Status);
    }

    private void Cleanup()
    {
        if (cleanedUp) return;
        cleanedUp = true;
        running = false;
        keys = Array.Empty<Key>();
        InputSystem.onBeforeUpdate -= InputUpdate;
        log?.Dispose(); log = null;
        if (campaign && saveSuppressionCaptured) campaign.SuppressSaves = oldSuppressSaves;
        if (inputConfigured)
        {
            InputSystem.settings.editorInputBehaviorInPlayMode = oldBehavior;
            if (playerInput)
            {
                playerInput.neverAutoSwitchControlSchemes = originalAutoSwitch;
                var available = originalDevices.Where(device => device != null && device.added).ToArray();
                if (!string.IsNullOrEmpty(originalScheme) && available.Length > 0)
                {
                    try { playerInput.SwitchCurrentControlScheme(originalScheme, available); }
                    catch (Exception exception) { Debug.LogWarning("Residence probe could not restore the original input pairing: " + exception.Message); }
                }
            }
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
        }
    }

    private void OnDestroy() { Cleanup(); }
}
#endif
