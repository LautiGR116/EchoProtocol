using EchoProtocol.Echo;
using EchoProtocol.Puzzle;
using UnityEngine;
using UnityEngine.Serialization;

namespace EchoProtocol.Horror
{
    public enum VS1StrongEventOnePhase
    {
        AwaitingRelay,
        AwaitingEchoEnd,
        AwaitingLaneCheck,
        AwaitingContinuation,
        LaneSequenceAdvancing,
        RevealingConsequence,
        Recovery
    }

    [DisallowMultipleComponent]
    public sealed class VS1StrongEventOne : MonoBehaviour
    {
        [Header("Trust Contract")]
        [SerializeField] private ParallelObservationRelay relay;
        [SerializeField] private LoopController loopController;
        [SerializeField] private Transform player;

        [Header("Authored Player Beats")]
        [SerializeField] private Transform knockTrigger;
        [SerializeField] private Transform laneCheckTarget;
        [SerializeField, Min(0.5f)] private float knockTriggerRadius = 7f;
        [SerializeField, Range(-1f, 1f)] private float checkAlignment = 0.55f;
        [SerializeField, Min(0f)] private float checkQualificationSeconds = 0.35f;
        [SerializeField, Min(0f)] private float postRelayGraceSeconds = 1.25f;

        [Header("Domestic Source")]
        [SerializeField] private AudioSource knockSource;
        [SerializeField, Range(0f, 1f)] private float eventSourceVolume = 1f;

        [Header("Occupied Lane Contraction")]
        [FormerlySerializedAs("footstepsSource")]
        [SerializeField] private AudioSource laneRelaySource;
        [SerializeField] private Light[] laneSequenceLights;
        [SerializeField] private Renderer[] laneSequencePanels;
        [SerializeField] private Light[] serviceSequenceLights;
        [SerializeField] private Renderer[] serviceSequencePanels;
        [SerializeField] private Light[] facilityBlackoutLights;
        [SerializeField] private Renderer[] facilityBlackoutPanels;
        [SerializeField, Min(0.1f)] private float laneStageInterval = 0.82f;
        [SerializeField, Range(0f, 0.25f)] private float lanePitchStep = 0.07f;

        [Header("Contrast")]
        [SerializeField] private AudioSource[] ambienceSources;
        [SerializeField] private EchoProtocol.Audio.EncounterAtmosphere encounterAtmosphere;
        [SerializeField, Range(0f, 1f)] private float laneAmbienceMultiplier = 0.38f;
        [SerializeField, Min(0.01f)] private float ambienceDuckSeconds = 0.2f;
        [SerializeField, Min(0.01f)] private float ambienceRecoverySeconds = 1.5f;
        [Tooltip("Optional: ties this bed to the actual facilityBlackoutLights. Unassigned keeps the legacy shared mix.")]
        [SerializeField] private AudioSource fluorescentCircuitSource;
        [SerializeField, Min(0.01f)] private float fluorescentFadeSeconds = 0.18f;

        [Header("Lighting Contrast")]
        [SerializeField, Range(0f, 1f)]
        private float ambientLightFarStageMultiplier = 0.35f;
        [SerializeField, Range(0f, 1f)]
        private float ambientLightMidStageMultiplier = 0.06f;
        [SerializeField, Range(0f, 1f)]
        private float ambientLightConsequenceMultiplier = 0.0015f;
        [SerializeField, Min(0.05f)] private float ambientLightFadeSeconds = 0.18f;

        [Header("Persistent Visual Consequence")]
        [SerializeField] private Light domesticWarmLight;
        [SerializeField] private Light thresholdSpill;
        [SerializeField] private VS1DomesticDoorReveal domesticDoorReveal;
        [SerializeField, Range(0f, 1f)] private float initialWarmMultiplier = 0.55f;
        [SerializeField, Range(0f, 1f)] private float initialSpillMultiplier = 0.15f;
        [SerializeField, Min(0f)] private float consequenceDelaySeconds = 0.75f;

