using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class IntruderFaceIntegration
{
    const string Private = "Assets/_Game/Art/Embodiment/RenderpeoplePrivate/";
    const string Source = Private + "Nathan/ArticulatedSurface.asset";
    const string Original = Private + "Embestida/Body.asset";
    const string Output = Private + "IntruderFace/";
    const string Receipt = Output + "Installation.asset";
    const int Revision = 1;

    sealed class Context
    {
        public Scene scene;
        public SkinnedMeshRenderer renderer;
        public Mesh scan, body;
        public Shader shader;
        public IntruderFaceInstallation installation;
        public bool installed;
    }

    [MenuItem("Echo Protocol/Intruder face/1. Validate current scene")]
    public static void ValidateCurrentScene()
    {
        var c = Validate();
        Debug.Log(c.installed ? "IntruderFace valid; Apply is a no-op."
            : "IntruderFace ready: restore adult head volume, compress lids/mouth, isolate skin/eyes. No bones, lights or scene save.", c.renderer);
    }

    [MenuItem("Echo Protocol/Intruder face/2. Apply to current scene")]
    public static void ApplyToCurrentScene()
    {
        var c = Validate();
        if (c.installed) { Debug.Log("IntruderFace already installed; nothing changed.", c.renderer); return; }
        var created = new List<string>();
        var temporary = new List<Object>();
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Restore intruder facial volume");
        try
        {
            var receipt = c.installation;
            if (!receipt)
            {
                if (!AssetDatabase.IsValidFolder(Output.TrimEnd('/')))
                    AssetDatabase.CreateFolder(Private.TrimEnd('/'), "IntruderFace");
                var mesh = IntruderFaceGeometry.Create(c.body, c.scan); temporary.Add(mesh);
                var skin = new Material(c.renderer.sharedMaterials[0]) { name = "IntruderFace / scan skin" }; temporary.Add(skin);
                skin.SetFloat("_IntruderFace", 1);
                var eyes = new Material(c.renderer.sharedMaterials[1]) { name = "IntruderFace / seated eyes" }; temporary.Add(eyes);
                // Retain the licensed iris/sclera and physical reflections, with
                // less broad plastic gloss and no skin-scale corneal normal.
                eyes.SetFloat("_Smoothness", .62f); eyes.SetFloat("_BumpScale", .04f);
                eyes.SetColor("_BaseColor", new Color(.94f, .93f, .91f, 1));
                SaveNew(mesh, Output + "Face.asset", created);
                SaveNew(skin, Output + "Skin.mat", created);
                SaveNew(eyes, Output + "Eyes.mat", created);
                receipt = ScriptableObject.CreateInstance<IntruderFaceInstallation>(); temporary.Add(receipt);
                receipt.revision = Revision;
                receipt.scenePath = c.scene.path;
                receipt.rendererId = GlobalObjectId.GetGlobalObjectIdSlow(c.renderer).ToString();
                receipt.sourceMeshHash = AssetDatabase.GetAssetDependencyHash(Source).ToString();
                receipt.originalMesh = c.renderer.sharedMesh;
                receipt.originalMaterials = c.renderer.sharedMaterials;
                receipt.originalBounds = c.renderer.localBounds;
                receipt.faceMesh = mesh;
                receipt.faceMaterials = new[] { skin, eyes };
                var bounds = receipt.originalBounds; bounds.Encapsulate(mesh.bounds);
                receipt.faceBounds = bounds;
                SaveNew(receipt, Receipt, created);
            }
            Assign(c, receipt.faceMesh, receipt.faceMaterials, receipt.faceBounds);
            // Read back the complete state before accepting the transaction.
            if (!Validate().installed) throw new InvalidOperationException("IntruderFace assignment did not validate.");
            Undo.CollapseUndoOperations(group);
            Debug.Log("IntruderFace applied to the one present actor; scene UNSAVED. Private mesh/materials saved. Undo or Intruder face > 3. Roll back restores prior renderer references. Central check: one close P3 view and pump entrance; no full route.", c.renderer);
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            for (int i = created.Count - 1; i >= 0; i--) AssetDatabase.DeleteAsset(created[i]);
            throw;
        }
        finally
        {
            foreach (var asset in temporary)
                if (asset && !EditorUtility.IsPersistent(asset)) Object.DestroyImmediate(asset);
        }
    }

    [MenuItem("Echo Protocol/Intruder face/3. Roll back current scene")]
    public static void RollbackCurrentScene()
    {
        var c = Validate();
        if (!c.installed) { Debug.Log("IntruderFace is not assigned; nothing changed.", c.renderer); return; }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Roll back intruder face");
        try
        {
            Assign(c, c.installation.originalMesh, c.installation.originalMaterials, c.installation.originalBounds);
            Undo.CollapseUndoOperations(group);
            Debug.Log("Previous intruder mesh/materials restored; scene UNSAVED. Private generated assets retained for Redo/reapply.", c.renderer);
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    static Context Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("Stop Play and finish script import before applying IntruderFace.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        if (!c.scene.IsValid() || !c.scene.isLoaded || !c.scene.path.EndsWith("/EchoProtocol_Incursion.unity", StringComparison.Ordinal))
            throw new InvalidOperationException("Use the existing open Incursion scene. This tool never opens or saves scenes.");
        var threat = c.scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MachineCoverThreat>(true)).Single();
        var performance = threat.GetComponent<MachineThreatPerformance>();
        var anatomy = performance ? new SerializedObject(performance).FindProperty("anatomy")?.objectReferenceValue as RecordedCharacterMotion : null;
        if (!anatomy || !anatomy.IsPresentThreat || !threat.GetComponent<IntruderStaging>()
            || anatomy.GetComponentInParent<EchoPlayback>() || anatomy.GetComponentInParent<LoopActor>())
            throw new InvalidOperationException("IntruderFace requires the one existing present Nathan, never an Echo or witness copy.");
        c.renderer = threat.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single(r => r.name == "RecordedAdultSurface");
        c.scan = AssetDatabase.LoadAssetAtPath<Mesh>(Source);
        c.body = AssetDatabase.LoadAssetAtPath<Mesh>(Original);
        IntruderFaceGeometry.ValidatePair(c.body, c.scan);
        if (!c.renderer.transform.IsChildOf(anatomy.transform) || c.renderer.bones.Length != c.scan.bindposes.Length
            || c.renderer.bones.Any(b => !b) || c.renderer.sharedMaterials.Length != 2)
            throw new InvalidOperationException("IntruderFace renderer/rig/material slots differ from the prepared Nathan.");
        c.shader = Shader.Find("EchoProtocol/EmbestidaSurface");
        if (!c.shader || c.shader.FindPropertyIndex("_IntruderFace") < 0)
            throw new InvalidOperationException("Import the round-four EmbestidaSurface shader before applying.");
        c.installation = AssetDatabase.LoadAssetAtPath<IntruderFaceInstallation>(Receipt);
        if (c.installation)
        {
            var r = c.installation;
            if (r.revision != Revision || r.scenePath != c.scene.path
                || r.rendererId != GlobalObjectId.GetGlobalObjectIdSlow(c.renderer).ToString()
                || r.sourceMeshHash != AssetDatabase.GetAssetDependencyHash(Source).ToString()
                || r.originalMesh != c.body || !r.faceMesh || r.originalMaterials == null || r.originalMaterials.Length != 2
                || r.faceMaterials == null || r.faceMaterials.Length != 2 || r.originalMaterials.Any(m => !m) || r.faceMaterials.Any(m => !m)
                || AssetDatabase.GetAssetPath(r.faceMesh) != Output + "Face.asset"
                || AssetDatabase.GetAssetPath(r.faceMaterials[0]) != Output + "Skin.mat"
                || AssetDatabase.GetAssetPath(r.faceMaterials[1]) != Output + "Eyes.mat"
                || r.faceMaterials[0].shader != c.shader || r.faceMaterials[0].GetFloat("_IntruderFace") != 1)
                throw new InvalidOperationException("IntruderFace receipt/source was changed. Resolve the partial installation without overwriting assets.");
            IntruderFaceGeometry.ValidatePair(r.faceMesh, c.scan);
            c.installed = Matches(c.renderer, r.faceMesh, r.faceMaterials, r.faceBounds);
            if (!c.installed && !Matches(c.renderer, r.originalMesh, r.originalMaterials, r.originalBounds))
                throw new InvalidOperationException("IntruderFace renderer was changed after installation; refusing to overwrite that work.");
        }
        else
        {
            if (c.renderer.sharedMesh != c.body
                || AssetDatabase.GetAssetPath(c.renderer.sharedMaterials[0]) != Private + "Embestida/Body.mat"
                || AssetDatabase.GetAssetPath(c.renderer.sharedMaterials[1]) != Private + "Nathan/M_Eyes.mat"
                || c.renderer.sharedMaterials[0].shader != c.shader)
                throw new InvalidOperationException("IntruderFace expects the existing Embestida body and original Nathan eye material.");
            foreach (string name in new[] { "Face.asset", "Skin.mat", "Eyes.mat", "Installation.asset" })
                if (File.Exists(Output + name) || AssetDatabase.LoadMainAssetAtPath(Output + name))
                    throw new InvalidOperationException("Unclaimed IntruderFace asset exists: " + name + ". Nothing was changed.");
        }
        return c;
    }

    static bool Matches(SkinnedMeshRenderer renderer, Mesh mesh, Material[] materials, Bounds bounds)
        => renderer.sharedMesh == mesh && renderer.sharedMaterials.SequenceEqual(materials)
            // With this existing mode Unity immediately derives the bounds from
            // the skinned pose, rather than retaining an assigned rest-pose box.
            && (renderer.updateWhenOffscreen
                || (renderer.localBounds.center - bounds.center).sqrMagnitude < .00000001f
                && (renderer.localBounds.extents - bounds.extents).sqrMagnitude < .00000001f);

    static void Assign(Context c, Mesh mesh, Material[] materials, Bounds bounds)
    {
        Undo.RegisterCompleteObjectUndo(c.renderer, "Assign intruder face variant");
        c.renderer.sharedMesh = mesh; c.renderer.sharedMaterials = materials; c.renderer.localBounds = bounds;
        PrefabUtility.RecordPrefabInstancePropertyModifications(c.renderer);
        EditorSceneManager.MarkSceneDirty(c.scene);
    }

    static void SaveNew(Object asset, string path, List<string> created)
    {
        if (File.Exists(path) || AssetDatabase.LoadMainAssetAtPath(path)) throw new InvalidOperationException("Refusing to overwrite " + path);
        AssetDatabase.CreateAsset(asset, path); created.Add(path);
        AssetDatabase.SaveAssetIfDirty(asset);
    }
}
