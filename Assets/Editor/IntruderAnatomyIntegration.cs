using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Upgrade the installed R4 mesh in place. Its object/GUID and the material
// references must survive the parallel surface integration. No rig writes.
public static class IntruderAnatomyIntegration
{
    const string Private = "Assets/_Game/Art/Embodiment/RenderpeoplePrivate/";
    const string Source = Private + "Nathan/ArticulatedSurface.asset";
    const string Face = Private + "IntruderFace/Face.asset";
    const string RoundFourReceipt = Private + "IntruderFace/Installation.asset";
    const string Output = Private + "IntruderAnatomy/";
    const string Receipt = Output + "Installation.asset";
    const string Backup = Output + "BackupR4.asset";
    const string Variant = Output + "VariantR5.asset";
    const string Geometry = "Assets/Editor/IntruderFaceGeometry.cs";
    const int Revision = 1;

    sealed class Context
    {
        public Scene scene;
        public SkinnedMeshRenderer renderer;
        public Mesh scan, face;
        public IntruderFaceInstallation roundFour;
        public IntruderAnatomyInstallation installation;
        public bool installed;
    }

    [MenuItem("Echo Protocol/Intruder anatomy/1. Validate current scene")]
    public static void ValidateCurrentScene()
    {
        var c = Validate(true);
        Debug.Log(c.installed ? "Intruder anatomy R5 valid; Upgrade is a no-op."
            : "Intruder anatomy ready: upgrade the installed R4 Face.asset in place; retain its GUID, rig and materials. Private R4 backup and R5 variant provide persistent rollback/reapply.", c.renderer);
    }

    [MenuItem("Echo Protocol/Intruder anatomy/2. Upgrade current scene to R5")]
    public static void UpgradeCurrentScene()
    {
        var c = Validate(true);
        if (c.installed)
        {
            // Persist a matching mesh made dirty by an ordinary Undo/Redo too.
            AssetDatabase.SaveAssetIfDirty(c.face);
            Debug.Log("Intruder anatomy R5 already installed; known mesh saved if dirty. Nothing regenerated.", c.renderer);
            return;
        }
        var created = new List<string>();
        var temporary = new List<Object>();
        bool createdFolder = false;
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Upgrade intruder anatomy R4 to R5");
        try
        {
            var receipt = c.installation;
            if (!receipt)
            {
                if (!AssetDatabase.IsValidFolder(Output.TrimEnd('/')))
                {
                    if (string.IsNullOrEmpty(AssetDatabase.CreateFolder(Private.TrimEnd('/'), "IntruderAnatomy")))
                        throw new InvalidOperationException("Could not create the private anatomy folder.");
                    createdFolder = true;
                }
                var backup = Object.Instantiate(c.face); temporary.Add(backup);
                backup.name = "Intruder anatomy / immutable R4 recovery";
                var variant = IntruderFaceGeometry.Create(backup, c.scan); temporary.Add(variant);
                IntruderFaceGeometry.ValidatePair(variant, c.scan);
                string before = Fingerprint(backup), after = Fingerprint(variant);
                if (before != Fingerprint(c.face) || before == after)
                    throw new InvalidOperationException("Anatomy generation did not preserve its R4 snapshot or produce a distinct R5 mesh.");
                SaveNew(backup, Backup, created);
                SaveNew(variant, Variant, created);
                receipt = ScriptableObject.CreateInstance<IntruderAnatomyInstallation>(); temporary.Add(receipt);
                receipt.revision = Revision;
                receipt.scenePath = c.scene.path;
                receipt.rendererId = GlobalObjectId.GetGlobalObjectIdSlow(c.renderer).ToString();
                receipt.sourceMeshHash = DependencyHash(Source);
                receipt.geometrySourceHash = FileHash(Geometry);
                receipt.roundFour = c.roundFour;
                receipt.installedMesh = c.face;
                receipt.backupR4 = backup;
                receipt.variantR5 = variant;
                receipt.roundFourFingerprint = before;
                receipt.roundFiveFingerprint = after;
                receipt.roundFourBounds = c.renderer.localBounds;
                var bounds = receipt.roundFourBounds; bounds.Encapsulate(variant.bounds);
                receipt.roundFiveBounds = bounds;
                SaveNew(receipt, Receipt, created);
            }
            CopyIntoInstalled(c, receipt.variantR5, receipt.roundFiveBounds);
            if (!Validate(true).installed) throw new InvalidOperationException("Intruder anatomy R5 did not validate after assignment.");
            Undo.CollapseUndoOperations(group);
            Debug.Log("Intruder anatomy upgraded: same Face.asset object/GUID, R4 backup retained. Mesh asset saved; scene UNSAVED. Materials, bones and transforms preserved. Undo or Intruder anatomy > 3. Roll back restores R4; the rollback menu also saves the restored mesh asset.", c.renderer);
        }
        catch
        {
            // Undo restores native mesh data too. Persist that restoration before
            // deleting anything from the failed transaction; if saving fails,
            // the recovery copies stay on disk for manual recovery.
            Undo.RevertAllDownToGroup(group);
            EditorUtility.SetDirty(c.face);
            AssetDatabase.SaveAssetIfDirty(c.face);
            for (int i = created.Count - 1; i >= 0; i--) AssetDatabase.DeleteAsset(created[i]);
            if (createdFolder && AssetDatabase.IsValidFolder(Output.TrimEnd('/')))
                AssetDatabase.DeleteAsset(Output.TrimEnd('/'));
            throw;
        }
        finally
        {
            foreach (var asset in temporary)
                if (asset && !EditorUtility.IsPersistent(asset)) Object.DestroyImmediate(asset);
        }
    }

