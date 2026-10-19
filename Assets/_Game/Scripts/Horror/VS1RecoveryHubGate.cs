using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Horror
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class VS1RecoveryHubGate : MonoBehaviour
    {
        [Header("Existing completion and actual Player")]
        [SerializeField] private VS1OmittedPassageEvent omittedPassage;
        [SerializeField] private LoopActor player;

        [Header("Separate scene roots")]
        [SerializeField] private GameObject baselineRoot;
        [SerializeField] private GameObject hubRoot;
        [SerializeField] private GameObject accessRoot;
        [SerializeField] private GameObject domesticResidualRoot;

        [Header("Physical boundaries")]
        [SerializeField] private Collider baselineBarrier;
        [SerializeField] private Collider observationBarrier;

        public bool IsOpen { get; private set; }
        public int OpeningCount { get; private set; }

        private void Awake()
        {
            if (!GetComponent<BoxCollider>().isTrigger || omittedPassage == null
                || player == null || player.Kind != LoopActorKind.Player
                || baselineRoot == null || hubRoot == null || accessRoot == null
                || domesticResidualRoot == null || baselineRoot == hubRoot
                || baselineBarrier == null || baselineBarrier.isTrigger
                || observationBarrier == null || observationBarrier.isTrigger
                || !baselineBarrier.transform.IsChildOf(baselineRoot.transform)
                || !accessRoot.transform.IsChildOf(hubRoot.transform)
                || !domesticResidualRoot.transform.IsChildOf(hubRoot.transform)
                || !observationBarrier.transform.IsChildOf(accessRoot.transform))
            {
                Debug.LogError("VS1RecoveryHubGate requires SE3, the actual Player, separate baseline/hub roots, hub access/residual children, and solid baseline/observation barriers.", this);
                enabled = false;
                return;
            }

            hubRoot.SetActive(false);
            baselineRoot.SetActive(true);
        }

        private void OnTriggerEnter(Collider other) => TryOpen(other);
        private void OnTriggerStay(Collider other) => TryOpen(other);

        private void TryOpen(Collider other)
        {
            if (IsOpen || !omittedPassage.isActiveAndEnabled
                || !omittedPassage.IsComplete || !omittedPassage.IsOverlayVisible
                || !player.isActiveAndEnabled
                || other.GetComponentInParent<LoopActor>() != player)
                return;

            // SE3 alone owns the valid return rule. Enable continuous support and
            // containment before removing the original wall. Never reset puzzles,
            // move an actor, or change the domestic RECALL boundary.
            accessRoot.SetActive(true);
            domesticResidualRoot.SetActive(true);
            hubRoot.SetActive(true);
            baselineRoot.SetActive(false);
            IsOpen = true;
            OpeningCount++;
        }

        // No loop-reset participation: once opened, the room remains traversable
        // for this Play session, including RECALL and component disable/re-enable.
    }
}