        private float relayCompletedTime;
        private float checkFacingDuration;
        private float laneStageElapsed;
        private float consequenceElapsed;
        private int laneStageIndex;
        private float ambienceMultiplier = 1f;
        private float ambienceTarget = 1f;
        private float ambientLightMultiplier = 1f;
        private float ambientLightTarget = 1f;
        private Color ambientLightBaseColor;
        private float ambientIntensityBase;
        private float reflectionIntensityBase;
        private float warmTargetIntensity;
        private float spillTargetIntensity;
        private float[] ambienceBaseVolumes;
        private float fluorescentBaseVolume, fluorescentPower = 1f;
        private float[] facilityLightBaseIntensities;
        private Color[] facilityPanelBaseEmissions;

        public VS1StrongEventOnePhase Phase { get; private set; } =
            VS1StrongEventOnePhase.AwaitingRelay;

        public int KnockPlayCount { get; private set; }

        public int LaneRelayPlayCount { get; private set; }

        public int LaneStagesExtinguished { get; private set; }

        public int PendingSequenceResetCount { get; private set; }

        public bool PlayerCheckRegistered { get; private set; }

        public bool LaneSequenceStartedAfterEchoEnded { get; private set; }

        public bool VisualConsequenceApplied { get; private set; }

        public float AmbienceMultiplier => ambienceMultiplier;

        public float MinimumAmbienceMultiplierObserved { get; private set; } = 1f;

        public float AmbientLightMultiplier => ambientLightMultiplier;

        public AudioSource FluorescentCircuitSource => fluorescentCircuitSource;
        public bool AmbienceControlReleased { get; private set; }
        public int AmbienceReleaseCount { get; private set; }
        public int AmbienceWriteCount { get; private set; }
        public float FluorescentPowerFraction
        {
            get
            {
                if (facilityLightBaseIntensities == null) return 1f;
                float powered = 0f; int count = 0;
                for (int i = 0; i < facilityBlackoutLights.Length; i++)
                {
                    float baseline = facilityLightBaseIntensities[i];
                    if (baseline <= Mathf.Epsilon) continue;
                    count++;
                    var light = facilityBlackoutLights[i];
                    if (light && light.enabled && light.gameObject.activeInHierarchy)
                        powered += Mathf.Clamp01(light.intensity / baseline);
                }
                return count > 0 ? powered / count : 0f;
            }
        }

        // One-way ownership transfer. Later RECALLs, returns and disable/enable
        // cycles cannot make the first act overwrite the campaign mix again.
        public void ReleaseAmbienceControl()
        {
            if (AmbienceControlReleased) return;
            AmbienceControlReleased = true;
            AmbienceReleaseCount++;
        }

        // A later chapter proves that this consequence already happened. Restore
        // its settled lighting, without replaying the knock, relay or door reveal.
        public bool RestoreCompletedLighting()
        {
            if (facilityLightBaseIntensities == null) return false;
            knockSource.Stop();
            laneRelaySource.Stop();
            ReleaseAmbienceControl();
            if (fluorescentCircuitSource) fluorescentCircuitSource.Stop();
            ExtinguishFacilityLightingThrough(float.PositiveInfinity);
            ambientLightMultiplier = ambientLightTarget = ambientLightConsequenceMultiplier;
            UpdateAmbientLighting();
            domesticWarmLight.intensity = warmTargetIntensity;
            thresholdSpill.intensity = spillTargetIntensity;
            VisualConsequenceApplied = true;
            Phase = VS1StrongEventOnePhase.Recovery;
            return true;
        }

