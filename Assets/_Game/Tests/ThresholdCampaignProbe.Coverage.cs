#if UNITY_EDITOR || ECHO_NATIVE_VERIFY
using System;using System.Collections;using System.Linq;
using System.IO;
using EchoProtocol.Campaign;using EchoProtocol.Echo;using EchoProtocol.Puzzle;using UnityEngine;using UnityEngine.InputSystem;
public sealed partial class ThresholdCampaignProbe
{
    bool coverageOnly,coverageFailure,coveragePresentation,expectedPreviewAccess;
    int previewResets;
    EchoRecording coverTake;
    RecordedMachineGrip[] coverGrips;
    EchoFrame[] coverFrames;
    public static void RunCoverage(bool failAndRetry=false)=>StartRun(false,null,coverOnly:true,coverFailure:failAndRetry);
    public static void RunCoverPresentation()=>StartRun(false,null,coverOnly:true,coverPresentation:true);
    IEnumerator CoverPressure()
    {
        var cover=campaign.CoverEncounter;
        var machine=cover.Machine;
        var crank=Field<MachineCoverInteraction>(cover,"crank");
        var release=Field<MachineCoverInteraction>(cover,"release");
        var gate=Field<SlidingDoor>(cover,"serviceGate");
        var threat=Field<MachineCoverThreat>(cover,"threat");
        step="coverage_enter";
        if(!cover.Active)yield return Go(0,-30,180);
        Check(cover.Active&&loop.enabled&&recorder.IsRecording,"physical basement entry begins a fresh recording contract");
        Check(!threat.Released&&!cover.BoltReleased&&!cover.ExitReleased,"the rule can be explored before danger");
        yield return Go(3.8f,-32,90);yield return Go(4.6f,-35.9f,180);
        yield return Aim(Field<Transform>(machine,"grip").position);
        yield return Wait(.2f);
        Check(interactor.FocusedTarget==machine&&interactor.HasUsableFocus,"camera ray and physical reach select the recorded pump control");
        yield return Capture("coverage-control");
        step="coverage_record_first_noise";keys=new[]{Key.E};yield return new WaitForSeconds(12);keys=Array.Empty<Key>();yield return Wait(.18f);
        Check(cover.RuleObserved&&recorder.CapturedMachineGripCount==2&&!machine.PlayerHeld,"safe first start and release captured as two actual grip events");
        step="coverage_record_silence";yield return Wait(6.82f);
        step="coverage_record_return_noise";keys=new[]{Key.E};yield return new WaitForSeconds(20);keys=Array.Empty<Key>();yield return Wait(.2f);
        Check(recorder.CapturedMachineGripCount==4,"two starts and two releases explicitly captured");
        yield return Recall();
        coverTake=loop.LastRecording;
        coverGrips=Enumerable.Range(0,coverTake.MachineGripCount).Select(coverTake.MachineGrip).ToArray();
        coverFrames=Enumerable.Range(0,coverTake.FrameCount).Select(i=>coverTake[i]).ToArray();
        File.WriteAllText(PathFor("machine-grips.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{duration=coverTake.Duration,frames=coverTake.FrameCount,events=coverGrips.Select(g=>new{time=g.Timestamp,target=g.Target,held=g.Held}),scope="Actual recording through physical control and normal E; immutable snapshot before replay."},Newtonsoft.Json.Formatting.Indented));
        Check(coverGrips.Length==4&&coverGrips[0].Held&&!coverGrips[1].Held&&coverGrips[2].Held&&!coverGrips[3].Held,"immutable take contains the actual on/off/on/off sequence");
        if(coverageOnly)
        {
            step="coverage_playback_pause";yield return Wait(.3f);
            expectedPause=true;yield return Press(Key.Escape);
            float echoTime=loop.ActiveEcho.ElapsedTime,pressure=cover.Pressure01;
            bool held=machine.ReplayHeld;
            yield return Wait(.5f);
            Check(session.IsPaused&&AudioListener.pause&&loop.ActiveEcho.ElapsedTime==echoTime&&cover.Pressure01==pressure&&machine.ReplayHeld==held,"real pause freezes playback, pressure and the recorded grip");
            yield return Capture("coverage-paused");
            yield return Press(Key.Escape);expectedPause=false;
            Check(!session.IsPaused,"real Escape resumes the segment");
        }
        yield return CoverWork(false);
        if(coverageFailure)
        {
            step="coverage_deliberate_failure";expectedPause=true;
            float deadline=Time.realtimeSinceStartup+70;bool approachSeen=false,captureRecallChecked=false;
            while(!cover.Failed)
            {
                if(Time.realtimeSinceStartup>deadline)throw new Exception("present threat never caught the exposed stationary Player");
                // Look using the same real mouse-delta input path as the route.
                // No camera transform, actor pose or simulation state is changed.
                Vector3 d=threat.transform.position+Vector3.up*1.35f-view.transform.position;
                yaw=Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg;pitch=-Mathf.Atan2(d.y,new Vector2(d.x,d.z).magnitude)*Mathf.Rad2Deg;
                bool clear=Physics.RaycastAll(view.transform.position,d.normalized,d.magnitude,~0,QueryTriggerInteraction.Ignore)
                    .All(h=>h.transform.IsChildOf(player.transform)||h.transform.IsChildOf(threat.transform));
                if(!approachSeen&&d.magnitude<3.2f&&clear)
                {yield return Capture("coverage-present-rush");approachSeen=true;}
                if(threat.PerformanceEnabled&&threat.IsCapturing&&!captureRecallChecked){int recalls=loop.CompletedLoopCount;yield return Press(Key.R);Check(loop.CompletedLoopCount==recalls,"physical capture suppresses live RECALL input without changing the take");captureRecallChecked=true;}
                yield return null;
            }
            Check(approachSeen,"present threat emerges around the physical baffle during actual mouse observation");
            Check(session.IsPaused&&cover.FailureCount==1&&!cover.Complete,"failure is concrete and stops the session");
            if(threat.PerformanceEnabled){var acting=threat.GetComponent<MachineThreatPerformance>();Check(threat.CaptureContactCount==1&&threat.LastCaptureSeconds>=.94f,"physical capture precedes failure by at least 940 ms");Check(acting.ReachCount>0,"present arms reach during physical capture");Check(acting.PlantedContacts>4,"physical steps follow planted supports");Check(acting.MaxPlantDrift<.025f,"planted feet remain within 25 mm of authored floor contact");}
            AuditCoverTake();yield return Capture("coverage-failure");
            yield return Press(Key.Enter);expectedPause=false;
            Check(!cover.Failed&&!session.IsPaused&&loop.PlaybackRestartCount==1&&ReferenceEquals(loop.ActiveEcho.Recording,coverTake),"actual failure-menu Enter replays the same immutable take");
            yield return CoverWork(true);
        }
        else yield return CoverEscape();
        Check(cover.Complete&&campaign.Chapter==CampaignChapter.Pressure&&!loop.enabled&&Field<SlidingDoor>(cover,"exitDoor").IsFullyOpen,"normal route secures the physical exit and suspends recording for the house");
        Check(threat.DiversionCount>0,"live machine sound diverted the present threat during the prepared return");
        if(coverageOnly)
        {
            step="coverage_natural_recorded_release";
            var replay=loop.ActiveEcho;
            Check(cover.Machine.ReplayHeld==coverTake.SampleMachineGrip(replay.ElapsedTime,machine.TargetId),"earned exit never truncates the remaining recorded grip");
            yield return Until(()=>replay.IsComplete&&!cover.Covered&&cover.Pressure01==0,35,"remaining take did not release the real pump and pressure naturally");
            Check(!machine.ReplayHeld&&!Field<AudioSource>(cover,"motor").isPlaying&&!Field<AudioSource>(cover,"purge").isPlaying,"natural end of the take leaves the real machine silent and depressurized");
            yield return Capture("coverage-natural-silence");
        }
        AuditCoverTake();
    }
    IEnumerator CoverWork(bool escape)
    {
        var cover=campaign.CoverEncounter;
        var crank=Field<MachineCoverInteraction>(cover,"crank");var release=Field<MachineCoverInteraction>(cover,"release");var gate=Field<SlidingDoor>(cover,"serviceGate");
        step="coverage_cross_to_crank";
        yield return Go(-3.6f,-37.3f,180);yield return Go(-3.6f,-38.55f,180);yield return Aim(crank.transform.position);
        yield return Until(()=>cover.Covered&&cover.Pressure01>.8f,8,"recorded grip failed to start and pressurize the physical pump");
        Check(cover.Machine.ReplayHeld&&!gate.IsOpenRequested&&cover.Pressure01>.8f,"real motor covers work while the pressurized grille stays closed");
        yield return Capture("coverage-work-before-release");
        step="coverage_hold_crank";keys=new[]{Key.E};yield return new WaitForSeconds(4.25f);keys=Array.Empty<Key>();yield return Wait(.08f);
        Check(cover.BoltReleased&&!cover.ExitReleased,"held real E frees only the jammed bolt");
        yield return Until(()=>gate.IsFullyOpen,18,"the recorded release did not drain pressure and open the physical grille");
        Check(!cover.Machine.ReplayHeld&&cover.Pressure01<.16f,"faithful recorded silence releases the pressure obstruction");
        yield return Capture("coverage-grille-open");
        step="coverage_enter_service";yield return Go(-4.65f,-38.6f,270);yield return Go(-4.65f,-41.6f,180);yield return Go(-4.0f,-43.2f,180);yield return Aim(release.transform.position);
        step="coverage_hold_release";
        var presentActor=Field<MachineCoverThreat>(cover,"threat");
        if(presentActor.PerformanceEnabled){Vector3 d=presentActor.GetComponent<MachineThreatPerformance>().Face.position-view.transform.position;Check(Physics.RaycastAll(view.transform.position,d.normalized,d.magnitude,~0,QueryTriggerInteraction.Ignore).Any(h=>!h.transform.IsChildOf(player.transform)&&!h.transform.IsChildOf(presentActor.transform)),"present actor is physically concealed before the release");}
        keys=new[]{Key.E};yield return new WaitForSeconds(Field<float>(release,"workSeconds")+.3f);keys=Array.Empty<Key>();yield return Wait(.08f);
        Check(cover.ExitReleased&&Field<MachineCoverThreat>(cover,"threat").Released,"manual exit release secures the return and releases a present-time threat");
        if(Field<MachineCoverThreat>(cover,"threat").PerformanceEnabled){Vector3 v=view.WorldToViewportPoint(Field<MachineCoverThreat>(cover,"threat").RecognitionPoint.position+Vector3.up*1.75f);Check(v.z>0&&v.x>.03f&&v.x<.97f&&v.y>.02f&&v.y<.98f,"authored recognition point fits normal free-camera release framing");Check(cover.HideEncounterHud,"encounter HUD clears immediately on release");}
        yield return Capture("coverage-release-and-threat");
        if(escape)yield return CoverEscape();
    }
    IEnumerator CoverEscape()
    {
        step="coverage_prepared_return";
        // Turn beside the wall-mounted release rather than trying to walk
        // through its real collision housing on the way to the lateral door.
        yield return Go(-4.2f,-43.6f,90);
        if(GameObject.Find("Return / early blind wing"))
        {yield return Go(-1.4f,-41.35f,90);yield return Go(0,-41.35f,90);yield return Go(0,-43.6f,90);}
        else yield return Go(0,-43.6f,90);
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="EchoProtocol_Impact"||UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="EchoProtocol_Incursion")
        {
            yield return Go(3.3f,-43.6f,90);
            yield return Capture("coverage-return-covered");
            yield return Go(3.3f,-46.85f,180);yield return Go(0,-46.85f,270);
            yield return Go(0,-49.5f,180);
        }
        else {yield return Capture("coverage-return-covered");yield return Go(0,-49.5f,180);}
    }
    void AuditCoverTake()
    {
        Check(coverTake!=null&&coverTake.FrameCount==coverFrames.Length&&Enumerable.Range(0,coverFrames.Length).All(i=>SameFrame(coverTake[i],coverFrames[i])),"machine take root and body frames remain unchanged");
        Check(coverTake.MachineGripCount==coverGrips.Length&&Enumerable.Range(0,coverGrips.Length).All(i=>coverTake.MachineGrip(i).Timestamp.Equals(coverGrips[i].Timestamp)&&coverTake.MachineGrip(i).Target==coverGrips[i].Target&&coverTake.MachineGrip(i).Held==coverGrips[i].Held),"every recorded machine event remains unchanged through work, pause, threat and retry");
    }
}
#endif
