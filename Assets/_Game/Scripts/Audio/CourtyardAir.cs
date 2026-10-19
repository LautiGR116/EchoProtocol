// Staged candidate only. One present-tense exterior source; never Echo audio.
using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Audio
{
    [DisallowMultipleComponent]
    public sealed class CourtyardAir : MonoBehaviour
    {
        [SerializeField] private GameSession session;
        [SerializeField] private Transform listener;
        [SerializeField] private AudioSource exteriorSource;
        [Tooltip("Two overlapping authored trigger boxes containing only the open courtyard.")]
        [SerializeField] private BoxCollider[] exteriorRegions = new BoxCollider[0];
        [Tooltip("Real walls, floors, roofs and closed glazing only. Exclude furniture, Player, Echo and triggers.")]
        [SerializeField] private Collider[] architecturalPartitions = new Collider[0];
        [SerializeField, Min(.01f)] private float boundaryFadeMetres = .60f;
        [SerializeField, Min(.01f)] private float transitionSeconds = .20f;

        private bool ready;
        private float authoredGain;
        public float RegionGain { get; private set; }
        public bool DirectPathBlocked { get; private set; }
        public float AppliedGain => exteriorSource ? exteriorSource.volume : 0f;

        private void Awake()
        {
            ready = session && listener && exteriorSource && exteriorSource.clip
                && exteriorSource.isActiveAndEnabled && exteriorSource.loop
                && !exteriorSource.playOnAwake && !exteriorSource.ignoreListenerPause
                && !exteriorSource.ignoreListenerVolume && !exteriorSource.bypassListenerEffects
                && exteriorSource.outputAudioMixerGroup && exteriorSource.spatialBlend >= .999f
                && exteriorRegions != null && exteriorRegions.Length == 2
                && exteriorRegions[0] && exteriorRegions[1]
                && exteriorRegions[0].isTrigger && exteriorRegions[1].isTrigger
                && architecturalPartitions != null && architecturalPartitions.Length > 0;
            if (!ready)
            {
                Debug.LogError("CourtyardAir requires one dedicated looping 3D source through the existing mixer/listener, session, two exterior boxes and explicit real partitions.", this);
                enabled = false;
                return;
            }
            // Capture the authored source gain once; there must be no other volume owner.
            authoredGain = exteriorSource.volume;
        }

        private void OnEnable()
        {
            if (!ready) return;
            exteriorSource.volume = 0f;
            exteriorSource.Play();
        }

        private void LateUpdate()
        {
            if (!ready || session.IsPaused || AudioListener.pause || Time.timeScale <= 0f) return;
            RegionGain = Mathf.Max(RegionWeight(exteriorRegions[0]), RegionWeight(exteriorRegions[1]));
            DirectPathBlocked = RegionGain > 0f && Obstructed();
            float target = DirectPathBlocked ? 0f : authoredGain * RegionGain;
            // At most .20 s of residual fade, never a sustained sound through a wall.
            exteriorSource.volume = Mathf.MoveTowards(exteriorSource.volume, target,
                authoredGain * Time.deltaTime / transitionSeconds);
        }

        private float RegionWeight(BoxCollider region)
        {
            if (!region.enabled || !region.gameObject.activeInHierarchy) return 0f;
            Vector3 p = region.transform.InverseTransformPoint(listener.position) - region.center;
            Vector3 half = region.size * .5f;
            if (Mathf.Abs(p.x) > half.x || Mathf.Abs(p.y) > half.y || Mathf.Abs(p.z) > half.z) return 0f;
            // Fade at physical planar limits. Height only rejects other floors.
            Vector3 scale = region.transform.lossyScale;
            float edge = Mathf.Min((half.x - Mathf.Abs(p.x)) * Mathf.Abs(scale.x),
                (half.z - Mathf.Abs(p.z)) * Mathf.Abs(scale.z));
            return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(edge / boundaryFadeMetres));
        }

        private bool Obstructed()
        {
            Vector3 offset = exteriorSource.transform.position - listener.position;
            float distance = offset.magnitude;
            if (distance <= .03f) return false;
            Vector3 direction = offset / distance;
            var ray = new Ray(listener.position + direction * .015f, direction);
            foreach (var partition in architecturalPartitions)
                if (partition && partition.enabled && !partition.isTrigger
                    && partition.gameObject.activeInHierarchy
                    && partition.Raycast(ray, out _, distance - .03f)) return true;
            return false;
        }

        private void OnDisable()
        {
            if (!ready || !exteriorSource) return;
            exteriorSource.volume = 0f;
            exteriorSource.Stop();
        }

        private void OnValidate()
        {
            boundaryFadeMetres = Mathf.Max(.01f, boundaryFadeMetres);
            transitionSeconds = Mathf.Max(.01f, transitionSeconds);
        }
    }
}