        private void Awake()
        {
            if (relay == null
                || loopController == null
                || player == null
                || knockTrigger == null
                || laneCheckTarget == null
                || knockSource == null
                || laneRelaySource == null
                || knockSource.clip == null
                || laneRelaySource.clip == null
                || !HasThreeDistinctReferences(laneSequenceLights)
                || !HasThreeDistinctReferences(laneSequencePanels)
                || !HasThreeDistinctReferences(serviceSequenceLights)
                || !HasThreeDistinctReferences(serviceSequencePanels)
                || !HasDistinctReferences(facilityBlackoutLights, 12)
                || !HasDistinctReferences(facilityBlackoutPanels, 12)
                || domesticWarmLight == null
                || thresholdSpill == null
                || domesticDoorReveal == null)
            {
                Debug.LogError(
                    "VS1StrongEventOne requires explicit trust, trigger, domestic source, three lane stages, and consequence references.",
                    this);
                enabled = false;
                return;
            }

            ConfigureWorldSource(knockSource);
            ConfigureWorldSource(laneRelaySource);
            laneRelaySource.minDistance = 3f;
            laneRelaySource.maxDistance = 18f;
            laneRelaySource.pitch = 1f;
            ambientLightBaseColor = RenderSettings.ambientLight;
            ambientIntensityBase = RenderSettings.ambientIntensity;
            reflectionIntensityBase = RenderSettings.reflectionIntensity;

            facilityLightBaseIntensities =
                new float[facilityBlackoutLights.Length];
            facilityPanelBaseEmissions =
                new Color[facilityBlackoutPanels.Length];
            for (int index = 0; index < facilityBlackoutLights.Length; index++)
            {
                facilityLightBaseIntensities[index] =
                    facilityBlackoutLights[index].intensity;
                facilityPanelBaseEmissions[index] = GetEmissionColor(
                    facilityBlackoutPanels[index]);
            }

            warmTargetIntensity = domesticWarmLight.intensity;
            spillTargetIntensity = thresholdSpill.intensity;
            domesticWarmLight.intensity = warmTargetIntensity * initialWarmMultiplier;
            thresholdSpill.intensity = spillTargetIntensity * initialSpillMultiplier;

            ambienceSources ??= System.Array.Empty<AudioSource>();
            ambienceBaseVolumes = new float[ambienceSources.Length];
            for (int index = 0; index < ambienceSources.Length; index++)
            {
                ambienceBaseVolumes[index] = ambienceSources[index] == null
                    ? 0f
                    : ambienceSources[index].volume;
            }
            if (fluorescentCircuitSource) fluorescentBaseVolume = fluorescentCircuitSource.volume;
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            UpdateAmbience();
            UpdateAmbientLighting();

            if (TryResetPendingEventAfterRelayReset())
            {
                return;
            }

            switch (Phase)
            {
                case VS1StrongEventOnePhase.AwaitingRelay:
                    if (relay.IsComplete)
                    {
                        relayCompletedTime = Time.time;
                        Phase = VS1StrongEventOnePhase.AwaitingEchoEnd;
                    }
                    break;

                case VS1StrongEventOnePhase.AwaitingEchoEnd:
                    if (IsEchoFinished()
                        && Time.time - relayCompletedTime >= postRelayGraceSeconds
                        && IsAtTrigger(knockTrigger, knockTriggerRadius))
                    {
                        DeliverKnock();
                    }
                    break;

                case VS1StrongEventOnePhase.AwaitingLaneCheck:
                    UpdateLaneCheck();
                    break;

                case VS1StrongEventOnePhase.AwaitingContinuation:
                    if (!knockSource.isPlaying)
                    {
                        DeliverLaneSequence();
                    }
                    break;

                case VS1StrongEventOnePhase.LaneSequenceAdvancing:
                    UpdateLaneSequence();
                    break;

                case VS1StrongEventOnePhase.RevealingConsequence:
                    UpdateConsequence();
                    break;
            }
        }

        private bool TryResetPendingEventAfterRelayReset()
        {
            bool isPendingBeforeSequence =
                Phase == VS1StrongEventOnePhase.AwaitingEchoEnd
                || Phase == VS1StrongEventOnePhase.AwaitingLaneCheck
                || Phase == VS1StrongEventOnePhase.AwaitingContinuation;
            if (!isPendingBeforeSequence || relay.IsComplete)
            {
                return false;
            }

            knockSource.Stop();
            laneRelaySource.Stop();
            relayCompletedTime = 0f;
            checkFacingDuration = 0f;
            laneStageElapsed = 0f;
            consequenceElapsed = 0f;
            laneStageIndex = 0;
            ambienceTarget = 1f;
            ambientLightTarget = 1f;
            PlayerCheckRegistered = false;
            Phase = VS1StrongEventOnePhase.AwaitingRelay;
            PendingSequenceResetCount++;
            return true;
        }

