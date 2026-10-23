using EchoProtocol.Audio;
using EchoProtocol.Campaign.IntruderReveal;
using UnityEngine;

namespace EchoProtocol.Campaign.P3RupturePresentation
{
    // Authorship/rollback data and the visual return's sight query. Staging,
    // P3BodyIntrusion and Presence keep ownership; no second Update or clock.
    [DisallowMultipleComponent]
    public sealed class P3RuptureRevealSetup : MonoBehaviour
    {
        public int revision = 6;
        public IntruderStaging staging;
        public MachineCoverPresence presence;
        public IntruderRevealSetup roundFive;
        public IntruderRevealSetup.LightEdit[] lights;
        public IntruderRevealSetup.PropEdit cheek;
        public MeshRenderer cheekRenderer;
        public MeshFilter cheekMesh;
        public Material[] previousMaterials, appliedMaterials;
        public AudioSource contact;
        public AudioClip previousContact, appliedContact;

        public bool Occludes(Vector3 from, Vector3 to)
        {
            if (!isActiveAndEnabled || !cheekRenderer || !cheekRenderer.enabled
                || !cheekRenderer.gameObject.activeInHierarchy || !cheekMesh || !cheekMesh.sharedMesh) return false;
            var t = cheekMesh.transform;
            Vector3 origin = t.InverseTransformPoint(from);
            Vector3 segment = t.InverseTransformPoint(to) - origin;
            return cheekMesh.sharedMesh.bounds.IntersectRay(new Ray(origin, segment.normalized), out float entry)
                && entry >= 0 && entry < segment.magnitude - .001f;
        }
    }
}
