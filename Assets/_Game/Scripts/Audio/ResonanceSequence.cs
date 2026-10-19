using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Audio
{
    // Authored chapter consequences; no RNG, scare timer, puzzle mutation or
    // recording access. Live state edges and actual proximity own the sequence.
    [DefaultExecutionOrder(300)]
    [DisallowMultipleComponent]
    public sealed class ResonanceSequence : MonoBehaviour
    {
        [SerializeField] private GameSession session;
        [SerializeField] private Transform listener;
        [SerializeField] private CampaignDirector campaign;
        [SerializeField] private LoopController loop;
        [SerializeField] private VS1ParallelFigureEvent passage;
        [SerializeField] private DomesticPresence presentPerson;
        [SerializeField] private RecombinationRelay transfer;
        [SerializeField] private PressureIsolation isolation;
        [SerializeField] private MachineCoverEncounter machineCover;
        [SerializeField] private HouseMemory house;
        [SerializeField] private HouseTelephone telephone;
        [SerializeField] private FinalDoorMemory review;
        [SerializeField] private ResonanceEmitter transferStrain, archiveDrag, pipeContraction, shaftCry;
        [SerializeField] private ResonanceEmitter phoneLine, recognitionScore, releaseScore, serviceAir;
        [SerializeField] private Transform archiveFrom, archiveTo;
        [SerializeField] private BoxCollider archiveRegion;
        private bool initialized, passageDone, transferDone, archiveDone, pressureDone, callDone, finalDone, keyDone;
        private bool previousTransfer, previousDrained, previousAnswered;
        private int restoreRevision;
        private float transferAt = -1, drainAt = -1, callAt = -1, archiveAt = -1;
        private float recallQuietUntil;
        public string LastCue { get; private set; } = "normality";
        public int CueCount { get; private set; }
        public int RecallCount { get; private set; }
        public bool BaselineEstablished => initialized;
        public bool CallCommitted => callDone;
        public bool PressureCommitted => pressureDone;

        private void Awake()
        {
            if (!session || !listener || !campaign || !loop || !passage || !transfer || !isolation || !house || !telephone || !review
                || !transferStrain || !archiveDrag || !pipeContraction || !shaftCry || !phoneLine || !recognitionScore || !releaseScore || !serviceAir
                || !archiveFrom || !archiveTo || !archiveRegion
                || !loop.gameObject.activeInHierarchy || !passage.gameObject.activeInHierarchy)
            { Debug.LogError("ResonanceSequence has incomplete authored campaign references.", this); enabled = false; }
        }
        private void OnEnable() { if (loop) loop.LoopCompleted += OnRecall; }
        private void OnDisable()
        {
            if (loop) loop.LoopCompleted -= OnRecall;
            foreach (var cue in new[] { transferStrain, archiveDrag, pipeContraction, shaftCry, phoneLine, recognitionScore, releaseScore, serviceAir }) if (cue) cue.StopNow();
        }
        private void OnRecall()
        {
            RecallCount++; recallQuietUntil = Time.time + .85f;
            // Leave a clear gap for the actual RECALL completion cue. Past dramatic
            // beats stay spent; resetting a puzzle cannot farm a loud encounter.
            transferStrain.FadeOut(); archiveDrag.FadeOut(); recognitionScore.FadeOut();
            transferAt = -1;
        }
        private void EstablishBaseline()
        {
            initialized = true; restoreRevision = isolation.RestoreRevision;
            previousTransfer = transfer.IsSecondLinkAccepted; previousDrained = isolation.IsDrained; previousAnswered = house.HasAnswered;
            bool later = campaign.Chapter != CampaignChapter.Protocol;
            passageDone = later || passage.PresentationCommitted;
            transferDone = later || transfer.IsSecondLinkAccepted;
            archiveDone = later || transfer.IsTransferComplete;
            pressureDone = isolation.IsLatched || campaign.Chapter >= CampaignChapter.House;
            callDone = house.HasAnswered || campaign.Chapter >= CampaignChapter.Archive;
            finalDone = review.ReviewedRecording != null;
            keyDone = review.KeyReturned;
            serviceAir.StopNow();
        }
        private void Cue(ResonanceEmitter emitter, string name)
        { emitter.Play(); LastCue = name; CueCount++; }
        private bool InArchive()
        { return archiveRegion.enabled && new Bounds(archiveRegion.center, archiveRegion.size).Contains(archiveRegion.transform.InverseTransformPoint(listener.position)); }
        private void LateUpdate()
        {
            if (!session.HasStarted || session.IsPaused || Time.deltaTime <= 0) return;
            if (!initialized) { EstablishBaseline(); return; }
            if (isolation.RestoreRevision != restoreRevision)
            {
                restoreRevision = isolation.RestoreRevision; previousDrained = isolation.IsDrained;
                pressureDone |= isolation.IsLatched; drainAt = -1; pipeContraction.StopNow(); shaftCry.StopNow();
            }
            // Physical pipe air is tied to the plant, not the player's head. It
            // drains with real pressure and never returns after loading a shut plant.
            float airLevel = machineCover && (machineCover.Active || machineCover.Complete) ? 0 : isolation.IsDrained ? 0 : isolation.MotorLevel01 * .8f;
            if (airLevel > 0 && !serviceAir.IsPlaying) serviceAir.Play();
            serviceAir.SetLevel(airLevel);
            if (!passageDone && passage.PresentationCommitted && passage.TraversalEntryCount > 0)
            {
                passageDone = true;
                if (!presentPerson) Cue(recognitionScore, "SE2 / recognition, internal score");
            }
            if (!previousTransfer && transfer.IsSecondLinkAccepted && !transferDone)
            { transferAt = Time.time + 1.35f; }
            previousTransfer = transfer.IsSecondLinkAccepted;
            if (transferAt >= 0 && !transfer.IsSecondLinkAccepted) transferAt = -1;
            if (transferAt >= 0 && Time.time >= transferAt && Time.time >= recallQuietUntil)
            { transferAt = -1; transferDone = true; Cue(transferStrain, "P3 / service wall strain after transfer"); }
            if (!archiveDone && transfer.IsTransferComplete && InArchive())
            {
                archiveDone = true; archiveAt = Time.time; archiveDrag.transform.position = archiveFrom.position;
                Cue(archiveDrag, "records return / drag beyond partition");
            }
            if (archiveAt >= 0 && archiveDrag.IsPlaying)
                archiveDrag.transform.position = Vector3.Lerp(archiveFrom.position, archiveTo.position, Mathf.SmoothStep(0, 1, (Time.time - archiveAt) / 6));
            if (!previousDrained && isolation.IsDrained && !pressureDone)
            { pressureDone = true; drainAt = Time.time; Cue(pipeContraction, "shutdown / pressure leaves the pipes"); }
            previousDrained = isolation.IsDrained;
            // The only full cry is distant, after the plant has fallen silent.
            // It lives in a fixed inaccessible shaft, never in a recorded body.
            if (drainAt >= 0 && Time.time - drainAt >= 5.6f)
            { drainAt = -1; Cue(shaftCry, "shutdown / distant adult cry in shaft"); }
            if (!previousAnswered && house.HasAnswered && !callDone)
            { callDone = true; callAt = Time.time + .8f; }
            previousAnswered = house.HasAnswered;
            if (callAt >= 0 && Time.time >= callAt)
            { callAt = -1; if (telephone.IsOffHook && house.Active) Cue(phoneLine, "house / live receiver line"); }
            // Hanging up is a meaningful way to stop listening. Lifting again
            // keeps a dead line; the original performance is never looped.
            if (!telephone.IsOffHook || !house.Active) phoneLine.FadeOut();
            if (!finalDone && review.Active && (review.UsesArrivalPresentation ? review.WitnessProgress > 0 : review.ReviewedRecording != null) && Time.time >= recallQuietUntil)
            { finalDone = true; Cue(recognitionScore, "6B / same internal recognition motif"); recognitionScore.SetLevel(.67f); }
            if (!keyDone && review.KeyReturned)
            {
                keyDone = true; recognitionScore.FadeOut(); Cue(releaseScore, "key returned / release");
                transferStrain.FadeOut(); archiveDrag.FadeOut(); pipeContraction.FadeOut(); shaftCry.FadeOut();
            }
        }
    }
}