        private void DeliverKnock()
        {
            knockSource.Stop();
            knockSource.volume = eventSourceVolume;
            knockSource.Play();
            KnockPlayCount++;
            Phase = VS1StrongEventOnePhase.AwaitingLaneCheck;
        }

        private void UpdateLaneCheck()
        {
            Vector3 toTarget = laneCheckTarget.position - player.position;
            toTarget.y = 0f;
            Vector3 playerForward = player.forward;
            playerForward.y = 0f;

            bool isFacing = toTarget.sqrMagnitude > Mathf.Epsilon
                && playerForward.sqrMagnitude > Mathf.Epsilon
                && Vector3.Dot(playerForward.normalized, toTarget.normalized)
                    >= checkAlignment;

            checkFacingDuration = isFacing
                ? checkFacingDuration + Time.deltaTime
                : 0f;

            if (checkFacingDuration < checkQualificationSeconds)
            {
                return;
            }

            PlayerCheckRegistered = true;
            Phase = VS1StrongEventOnePhase.AwaitingContinuation;
        }

        private void DeliverLaneSequence()
        {
            LaneSequenceStartedAfterEchoEnded = IsEchoFinished();
            if (!LaneSequenceStartedAfterEchoEnded)
            {
                return;
            }

            laneStageElapsed = 0f;
            laneStageIndex = 0;
            ambienceTarget = laneAmbienceMultiplier;
            Phase = VS1StrongEventOnePhase.LaneSequenceAdvancing;
            ExtinguishLaneStage(laneStageIndex);
        }

        private void UpdateLaneSequence()
        {
            laneStageElapsed += Time.deltaTime;
            if (laneStageElapsed < laneStageInterval)
            {
                return;
            }

            laneStageElapsed = 0f;
            laneStageIndex++;
            if (laneStageIndex < laneSequenceLights.Length)
            {
                ExtinguishLaneStage(laneStageIndex);
                return;
            }

            ambienceTarget = 1f;
            consequenceElapsed = 0f;
            Phase = VS1StrongEventOnePhase.RevealingConsequence;
        }

        private void ExtinguishLaneStage(int index)
        {
            laneRelaySource.transform.position =
                laneSequenceLights[index].transform.position + Vector3.down * 0.2f;
            laneRelaySource.Stop();
            laneRelaySource.pitch = 1f + lanePitchStep * index;
            laneRelaySource.volume = eventSourceVolume;
            laneRelaySource.Play();

            ambientLightTarget = GetAmbientLightTarget(index);
            ExtinguishFacilityLightingThrough(
                laneSequenceLights[index].transform.position.z);
            LaneRelayPlayCount++;
            LaneStagesExtinguished++;
        }

        private float GetAmbientLightTarget(int stageIndex)
        {
            if (stageIndex <= 0)
            {
                return ambientLightFarStageMultiplier;
            }

            return stageIndex == 1
                ? ambientLightMidStageMultiplier
                : ambientLightConsequenceMultiplier;
        }

        private void ExtinguishFacilityLightingThrough(float stageZ)
        {
            for (int index = 0; index < facilityBlackoutLights.Length; index++)
            {
                if (facilityBlackoutLights[index].transform.position.z
                    > stageZ + 0.01f)
                {
                    continue;
                }

                facilityBlackoutLights[index].intensity = 0f;
                SetEmissionColor(facilityBlackoutPanels[index], Color.black);
            }
        }

