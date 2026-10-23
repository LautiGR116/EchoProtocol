using EchoProtocol.Campaign.P3Intrusion;
using UnityEngine;

namespace EchoProtocol.Campaign.IntruderActing
{
    // R6's own rollback data. No runtime behavior and no historical receipt edits.
    [DisallowMultipleComponent, AddComponentMenu("")]
    public sealed class P3RupturePerformanceReceipt : MonoBehaviour
    {
        public P3BodyIntrusion owner;
        public Transform stop, hand, stopParent, handParent;
        public MeshFilter frame;
        public Vector3 previousStop, previousHand, installedHand;
        public Quaternion previousStopRotation, previousHandRotation;
    }
}
