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

// Opt-in R6 upgrade of the existing R5 Face.asset. No rig writes, scene loads,
// scene saves, historical generators, source scan or R5 material mutations.
public static class IntruderRuptureIntegration
{
    const string Private = "Assets/_Game/Art/Embodiment/RenderpeoplePrivate/";
    const string Source = Private + "Nathan/ArticulatedSurface.asset";
    const string Face = Private + "IntruderFace/Face.asset";
    const string RoundFiveReceipt = Private + "IntruderAnatomy/Installation.asset";
    const string Output = Private + "IntruderRupture/";
    const string Receipt = Output + "Installation.asset";
    const string Backup = Output + "BackupR5.asset";
    const string Variant = Output + "VariantR6.asset";
    const string Skin = Output + "Skin.mat";
    const string Mouth = Output + "Mouth.mat";
    const string Geometry = "Assets/Editor/IntruderRuptureGeometry.cs";
    const string RoundFiveGeometry = "Assets/Editor/IntruderFaceGeometry.cs";
    const int Revision = 1;

    sealed class Context
    {
        public Scene scene;
        public SkinnedMeshRenderer renderer;
        public Mesh scan, face;
        public IntruderAnatomyInstallation roundFive;
        public IntruderRuptureInstallation installation;
        public Shader mouthShader;
        public bool installed, atRoundFive;
    }

    [MenuItem("Echo Protocol/Intruder rupture/1. Validate current scene")]
    public static void ValidateCurrentScene()
    {
        var c = Validate(true);
        Debug.Log(c.installed ? "Intruder rupture R6 valid; Upgrade is a no-op."
            : c.atRoundFive ? "Intruder rupture ready: upgrade the exact R5 Face.asset, retain its GUID and rig, and assign private R6 skin/oral materials. Immutable recovery copies retained."
            : "Intruder rupture has known R5/R6 mesh and material states after an unsaved-scene reload or Undo. Upgrade or rollback will reconcile only those known states.", c.renderer);
    }

    [MenuItem("Echo Protocol/Intruder rupture/2. Upgrade current scene to R6")]
    public static void UpgradeCurrentScene()
    {
        var c = Validate(true);
        if (c.installed)
        {
            AssetDatabase.SaveAssetIfDirty(c.face);
            Debug.Log("Intruder rupture R6 already installed; known mesh saved if dirty. Nothing regenerated.", c.renderer);
            return;
        }
        var created = new List<string>();
        var temporary = new List<Object>();
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Upgrade intruder face R5 to rupture R6");
        try
        {
            if (!c.installation) CreateReceipt(c, created, temporary);
            Apply(c, c.installation.variantR6, c.installation.roundSixMaterials, c.installation.roundSixBounds);
            Require(Validate(true).installed, "Intruder rupture R6 did not validate after assignment.");
            Undo.CollapseUndoOperations(group);
            SceneView.RepaintAll();
            Debug.Log("Intruder rupture R6 installed and mesh saved into the same Face.asset. Scene UNSAVED: save it explicitly after review. R5 materials/receipts, bones and transforms preserved; third material serves the oral geometry. Undo or Intruder rupture > 3. Roll back restores R5.", c.renderer);
        }
        catch (Exception failure)
        {
            RestoreFailedTransaction(c, group, failure);
            // Do not recursively delete the folder: another author may have
            // added files. Delete only the assets this transaction created.
            for (int i = created.Count - 1; i >= 0; i--) AssetDatabase.DeleteAsset(created[i]);
            throw;
        }
        finally
        {
            foreach (var asset in temporary)
                if (asset && !EditorUtility.IsPersistent(asset)) Object.DestroyImmediate(asset);
        }
    }

