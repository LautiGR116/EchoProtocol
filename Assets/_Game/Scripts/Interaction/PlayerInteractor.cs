using System;
using EchoProtocol.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace EchoProtocol.Interaction
{
    // Session pause (-200), locomotion and real RECALL (0) settle first.
    // HUD and input consume the same snapshot; OnGUI never casts a second ray.
    [DefaultExecutionOrder(100), RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera viewCamera;
        [SerializeField] private PlayerInput playerInput;
        [Header("Reach and physical visibility")]
        [SerializeField, Min(0f)] private float interactionDistance = 3f;
        [SerializeField] private LayerMask interactionLayers = Physics.DefaultRaycastLayers;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
        [SerializeField, Range(0f, .12f)] private float focusRadius = .075f;
        [SerializeField, Range(0f, 3f)] private float focusAngle = 1.6f;
        [SerializeField, Range(0f, .15f)] private float releaseMargin = .08f;
        readonly RaycastHit[] hits = new RaycastHit[64];
        readonly RaycastHit[] sightHits = new RaycastHit[64];
        InputAction interactAction, recallAction;
        FirstPersonController controller;
        IInteractable focus;
        Collider focusCollider;
        Vector3 focusPoint;
        bool ready, requireRelease = true;
        int blockedFrame = -1, resolvedFrame = -1, activationFrame = -1;

        public float InteractionDistance => interactionDistance;
        public IInteractable FocusedTarget => IsAlive(focus) ? focus : null;
        public Vector3 FocusPoint => focusPoint;
        public Collider FocusCollider => focusCollider;
        public bool HasUsableFocus => enabled && Time.timeScale > 0f && IsAlive(focus) && focus.CanInteract(gameObject);
        public int FocusFrame => resolvedFrame;
        public int ActivationCount { get; private set; }
        public int LastActivationFrame => activationFrame;
        public IInteractable LastActivatedTarget { get; private set; }
        public event Action<IInteractable> Interacted;

        void Awake()
        {
            if (!playerInput) playerInput = GetComponent<PlayerInput>();
            if (!viewCamera) viewCamera = GetComponentInChildren<Camera>();
            controller = GetComponent<FirstPersonController>();
            var map = playerInput ? playerInput.actions?.FindActionMap("Player") : null;
            interactAction = map?.FindAction("Interact");
            recallAction = map?.FindAction("ResetLoop");
            ready = playerInput && viewCamera && interactAction != null;
            if (!ready) { Debug.LogError("PlayerInteractor requires PlayerInput, a Camera and Player/Interact.", this); enabled = false; return; }
            if (controller) controller.PoseReset += OnPoseReset;
        }
        void OnEnable() { requireRelease = true; ClearFocus(); }
        void OnDisable() { requireRelease = true; ClearFocus(); }
        void OnDestroy() { if (controller) controller.PoseReset -= OnPoseReset; }
        void OnApplicationFocus(bool focused) { if (!focused) { requireRelease = true; ClearFocus(); } }
        void OnPoseReset() { blockedFrame = Time.frameCount; requireRelease = true; ClearFocus(); }

        void Update()
        {
            if (!ready || Time.timeScale <= 0f || !interactAction.enabled || (controller && !controller.isActiveAndEnabled))
            { ClearFocus(); return; }
            ResolveFocus();
            if (!PhysicallyPressed()) requireRelease = false;
            // An ambiguous E+R chord must not open a document before RECALL can run.
            if (recallAction != null && recallAction.WasPressedThisFrame())
            { blockedFrame = Time.frameCount; requireRelease = true; return; }
            if (!requireRelease && blockedFrame != Time.frameCount && interactAction.WasPressedThisFrame())
            {
                requireRelease = true;
                TryInteract();
            }
        }
        bool PhysicallyPressed()
        {
            if (interactAction.IsPressed()) return true;
            // On action-map re-enable, the physical held key can precede the
            // action's next initial-state check. It still must be released.
            foreach (var control in interactAction.controls)
                if (control is ButtonControl button && button.isPressed) return true;
            return false;
        }
        public bool TryInteract()
        {
            if (!ready || !isActiveAndEnabled || Time.timeScale <= 0f || resolvedFrame != Time.frameCount
                || blockedFrame == Time.frameCount || activationFrame == Time.frameCount || !HasUsableFocus) return false;
            var accepted = focus;
            activationFrame = Time.frameCount;
            LastActivatedTarget = accepted;
            ActivationCount++;
            accepted.Interact(gameObject);
            Interacted?.Invoke(accepted);
            return true;
        }
        public bool TryGetTarget(out IInteractable target)
        {
            target = isActiveAndEnabled && Time.timeScale > 0f ? FocusedTarget : null;
            return target != null;
        }
        static bool IsAlive(IInteractable target) => target is MonoBehaviour owner && owner && owner.isActiveAndEnabled;
        bool IsSelf(Collider collider) => collider && collider.transform.IsChildOf(transform);
        static IInteractable Owner(Collider collider)
        {
            var surface = collider.GetComponentInParent<InteractionSurface>();
            if (surface) return surface.Target;
            var target = collider.GetComponentInParent<IInteractable>();
            return IsAlive(target) ? target : null;
        }
        void ClearFocus() { focus = null; focusCollider = null; resolvedFrame = -1; }
        void Select(IInteractable target, Collider collider, Vector3 point)
        { focus = target; focusCollider = collider; focusPoint = point; resolvedFrame = Time.frameCount; }

        void ResolveFocus()
        {
            Vector3 eye = viewCamera.transform.position, forward = viewCamera.transform.forward;
            var previous = focus;
            float reach = interactionDistance + releaseMargin;
            int count = Physics.RaycastNonAlloc(eye, forward, hits, reach, interactionLayers, triggerInteraction);
            if (count == hits.Length) { ClearFocus(); return; }
            int closest = -1; float distance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
                if (!IsSelf(hits[i].collider) && hits[i].distance < distance)
                { closest = i; distance = hits[i].distance; }
            if (closest >= 0)
            {
                var hit = hits[closest]; var target = Owner(hit.collider);
                float allowed = interactionDistance + (ReferenceEquals(target, previous) ? releaseMargin : 0f);
                if (target != null && hit.distance <= allowed)
                { Select(target, hit.collider, hit.point); return; }
            }
            // Small angular tolerance handles ordinary hand movement at an edge.
            // Every candidate still needs an unobstructed physical sightline;
            // retaining focus never grants permission through an obstacle.
            count = focusRadius > 0f ? Physics.SphereCastNonAlloc(eye, focusRadius, forward, hits, reach,
                interactionLayers, triggerInteraction) : 0;
            if (count == hits.Length) { ClearFocus(); return; }
            float best = float.PositiveInfinity; IInteractable winner = null; Collider shape = null; Vector3 point = default;
            for (int i = 0; i < count; i++)
            {
                var hit = hits[i]; if (IsSelf(hit.collider)) continue;
                var target = Owner(hit.collider); if (target == null) continue;
                Vector3 p = hit.point;
                if (hit.distance <= .0001f) p = hit.collider.ClosestPoint(eye + forward * .1f);
                Vector3 offset = p - eye; float d = offset.magnitude;
                bool retaining = ReferenceEquals(target, previous);
                if (d < .02f || d > interactionDistance + (retaining ? releaseMargin : 0f)) continue;
                float angle = Vector3.Angle(forward, offset);
                if (angle > focusAngle + (retaining ? .35f : 0f) || !Visible(eye, p, target)) continue;
                float score = angle + d * .04f - (retaining ? .18f : 0f);
                if (score >= best) continue;
                best = score; winner = target; shape = hit.collider; point = p;
            }
            if (winner != null) Select(winner, shape, point);
            else { ClearFocus(); resolvedFrame = Time.frameCount; }
        }
        bool Visible(Vector3 eye, Vector3 point, IInteractable owner)
        {
            Vector3 direction = point - eye; float distance = direction.magnitude;
            int count = Physics.RaycastNonAlloc(eye, direction / distance, sightHits, distance + .002f,
                interactionLayers, triggerInteraction);
            if (count == sightHits.Length) return false;
            for (int i = 0; i < count; i++)
            {
                var hit = sightHits[i];
                if (IsSelf(hit.collider) || ReferenceEquals(Owner(hit.collider), owner)) continue;
                if (hit.distance < distance - .004f) return false;
            }
            return true;
        }
        void OnValidate() { interactionDistance = Mathf.Max(0f, interactionDistance); }
    }
}
