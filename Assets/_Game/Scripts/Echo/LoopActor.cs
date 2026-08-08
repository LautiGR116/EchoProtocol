using System;
using UnityEngine;

namespace EchoProtocol.Echo
{
    public enum LoopActorKind
    {
        Player,
        Echo
    }

    [DisallowMultipleComponent]
    public sealed class LoopActor : MonoBehaviour
    {
        [SerializeField] private LoopActorKind kind;

        public LoopActorKind Kind => kind;

        public event Action<LoopActor> BecameUnavailable;

        private void OnDisable()
        {
            BecameUnavailable?.Invoke(this);
        }
    }
}
