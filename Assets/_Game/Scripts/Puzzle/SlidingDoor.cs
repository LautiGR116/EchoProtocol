using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Puzzle
{
    [DisallowMultipleComponent]
    public sealed class SlidingDoor : MonoBehaviour, ILoopResettable
    {
        [SerializeField] private Rigidbody doorBody;
        [SerializeField] private Vector3 closedLocalPosition;
        [SerializeField] private Vector3 openLocalPosition = new Vector3(0f, 3f, 0f);
        [SerializeField, Min(0.01f)] private float moveSpeed = 3f;

        public bool IsOpenRequested { get; private set; }

        public bool IsFullyOpen => IsAtLocalPosition(openLocalPosition);

        public bool IsFullyClosed => IsAtLocalPosition(closedLocalPosition);

        private void Awake()
        {
            if (doorBody == null || !doorBody.isKinematic)
            {
                Debug.LogError(
                    "SlidingDoor requires a referenced kinematic Rigidbody.",
                    this);
                enabled = false;
                return;
            }

            ResetLoopState();
        }

        private void FixedUpdate()
        {
            Vector3 targetPosition = GetWorldPosition(
                IsOpenRequested ? openLocalPosition : closedLocalPosition);
            Vector3 nextPosition = Vector3.MoveTowards(
                doorBody.position,
                targetPosition,
                moveSpeed * Time.fixedDeltaTime);
            doorBody.MovePosition(nextPosition);
        }

        public void SetOpen(bool shouldOpen)
        {
            IsOpenRequested = shouldOpen;
        }

        public void ResetLoopState()
        {
            IsOpenRequested = false;

            if (doorBody == null)
            {
                return;
            }

            doorBody.transform.localPosition = closedLocalPosition;
            Physics.SyncTransforms();
        }

        private Vector3 GetWorldPosition(Vector3 localPosition)
        {
            Transform parent = doorBody.transform.parent;
            return parent == null
                ? localPosition
                : parent.TransformPoint(localPosition);
        }

        private bool IsAtLocalPosition(Vector3 localPosition)
        {
            return doorBody != null
                && Vector3.Distance(
                    doorBody.transform.localPosition,
                    localPosition) < 0.01f;
        }

        private void OnValidate()
        {
            moveSpeed = Mathf.Max(0.01f, moveSpeed);
        }
    }
}
