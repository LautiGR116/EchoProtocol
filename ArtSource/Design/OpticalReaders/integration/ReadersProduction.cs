using System;
using System.IO;
using System.Linq;
using EchoProtocol.Campaign;
using EchoProtocol.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class ReadersProduction
{
    public const string ScenePath="Assets/_Game/Scenes/Production/EchoProtocol_Readers.unity";
    public const string RootName="Readers / recessed optical heads";
    const string Art="Assets/_Game/Art/Readers";
    [MenuItem("Echo Protocol/Author optical reader presentation")]
    public static void Author()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play before authoring Readers");
        if(File.Exists(ScenePath)||Directory.Exists(Art))throw new Exception("Preserve existing Readers before authoring again");
        var scene=EditorSceneManager.OpenScene(PaneProduction.ScenePath);
        var readers=Object.FindObjectsByType<OpticalReader>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(r=>r.name).ToArray();
        if(readers.Length!=6)throw new Exception("Expected six genuine optical readers");
        var filters=Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        var contracts=readers.Select(r=>EditorJsonUtility.ToJson(r)).ToArray();
        var colliders=Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        var colliderContracts=colliders.Select(c=>EditorJsonUtility.ToJson(c)).ToArray();
        var audio=Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        var audioContracts=audio.Select(c=>EditorJsonUtility.ToJson(c)).ToArray();
        if(!EditorSceneManager.SaveScene(scene,ScenePath))throw new Exception("Could not derive Readers");
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();
        var enamel=Material("Enamel",new Color(.30f,.34f,.31f),.10f,.35f);
        var carbon=Material("Bezel",new Color(.025f,.031f,.029f),.20f,.32f);
        var glass=Material("FixedOptic",new Color(.11f,.082f,.038f),0,.78f);
        var report="Readers authored from Pane. Original source geometry; six real OpticalReaders, no P-02 placeholder.\n";
        for(int i=0;i<readers.Length;i++)
        {
            var r=readers[i];var s=new SerializedObject(r);
            var indicator=(GameObject)s.FindProperty("activeIndicator").objectReferenceValue;
            var shape=OpticalReaderMeshRecipe.BuildForFinishIndicator(indicator.transform,r.name);
            var body=Near(filters,"Optical enamel body",shape.position+Vector3.right*.12f);
            var bezel=Near(filters,"Optical bezel",shape.position+Vector3.right*.018f);
            var lens=Near(filters,"Optical neutral lens",shape.position);
            var fixedFilters=new[]{body,bezel,lens};var meshes=new[]{shape.housing,shape.bezel,shape.lens};
            var materials=new[]{enamel,carbon,glass};
            for(int j=0;j<3;j++)
            {
                OpticalReaderMeshRecipe.BakeFixedMeshToExistingTransform(meshes[j],shape,fixedFilters[j].transform);
                AssetDatabase.CreateAsset(meshes[j],Art+"/Reader"+i+"_"+j+".asset");
                fixedFilters[j].sharedMesh=meshes[j];fixedFilters[j].GetComponent<Renderer>().sharedMaterial=materials[j];
            }
            AssetDatabase.CreateAsset(shape.activeRing,Art+"/Reader"+i+"_ActiveRing.asset");
            indicator.GetComponent<MeshFilter>().sharedMesh=shape.activeRing;
            if(contracts[i]!=EditorJsonUtility.ToJson(r))throw new Exception("Reader contract changed: "+r.name);
            report+=r.name+": unchanged trigger/facing/qualification/indicator reference; four meshes replaced at original transforms.\n";
        }
        for(int i=0;i<colliders.Length;i++)if(colliderContracts[i]!=EditorJsonUtility.ToJson(colliders[i]))throw new Exception("Collider changed: "+colliders[i].name);
        for(int i=0;i<audio.Length;i++)if(audioContracts[i]!=EditorJsonUtility.ToJson(audio[i]))throw new Exception("Audio source changed: "+audio[i].name);
        // The existing B reminder physically covered its lens at eye level.
        // Lower both text and its backplate together, without changing the contract.
        foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None)
            .Where(t=>t.name=="At B recall instruction"||t.name=="At B recall instruction backing"))
        { var p=t.position;p.y-=.52f;t.position=p; }
        // These are the original circuit-owned fixtures: broad fluorescent spill
        // must reach the reader faces as well as the floor beneath the tube.
        var fixtureNames=new[]{"Panel Light - Cold Ceiling Panel Lane 2","Panel Light - Cold Ceiling Panel Lane 3","Panel Light - Service Ceiling Panel 3"};
        var aim=new[]{new Vector3(-1.55f,.7f,3.5f),new Vector3(-1.55f,.7f,10),new Vector3(3.40f,.7f,9.5f)};
        var lights=Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        for(int i=0;i<fixtureNames.Length;i++)
        {
            var light=lights.Single(l=>l.name==fixtureNames[i]);
            light.transform.LookAt(aim[i]);light.spotAngle=110;light.innerSpotAngle=85;
            if(i==2)light.intensity=1.15f;
            report+=light.name+": existing circuit/source retained, broadened and aimed spill at " + aim[i]+"; intensity="+light.intensity+".\n";
        }
        new GameObject(RootName);
        RefineDomesticMount();
        var director=new SerializedObject(Object.FindFirstObjectByType<CampaignDirector>());
        director.FindProperty("checkpointKey").stringValue="EP.CampaignCheckpoint.Readers.v1";director.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new Exception("Could not save Readers");
        AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Directory.CreateDirectory("Docs/VisualEvidence/2026-10-01-Readers");
        File.WriteAllText("Docs/VisualEvidence/2026-10-01-Readers/authoring.txt",report+colliders.Length+" original collider serializations checked before P2 curtain relocation, and "+audio.Length+" original audio source serializations unchanged.\nB reminder lowered .52 m; three original fixture cones revised; P2 curtain assembly moved .95 m with existing colliders and printed plate. Camera review required for a fresh authoring run.\n");
        Debug.Log("Readers geometry authored; inactive/active actual-input camera comparison pending.");
    }
    // Optional post-camera refinement; also included by Author for reproduction.
    public static void RefineDomesticMount()
    {
        if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=ScenePath)
            throw new Exception("Open Readers in Edit Mode");
        var root=UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().Single(g=>g.name==RootName);
        if(root.transform.Find("P2 / clear mounted optic"))throw new Exception("P2 refinement already exists");
        var marker=new GameObject("P2 / clear mounted optic");marker.transform.SetParent(root.transform);
        var all=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        var curtain=all.Single(t=>t.name=="Domestic Curtain");curtain.position+=Vector3.back*.95f;
        var cloth=curtain.GetComponentsInChildren<MeshFilter>().Single(f=>f.name=="Curtain Panel");
        var vertices=new System.Collections.Generic.List<Vector3>();var uv=new System.Collections.Generic.List<Vector2>();var triangles=new System.Collections.Generic.List<int>();
        const int columns=96,rows=8;
        for(int y=0;y<=rows;y++)for(int z=0;z<=columns;z++)
        {
            float u=(float)z/columns,v=(float)y/rows;
            float wave=Mathf.Cos(u*Mathf.PI*24);float sag=(1-v)*.008f*(1+wave);
            vertices.Add(new Vector3(.40f*wave*(.85f+.15f*(1-v)),v-.5f+sag,u-.5f));uv.Add(new Vector2(u,v));
        }
        for(int y=0;y<rows;y++)for(int z=0;z<columns;z++)
        {int a=y*(columns+1)+z,b=a+columns+1;triangles.AddRange(new[]{a,a+1,b+1,a,b+1,b});}
        // The textile is seen from west; the front winding faces world -X.
        var mesh=new Mesh{name="Original pleated domestic reader curtain"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Art+"/P2_PleatedCurtain.asset");cloth.sharedMesh=mesh;
        var label=all.Single(t=>t.name=="P2 hold instruction").GetComponent<TextMesh>();
        label.transform.position=new Vector3(-.567f,1.98f,23.1f);label.characterSize*=.44f;label.color=new Color(.76f,.74f,.66f);
        var ink=new Material(Shader.Find("EchoProtocol/PrintedText")){name="M_P2PrintedInstruction"};ink.mainTexture=label.font.material.mainTexture;AssetDatabase.CreateAsset(ink,Art+"/M_P2PrintedInstruction.mat");label.GetComponent<Renderer>().sharedMaterial=ink;
        var textOwner=new SerializedObject(label.GetComponent<EchoProtocol.Presentation.DiegeticText>());textOwner.FindProperty("depthTestedShader").objectReferenceValue=ink.shader;textOwner.ApplyModifiedPropertiesWithoutUndo();
        var plate=GameObject.CreatePrimitive(PrimitiveType.Cube);plate.name="P2 mounted instruction plate";plate.transform.SetParent(marker.transform);plate.transform.position=new Vector3(-.545f,1.98f,23.1f);plate.transform.localScale=new Vector3(.026f,.25f,1.06f);plate.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"/M_Bezel.mat");Object.DestroyImmediate(plate.GetComponent<Collider>());
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Debug.Log("P2 curtain/rail moved .95m off the optic; original pleated textile and small printed wall plate authored. Existing curtain colliders move with that assembly; reader trigger and interaction rules unchanged.");
    }
    static MeshFilter Near(MeshFilter[] all,string name,Vector3 p)=>all.Single(f=>f.name==name&&Vector3.Distance(f.transform.position,p)<.001f);
    static Material Material(string name,Color color,float metallic,float smoothness)
    {
        var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="M_Readers"+name};
        m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smoothness);
        m.SetFloat("_SpecularHighlights",1);m.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
        AssetDatabase.CreateAsset(m,Art+"/M_"+name+".mat");return m;
    }
}
