using UnityEngine;

// Private authoring receipt; never attached to the actor. The immutable copies
// allow recovery after reloading the Editor while Face.asset keeps its GUID.
public sealed class IntruderAnatomyInstallation : ScriptableObject
{
    public int revision;
    public string scenePath, rendererId, sourceMeshHash, geometrySourceHash;
    public IntruderFaceInstallation roundFour;
    public Mesh installedMesh, backupR4, variantR5;
    public string roundFourFingerprint, roundFiveFingerprint;
    public Bounds roundFourBounds, roundFiveBounds;
}