        private void UpdateConsequence()
        {
            consequenceElapsed += Time.deltaTime;
            if (consequenceElapsed < consequenceDelaySeconds)
            {
                return;
            }

            domesticDoorReveal.BeginOpen();
            float reveal = domesticDoorReveal.OpenProgress;
            domesticWarmLight.intensity = Mathf.Lerp(
                warmTargetIntensity * initialWarmMultiplier,
                warmTargetIntensity,
                reveal);
            thresholdSpill.intensity = Mathf.Lerp(
                spillTargetIntensity * initialSpillMultiplier,
                spillTargetIntensity,
                reveal);

            if (!domesticDoorReveal.IsFullyOpen)
            {
                return;
            }

            VisualConsequenceApplied = true;
            Phase = VS1StrongEventOnePhase.Recovery;
        }

        private void UpdateAmbience()
        {
            if (AmbienceControlReleased) return;
            float duration = ambienceTarget < ambienceMultiplier
                ? ambienceDuckSeconds
                : ambienceRecoverySeconds;
            ambienceMultiplier = Mathf.MoveTowards(
                ambienceMultiplier,
                ambienceTarget,
                Time.deltaTime / duration);
            MinimumAmbienceMultiplierObserved = Mathf.Min(
                MinimumAmbienceMultiplierObserved,
                ambienceMultiplier);

            for (int index = 0; index < ambienceSources.Length; index++)
            {
                if (ambienceSources[index] != null && ambienceSources[index] != fluorescentCircuitSource)
                {
                    ambienceSources[index].volume =
                        ambienceBaseVolumes[index] * ambienceMultiplier * (encounterAtmosphere && encounterAtmosphere.isActiveAndEnabled ? encounterAtmosphere.InstitutionGain : 1f);
                }
            }
            if (fluorescentCircuitSource)
            {
                fluorescentPower = Mathf.MoveTowards(fluorescentPower, FluorescentPowerFraction,
                    Time.deltaTime / fluorescentFadeSeconds);
                fluorescentCircuitSource.volume = fluorescentBaseVolume * fluorescentPower * ambienceMultiplier * (encounterAtmosphere && encounterAtmosphere.isActiveAndEnabled ? encounterAtmosphere.InstitutionGain : 1f);
            }
            AmbienceWriteCount++;
        }

        private void UpdateAmbientLighting()
        {
            ambientLightMultiplier = Mathf.MoveTowards(
                ambientLightMultiplier,
                ambientLightTarget,
                Time.deltaTime / ambientLightFadeSeconds);
            Color targetColor = ambientLightBaseColor * ambientLightMultiplier;
            targetColor.a = ambientLightBaseColor.a;
            RenderSettings.ambientLight = targetColor;
            RenderSettings.ambientIntensity =
                ambientIntensityBase * ambientLightMultiplier;
            RenderSettings.reflectionIntensity =
                reflectionIntensityBase * ambientLightMultiplier;
        }

        private bool IsAtTrigger(Transform trigger, float radius)
        {
            Vector3 offset = player.position - trigger.position;
            offset.y = 0f;
            return offset.sqrMagnitude <= radius * radius;
        }

        private bool IsEchoFinished()
        {
            EchoPlayback activeEcho = loopController.ActiveEcho;
            return activeEcho == null
                || activeEcho.IsComplete
                || !activeEcho.gameObject.activeInHierarchy;
        }

        private void ConfigureWorldSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 1f;
            source.dopplerLevel = 0f;
            source.priority = 145;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 6f;
            source.maxDistance = 22f;
            source.volume = eventSourceVolume;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (knockSource != null)
            {
                knockSource.Stop();
            }

            if (laneRelaySource != null)
            {
                laneRelaySource.Stop();
                laneRelaySource.pitch = 1f;
            }

            if (!AmbienceControlReleased && ambienceSources != null && ambienceBaseVolumes != null)
            {
                for (int index = 0; index < ambienceSources.Length; index++)
                {
                    if (ambienceSources[index] != null)
                    {
                        ambienceSources[index].volume = ambienceBaseVolumes[index];
                    }
                }
                if (fluorescentCircuitSource) fluorescentCircuitSource.volume = fluorescentBaseVolume;
                AmbienceWriteCount++;
            }

