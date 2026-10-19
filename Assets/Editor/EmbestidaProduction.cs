using System;using System.IO;using System.Linq;using System.Collections.Generic;
using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
using EchoProtocol.Campaign;using EchoProtocol.Echo;
public static class EmbestidaProduction
{
 const string Art="Assets/_Game/Art/Embodiment/RenderpeoplePrivate/Embestida/";
 static T Field<T>(UnityEngine.Object o,string n)where T:UnityEngine.Object=>(T)new SerializedObject(o).FindProperty(n).objectReferenceValue;
 static void Ref(UnityEngine.Object o,string n,UnityEngine.Object v){var s=new SerializedObject(o);s.FindProperty(n).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
 static void Save(UnityEngine.Object o,string name)
 {
  string p=Art+name;o.name=Path.GetFileNameWithoutExtension(name);
  var old=AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(p);
  if(old)
  {
   if(o is Mesh from && old is Mesh to)
   {
    to.Clear();to.indexFormat=from.indexFormat;to.vertices=from.vertices;to.normals=from.normals;to.tangents=from.tangents;to.uv=from.uv;
    var rest=new List<Vector3>();from.GetUVs(2,rest);to.SetUVs(2,rest);to.boneWeights=from.boneWeights;to.bindposes=from.bindposes;
    to.subMeshCount=from.subMeshCount;for(int i=0;i<from.subMeshCount;i++)to.SetTriangles(from.GetTriangles(i),i);to.bounds=from.bounds;to.name=o.name;
   }
   else EditorUtility.CopySerialized(o,old);
   EditorUtility.SetDirty(old);AssetDatabase.SaveAssetIfDirty(old);UnityEngine.Object.DestroyImmediate(o);
  }
  else AssetDatabase.CreateAsset(o,p);
 }

 static float G(Vector3 p,Vector3 c,Vector3 s){var d=p-c;return Mathf.Exp(-2*(d.x*d.x/(s.x*s.x)+d.y*d.y/(s.y*s.y)+d.z*d.z/(s.z*s.z)));}
 static Transform Mark(string name,Transform parent,Vector3 p){var t=parent.Cast<Transform>().FirstOrDefault(c=>c.name==name);if(!t){t=new GameObject(name).transform;t.SetParent(parent);}t.position=p;return t;}
 static AudioSource Sound(string name,Transform parent,AudioSource source,string clip,float gain){var t=Mark(name,parent,parent.position);var a=t.GetComponent<AudioSource>();if(!a)a=t.gameObject.AddComponent<AudioSource>();EditorUtility.CopySerialized(source,a);a.playOnAwake=false;a.loop=false;a.volume=gain;a.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(clip);return a;}
 public static object Main()
 {
  if(EditorApplication.isPlaying)throw new Exception("Stop play before authoring");
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(!scene.path.EndsWith("EchoProtocol_Incursion.unity"))throw new Exception("Incursion only");
  var threat=UnityEngine.Object.FindFirstObjectByType<MachineCoverThreat>();var staging=threat.Intruder;
  foreach(var group in threat.transform.parent.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Embestida / ")).GroupBy(t=>new{t.parent,t.name}).ToArray())
   foreach(var duplicate in group.Skip(1).ToArray())UnityEngine.Object.DestroyImmediate(duplicate.gameObject);
  var r=threat.GetComponentsInChildren<SkinnedMeshRenderer>(true).First(x=>x.name=="RecordedAdultSurface");
  var source=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/_Game/Art/Embodiment/RenderpeoplePrivate/Nathan/ArticulatedSurface.asset");
  var mesh=UnityEngine.Object.Instantiate(source);mesh.name="Embestida / gaunt adult, damaged hands, unequal shoulder";
  var rest=source.vertices;var vertices=source.vertices;
  for(int i=0;i<vertices.Length;i++)
  {
   var p=rest[i];var q=p;
   float face=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.54f,1.61f,p.y))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.13f,.19f,Mathf.Abs(p.x))));
   float cheek=G(p,new Vector3(-.048f,1.658f,.108f),new Vector3(.035f,.038f,.065f))+G(p,new Vector3(.048f,1.658f,.108f),new Vector3(.035f,.038f,.065f));
   q.x-=Mathf.Sign(p.x)*cheek*.010f;q.z-=cheek*.018f;
   float temple=G(p,new Vector3(-.069f,1.744f,.025f),new Vector3(.034f,.065f,.09f))+G(p,new Vector3(.066f,1.744f,.025f),new Vector3(.034f,.065f,.09f));q.x-=Mathf.Sign(p.x)*temple*.009f;
   float jaw=G(p,new Vector3(.006f,1.607f,.09f),new Vector3(.055f,.043f,.06f));q.x+=jaw*.013f;q.y-=jaw*.012f;
   float lip=G(p,new Vector3(.023f,1.650f,.111f),new Vector3(.024f,.017f,.035f));q.y-=lip*.022f;q.x+=lip*.007f;
   float hollow=G(p,new Vector3(-.049f,1.674f,.093f),new Vector3(.027f,.032f,.04f));q.z-=hollow*.026f;q.x+=hollow*.013f;
   float brow=G(p,new Vector3(.023f,1.739f,.108f),new Vector3(.053f,.014f,.04f));q.z+=brow*.009f;
   if(p.y>1.77f&&Mathf.Abs(p.x)<.13f){q.y-=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.77f,1.83f,p.y))*.025f;q.z-=.007f*Mathf.Sin(p.x*27);}

   float shoulder=G(p,new Vector3(-.185f,1.43f,-.065f),new Vector3(.145f,.18f,.10f));q.z-=shoulder*.055f;q.y+=shoulder*.023f;
   float torso=G(p,new Vector3(.01f,1.25f,.10f),new Vector3(.16f,.23f,.08f));q.z-=torso*.028f;
   float handWeight=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.70f,.81f,Mathf.Abs(p.x)));q.y+=handWeight*.008f*Mathf.Sin(p.x*43);q.z+=handWeight*.005f;
   vertices[i]=q;
  }
  mesh.vertices=vertices;mesh.SetUVs(2,rest.ToList());mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();Save(mesh,"Body.asset");r.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"Body.asset");
  var original=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Embodiment/RenderpeoplePrivate/Nathan/M_Nathan.mat");
  var skin=new Material(Shader.Find("EchoProtocol/EmbestidaSurface")){name="Embestida / sallow flesh and saturated work cloth"};skin.SetTexture("_BaseMap",original.GetTexture("_BaseMap"));skin.SetTexture("_BumpMap",original.GetTexture("_BumpMap"));skin.SetFloat("_BumpScale",.58f);Save(skin,"Body.mat");
  var mats=r.sharedMaterials;mats[0]=AssetDatabase.LoadAssetAtPath<Material>(Art+"Body.mat");r.sharedMaterials=mats;
  // A new weighted rubber bib changes the costume and contour independently of
  // the scanned shirt. Fold ridges and unequal hem are actual mesh geometry.
  var v=new List<Vector3>();var uv=new List<Vector2>();var weights=new List<BoneWeight>();var triangles=new List<int>();int cols=20,rows=30;
  var sourceWeights=source.boneWeights;
  int hip=Array.FindIndex(r.bones,b=>b.name.EndsWith("_hip"));
  int s1=Array.FindIndex(r.bones,b=>b.name.EndsWith("_spine_01"));
  int s2=Array.FindIndex(r.bones,b=>b.name.EndsWith("_spine_02"));
  int s3=Array.FindIndex(r.bones,b=>b.name.EndsWith("_spine_03"));
  for(int y=0;y<=rows;y++)for(int x=0;x<=cols;x++)
  {
   float u=x/(float)cols,t=y/(float)rows;float width=Mathf.Lerp(.13f,.215f,Mathf.SmoothStep(0,1,t*2));
   float xx=(u*2-1)*width;float yy=Mathf.Lerp(1.435f,.56f,t);
   yy+=Mathf.Pow(t,12)*(.045f*Mathf.Sin(u*19)+.05f*u);
   float zz=.166f+.025f*Mathf.Sin(t*Mathf.PI)+.009f*Mathf.Sin(u*36+t*7)+.012f*Mathf.Pow(t,3);
   var p=new Vector3(xx,yy,zz);v.Add(p);uv.Add(new Vector2(u,t));
   int low=yy<1.094f?hip:yy<1.243f?s1:s2;int high=yy<1.094f?s1:yy<1.243f?s2:s3;
   float blend=Mathf.InverseLerp(yy<1.094f?.944f:yy<1.243f?1.094f:1.243f,yy<1.094f?1.094f:yy<1.243f?1.243f:1.3925f,yy);
   weights.Add(new BoneWeight{boneIndex0=low,boneIndex1=high,weight0=1-blend,weight1=blend});
   if(y<rows&&x<cols){int a=y*(cols+1)+x,b=a+cols+1;triangles.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
  }
  // Two narrow load-bearing straps connect the bib to the shoulder line.
  foreach(float sign in new[]{-1f,1f})
  {
   int a=v.Count;
   v.AddRange(new[]{new Vector3(sign*.087f,1.425f,.178f),new Vector3(sign*.117f,1.425f,.178f),new Vector3(sign*.111f,1.548f,.172f),new Vector3(sign*.081f,1.548f,.172f)});
   uv.AddRange(new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up});
   for(int k=0;k<4;k++)weights.Add(new BoneWeight{boneIndex0=s3,weight0=1});
   triangles.AddRange(new[]{a,a+1,a+2,a,a+2,a+3});
  }
  // Unequal dirty wraps cover the wrist accessories and join the damaged hands
  // to the work costume. They share the body's bind space, not a runtime prop.
  foreach(float sign in new[]{-1f,1f})
  {
   int handBone=Array.FindIndex(r.bones,b=>b.name.EndsWith(sign<0?"_hand_l":"_hand_r"));
   int first=v.Count;int segments=20;
   for(int ring=0;ring<2;ring++)for(int j=0;j<=segments;j++)
   {
    float angle=j/(float)segments*Mathf.PI*2;
    var p=new Vector3(sign*(.699f+ring*.054f),1.5247f+Mathf.Cos(angle)*.040f,-.016f+Mathf.Sin(angle)*.039f);
    v.Add(p);uv.Add(new Vector2(j/(float)segments,ring));weights.Add(new BoneWeight{boneIndex0=handBone,weight0=1});
    if(ring==0&&j<segments){int a=first+j,b=a+segments+1;triangles.AddRange(sign<0?new[]{a,b,a+1,a+1,b,b+1}:new[]{a,a+1,b,a+1,b+1,b});}
   }
  }
  var garment=new Mesh{name="Original uneven heavy rubber bib",vertices=v.ToArray(),uv=uv.ToArray(),triangles=triangles.ToArray(),boneWeights=weights.ToArray(),bindposes=source.bindposes};garment.SetUVs(2,v);garment.RecalculateNormals();garment.RecalculateTangents();garment.RecalculateBounds();Save(garment,"RubberBib.asset");
  var bibMaterial=new Material(Shader.Find("EchoProtocol/EmbestidaSurface")){name="Embestida / oily ochre rubber"};bibMaterial.SetFloat("_Apron",1);bibMaterial.SetFloat("_BumpScale",0);Save(bibMaterial,"RubberBib.mat");
  var bib=r.transform.Cast<Transform>().FirstOrDefault(c=>c.name=="Embestida / torn rubber bib");if(!bib){bib=new GameObject("Embestida / torn rubber bib").transform;bib.SetParent(r.transform,false);}
  var br=bib.GetComponent<SkinnedMeshRenderer>();if(!br)br=bib.gameObject.AddComponent<SkinnedMeshRenderer>();br.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"RubberBib.asset");br.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"RubberBib.mat");br.bones=r.bones;br.rootBone=r.rootBone;br.localBounds=r.localBounds;br.updateWhenOffscreen=true;
  var rush=threat.GetComponent<EmbestidaRush>();if(!rush)rush=threat.gameObject.AddComponent<EmbestidaRush>();
  var threatSettings=new SerializedObject(threat);threatSettings.FindProperty("pursuitSpeed").floatValue=2.1f;threatSettings.ApplyModifiedPropertiesWithoutUndo();
  var parent=staging.transform.parent;
  var threshold=Mark("Embestida / cleared threshold",parent,new Vector3(-4.40f,.03f,-45.66f));
  var stand=Mark("Embestida / shoulder landing beside lever",parent,new Vector3(-4.04f,.03f,-44.72f));stand.rotation=Quaternion.Euler(0,15,0);
  var hand=Mark("Embestida / right palm on steel",parent,new Vector3(-3.63f,1.25f,-44.68f));hand.rotation=Quaternion.Euler(0,90,270);
  var panel=Mark("Embestida / dented contact plate",parent,new Vector3(-3.59f,1.30f,-44.69f));
  var mf=panel.GetComponent<MeshFilter>();if(!mf)mf=panel.gameObject.AddComponent<MeshFilter>();mf.sharedMesh=Resources.GetBuiltinResource<Mesh>("Cube.fbx");
  var mr=panel.GetComponent<MeshRenderer>();if(!mr)mr=panel.gameObject.AddComponent<MeshRenderer>();mr.sharedMaterial=GameObject.Find("Release / enamel housing").GetComponent<MeshRenderer>().sharedMaterial;
  panel.localScale=new Vector3(.10f,.70f,.25f);var box=panel.GetComponent<BoxCollider>();if(!box)box=panel.gameObject.AddComponent<BoxCollider>();
  var reference=Field<AudioSource>(staging,"doorImpact");
  var contact=Sound("Embestida / body hits release housing",hand,reference,"Assets/_Game/Audio/Clips/Embestida/Embestida_HousingContact.wav",.78f);
  var effort=Field<AudioSource>(threat,"cry");effort.clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Embestida/Embestida_ExpelledEffort.wav");effort.volume=.42f;
  var breath=Field<AudioSource>(staging,"breath");breath.clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Embestida/Embestida_BodyBreath.wav");breath.volume=.58f;breath.loop=true;
  var head=threat.GetComponent<MachineThreatPerformance>().Face;if(!head)head=Field<Transform>(threat.GetComponent<MachineThreatPerformance>(),"head");breath.transform.SetParent(head,true);breath.transform.localPosition=Vector3.zero;
  var chase=Field<AudioSource>(threat,"chaseVoice");chase.clip=effort.clip;chase.volume=.27f;
  Ref(rush,"threshold",threshold);Ref(rush,"impactStand",stand);Ref(rush,"impactHand",hand);Ref(rush,"impactPanel",panel);Ref(rush,"contact",contact);Ref(rush,"effort",effort);Ref(rush,"breathing",breath);Ref(rush,"threat",threat);Ref(staging,"embestida",rush);
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
  return new{saved=scene.path,meshVertices=r.sharedMesh.vertexCount,garmentVertices=br.sharedMesh.vertexCount,art=Art,impact=stand.position.ToString()};
 }
}
