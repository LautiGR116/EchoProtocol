#if UNITY_EDITOR
using System;using System.Collections;using System.Collections.Generic;using System.IO;using System.Linq;using EchoProtocol.Campaign;using EchoProtocol.Presentation;using UnityEngine;using UnityEngine.SceneManagement;
// Isolated visual evidence. Actual checkpoint writes/fresh loads/public Continue
// restoration; render-only camera fixtures never move the Player or trigger gameplay.
public sealed class LightingCheckpointSurvey:MonoBehaviour
{
 [Serializable] sealed class Preference{public string key;public bool exists;public int value;}
 [Serializable] sealed class View{public string label;public Vector3 position,target;public string chapter,power;public bool answered,latched,drained;}
 string scene,output,key;readonly List<Preference> saved=new List<Preference>();int reloads,renders,stateActions;bool done;public string Status{get;private set;}="starting";
 public static void Run(string destination){var g=new GameObject("Temporary lighting saved-state survey");DontDestroyOnLoad(g);g.AddComponent<LightingCheckpointSurvey>().Begin(destination);}
 void Begin(string dest){output=Path.GetFullPath(dest);Directory.CreateDirectory(output);scene=SceneManager.GetActiveScene().path;var c=FindFirstObjectByType<CampaignDirector>();key=(string)typeof(CampaignDirector).GetField("checkpointKey",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(c);foreach(string suffix in new[]{"",".answered",".light",".isolated"})saved.Add(new Preference{key=key+suffix,exists=PlayerPrefs.HasKey(key+suffix),value=PlayerPrefs.GetInt(key+suffix)});StartCoroutine(Guard(Execute()));}
 IEnumerator Guard(IEnumerator e){var stack=new Stack<IEnumerator>();stack.Push(e);while(stack.Count>0){object next;try{if(!stack.Peek().MoveNext()){stack.Pop();continue;}next=stack.Peek().Current;}catch(Exception ex){Finish("FAIL "+ex);yield break;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}}
 IEnumerator Load(int chapter,bool answered,int power,bool isolated)
 {var s=FindFirstObjectByType<GameSession>();if(s)s.SetPaused(true);PlayerPrefs.SetInt(key,chapter);PlayerPrefs.SetInt(key+".answered",answered?1:0);PlayerPrefs.SetInt(key+".light",power);PlayerPrefs.SetInt(key+".isolated",isolated?1:0);PlayerPrefs.Save();var op=SceneManager.LoadSceneAsync(scene);while(!op.isDone)yield return null;yield return null;reloads++;var c=FindFirstObjectByType<CampaignDirector>();s=FindFirstObjectByType<GameSession>();c.SuppressSaves=true;s.BeginOrResume();if(!c.RestoreCheckpoint())throw new Exception("Actual Continue failed");yield return new WaitForSecondsRealtime(.3f);}
 IEnumerator Execute()
 {
  foreach(bool isolated in new[]{false,true})
  {yield return Load(2,false,0,isolated);var p=FindFirstObjectByType<PressureIsolation>();if(p.IsLatched!=isolated||p.IsDrained!=isolated)throw new Exception("Wrong pressure state");string label=isolated?"pressure-drained":"pressure-running";
   Render(label+"-entry",new Vector3(0,1.63f,-29.0f),new Vector3(3,1.15f,-35.7f));
   Render(label+"-control-approach",new Vector3(3.1f,1.63f,-32.7f),new Vector3(5.5f,1.45f,-35.85f));
   Render(label+"-control",new Vector3(4.34f,1.63f,-35.38f),new Vector3(5.75f,1.4f,-35.9f));
   Render(label+"-aisle-forward",new Vector3(4.6f,1.63f,-39.5f),new Vector3(4.6f,1.0f,-44f));
   Render(label+"-aisle-return",new Vector3(4.6f,1.63f,-42.8f),new Vector3(4.6f,1.0f,-36.5f));
   Render(label+"-door-approach",new Vector3(2.3f,1.63f,-46.3f),new Vector3(0,1f,-48f));yield return null;
  }
  for(int state=0;state<5;state++)
  {bool answered=state>0;int power=state==0?0:state-1;yield return Load(5,answered,power,true);var h=FindFirstObjectByType<HouseMemory>();if(h.HasAnswered!=answered||(answered&&(int)h.PowerLocation!=power))throw new Exception("Wrong saved reserve state");string label="house-"+(answered?((HousePowerLocation)power).ToString():"general");
   Render(label+"-entrance",new Vector3(0,1.63f,-49.45f),new Vector3(0,1.2f,-55f));
   Render(label+"-hall-dock-approach",new Vector3(-.3f,1.63f,-52.0f),new Vector3(.88f,1.3f,-50.8f));
   Render(label+"-parlor-threshold",new Vector3(-.55f,1.63f,-52.3f),new Vector3(-4.4f,1.2f,-51.9f));
   Render(label+"-phone",new Vector3(-3.39f,1.63f,-51.95f),new Vector3(-4.4f,.86f,-51.9f));
   Render(label+"-parlor-return",new Vector3(-4.5f,1.63f,-53.6f),new Vector3(-.2f,1.2f,-52.3f));
   Render(label+"-kitchen-approach",new Vector3(-.55f,1.63f,-57.3f),new Vector3(-3.3f,1.2f,-58.3f));
   Render(label+"-kitchen-dock",new Vector3(-3.8f,1.63f,-57.5f),new Vector3(-5.242f,1.3f,-57.5f));
   Render(label+"-kitchen-return",new Vector3(-3.3f,1.63f,-58.3f),new Vector3(0,1.2f,-58.2f));
   Render(label+"-bedroom-approach",new Vector3(.6f,1.63f,-53.3f),new Vector3(3.4f,1f,-50.2f));
   Render(label+"-bedroom-key-table",new Vector3(3.3f,1.63f,-51.3f),new Vector3(3.4f,.8f,-49.6f));
   Render(label+"-bedroom-dock",new Vector3(3.9f,1.63f,-53.45f),new Vector3(5.242f,1.3f,-53.45f));
   Render(label+"-bedroom-return",new Vector3(3.5f,1.63f,-53.0f),new Vector3(0,1.1f,-53.3f));
   Render(label+"-rear-exit",new Vector3(0,1.63f,-60.8f),new Vector3(0,1f,-63.5f));
   Render(label+"-window-outside",new Vector3(-6.85f,1.63f,-52.3f),new Vector3(-4.4f,1.2f,-51.9f));
   Render(label+"-6B-side-approach",new Vector3(-10,1.63f,-49.1f),new Vector3(-13,1.1f,-48.9f));
   Render(label+"-6B-origin",new Vector3(-10.4f,1.63f,-52.4f),new Vector3(-13.3f,1.3f,-52.2f));yield return null;
  }
  // Live house fixtures keep the uncollected physical key in place. These
  // direct public state actions are declared; the continuous probe separately
  // earns the same reserve states through real E input.
  for(int state=0;state<5;state++)
  {
   yield return Load(3,false,0,true);var h=FindFirstObjectByType<HouseMemory>();var player=FindFirstObjectByType<EchoProtocol.Player.FirstPersonController>();
   if(state>0){h.Use(HouseInteractionKind.Telephone,player.gameObject,new Vector3(-4.4f,.8f,-51.9f));stateActions++;}
   int power=state==0?0:state-1;
   if(state>1){h.Use(HouseInteractionKind.HallSocket,player.gameObject,new Vector3(.88f,1.3f,-50.8f));stateActions++;if(power<3){h.Use(power==1?HouseInteractionKind.KitchenSocket:HouseInteractionKind.BedroomSocket,player.gameObject,power==1?new Vector3(-5.242f,1.3f,-57.5f):new Vector3(5.242f,1.3f,-53.45f));stateActions++;}}
   if(h.HasKey||h.HasAnswered!=(state>0)||(state>0&&(int)h.PowerLocation!=power))throw new Exception("Live-house fixture state failed");
   string label="live-house-"+(state==0?"general":((HousePowerLocation)power).ToString());
   Render(label+"-key-approach",new Vector3(.6f,1.63f,-53.3f),new Vector3(3.415f,.8f,-49.6f));
   Render(label+"-key-near",new Vector3(3.3f,1.63f,-51.3f),new Vector3(3.415f,.80f,-49.6f));
   Render(label+"-kitchen-entry",new Vector3(-.55f,1.63f,-57.3f),new Vector3(-3.3f,1.2f,-58.3f));
   Render(label+"-hall-dock",new Vector3(-.3f,1.63f,-52f),new Vector3(.88f,1.3f,-50.8f));yield return null;
  }
  Finish("PASS");
 }
 void Render(string label,Vector3 pos,Vector3 aim)
 {var camera=Camera.main;var oldPos=camera.transform.position;var oldRot=camera.transform.rotation;var rtOld=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);
  try{camera.transform.SetPositionAndRotation(pos,Quaternion.LookRotation(aim-pos));camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(output+"/"+label+".png",tex.EncodeToPNG());var c=FindFirstObjectByType<CampaignDirector>();var h=FindFirstObjectByType<HouseMemory>();var p=FindFirstObjectByType<PressureIsolation>();File.WriteAllText(output+"/"+label+".json",JsonUtility.ToJson(new View{label=label,position=pos,target=aim,chapter=c.Chapter.ToString(),power=h.PowerLocation.ToString(),answered=h.HasAnswered,latched=p.IsLatched,drained=p.IsDrained},true));renders++;}
  finally{camera.transform.SetPositionAndRotation(oldPos,oldRot);camera.targetTexture=rtOld;RenderTexture.active=active;Destroy(rt);Destroy(tex);}}
 void Finish(string status){if(done)return;done=true;foreach(var p in saved)if(p.exists)PlayerPrefs.SetInt(p.key,p.value);else PlayerPrefs.DeleteKey(p.key);PlayerPrefs.Save();FindFirstObjectByType<GameSession>()?.SetPaused(true);Status=status;File.WriteAllText(output+"/result.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{status,scene,reloads,cameraFixtureRenders=renders,explicitPublicStateActions=stateActions,gameplayPosePlacements=0,preferencesRestored=saved.All(p=>PlayerPrefs.HasKey(p.key)==p.exists&&(!p.exists||PlayerPrefs.GetInt(p.key)==p.value)),scope="Isolated public Continue restoration with twelve actual PlayerPrefs writes and fresh scene loads. Render-only camera fixtures at normal approach height; Live key views additionally use explicitly counted public telephone/socket state actions. No Player movement, Echo take or campaign playthrough is inferred."},Newtonsoft.Json.Formatting.Indented));}
 void OnDestroy(){if(!done&&saved.Count>0)Finish("interrupted");}
}
#endif
