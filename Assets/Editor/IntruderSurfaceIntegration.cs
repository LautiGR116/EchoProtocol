using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class IntruderSurfaceIntegration
{
    const string Private = "Assets/_Game/Art/Embodiment/RenderpeoplePrivate/";
    const string Face = Private + "IntruderFace/";
    const string Output = Private + "IntruderDreadSurface/";
    const string Receipt = Output + "Installation.asset";
    const string OptIn = "_IntruderDreadSurface";
    const int Revision = 1;
    static readonly string[] TargetPaths = { Face + "Skin.mat", Face + "Eyes.mat", Private + "Embestida/RubberBib.mat" };
    static readonly string[] BackupNames = { "Skin.Round4.mat", "Eyes.Round4.mat", "RubberBib.Round4.mat" };

    sealed class Context
    {
        public Scene scene;
        public SkinnedMeshRenderer body, bib;
        public Material[] materials;
        public Shader surfaceShader, eyeShader;
        public IntruderSurfaceInstallation receipt;
        public bool installed;
    }

    [MenuItem("Echo Protocol/Intruder surface/1. Validate current scene")]
    public static void ValidateCurrentScene()
    {
        var c = Validate(true);
        Debug.Log(c.installed ? "Intruder surface round 5 valid; Upgrade is a no-op."
            : "Intruder surface ready: three exclusive private materials, scan coordinates and texture references verified. Upgrade preserves renderer/mesh/bone references and the round-four receipt.", c.body);
    }

    [MenuItem("Echo Protocol/Intruder surface/2. Upgrade current scene")]
    public static void UpgradeCurrentScene()
    {
        var c = Validate(true);
        if (c.installed)
        {
            SaveMaterials(c.materials); // Persist a preceding Redo, if still dirty.
            Debug.Log("Intruder surface round 5 already installed; no material values changed.", c.body); return;
        }
        var created = new List<string>();
        var temporary = new List<Object>();
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Upgrade intruder surface to round 5");
        try
        {
            if (!c.receipt) CreateReceipt(c, created, temporary);
            RestoreMaterials(c.materials, c.receipt.surfaceMaterials, "Upgrade private intruder materials");
            if (!Validate(true).installed) throw new InvalidOperationException("Intruder surface upgrade did not validate.");
            SaveMaterials(c.materials);
            Undo.CollapseUndoOperations(group);
            SceneView.RepaintAll();
            Debug.Log("Intruder surface round 5 saved into the same three private material objects. No scene opened/saved, renderer reassigned or mesh/bone changed. Undo or Intruder surface > 3. Roll back restores full prior material contents, including the eye shader. Central visual check remains required.", c.body);
        }
        catch (Exception failure)
        {
            // If a save failed midway, persist the reverted state before deleting
            // new backups. If this also fails, retain those recovery assets.
            try { Undo.RevertAllDownToGroup(group); SaveMaterials(c.materials); }
            catch (Exception recoveryFailure)
            {
                throw new AggregateException("Surface upgrade failed and recovery could not be saved. Private backups were retained.", failure, recoveryFailure);
            }
            for (int i = created.Count - 1; i >= 0; i--) AssetDatabase.DeleteAsset(created[i]);
            throw;
        }
        finally
        {
            foreach (var item in temporary)
                if (item && !EditorUtility.IsPersistent(item)) Object.DestroyImmediate(item);
        }
    }

    [MenuItem("Echo Protocol/Intruder surface/3. Roll back current scene")]
    public static void RollbackCurrentScene()
    {
        // Rollback deliberately does not inspect mesh identity, UV data or the
        // new shader contract: an independent A upgrade cannot block recovery.
        var c = Validate(false);
        if (!c.receipt || !c.installed)
        {
            if (c.receipt) SaveMaterials(c.materials); // Persist a preceding Undo.
            Debug.Log("Intruder surface is already at its round-four state; no material values changed.", c.body); return;
        }
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Restore intruder round-four materials");
        try
        {
            RestoreMaterials(c.materials, c.receipt.previousMaterials, "Restore private intruder materials");
            if (Validate(false).installed) throw new InvalidOperationException("Intruder surface rollback did not validate.");
            SaveMaterials(c.materials);
            Undo.CollapseUndoOperations(group);
            SceneView.RepaintAll();
            Debug.Log("Round-four material contents/shaders restored and saved in place. Backups retained for Redo/reapply. Scene, anatomy, acting and the original face receipt remain untouched.", c.body);
        }
        catch (Exception failure)
        {
            try { Undo.RevertAllDownToGroup(group); SaveMaterials(c.materials); }
            catch (Exception recoveryFailure)
            {
                throw new AggregateException("Surface rollback failed and recovery could not be saved. Private backups were retained.", failure, recoveryFailure);
            }
            throw;
        }
    }

    static Context Validate(bool requireSurfaceContract)
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling,
            "Stop Play and finish import before integrating intruder surface.");
        var c = new Context { scene = SceneManager.GetActiveScene() };
        Require(c.scene.IsValid() && c.scene.isLoaded && c.scene.path == "Assets/_Game/Scenes/Production/EchoProtocol_Incursion.unity",
            "Use the existing open Incursion scene. This tool never opens or saves scenes.");
        var threats = All<MachineCoverThreat>(c.scene).ToArray();
        Require(threats.Length == 1, "Expected the one present Incursion threat.");
        var threat = threats[0];
        var performance = threat.GetComponent<MachineThreatPerformance>();
        var anatomy = performance ? new SerializedObject(performance).FindProperty("anatomy")?.objectReferenceValue as RecordedCharacterMotion : null;
        Require(anatomy && anatomy.IsPresentThreat && threat.GetComponent<IntruderStaging>()
            && anatomy.transform.IsChildOf(threat.transform) && !anatomy.GetComponentInParent<EchoPlayback>()
            && !anatomy.GetComponentInParent<LoopActor>(), "Only the installed present intruder may receive these materials; never an Echo or witness.");
        var bodies = threat.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(r => r.name == "RecordedAdultSurface").ToArray();
        Require(bodies.Length == 1, "Expected one installed RecordedAdultSurface renderer.");
        c.body = bodies[0];
        Require(c.body.transform.IsChildOf(anatomy.transform) && c.body.sharedMaterials.Length == 2,
            "The present intruder's body/material slots changed; nothing was overwritten.");
        c.materials = TargetPaths.Select(AssetDatabase.LoadAssetAtPath<Material>).ToArray();
        Require(c.materials.All(m => m) && c.body.sharedMaterials.SequenceEqual(c.materials.Take(2)),
            "Install/retain the private round-four Skin/Eyes material references first.");
        var bibs = threat.GetComponentsInChildren<SkinnedMeshRenderer>(true)
            .Where(r => r.sharedMaterials.Contains(c.materials[2])).ToArray();
        Require(bibs.Length == 1, "Expected the single private RubberBib material on the present actor.");
        c.bib = bibs[0];
        Require(c.bib != c.body && c.bib.transform.IsChildOf(anatomy.transform)
            && c.bib.sharedMaterials.Length == 1, "The private bib renderer/material assignment changed.");
        var faceReceipt = AssetDatabase.LoadAssetAtPath<IntruderFaceInstallation>(Face + "Installation.asset");
        Require(faceReceipt && faceReceipt.revision == 1 && faceReceipt.scenePath == c.scene.path
            && faceReceipt.rendererId == Id(c.body) && faceReceipt.faceMaterials != null
            && faceReceipt.faceMaterials.SequenceEqual(c.materials.Take(2)),
            "The existing round-four face receipt must still own these exact material references. It will not be edited.");
        ValidateExclusiveUsage(c);
        c.receipt = AssetDatabase.LoadAssetAtPath<IntruderSurfaceInstallation>(Receipt);
        if (c.receipt)
        {
            var r = c.receipt;
            Require(r.revision == Revision && r.scenePath == c.scene.path && r.rendererId == Id(c.body)
                && r.bibRendererId == Id(c.bib) && r.targets != null && r.targets.SequenceEqual(c.materials)
                && r.previousMaterials != null && r.previousMaterials.Length == 3
                && r.surfaceMaterials != null && r.surfaceMaterials.Length == 3,
                "Intruder surface receipt does not match this actor; nothing was overwritten.");
            for (int i = 0; i < 3; i++)
                Require(r.previousMaterials[i] && AssetDatabase.GetAssetPath(r.previousMaterials[i]) == Output + BackupNames[i]
                    && r.surfaceMaterials[i] && AssetDatabase.GetAssetPath(r.surfaceMaterials[i]) == Receipt
                    && AssetDatabase.IsSubAsset(r.surfaceMaterials[i]), "Missing/altered private surface backup or expected state.");
            bool previous = Matches(c.materials, r.previousMaterials);
            c.installed = Matches(c.materials, r.surfaceMaterials);
            Require(previous != c.installed, "Private material drift or a partial installation was detected. Undo the unrelated material edits before retrying; nothing was overwritten.");
        }
        else
        {
            foreach (string path in BackupNames.Select(n => Output + n).Append(Receipt))
                Require(!File.Exists(path) && !AssetDatabase.LoadMainAssetAtPath(path), "Unclaimed IntruderDreadSurface asset exists: " + path);
        }
        if (requireSurfaceContract)
        {
            c.surfaceShader = Shader.Find("EchoProtocol/EmbestidaSurface");
            c.eyeShader = Shader.Find("EchoProtocol/IntruderDreadEyes");
            RequireProperties(c.surfaceShader, "_BaseMap", "_BaseColor", "_BumpMap", "_BumpScale", "_Apron", "_IntruderFace", OptIn);
            RequireProperties(c.eyeShader, "_BaseMap", "_BaseColor", "_BumpMap", "_BumpScale", "_Smoothness", OptIn);
            CheckCoordinates(c.body.sharedMesh); CheckCoordinates(c.bib.sharedMesh);
            var previous = c.receipt ? c.receipt.previousMaterials : c.materials;
            Require(previous[0].shader == c.surfaceShader && previous[2].shader == c.surfaceShader
                && previous[0].GetFloat("_IntruderFace") == 1 && previous[0].GetFloat("_Apron") == 0
                && previous[2].GetFloat("_Apron") == 1
                && previous[0].GetFloat(OptIn) == 0 && previous[2].GetFloat(OptIn) == 0
                && previous[1].shader && previous[1].shader.name == "Universal Render Pipeline/Lit",
                "Expected the installed round-four skin, lit eyes and rubber bib before this explicit opt-in.");
            foreach (var material in previous.Take(2))
                foreach (string property in new[] { "_BaseMap", "_BumpMap" })
                    Require(material.HasProperty(property) && material.GetTexture(property)
                        && AssetDatabase.GetAssetPath(material.GetTexture(property)).StartsWith(Private, StringComparison.Ordinal),
                        "Missing licensed private scan texture: " + material.name + " / " + property);
            if (c.receipt)
                for (int i = 0; i < 3; i++)
                {
                    var expected = Clone(previous[i]);
                    try
                    {
                        Configure(expected, i, c.eyeShader);
                        Require(SameContents(expected, c.receipt.surfaceMaterials[i]), "Saved surface state differs from this upgrade contract; restore or review the receipt before changing revisions.");
                    }
                    finally { Object.DestroyImmediate(expected); }
                }
        }
        return c;
    }

    static void ValidateExclusiveUsage(Context c)
    {
        // Include inactive renderers in every loaded scene, not only the active
        // threat: mutating a shared material must never affect another actor.
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.IsValid() || !scene.isLoaded) continue;
            foreach (var renderer in All<Renderer>(scene))
                for (int index = 0; index < c.materials.Length; index++)
                    if (renderer.sharedMaterials.Contains(c.materials[index]))
                        Require(renderer == (index == 2 ? c.bib : c.body),
                            "A private intruder material is also used by " + renderer.name + " in " + scene.path + ". Refusing shared edits.");
        }
    }

    static void CheckCoordinates(Mesh mesh)
    {
        Require(mesh && mesh.HasVertexAttribute(VertexAttribute.TexCoord0)
            && mesh.GetVertexAttributeDimension(VertexAttribute.TexCoord2) == 3,
            "Intruder body/bib must retain UV0 and original 3D scan coordinates in UV channel 2 (TEXCOORD2). No exact face mesh identity is required.");
    }

    static void CreateReceipt(Context c, List<string> created, List<Object> temporary)
    {
        if (!AssetDatabase.IsValidFolder(Output.TrimEnd('/')))
            Require(!string.IsNullOrEmpty(AssetDatabase.CreateFolder(Private.TrimEnd('/'), "IntruderDreadSurface")), "Could not create private surface backup folder.");
        var receipt = ScriptableObject.CreateInstance<IntruderSurfaceInstallation>(); temporary.Add(receipt);
        receipt.revision = Revision; receipt.scenePath = c.scene.path;
        receipt.rendererId = Id(c.body); receipt.bibRendererId = Id(c.bib);
        receipt.targets = c.materials.ToArray();
        receipt.previousMaterials = new Material[3]; receipt.surfaceMaterials = new Material[3];
        for (int i = 0; i < 3; i++)
        {
            var previous = Clone(c.materials[i]); temporary.Add(previous);
            SaveNew(previous, Output + BackupNames[i], created);
            receipt.previousMaterials[i] = previous;
            var surface = Clone(c.materials[i]); temporary.Add(surface);
            Configure(surface, i, c.eyeShader);
            receipt.surfaceMaterials[i] = surface;
        }
        SaveNew(receipt, Receipt, created);
        foreach (var surface in receipt.surfaceMaterials) AssetDatabase.AddObjectToAsset(surface, receipt);
        EditorUtility.SetDirty(receipt); AssetDatabase.SaveAssetIfDirty(receipt);
        c.receipt = receipt;
    }

    static void Configure(Material material, int index, Shader eyeShader)
    {
        if (index == 1)
        {
            // Shader replacement retains the original scan and both transforms.
            var maps = new[] { "_BaseMap", "_BumpMap" };
            var textures = maps.Select(material.GetTexture).ToArray();
            var scales = maps.Select(material.GetTextureScale).ToArray();
            var offsets = maps.Select(material.GetTextureOffset).ToArray();
            material.shader = eyeShader;
            for (int i = 0; i < maps.Length; i++)
            {
                material.SetTexture(maps[i], textures[i]);
                material.SetTextureScale(maps[i], scales[i]);
                material.SetTextureOffset(maps[i], offsets[i]);
            }
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Smoothness", .78f); material.SetFloat("_BumpScale", 0);
        }
        material.SetFloat(OptIn, 1);
    }

    static Material Clone(Material source)
    {
        var copy = new Material(source);
        EditorUtility.CopySerialized(source, copy);
        return copy;
    }

    static void RestoreMaterials(Material[] targets, Material[] states, string label)
    {
        Undo.RegisterCompleteObjectUndo(targets.Cast<Object>().ToArray(), label);
        for (int i = 0; i < targets.Length; i++)
        {
            string name = targets[i].name;
            EditorUtility.CopySerialized(states[i], targets[i]);
            targets[i].name = name;
            EditorUtility.SetDirty(targets[i]);
        }
    }

    static bool Matches(Material[] targets, Material[] states)
        => targets.Zip(states, SameContents).All(equal => equal);

    static bool SameContents(Material left, Material right)
    {
        // Compare native serialized data, including shader, all saved texture
        // transforms, keywords, render state and hidden properties. Names can
        // differ because Unity names standalone backups after their filenames.
        using var a = new SerializedObject(left);
        using var b = new SerializedObject(right);
        var x = a.GetIterator(); var y = b.GetIterator(); bool first = true;
        while (x.Next(first))
        {
            if (!y.Next(first) || x.propertyPath != y.propertyPath) return false;
            first = false;
            if (x.propertyPath == "m_Name" || x.propertyPath == "m_ObjectHideFlags") continue;
            if (!SerializedProperty.DataEquals(x, y)) return false;
        }
        return !y.Next(false);
    }

    static void SaveMaterials(IEnumerable<Material> materials)
    {
        foreach (var material in materials) AssetDatabase.SaveAssetIfDirty(material);
    }

    static void SaveNew(Object asset, string path, List<string> created)
    {
        Require(!File.Exists(path) && !AssetDatabase.LoadMainAssetAtPath(path), "Refusing to overwrite " + path);
        AssetDatabase.CreateAsset(asset, path); created.Add(path);
        AssetDatabase.SaveAssetIfDirty(asset);
    }

    static void RequireProperties(Shader shader, params string[] names)
        => Require(shader && names.All(name => shader.FindPropertyIndex(name) >= 0), "Import both round-five intruder surface shaders before upgrading.");
    static string Id(Object value) => GlobalObjectId.GetGlobalObjectIdSlow(value).ToString();
    static IEnumerable<T> All<T>(Scene scene) where T : Component
        => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true));
    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
