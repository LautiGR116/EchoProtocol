#if UNITY_EDITOR || ECHO_NATIVE_VERIFY
using System;
using System.Collections;
using System.IO;
using System.Linq;
using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed partial class ThresholdCampaignProbe
{
    IEnumerator FocusedCoverPresentation()
    {
        step="coverage_contact_record";
        var machine=campaign.CoverEncounter.Machine;
        yield return Go(3.8f,-32,90);yield return Go(4.6f,-35.9f,180);
        yield return Aim(Field<Transform>(machine,"grip").position);
        Check(interactor.FocusedTarget==machine,"normal camera reach selects the pump for the contact check");
        keys=new[]{Key.E};yield return new WaitForSeconds(14);keys=Array.Empty<Key>();yield return Wait(.15f);
        yield return Recall();
        var take=loop.LastRecording;
        var immutable=Enumerable.Range(0,take.FrameCount).Select(i=>take[i]).ToArray();
        step="coverage_contact_approach";
        yield return Go(3.8f,-32,90);yield return Go(4.6f,-34.7f,180);
        yield return Until(()=>machine.ReplayHeld,7,"recorded Echo did not arrive and hold its pump contact");
        var torso=loop.ActiveEcho.GetComponent<EchoBodyRig>().CharacterMotion.Joints.First(j=>j.name.EndsWith("spine_03",StringComparison.Ordinal));
        yield return Aim(torso.position+Vector3.up*.12f);
        var clearance=player.GetComponent<PlayerEchoClearance>();
        Check(clearance&&clearance.enabled,"explicit Player clearance is active");
        int contactsBefore=clearance.Contacts;
        keys=new[]{Key.W};yield return new WaitForSeconds(1.2f);keys=Array.Empty<Key>();yield return Wait(.1f);
        var gap=player.transform.position-torso.position;gap.y=0;
        File.WriteAllText(Path.Combine(output,"contact-attempt.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{contacts=clearance.Contacts,gap=gap.magnitude,torso=torso.position.ToString("F3"),player=player.transform.position.ToString("F3"),echo=loop.ActiveEcho.transform.position.ToString("F3"),owner=typeof(PlayerEchoClearance).GetField("owner",Private).GetValue(clearance)!=null,left=typeof(PlayerEchoClearance).GetField("leftOrigin",Private).GetValue(clearance),chest=typeof(PlayerEchoClearance).GetField("chest",Private).GetValue(clearance)!=null,echoActive=loop.ActiveEcho.gameObject.activeInHierarchy,elapsed=loop.ActiveEcho.ElapsedTime}));
        Check(clearance.Contacts>contactsBefore&&gap.magnitude>=.59f&&gap.magnitude<.74f,"actual sustained movement into the recorded torso stops beside it and keeps the live camera outside");
        Check(Enumerable.Range(0,take.FrameCount).All(i=>SameFrame(immutable[i],take[i])),"body contact leaves the complete recorded take unchanged");
        yield return Capture("impact-echo-contact");
        File.WriteAllText(Path.Combine(output,"contact.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{status="PASS",contacts=clearance.Contacts,torsoGap=gap.magnitude,scope="Actual title access, E recording, R replay and W approach. Live Player yields; recorded root/body/take unchanged."},Newtonsoft.Json.Formatting.Indented));

        // Explicit presentation-only fixture: combine a short countdown, optional
        // note and immediate subtitle without waiting a whole recording minute.
        step="coverage_hud_fixture";
        var hints=FindFirstObjectByType<ContextualHints>();
        var subtitles=FindFirstObjectByType<SD1SubtitleDisplay>();
        typeof(EchoRecorder).GetField("recordingStartTime",Private).SetValue(recorder,Time.time-recorder.MaxRecordingDuration+5);
        hints.enabled=false;
        typeof(ContextualHints).GetField("visibleRemaining",Private).SetValue(hints,5f);
        typeof(ContextualHints).GetField("<VisibleText>k__BackingField",Private).SetValue(hints,"La bomba cubre los golpes; esperá el silencio para atravesar la reja.");
        subtitles.Show("El cierre quedó libre. Volvé por el paso lateral.",5);
        yield return Wait(.15f);yield return Capture("impact-hud-countdown-subtitle");
        Check(!new Rect(32,98,355,90).Overlaps(new Rect(846,26,402,180)),"countdown and objective occupy separate HUD columns at 1280 by 720");
        File.WriteAllText(Path.Combine(output,"hud-fixture.json"),"{\"scope\":\"Explicit render fixture only: shortened recording countdown, forced optional hint and subtitle. No route or fear claim.\",\"screenshots\":[\"campaign-impact-hud-countdown-subtitle-Game.png\"]}");
        expectedPause=true;yield return Press(Key.Escape);
        Check(session.IsPaused,"focused verification ends at an actual pause");
    }
}
#endif