            if (domesticWarmLight != null)
            {
                domesticWarmLight.intensity = warmTargetIntensity;
            }

            if (thresholdSpill != null)
            {
                thresholdSpill.intensity = spillTargetIntensity;
            }

            RenderSettings.ambientLight = ambientLightBaseColor;
            RenderSettings.ambientIntensity = ambientIntensityBase;
            RenderSettings.reflectionIntensity = reflectionIntensityBase;

            if (facilityBlackoutLights != null
                && facilityLightBaseIntensities != null)
            {
                for (int index = 0; index < facilityBlackoutLights.Length; index++)
                {
                    if (facilityBlackoutLights[index] != null)
                    {
                        facilityBlackoutLights[index].intensity =
                            facilityLightBaseIntensities[index];
                    }
                }
            }

            if (facilityBlackoutPanels != null
                && facilityPanelBaseEmissions != null)
            {
                for (int index = 0; index < facilityBlackoutPanels.Length; index++)
                {
                    if (facilityBlackoutPanels[index] != null)
                    {
                        SetEmissionColor(
                            facilityBlackoutPanels[index],
                            facilityPanelBaseEmissions[index]);
                    }
                }
            }
        }

        private void OnValidate()
        {
            knockTriggerRadius = Mathf.Max(0.5f, knockTriggerRadius);
            checkQualificationSeconds = Mathf.Max(0f, checkQualificationSeconds);
            postRelayGraceSeconds = Mathf.Max(0f, postRelayGraceSeconds);
            laneStageInterval = Mathf.Max(0.1f, laneStageInterval);
            lanePitchStep = Mathf.Clamp(lanePitchStep, 0f, 0.25f);
            eventSourceVolume = Mathf.Clamp01(eventSourceVolume);
            ambienceDuckSeconds = Mathf.Max(0.01f, ambienceDuckSeconds);
            ambienceRecoverySeconds = Mathf.Max(0.01f, ambienceRecoverySeconds);
            fluorescentFadeSeconds = Mathf.Max(0.01f, fluorescentFadeSeconds);
            ambientLightConsequenceMultiplier = Mathf.Clamp01(
                ambientLightConsequenceMultiplier);
            ambientLightFarStageMultiplier = Mathf.Clamp(
                ambientLightFarStageMultiplier,
                ambientLightConsequenceMultiplier,
                1f);
            ambientLightMidStageMultiplier = Mathf.Clamp(
                ambientLightMidStageMultiplier,
                ambientLightConsequenceMultiplier,
                ambientLightFarStageMultiplier);
            ambientLightFadeSeconds = Mathf.Max(0.05f, ambientLightFadeSeconds);
            consequenceDelaySeconds = Mathf.Max(0f, consequenceDelaySeconds);
        }

        private static bool HasThreeDistinctReferences<T>(T[] references)
            where T : Object
        {
            return HasDistinctReferences(references, 3);
        }

        private static bool HasDistinctReferences<T>(
            T[] references,
            int requiredCount)
            where T : Object
        {
            if (references == null || references.Length != requiredCount)
            {
                return false;
            }

            for (int index = 0; index < references.Length; index++)
            {
                if (references[index] == null)
                {
                    return false;
                }

                for (int comparison = index + 1;
                     comparison < references.Length;
                     comparison++)
                {
                    if (references[index] == references[comparison])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static Color GetEmissionColor(Renderer target)
        {
            return target.sharedMaterial != null
                && target.sharedMaterial.HasProperty("_EmissionColor")
                    ? target.sharedMaterial.GetColor("_EmissionColor")
                    : Color.black;
        }

        private static void SetEmissionColor(Renderer target, Color color)
        {
            MaterialPropertyBlock properties = new MaterialPropertyBlock();
            target.GetPropertyBlock(properties);
            properties.SetColor("_EmissionColor", color);
            target.SetPropertyBlock(properties);
        }
    }
}
