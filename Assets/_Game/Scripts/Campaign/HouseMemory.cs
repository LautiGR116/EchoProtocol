using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;

namespace EchoProtocol.Campaign
{
    public enum HousePowerLocation { Hall, Kitchen, Bedroom, Carried }

    /// <summary>
    /// A single movable reserve, a physical key, and an independent domestic encounter.
    /// No light, interaction, voice or door action is added to the Echo recording.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HouseMemory : MonoBehaviour
    {
        [Header("Chapter ownership")]
        [SerializeField] private GameSession session;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private Camera view;
        [SerializeField] private HouseSwingDoor exitDoor;

        [Header("General supply is removed by answering; emergency light is separate")]
        [SerializeField] private Light[] generalLights = new Light[0];
        [SerializeField] private Light[] hallLights = new Light[0];
        [SerializeField] private Light[] kitchenLights = new Light[0];
        [SerializeField] private Light[] bedroomLights = new Light[0];
        [Header("Exactly one reserve representation")]
        [SerializeField] private GameObject hallFuse;
        [SerializeField] private GameObject kitchenFuse;
        [SerializeField] private GameObject bedroomFuse;
        [SerializeField] private GameObject carriedFuse;
        [SerializeField] private GameObject keyWorld;

        [Header("Optional physical sound sources")]
        [SerializeField] private AudioSource telephoneAudio;
        [SerializeField] private AudioClip telephoneRing;
        [SerializeField] private AudioClip receiverClick;
        [SerializeField, Min(2f)] private float ringInterval = 8f;
        [SerializeField] private AudioSource objectAudio;
        [SerializeField] private AudioClip fuseRemoval;
        [SerializeField] private AudioClip fuseInsertion;
        [SerializeField] private AudioClip keyPickup;
        [SerializeField] private AudioClip lockedDoor;
        [SerializeField] private AudioSource kitchenApplianceAudio;
        [SerializeField] private AudioClip kitchenApplianceClip;

        [Header("Optional presence, physically contained behind glazing")]
        [SerializeField] private GameObject presenceRoot;
        [Tooltip("A separate authored trigger enclosing visible body and light spill. Its center must lie on the visible torso.")]
        [SerializeField] private BoxCollider presenceVisibilityVolume;
        [Tooltip("Colliders that permit sight, such as the sealed observation glass. Opaque walls are not included.")]
        [SerializeField] private Collider[] transparentSightColliders = new Collider[0];
        [SerializeField] private LayerMask sightMask = ~0;
        [SerializeField] private AudioSource presenceAudio;
        [SerializeField] private AudioClip presenceArrival;
        [SerializeField] private AudioClip presenceDeparture;

        private bool initialized, entered, presenceTriggered, presenceSeen, presenceRetired, presenceArrivalPlayed;
        private float ringAt;
        private readonly Plane[] frustum = new Plane[6];
        private readonly Vector3[] sightPoints = new Vector3[9];
        private readonly RaycastHit[] sightHits = new RaycastHit[64];

        public bool Active { get; private set; }
        public bool HasKey { get; private set; }
        public bool HasAnswered { get; private set; }
        public bool HasExited { get; private set; }
        public HousePowerLocation PowerLocation { get; private set; } = HousePowerLocation.Hall;
        public bool HallLightLeftOn { get { foreach (var light in hallLights) if (light && light.enabled && light.gameObject.activeInHierarchy) return true; return false; } }
        public bool PresenceSeen => presenceSeen;
        public bool PresenceRetired => presenceRetired;

        public string Objective => !HasKey
            ? (HasAnswered ? "Encontrá la llave de 6B. Podés trasladar la reserva de luz." : "El teléfono sigue sonando. Buscá la llave de 6B.")
            : HasExited ? "La puerta está abierta. Decidí dónde dejar la luz antes de salir."
            : "Usá la llave en la puerta. Mara pidió dejar encendido el recibidor.";
        public string StatusText => (HasAnswered ? "RESERVA: " + PlaceName(PowerLocation).ToUpperInvariant() : "SUMINISTRO GENERAL")
            + (HasKey ? "  ·  LLAVE DE 6B" : "");
        public string Prompt => PowerLocation == HousePowerLocation.Carried ? "Colocar reserva de luz"
            : HasKey ? "Abrir con la llave" : HasAnswered ? "Buscar la llave de 6B" : "Atender el teléfono";

        private void Awake()
        {
            if (!session || !player || !view || !exitDoor)
            {
                Debug.LogError("HouseMemory requires explicit GameSession, Player, Camera and HouseSwingDoor references.", this);
                enabled = false;
                return;
            }
            initialized = true;
            if (presenceRoot) presenceRoot.SetActive(false);
            if (presenceRoot && !presenceVisibilityVolume)
                Debug.LogWarning("HouseMemory presence is disabled until an explicit visibility volume is assigned.", this);
            if (telephoneAudio) { telephoneAudio.playOnAwake = false; telephoneAudio.Stop(); }
            if (kitchenApplianceAudio) { kitchenApplianceAudio.playOnAwake = false; kitchenApplianceAudio.Stop(); }
            ApplyFuseVisuals();
        }

        /// <summary>Called by the campaign only after the house chapter is committed.</summary>
        public void EnterHouse()
        {
            if (!initialized || !enabled) return;
            Active = true;
            ringAt = Time.time + 3f;
            ApplyApplianceSound();
            if (entered) return;
            entered = true;
            ApplyFuseVisuals();
        }

        /// <summary>Stops chapter interactions. The chosen light, key and open door remain physical consequences.</summary>
        public void LeaveHouse()
        {
            Active = false;
            if (telephoneAudio) telephoneAudio.Stop();
        }

        /// <summary>Restores earned domestic consequences from a final-review checkpoint, never an Echo take.</summary>
        public void RestoreAtReview(bool answered, HousePowerLocation power)
        {
            if (!initialized || !enabled) return;
            entered = true;
            Active = false;
            HasKey = HasExited = true;
            HasAnswered = answered;
            PowerLocation = answered ? power : HousePowerLocation.Hall;
            if (keyWorld) keyWorld.SetActive(false);
            if (presenceRoot) presenceRoot.SetActive(false);
            presenceTriggered = presenceRetired = true;
            if (telephoneAudio) telephoneAudio.Stop();
            exitDoor.Open();
            ApplyFuseVisuals();
            if (answered) ApplyReserveLighting();
            else ApplyApplianceSound();
        }

        public bool AllowsInteraction(GameObject who) => initialized && enabled && Active && !session.IsPaused && who == player.gameObject;

        public bool CanUse(HouseInteractionKind kind, GameObject who)
        {
            if (!AllowsInteraction(who)) return false;
            switch (kind)
            {
                case HouseInteractionKind.Telephone: return !HasAnswered;
                case HouseInteractionKind.Key: return !HasKey;
                case HouseInteractionKind.ExitDoor: return !HasExited;
                case HouseInteractionKind.Document: return true;
                default:
                    if (!TrySocket(kind, out var location)) return false;
                    return HasAnswered && (PowerLocation == location || PowerLocation == HousePowerLocation.Carried);
            }
        }

        public string GetPrompt(HouseInteractionKind kind)
        {
            if (kind == HouseInteractionKind.Telephone) return "Atender";
            if (kind == HouseInteractionKind.Key) return "Tomar la llave de 6B";
            if (kind == HouseInteractionKind.Document) return "Leer";
            if (kind == HouseInteractionKind.ExitDoor) return HasKey ? "Abrir con la llave" : "Probar la puerta";
            if (!TrySocket(kind, out var location)) return "";
            return PowerLocation == location ? "Retirar la reserva de luz" : "Colocar la reserva en " + PlaceName(location);
        }

        public void Use(HouseInteractionKind kind, GameObject who, Vector3 sourcePosition)
        {
            if (!CanUse(kind, who)) return;
            switch (kind)
            {
                case HouseInteractionKind.Telephone:
                    HasAnswered = true;
                    if (telephoneAudio)
                    {
                        telephoneAudio.Stop();
                        if (receiverClick) telephoneAudio.PlayOneShot(receiverClick);
                    }
                    ApplyReserveLighting();
                    break;
                case HouseInteractionKind.Key:
                    HasKey = true;
                    if (keyWorld) keyWorld.SetActive(false);
                    PlayObject(keyPickup, sourcePosition);
                    break;
                case HouseInteractionKind.ExitDoor:
                    if (!HasKey) { PlayObject(lockedDoor, sourcePosition); return; }
                    if (exitDoor.Open()) HasExited = true;
                    break;
                default:
                    if (!TrySocket(kind, out var location)) return;
                    bool removing = PowerLocation == location;
                    PowerLocation = removing ? HousePowerLocation.Carried : location;
                    ApplyFuseVisuals();
                    ApplyReserveLighting();
                    PlayObject(removing ? fuseRemoval : fuseInsertion, sourcePosition);
                    break;
            }
        }

        public void ReadDocument(GameObject who, string title, string body)
        {
            if (CanUse(HouseInteractionKind.Document, who)) session.ShowDocument(title, body);
        }

        private void Update()
        {
            if (!Active || session.IsPaused) return;
            if (!HasAnswered && telephoneAudio && telephoneRing && Time.time >= ringAt)
            {
                telephoneAudio.PlayOneShot(telephoneRing);
                ringAt = Time.time + Mathf.Max(ringInterval, telephoneRing.length + .25f);
            }
            UpdatePresence();
        }

        private void ApplyFuseVisuals()
        {
            if (hallFuse) hallFuse.SetActive(PowerLocation == HousePowerLocation.Hall);
            if (kitchenFuse) kitchenFuse.SetActive(PowerLocation == HousePowerLocation.Kitchen);
            if (bedroomFuse) bedroomFuse.SetActive(PowerLocation == HousePowerLocation.Bedroom);
            if (carriedFuse) carriedFuse.SetActive(PowerLocation == HousePowerLocation.Carried);
        }

        private void ApplyReserveLighting()
        {
            SetLights(generalLights, false);
            SetLights(hallLights, false);
            SetLights(kitchenLights, false);
            SetLights(bedroomLights, false);
            switch (PowerLocation)
            {
                case HousePowerLocation.Hall: SetLights(hallLights, true); break;
                case HousePowerLocation.Kitchen: SetLights(kitchenLights, true); break;
                case HousePowerLocation.Bedroom: SetLights(bedroomLights, true); break;
            }
            ApplyApplianceSound();
        }

        private void ApplyApplianceSound()
        {
            if (!kitchenApplianceAudio || !kitchenApplianceAudio.isActiveAndEnabled) return;
            bool powered = !HasAnswered || PowerLocation == HousePowerLocation.Kitchen;
            if (!powered) { kitchenApplianceAudio.Stop(); return; }
            if (!kitchenApplianceClip || kitchenApplianceAudio.isPlaying) return;
            kitchenApplianceAudio.clip = kitchenApplianceClip;
            kitchenApplianceAudio.loop = true;
            kitchenApplianceAudio.Play();
        }

        private static void SetLights(Light[] lights, bool value)
        {
            foreach (var light in lights) if (light) light.enabled = value;
        }

        private void PlayObject(AudioClip clip, Vector3 sourcePosition)
        {
            if (!objectAudio || !objectAudio.isActiveAndEnabled || !clip) return;
            objectAudio.Stop();
            objectAudio.transform.position = sourcePosition;
            objectAudio.PlayOneShot(clip);
        }

        private void UpdatePresence()
        {
            if (!HasKey || !presenceRoot || !presenceVisibilityVolume || presenceRetired) return;
            Bounds bounds = BuildSightPoints();
            GeometryUtility.CalculateFrustumPlanes(view, frustum);
            bool inFrame = GeometryUtility.TestPlanesAABB(frustum, bounds);

            if (!presenceTriggered)
            {
                // Conservative activation: even an occluded volume must leave the frame first.
                // Include any visible backlight/spill in the authored visibility volume.
                if (inFrame) return;
                presenceTriggered = true;
                presenceRoot.SetActive(true);

                return;
            }

            // The weight is heard at the return, not silently spent while the Player is distant in the bedroom.
            if(!presenceArrivalPlayed && presenceAudio && presenceAudio.isActiveAndEnabled && presenceArrival &&
                Vector3.Distance(view.transform.position,presenceAudio.transform.position)<5f)
            {presenceArrivalPlayed=true;presenceAudio.PlayOneShot(presenceArrival);}

            bool allOccluded = true;
            bool visible = false;
            for (int i = 0; i < sightPoints.Length; i++)
            {
                bool occluded = IsOpaqueBetween(sightPoints[i]);
                allOccluded &= occluded;
                // A visible empty corner of the safety volume is not evidence of seeing a body.
                if (i == 0 && !occluded && inFrame)
                {
                    Vector3 viewport = view.WorldToViewportPoint(sightPoints[i]);
                    if (viewport.z > view.nearClipPlane && viewport.z < view.farClipPlane &&
                        viewport.x >= 0 && viewport.x <= 1 && viewport.y >= 0 && viewport.y <= 1)
                        visible = true;
                }
            }

            if (visible) presenceSeen = true;
            // Turning away alone does not remove it. All rays require real opaque geometry.
            if (presenceSeen && allOccluded && !inFrame)
            {
                presenceRetired = true;
                presenceRoot.SetActive(false);
                if (presenceAudio && presenceAudio.isActiveAndEnabled && presenceDeparture) presenceAudio.PlayOneShot(presenceDeparture);
            }
        }

        private Bounds BuildSightPoints()
        {
            Transform t = presenceVisibilityVolume.transform;
            Vector3 c = presenceVisibilityVolume.center;
            Vector3 half = presenceVisibilityVolume.size * .5f;
            sightPoints[0] = t.TransformPoint(c);
            Bounds bounds = new Bounds(sightPoints[0], Vector3.zero);
            int index = 1;
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                    {
                        var point = t.TransformPoint(c + Vector3.Scale(half, new Vector3(x, y, z)));
                        sightPoints[index++] = point;
                        bounds.Encapsulate(point);
                    }
            return bounds;
        }

        private bool IsOpaqueBetween(Vector3 point)
        {
            Vector3 offset = point - view.transform.position;
            float distance = offset.magnitude;
            if (distance < .01f) return false;
            int count = Physics.RaycastNonAlloc(view.transform.position, offset / distance, sightHits,
                distance - .01f, sightMask, QueryTriggerInteraction.Ignore);
            // An overflow is uncertain visibility; keep the presence rather than hiding it.
            if (count >= sightHits.Length) return false;
            for (int i = 0; i < count; i++)
            {
                Collider collider = sightHits[i].collider;
                if (!collider || collider.transform.IsChildOf(player.transform) ||
                    collider.transform.IsChildOf(presenceRoot.transform) ||
                    collider == presenceVisibilityVolume || IsTransparent(collider)) continue;
                return true;
            }
            return false;
        }

        private bool IsTransparent(Collider collider)
        {
            foreach (var glass in transparentSightColliders) if (glass == collider) return true;
            return false;
        }

        private static bool TrySocket(HouseInteractionKind kind, out HousePowerLocation location)
        {
            location = HousePowerLocation.Hall;
            switch (kind)
            {
                case HouseInteractionKind.HallSocket: return true;
                case HouseInteractionKind.KitchenSocket: location = HousePowerLocation.Kitchen; return true;
                case HouseInteractionKind.BedroomSocket: location = HousePowerLocation.Bedroom; return true;
                default: return false;
            }
        }

        private static string PlaceName(HousePowerLocation location)
        {
            switch (location)
            {
                case HousePowerLocation.Hall: return "recibidor";
                case HousePowerLocation.Kitchen: return "cocina";
                case HousePowerLocation.Bedroom: return "dormitorio";
                default: return "en la mano";
            }
        }

        private void OnDisable()
        {
            if (telephoneAudio) telephoneAudio.Stop();
            if (kitchenApplianceAudio) kitchenApplianceAudio.Stop();
        }
    }
}
