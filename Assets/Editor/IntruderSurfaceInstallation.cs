using UnityEngine;

// Private material-only receipt. The round-four face receipt and all renderer
// references remain untouched. State is inferred from materials, never a flag
// that can disagree with Undo/Redo or an independent anatomy upgrade.
public sealed class IntruderSurfaceInstallation : ScriptableObject
{
    public int revision;
    public string scenePath, rendererId, bibRendererId;
    public Material[] targets, previousMaterials, surfaceMaterials;
}
