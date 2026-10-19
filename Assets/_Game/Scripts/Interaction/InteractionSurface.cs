using UnityEngine;

namespace EchoProtocol.Interaction
{
    // A visible moving part can address its explicit owner without requiring
    // the entire assembly to sit under that owner's Transform.
    [DisallowMultipleComponent, RequireComponent(typeof(Collider))]
    public sealed class InteractionSurface : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour owner;
        public IInteractable Target => owner && owner.isActiveAndEnabled ? owner as IInteractable : null;
        private void OnValidate()
        {
            if (owner && !(owner is IInteractable))
                Debug.LogError("InteractionSurface owner must implement IInteractable.", this);
        }
    }
}
