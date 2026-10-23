using UnityEngine;

// Private authoring receipt, saved beside the private derived assets. No scene
// component, runtime writer or dependency on the other round-four branches.
public sealed class IntruderFaceInstallation : ScriptableObject
{
    public int revision;
    public string scenePath, rendererId, sourceMeshHash;
    public Mesh originalMesh, faceMesh;
    public Material[] originalMaterials, faceMaterials;
    public Bounds originalBounds, faceBounds;
}