    [MenuItem("Echo Protocol/Intruder rupture/3. Roll back current scene to R5")]
    public static void RollbackCurrentScene()
    {
        // A later R6 generator or shader edit must never prevent recovery.
        var c = Validate(false);
        if (!c.installation || c.atRoundFive)
        {
            AssetDatabase.SaveAssetIfDirty(c.face);
            Debug.Log("Intruder rupture is already R5; known mesh saved if dirty. Nothing regenerated.", c.renderer);
            return;
        }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Restore intruder R5 face and materials");
        try
        {
            Apply(c, c.installation.backupR5, c.installation.roundFiveMaterials, c.installation.roundFiveBounds);
            Require(Validate(false).atRoundFive, "Intruder rupture rollback did not validate.");
            Undo.CollapseUndoOperations(group);
            SceneView.RepaintAll();
            Debug.Log("R5 face restored and saved in the same Face.asset, original material references restored. Scene UNSAVED. Private R6 backup/variant/materials/receipt retained for reapply. No historical receipt, rig, source scan or shared texture changed.", c.renderer);
        }
        catch (Exception failure)
        {
            RestoreFailedTransaction(c, group, failure);
            throw;
        }
    }

    static Context Validate(bool requireCurrentGeometry)
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling,
            "Stop Play and finish script import before integrating rupture.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        Require(c.scene.IsValid() && c.scene.isLoaded && c.scene.path == "Assets/_Game/Scenes/Production/EchoProtocol_Incursion.unity",
            "Use the existing open Incursion scene. Rupture never opens or saves scenes.");
        var threats = c.scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MachineCoverThreat>(true)).ToArray();
        Require(threats.Length == 1, "Rupture requires exactly one present MachineCoverThreat.");
        var threat = threats[0];
        var performance = threat.GetComponent<MachineThreatPerformance>();
        var anatomy = performance ? new SerializedObject(performance).FindProperty("anatomy")?.objectReferenceValue as RecordedCharacterMotion : null;
        Require(anatomy && anatomy.IsPresentThreat && threat.GetComponent<IntruderStaging>()
            && anatomy.transform.IsChildOf(threat.transform) && !anatomy.GetComponentInParent<EchoPlayback>()
            && !anatomy.GetComponentInParent<LoopActor>(), "Rupture only accepts the present Nathan, never an Echo or witness.");
        var renderers = threat.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(r => r.name == "RecordedAdultSurface").ToArray();
        Require(renderers.Length == 1, "Expected one present RecordedAdultSurface renderer.");
        c.renderer = renderers[0];
        c.scan = AssetDatabase.LoadAssetAtPath<Mesh>(Source);
        c.face = AssetDatabase.LoadAssetAtPath<Mesh>(Face);
        c.roundFive = AssetDatabase.LoadAssetAtPath<IntruderAnatomyInstallation>(RoundFiveReceipt);
        ValidateRoundFive(c, requireCurrentGeometry);
        Require(c.renderer.transform.IsChildOf(anatomy.transform) && c.renderer.bones.Length == c.scan.bindposes.Length
            && c.renderer.bones.All(b => b) && !EditorUtility.IsDirty(c.scan), "Nathan rig or saved source differs from R5.");
        Require(!Resources.FindObjectsOfTypeAll<SkinnedMeshRenderer>().Any(r => r != c.renderer
            && r.gameObject.scene.IsValid() && r.gameObject.scene.isLoaded && r.sharedMesh == c.face),
            "Another loaded actor shares Face.asset; isolate that work before changing the present mesh.");

        c.installation = AssetDatabase.LoadAssetAtPath<IntruderRuptureInstallation>(Receipt);
        var current = Fingerprint(c.face);
        if (!c.installation)
        {
            foreach (var path in new[] { Receipt, Backup, Variant, Skin, Mouth })
                Require(!Exists(path), "Unclaimed/partial rupture asset: " + path + ". Nothing was overwritten.");
            Require(current == c.roundFive.roundFiveFingerprint && !EditorUtility.IsDirty(c.face)
                && MatchesBounds(c.renderer, c.roundFive.roundFiveBounds),
                "Expected exact, saved R5 face and bounds before creating the immutable backup.");
            var materials = c.renderer.sharedMaterials;
            Require(materials.SequenceEqual(c.roundFive.roundFour.faceMaterials) && materials.Length == 2
                && materials.All(m => m && !EditorUtility.IsDirty(m)), "Expected unchanged R5 Skin/Eyes assignments and saved materials.");
            Require(materials[0].shader && materials[0].shader.name == "EchoProtocol/EmbestidaSurface"
                && materials[0].HasProperty("_IntruderDreadSurface") && materials[0].GetFloat("_IntruderDreadSurface") == 1,
                "Install the R5 surface first; rupture starts from that skin material.");
            c.atRoundFive = true;
        }
        else
        {
            var r = c.installation;
            Require(r.revision == Revision && r.scenePath == c.scene.path && r.rendererId == Id(c.renderer)
                && r.roundFive == c.roundFive && r.installedMesh == c.face && r.sourceMeshHash == DependencyHash(Source)
                && r.roundFiveReceiptHash == FileHash(RoundFiveReceipt) && !EditorUtility.IsDirty(r)
                && r.backupR5 && AssetDatabase.GetAssetPath(r.backupR5) == Backup && !EditorUtility.IsDirty(r.backupR5)
                && r.variantR6 && AssetDatabase.GetAssetPath(r.variantR6) == Variant && !EditorUtility.IsDirty(r.variantR6)
                && r.roundFiveFingerprint == c.roundFive.roundFiveFingerprint && r.roundFiveFingerprint != r.roundSixFingerprint,
                "Rupture receipt/source is partial or stale; preserve recovery assets and resolve before overwriting work.");
            Require(Fingerprint(r.backupR5) == r.roundFiveFingerprint && Fingerprint(r.variantR6) == r.roundSixFingerprint,
                "Rupture recovery mesh content changed; refusing to overwrite it or the installed mesh.");
            ValidateMaterials(r);
            bool meshR5 = current == r.roundFiveFingerprint, meshR6 = current == r.roundSixFingerprint;
            bool materialsR5 = c.renderer.sharedMaterials.SequenceEqual(r.roundFiveMaterials);
            bool materialsR6 = c.renderer.sharedMaterials.SequenceEqual(r.roundSixMaterials);
            bool boundsR5 = MatchesBounds(c.renderer, r.roundFiveBounds), boundsR6 = MatchesBounds(c.renderer, r.roundSixBounds);
            Require((meshR5 || meshR6) && (materialsR5 || materialsR6) && (boundsR5 || boundsR6),
                "Installed mesh/material assignments/bounds contain unexpected edits; nothing was overwritten.");
            c.installed = meshR6 && materialsR6 && boundsR6;
            c.atRoundFive = meshR5 && materialsR5 && boundsR5;
            if (requireCurrentGeometry)
            {
                Require(r.geometrySourceHash == FileHash(Geometry),
                    "R6 geometry changed since this receipt. Roll back R5, then archive the private IntruderRupture folder before explicitly regenerating. Never overwrite the backup.");
                IntruderRuptureGeometry.ValidateSource(r.backupR5);
                IntruderRuptureGeometry.ValidateResult(r.backupR5, r.variantR6);
            }
        }
        if (requireCurrentGeometry)
        {
            c.mouthShader = Shader.Find("EchoProtocol/IntruderRuptureMouth");
            Require(c.mouthShader && !ShaderUtil.ShaderHasError(c.mouthShader), "Import the error-free IntruderRuptureMouth shader before upgrading.");
            var skin = c.installation ? c.installation.roundFiveMaterials[0] : c.renderer.sharedMaterials[0];
            Require(skin.HasProperty("_IntruderRupture") && !ShaderUtil.ShaderHasError(skin.shader),
                "Import the updated EmbestidaSurface shader with _IntruderRupture before upgrading.");
            if (!c.installation) IntruderRuptureGeometry.ValidateSource(c.face);
        }
        return c;
    }

    static void ValidateRoundFive(Context c, bool requireCurrentGeometry)
    {
        var r = c.roundFive;
        Require(c.scan && c.face && c.renderer.sharedMesh == c.face && r && r.revision == 1
            && r.scenePath == c.scene.path && r.rendererId == Id(c.renderer) && r.installedMesh == c.face
            && r.sourceMeshHash == DependencyHash(Source) && !EditorUtility.IsDirty(r)
            && r.roundFour && AssetDatabase.GetAssetPath(r.roundFour) == Private + "IntruderFace/Installation.asset"
            && !EditorUtility.IsDirty(r.roundFour) && r.roundFour.revision == 1 && r.roundFour.scenePath == c.scene.path
            && r.roundFour.rendererId == Id(c.renderer) && r.roundFour.faceMesh == c.face
            && r.roundFour.sourceMeshHash == r.sourceMeshHash
            && r.roundFour.faceMaterials != null && r.roundFour.faceMaterials.Length == 2
            && r.roundFour.faceMaterials.All(m => m)
            && AssetDatabase.GetAssetPath(r.roundFour.faceMaterials[0]) == Private + "IntruderFace/Skin.mat"
            && AssetDatabase.GetAssetPath(r.roundFour.faceMaterials[1]) == Private + "IntruderFace/Eyes.mat"
            && r.backupR4 && AssetDatabase.GetAssetPath(r.backupR4) == Private + "IntruderAnatomy/BackupR4.asset"
            && r.variantR5 && AssetDatabase.GetAssetPath(r.variantR5) == Private + "IntruderAnatomy/VariantR5.asset"
            && !EditorUtility.IsDirty(r.backupR4) && !EditorUtility.IsDirty(r.variantR5)
            && !string.IsNullOrEmpty(r.roundFiveFingerprint) && r.roundFiveFingerprint != r.roundFourFingerprint,
            "Expected the exact installed R5 anatomy receipt, immutable recovery meshes and unchanged Nathan source. Do not rerun a historical generator.");
        Require(Fingerprint(r.backupR4) == r.roundFourFingerprint && Fingerprint(r.variantR5) == r.roundFiveFingerprint,
            "R5 recovery meshes no longer match the R5 receipt; resolve before applying rupture.");
        if (requireCurrentGeometry)
        {
            Require(r.geometrySourceHash == FileHash(RoundFiveGeometry), "The R5 geometry source differs from its receipt. R6 must start from the exact approved R5 generator identity.");
            IntruderFaceGeometry.ValidatePair(r.variantR5, c.scan);
        }
    }

    static void ValidateMaterials(IntruderRuptureInstallation r)
    {
        Require(r.roundFiveMaterials != null && r.roundFiveMaterials.Length == 2 && r.roundSixMaterials != null
            && r.roundSixMaterials.Length == 3 && r.roundFiveMaterials.SequenceEqual(r.roundFive.roundFour.faceMaterials)
            && r.roundFiveMaterialHashes != null && r.roundFiveMaterialHashes.Length == 2
            && r.roundSixMaterialHashes != null && r.roundSixMaterialHashes.Length == 3
            && r.roundSixMaterials[0] && AssetDatabase.GetAssetPath(r.roundSixMaterials[0]) == Skin
            && r.roundSixMaterials[1] == r.roundFiveMaterials[1] && r.roundSixMaterials[2]
            && AssetDatabase.GetAssetPath(r.roundSixMaterials[2]) == Mouth,
            "Rupture material receipt/assignments changed; nothing was overwritten.");
        for (int i = 0; i < 2; i++) Require(SavedMaterialMatches(r.roundFiveMaterials[i], r.roundFiveMaterialHashes[i]),
            "Original R5 material changed after rupture installation; preserve the edit and reconcile before integrating.");
        for (int i = 0; i < 3; i++) Require(SavedMaterialMatches(r.roundSixMaterials[i], r.roundSixMaterialHashes[i]),
            "Private R6 material changed after rupture installation; preserve the edit and reconcile before integrating.");
    }

    static void CreateReceipt(Context c, List<string> created, List<Object> temporary)
    {
        if (!AssetDatabase.IsValidFolder(Output.TrimEnd('/')))
            Require(!string.IsNullOrEmpty(AssetDatabase.CreateFolder(Private.TrimEnd('/'), "IntruderRupture")), "Could not create private rupture folder.");
        var backup = Object.Instantiate(c.face); temporary.Add(backup);
        backup.name = "Intruder rupture / immutable R5 recovery";
        var variant = IntruderRuptureGeometry.Create(backup); temporary.Add(variant);
        IntruderRuptureGeometry.ValidateResult(backup, variant);
        string before = Fingerprint(backup), after = Fingerprint(variant);
        Require(before == Fingerprint(c.face) && before == c.roundFive.roundFiveFingerprint && before != after,
            "Rupture generation did not preserve its R5 source or produce a distinct R6 mesh.");
        Require(variant.subMeshCount == 3, "R6 requires existing body/eye slots and the third oral submesh.");
        SaveNew(backup, Backup, created); SaveNew(variant, Variant, created);
        var materials = c.renderer.sharedMaterials;
        var skin = new Material(materials[0]); temporary.Add(skin);
        EditorUtility.CopySerialized(materials[0], skin);
        skin.name = "Intruder rupture / private R6 skin"; skin.SetFloat("_IntruderRupture", 1);
        var mouth = new Material(c.mouthShader) { name = "Intruder rupture / oral volume" }; temporary.Add(mouth);
        mouth.SetTexture("_BaseMap", skin.GetTexture("_BaseMap"));
        mouth.SetColor("_BaseColor", skin.GetColor("_BaseColor"));
        SaveNew(skin, Skin, created); SaveNew(mouth, Mouth, created);
        var r = ScriptableObject.CreateInstance<IntruderRuptureInstallation>(); temporary.Add(r);
        r.revision = Revision; r.scenePath = c.scene.path; r.rendererId = Id(c.renderer);
        r.sourceMeshHash = DependencyHash(Source); r.geometrySourceHash = FileHash(Geometry);
        r.roundFive = c.roundFive; r.roundFiveReceiptHash = FileHash(RoundFiveReceipt);
        r.installedMesh = c.face; r.backupR5 = backup; r.variantR6 = variant;
        r.roundFiveFingerprint = before; r.roundSixFingerprint = after;
        r.roundFiveBounds = c.renderer.localBounds;
        var bounds = r.roundFiveBounds; bounds.Encapsulate(variant.bounds); r.roundSixBounds = bounds;
        r.roundFiveMaterials = materials.ToArray(); r.roundSixMaterials = new[] { skin, materials[1], mouth };
        r.roundFiveMaterialHashes = r.roundFiveMaterials.Select(MaterialFileHash).ToArray();
        r.roundSixMaterialHashes = r.roundSixMaterials.Select(MaterialFileHash).ToArray();
        SaveNew(r, Receipt, created); c.installation = r;
    }

    static void Apply(Context c, Mesh mesh, Material[] materials, Bounds bounds)
    {
        Undo.RegisterCompleteObjectUndo(new Object[] { c.face, c.renderer }, "Replace installed intruder rupture");
        string name = c.face.name;
        // CopySerialized preserves the asset identity but does not guarantee
        // invalidation of the renderer's old skinned vertex layout. R6 adds
        // UV1 and changes the stride. Release that binding before replacing it.
        c.renderer.sharedMesh = null;
        c.face.Clear(false);
        RebuildMeshData(mesh, c.face); c.face.name = name;
        c.renderer.sharedMaterials = materials.ToArray();
        c.renderer.sharedMesh = c.face;
        c.face.MarkModified();
        c.renderer.localBounds = bounds;
        EditorUtility.SetDirty(c.face); AssetDatabase.SaveAssetIfDirty(c.face);
        PrefabUtility.RecordPrefabInstancePropertyModifications(c.renderer);
        EditorSceneManager.MarkSceneDirty(c.scene);
    }

    // Repair the already-applied R6 without regenerating geometry, changing
    // fingerprints or taking another backup. Useful after the stride failure.
    [MenuItem("Echo Protocol/Intruder rupture/4. Repair current R6 mesh buffers")]
    public static void RepairCurrentSceneBuffers()
    {
        var c = Validate(false);
        Require(c.installation && c.installed, "Buffer repair requires the known, currently assigned R6 mesh/materials.");
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Rebuild intruder R6 native mesh buffers");
        try
        {
            Apply(c, c.installation.variantR6, c.installation.roundSixMaterials, c.installation.roundSixBounds);
            Require(Validate(false).installed, "R6 buffer repair changed the saved content fingerprint.");
            Undo.CollapseUndoOperations(group);
            SceneView.RepaintAll();
            Debug.Log("R6 buffers rebuilt from the existing immutable variant; Face.asset GUID, geometry fingerprint and receipts unchanged. Scene UNSAVED. Inspect the affected renderer before saving the scene explicitly.", c.renderer);
        }
        catch (Exception failure)
        {
            RestoreFailedTransaction(c, group, failure);
            throw;
        }
    }

    static void RebuildMeshData(Mesh source, Mesh destination)
    {
        Require(source != destination && source.blendShapeCount == 0, "Mesh rebuild requires a separate immutable, unblended snapshot.");
        // Use public Mesh setters to allocate fresh native buffers. CopySerialized
        // copied valid CPU arrays but retained a stale skinned layout in central
        // Play. Keep dimensions exact: the content fingerprint includes them.
        destination.indexFormat = source.indexFormat;
        destination.vertices = source.vertices;
        destination.normals = source.normals;
        destination.tangents = source.tangents;
        var colors = source.colors;
        if (colors.Length > 0) destination.colors = colors;
        var values = new List<Vector4>();
        for (int channel = 0; channel < 8; channel++)
        {
            var attribute = (VertexAttribute)((int)VertexAttribute.TexCoord0 + channel);
            if (!source.HasVertexAttribute(attribute)) continue;
            values.Clear(); source.GetUVs(channel, values);
            int dimension = source.GetVertexAttributeDimension(attribute);
            if (dimension == 2) destination.SetUVs(channel, values.Select(v => new Vector2(v.x, v.y)).ToList());
            else if (dimension == 3) destination.SetUVs(channel, values.Select(v => new Vector3(v.x, v.y, v.z)).ToList());
            else if (dimension == 4) destination.SetUVs(channel, values);
            else throw new InvalidOperationException("Unexpected UV dimension in immutable intruder snapshot.");
        }
        destination.bindposes = source.bindposes;
        destination.boneWeights = source.boneWeights;
        destination.subMeshCount = source.subMeshCount;
        for (int sub = 0; sub < source.subMeshCount; sub++)
            destination.SetIndices(source.GetIndices(sub), source.GetTopology(sub), sub, false);
        destination.bounds = source.bounds;
        Require(Fingerprint(destination) == Fingerprint(source), "Native buffer reconstruction did not preserve the mesh content.");
        destination.UploadMeshData(false);
    }

    static void RestoreFailedTransaction(Context c, int group, Exception failure)
    {
        try
        {
            Undo.RevertAllDownToGroup(group);
            EditorUtility.SetDirty(c.face); AssetDatabase.SaveAssetIfDirty(c.face);
        }
        catch (Exception recoveryFailure)
        {
            throw new AggregateException("Rupture operation failed and recovery could not be saved. Private recovery assets were retained.", failure, recoveryFailure);
        }
    }

    static bool MatchesBounds(SkinnedMeshRenderer renderer, Bounds bounds)
        // The installed actor updates bounds from the skinned pose offscreen.
        => renderer.updateWhenOffscreen || (renderer.localBounds.center - bounds.center).sqrMagnitude < .00000001f
            && (renderer.localBounds.extents - bounds.extents).sqrMagnitude < .00000001f;
    static bool SavedMaterialMatches(Material m, string hash) => m && !EditorUtility.IsDirty(m)
        && !string.IsNullOrEmpty(hash) && MaterialFileHash(m) == hash;
    // Hash the saved material only, excluding shader/texture dependencies so
    // coordinated shader changes never invalidate the immutable R5 receipt.
    static string MaterialFileHash(Material material) => FileHash(AssetDatabase.GetAssetPath(material));
    static string Id(Object value) => GlobalObjectId.GetGlobalObjectIdSlow(value).ToString();
    static string DependencyHash(string path) => AssetDatabase.GetAssetDependencyHash(path).ToString();
    static bool Exists(string path) => File.Exists(path) || File.Exists(path + ".meta") || AssetDatabase.LoadMainAssetAtPath(path);
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    static string FileHash(string path)
    {
        using (var stream = File.OpenRead(path))
        using (var hash = SHA256.Create())
            return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }
    static void SaveNew(Object asset, string path, List<string> created)
    {
        Require(!Exists(path), "Refusing to overwrite " + path);
        AssetDatabase.CreateAsset(asset, path); created.Add(path); AssetDatabase.SaveAssetIfDirty(asset);
    }

    // This algorithm is intentionally identical to R5's fingerprint. It must
    // match the R5 receipt byte-for-byte; no call edits that older installer.
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
