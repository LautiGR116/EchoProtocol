using EchoProtocol.Campaign;
using EchoProtocol.Interaction;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoProtocol.Echo
{
    [DisallowMultipleComponent]
    public sealed class RecordedMachineControl : MonoBehaviour, IInteractable
    {
        [SerializeField] private int targetId = 101;
        [SerializeField] private MachineCoverEncounter encounter;
        [SerializeField] private GameSession session;
        [SerializeField] private EchoRecorder recorder;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private Camera view;
        [SerializeField] private PlayerInput input;
        [SerializeField] private Transform grip;
        [SerializeField] private Transform lever;
        [SerializeField] private float reach = 1.8f;
        InputAction interact;
        EchoPlayback replayOwner;
        bool requireRelease;
        Quaternion rest;
        public int TargetId => targetId;
        public bool PlayerHeld { get; private set; }
        public bool ReplayHeld { get; private set; }
        public bool Held => PlayerHeld || ReplayHeld;
        public string Prompt => "Hacer marchar la bomba";
        // Only the live capture source queries this. Playback uses the immutable
        // root-local contact channel and never reads the current world anchor.
        public bool TryGetPlayerGrip(out Vector3 point)
        {
            point = grip ? grip.position : default;
            return PlayerHeld && encounter.Active && !encounter.Failed && !session.IsPaused;
        }
        void Awake()
        {
            interact = input ? input.actions.FindActionMap("Player")?.FindAction("Interact") : null;
            if (!encounter || !session || !recorder || !player || !view || !grip || !lever || interact == null)
            { Debug.LogError("Machine control requires explicit capture, input and physical references.", this); enabled = false; return; }
            rest = lever.localRotation;
        }
        public bool CanInteract(GameObject who) => enabled && encounter.Active && !encounter.Failed && !encounter.Complete
            && !session.IsPaused && who == player.gameObject && !PlayerHeld && !requireRelease
            && recorder.HasRecordingStarted && InReach();
        public void Interact(GameObject who)
        {
            if (CanInteract(who) && interact.IsPressed() && recorder.CaptureMachineGrip(targetId, true)) PlayerHeld = true;
        }
        bool InReach()
        {
            Vector3 offset = grip.position - view.transform.position;
            if (offset.magnitude > reach) return false;
            foreach (var hit in Physics.RaycastAll(view.transform.position, offset.normalized, offset.magnitude, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(transform) && !hit.transform.IsChildOf(player.transform)) return false;
            return true;
        }
        void LateUpdate()
        {
            if (!encounter.Active) return;
            if (session.IsPaused) { StopPlayer(true); return; }
            if (!interact.IsPressed()) requireRelease = false;
            if (PlayerHeld && (!interact.IsPressed() || !InReach() || !player.isActiveAndEnabled || encounter.Failed || encounter.Complete)) StopPlayer(false);
            lever.localRotation = rest * Quaternion.AngleAxis(Held ? 0 : 70, Vector3.right);
        }
        void StopPlayer(bool release)
        {
            if (PlayerHeld) recorder.CaptureMachineGrip(targetId, false);
            PlayerHeld = false; requireRelease |= release;
        }
        public void ApplyReplay(EchoPlayback owner, bool held)
        {
            if (!enabled || !encounter.Active || encounter.Failed) return;
            replayOwner = owner; ReplayHeld = held;
        }
        public void ReleaseReplay(EchoPlayback owner)
        { if (replayOwner == owner) { ReplayHeld = false; replayOwner = null; } }
        public void ResetControl() { PlayerHeld = ReplayHeld = false; replayOwner = null; requireRelease = true; }
        void OnDisable() { ResetControl(); }
    }
}
