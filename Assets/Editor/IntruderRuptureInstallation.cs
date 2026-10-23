using UnityEngine;

// Private authoring receipt only. R5 geometry/material receipts remain intact;
// recovery does not depend on rerunning either generation algorithm.
public sealed class IntruderRuptureInstallation : ScriptableObject
{
    public int revision;
    public string scenePath, rendererId, sourceMeshHash, geometrySourceHash;
    public IntruderAnatomyInstallation roundFive;
    public string roundFiveReceiptHash;
    public Mesh installedMesh, backupR5, variantR6;
    public string roundFiveFingerprint, roundSixFingerprint;
    public Bounds roundFiveBounds, roundSixBounds;
    public Material[] roundFiveMaterials, roundSixMaterials;
    public string[] roundFiveMaterialHashes, roundSixMaterialHashes;
}