    [MenuItem("Echo Protocol/Intruder anatomy/3. Roll back current scene to R4")]
    public static void RollbackCurrentScene()
    {
        // A later generator edit must never prevent recovery of the saved R4.
        var c = Validate(false);
        if (!c.installed)
        {
            AssetDatabase.SaveAssetIfDirty(c.face);
            Debug.Log("Intruder anatomy is already R4; known mesh saved if dirty. Nothing regenerated.", c.renderer);
            return;
        }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Restore intruder R4 anatomy");
        try
        {
            CopyIntoInstalled(c, c.installation.backupR4, c.installation.roundFourBounds);
            if (Validate(false).installed) throw new InvalidOperationException("Intruder anatomy rollback did not validate.");
            Undo.CollapseUndoOperations(group);
            Debug.Log("R4 mesh restored and saved into the same Face.asset; scene UNSAVED. Materials, bones and transforms preserved. Private backup/variant/receipt retained for reapply and Redo.", c.renderer);
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            EditorUtility.SetDirty(c.face);
            AssetDatabase.SaveAssetIfDirty(c.face);
            throw;
        }
    }

    static Context Validate(bool requireCurrentGeometry)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("Stop Play and finish script import before integrating anatomy.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        if (!c.scene.IsValid() || !c.scene.isLoaded || !c.scene.path.EndsWith("/EchoProtocol_Incursion.unity", StringComparison.Ordinal))
            throw new InvalidOperationException("Use the existing open Incursion scene. Anatomy never opens or saves a scene.");
        var threats = c.scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MachineCoverThreat>(true)).ToArray();
        if (threats.Length != 1) throw new InvalidOperationException("Anatomy requires exactly one present MachineCoverThreat.");
        var threat = threats[0];
        var performance = threat.GetComponent<MachineThreatPerformance>();
        var anatomy = performance ? new SerializedObject(performance).FindProperty("anatomy")?.objectReferenceValue as RecordedCharacterMotion : null;
        if (!anatomy || !anatomy.IsPresentThreat || !threat.GetComponent<IntruderStaging>()
            || anatomy.GetComponentInParent<EchoPlayback>() || anatomy.GetComponentInParent<LoopActor>())
            throw new InvalidOperationException("Anatomy only accepts the present Nathan, never an Echo or witness.");
        var renderers = threat.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(r => r.name == "RecordedAdultSurface").ToArray();
        if (renderers.Length != 1) throw new InvalidOperationException("Expected one present RecordedAdultSurface renderer.");
        c.renderer = renderers[0];
        c.scan = AssetDatabase.LoadAssetAtPath<Mesh>(Source);
        c.face = AssetDatabase.LoadAssetAtPath<Mesh>(Face);
        c.roundFour = AssetDatabase.LoadAssetAtPath<IntruderFaceInstallation>(RoundFourReceipt);
        if (!c.roundFour || c.roundFour.revision != 1 || c.roundFour.scenePath != c.scene.path
            || c.roundFour.rendererId != GlobalObjectId.GetGlobalObjectIdSlow(c.renderer).ToString()
            || !c.face || c.roundFour.faceMesh != c.face || c.renderer.sharedMesh != c.face
            || c.roundFour.sourceMeshHash != DependencyHash(Source))
            throw new InvalidOperationException("Expected the exact installed R4 Face.asset and its unchanged Nathan source/receipt. Run no historical production generator.");
        IntruderFaceGeometry.ValidatePair(c.face, c.scan);
        if (Resources.FindObjectsOfTypeAll<SkinnedMeshRenderer>().Any(r => r != c.renderer
            && r.gameObject.scene.IsValid() && r.gameObject.scene.isLoaded && r.sharedMesh == c.face))
            throw new InvalidOperationException("Another loaded actor shares Face.asset; isolate that work before modifying the present Nathan mesh.");
        if (!c.renderer.transform.IsChildOf(anatomy.transform) || c.renderer.bones.Length != c.scan.bindposes.Length
            || c.renderer.bones.Any(b => !b) || EditorUtility.IsDirty(c.scan))
            throw new InvalidOperationException("Nathan rig or saved source differs from the prepared installation.");
        // Do not inspect material slots or the R4 receipt's dependency hash:
        // surface authoring owns those assets and must remain independent.
        c.installation = AssetDatabase.LoadAssetAtPath<IntruderAnatomyInstallation>(Receipt);
        if (!c.installation)
        {
            foreach (string path in new[] { Receipt, Backup, Variant })
                if (Exists(path)) throw new InvalidOperationException("Unclaimed/partial anatomy asset: " + path + ". Nothing was overwritten.");
            if (EditorUtility.IsDirty(c.face) || !MatchesBounds(c.renderer, c.roundFour.faceBounds))
                throw new InvalidOperationException("Save/reconcile the changed R4 mesh or renderer bounds before taking the immutable snapshot.");
            return c;
        }
        var r = c.installation;
        if (r.revision != Revision || r.scenePath != c.scene.path
            || r.rendererId != GlobalObjectId.GetGlobalObjectIdSlow(c.renderer).ToString()
            || r.roundFour != c.roundFour || r.installedMesh != c.face || r.sourceMeshHash != DependencyHash(Source)
            || EditorUtility.IsDirty(r) || !r.backupR4 || !r.variantR5
            || AssetDatabase.GetAssetPath(r.backupR4) != Backup || AssetDatabase.GetAssetPath(r.variantR5) != Variant
            || EditorUtility.IsDirty(r.backupR4) || EditorUtility.IsDirty(r.variantR5)
            || string.IsNullOrEmpty(r.roundFourFingerprint) || r.roundFourFingerprint == r.roundFiveFingerprint)
            throw new InvalidOperationException("Anatomy receipt/source is partial or stale; preserve its private recovery assets and resolve before overwriting work.");
        IntruderFaceGeometry.ValidatePair(r.backupR4, c.scan);
        IntruderFaceGeometry.ValidatePair(r.variantR5, c.scan);
        if (Fingerprint(r.backupR4) != r.roundFourFingerprint || Fingerprint(r.variantR5) != r.roundFiveFingerprint)
            throw new InvalidOperationException("Anatomy recovery mesh content changed; refusing to overwrite it or the installed mesh.");
        string current = Fingerprint(c.face);
        c.installed = current == r.roundFiveFingerprint && MatchesBounds(c.renderer, r.roundFiveBounds);
        if (!c.installed && !(current == r.roundFourFingerprint && MatchesBounds(c.renderer, r.roundFourBounds)))
            throw new InvalidOperationException("Installed anatomy or bounds contain unexpected edits; refusing to overwrite that work.");
        if (requireCurrentGeometry && r.geometrySourceHash != FileHash(Geometry))
            throw new InvalidOperationException("The anatomy generator changed after this receipt. Roll back R4, then archive the private IntruderAnatomy folder before explicitly regenerating; do not overwrite its backup.");
        return c;
    }

    static bool MatchesBounds(SkinnedMeshRenderer renderer, Bounds bounds)
        // This is true on the actual R4 actor: Unity derives bounds immediately
        // from the current skinned pose rather than retaining our rest-pose box.
        => renderer.updateWhenOffscreen || (renderer.localBounds.center - bounds.center).sqrMagnitude < .00000001f
            && (renderer.localBounds.extents - bounds.extents).sqrMagnitude < .00000001f;

    static void CopyIntoInstalled(Context c, Mesh snapshot, Bounds bounds)
    {
        Undo.RegisterCompleteObjectUndo(new Object[] { c.face, c.renderer }, "Replace installed intruder anatomy");
        string name = c.face.name;
        EditorUtility.CopySerialized(snapshot, c.face);
        c.face.name = name;
        c.renderer.localBounds = bounds;
        EditorUtility.SetDirty(c.face);
        AssetDatabase.SaveAssetIfDirty(c.face);
        PrefabUtility.RecordPrefabInstancePropertyModifications(c.renderer);
        EditorSceneManager.MarkSceneDirty(c.scene);
    }

    static bool Exists(string path) => File.Exists(path) || File.Exists(path + ".meta") || AssetDatabase.LoadMainAssetAtPath(path);
    static string DependencyHash(string path) => AssetDatabase.GetAssetDependencyHash(path).ToString();

    // Hash only the generator text, not MonoScript dependencies or unrelated
    // assembly imports from the parallel surface/performance integrations.
    static string FileHash(string path)
    {
        using (var stream = File.OpenRead(path))
        using (var hash = SHA256.Create())
            return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }

    static void SaveNew(Object asset, string path, List<string> created)
    {
        if (Exists(path)) throw new InvalidOperationException("Refusing to overwrite " + path);
        AssetDatabase.CreateAsset(asset, path); created.Add(path);
        AssetDatabase.SaveAssetIfDirty(asset);
    }

    // Content identity deliberately excludes asset GUID and name, so a saved
    // clone and its CopySerialized destination match after an Editor reload.
    // All Nathan streams, topology, bounds and skin data participate; this is
    // recovery validation, never a visual or emotional approval metric.
    static string Fingerprint(Mesh mesh)
    {
        using (var bytes = new MemoryStream())
        using (var writer = new BinaryWriter(bytes))
        {
            writer.Write(mesh.vertexCount); writer.Write((int)mesh.indexFormat);
            foreach (var v in mesh.vertices) Write(writer, v);
            foreach (var v in mesh.normals) Write(writer, v);
            foreach (var v in mesh.tangents) Write(writer, v);
            var colors = mesh.colors; writer.Write(colors.Length);
            foreach (var color in colors) { writer.Write(color.r); writer.Write(color.g); writer.Write(color.b); writer.Write(color.a); }
            var uv = new List<Vector4>();
            for (int channel = 0; channel < 8; channel++)
            {
                uv.Clear(); mesh.GetUVs(channel, uv); writer.Write(uv.Count);
                var attribute = (VertexAttribute)((int)VertexAttribute.TexCoord0 + channel);
                writer.Write(mesh.HasVertexAttribute(attribute) ? mesh.GetVertexAttributeDimension(attribute) : 0);
                foreach (var value in uv) Write(writer, value);
            }
            var bindposes = mesh.bindposes; writer.Write(bindposes.Length);
            foreach (var matrix in bindposes) for (int i = 0; i < 16; i++) writer.Write(matrix[i]);
            var weights = mesh.boneWeights; writer.Write(weights.Length);
            foreach (var weight in weights)
            {
                writer.Write(weight.boneIndex0); writer.Write(weight.boneIndex1); writer.Write(weight.boneIndex2); writer.Write(weight.boneIndex3);
                writer.Write(weight.weight0); writer.Write(weight.weight1); writer.Write(weight.weight2); writer.Write(weight.weight3);
            }
            writer.Write(mesh.subMeshCount);
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                writer.Write((int)mesh.GetTopology(sub)); var indices = mesh.GetIndices(sub); writer.Write(indices.Length);
                foreach (int index in indices) writer.Write(index);
            }
            Write(writer, mesh.bounds.center); Write(writer, mesh.bounds.extents);
            writer.Flush(); bytes.Position = 0;
            using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
    }

    static void Write(BinaryWriter writer, Vector3 v) { writer.Write(v.x); writer.Write(v.y); writer.Write(v.z); }
    static void Write(BinaryWriter writer, Vector4 v) { writer.Write(v.x); writer.Write(v.y); writer.Write(v.z); writer.Write(v.w); }
}
