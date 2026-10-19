#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using EchoProtocol.Echo;

// Material-only comparisons at actual full-route captures. No pose or game-state fixture.
public sealed class RecordedPresenceComparison:MonoBehaviour
{
    const string Output="Docs/VisualEvidence/2026-10-01-Presence/compare";
    static readonly string[] Frames={"campaign-P1-visible","campaign-01-P1","campaign-SE3-entry","campaign-SE3-codo","campaign-21-Echo-at-6B","campaign-22-witnessed"};
    Material legacy,candidate;LoopController loop;
    public static void Run()
    {
        if(!Application.isPlaying)throw new Exception("Fresh Play required");
        Directory.CreateDirectory(Output);
        var p=new GameObject("Temporary recorded-surface comparison").AddComponent<RecordedPresenceComparison>();
        p.legacy=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Threshold/M_EchoSurface.mat");
        p.candidate=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Presence/M_RecordedPresence.mat");p.loop=FindFirstObjectByType<LoopController>();
        if(!p.legacy||!p.candidate||!p.loop)throw new Exception("Missing explicit comparison materials/loop");
        File.WriteAllText(Output+"/scope.txt","Actual continuous-route cameras and live Echo, with no pose fixture. A synchronous material swap compares the legacy shader to the candidate on the same enabled body renderers; the candidate's explicit self-depth proxy is disabled for legacy. Geometry, transforms, gameplay, time, recording and scene lights are unchanged. Camera.Render excludes OnGUI; original renderer assignments are restored in finally. No human approval.\n");
        ThresholdCampaignProbe.ViewCaptured+=p.Compare;ThresholdCampaignProbe.Run();
    }
    void Compare(string frame,Camera camera)
    {
        if(!Frames.Contains(frame))return;
        var echo=loop.ActiveEcho;if(!echo){File.AppendAllText(Output+"/scope.txt",frame+": no live Echo, no comparison\n");return;}
        var depthRenderers=echo.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&r.name=="Recorded self depth").ToArray();
        var renderers=echo.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&r.name!="Recorded self depth").ToArray();
        var originals=renderers.Select(r=>r.sharedMaterials).ToArray();var position=echo.transform.position;var rotation=echo.transform.rotation;var recording=loop.LastRecording;
        try
        {
            foreach(var r in depthRenderers)r.enabled=false;
            for(int i=0;i<renderers.Length;i++)renderers[i].sharedMaterials=originals[i].Select(_=>legacy).ToArray();Capture(camera,frame+"-legacy");
            foreach(var r in depthRenderers)r.enabled=true;
            for(int i=0;i<renderers.Length;i++)renderers[i].sharedMaterials=originals[i].Select(_=>candidate).ToArray();Capture(camera,frame+"-candidate");
        }
        finally {for(int i=0;i<renderers.Length;i++)renderers[i].sharedMaterials=originals[i];foreach(var r in depthRenderers)r.enabled=true;}
        if(echo.transform.position!=position||echo.transform.rotation!=rotation||!ReferenceEquals(recording,loop.LastRecording))throw new Exception("Material comparison altered a pose or recording");
        File.AppendAllText(Output+"/scope.txt",frame+" camera="+camera.transform.position.ToString("F3")+" euler="+camera.transform.eulerAngles.ToString("F2")+" Echo="+position.ToString("F3")+" enabledBodyRenderers="+renderers.Length+" selfDepthRenderers="+depthRenderers.Length+" unchangedPoseAndTake=true\n");
    }
    static void Capture(Camera camera,string file)
    {
        var previous=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
        try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Output+"/"+file+".png",image.EncodeToPNG());}
        finally{camera.targetTexture=previous;RenderTexture.active=active;Destroy(rt);Destroy(image);}
    }
    void OnDestroy(){ThresholdCampaignProbe.ViewCaptured-=Compare;}
}
#endif
