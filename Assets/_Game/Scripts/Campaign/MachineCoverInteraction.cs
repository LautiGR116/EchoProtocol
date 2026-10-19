using EchoProtocol.Interaction;
using EchoProtocol.Player;
using EchoProtocol.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoProtocol.Campaign
{
    public sealed class MachineCoverInteraction : MonoBehaviour, IInteractable
    {
        public enum Kind { Crank, Release }
        [SerializeField] private Kind kind;
        [SerializeField] private MachineCoverEncounter encounter;
        [SerializeField] private GameSession session;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private PlayerInput input;
        [SerializeField] private Camera view;
        [SerializeField] private Transform handle;
        [SerializeField] private Vector3 handleAxis = Vector3.forward;
        [SerializeField] private AudioSource workContact;
        [SerializeField] private float workSeconds = 4;
        InputAction interact;
        Quaternion rest;
        bool requireRelease;
        float nextContact;
        public bool IsHolding { get; private set; }
        public float Fraction { get; private set; }
        public Kind Operation => kind;
        public string Prompt => kind == Kind.Crank ? "Aflojar el cierre de la reja" : "Liberar la compuerta de salida";
        bool Accepted => kind == Kind.Crank ? encounter.BoltReleased : encounter.ExitReleased;
        void Awake()
        {
            interact = input ? input.actions.FindActionMap("Player")?.FindAction("Interact") : null;
            if (!encounter || !session || !player || !view || !handle || !workContact || interact == null)
            { Debug.LogError("Cover interaction has incomplete references.", this); enabled = false; return; }
            rest = handle.localRotation;
        }
        bool InReach() => Vector3.Distance(view.transform.position, transform.position) < 2;
        public bool CanInteract(GameObject who) => enabled && encounter.Active && !encounter.Failed && !encounter.Complete
            && !session.IsPaused && !Accepted && who == player.gameObject && !IsHolding && !requireRelease && InReach();
        public void Interact(GameObject who)
        {
            if (!CanInteract(who) || !interact.IsPressed()) return;
            if (kind == Kind.Crank && !encounter.Covered) { encounter.ExplainNoCover(); requireRelease = true; return; }
            IsHolding = true;
            nextContact = Time.time;
        }
        void LateUpdate()
        {
            if (!encounter.Active) return;
            if (session.IsPaused) { IsHolding = false; requireRelease = true; return; }
            if (!interact.IsPressed()) requireRelease = false;
            if (IsHolding && (!interact.IsPressed() || !InReach() || encounter.Failed || !player.isActiveAndEnabled
                || (kind == Kind.Crank && !encounter.Covered))) IsHolding = false;
            if (!Accepted) Fraction = Mathf.MoveTowards(Fraction, IsHolding ? 1 : 0, Time.deltaTime / (IsHolding ? workSeconds : .65f));
            if(kind==Kind.Release && IsHolding && Fraction>.28f)encounter.PrimeThreat();
            if (IsHolding && Time.time >= nextContact)
            {
                workContact.Play();
                nextContact = Time.time + (kind == Kind.Crank ? .46f : .65f);
            }
            handle.localRotation = rest * Quaternion.AngleAxis(Fraction * (kind == Kind.Crank ? 540 : 65), handleAxis.normalized);
            if (IsHolding && Fraction >= 1) { IsHolding = false; encounter.AcceptWork(kind); }
        }
        public void ResetWork() { IsHolding = false; Fraction = 0; requireRelease = true; if(workContact)workContact.Stop(); }
    }
}
