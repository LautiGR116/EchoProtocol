using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.SceneManagement;using System.Linq;
internal class CommandScript:IRunCommand {
 ExecutionResult r; Transform root; Material wall,floor,glass;
 GameObject F(string n)=>Resources.FindObjectsOfTypeAll<Transform>().First(t=>t.gameObject.scene==SceneManager.GetActiveScene()&&t.name==n).gameObject;
 void Box(string n,Vector3 p,Vector3 s,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(root,false);g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;r.RegisterObjectCreation(g);}
 public void Execute(ExecutionResult result){r=result;root=F("VS1 R2 Return Gallery").transform;wall=F("P3 lane west boundary").GetComponent<Renderer>().sharedMaterial;floor=F("P3 lanes floor").GetComponent<Renderer>().sharedMaterial;glass=F("P3 distant hub sealed glazing").GetComponent<Renderer>().sharedMaterial;
 foreach(var n in new[]{"P3 vestibule west","P3 vestibule south west"}){var o=F(n).GetComponent<Renderer>();r.RegisterObjectModification(o);o.sharedMaterial=glass;}
 Box("P3 sealed lightwell floor",new Vector3(-13.2f,-.18f,37.4f),new Vector3(8,.2f,14.4f),floor);
 Box("P3 sealed lightwell west wall",new Vector3(-17.2f,1.6f,37.4f),new Vector3(.2f,3.2f,14.4f),wall);
 Box("P3 sealed lightwell south wall",new Vector3(-13.2f,1.6f,30.2f),new Vector3(8,3.2f,.2f),wall);
 Box("P3 sealed lightwell ceiling",new Vector3(-13.2f,3.3f,37.4f),new Vector3(8.2f,.2f,14.6f),wall);
 // Aperture clear width is 1.9 m; preserve all other R1 trim and cabinets.
 foreach(var t in F("R1 Clinical Observation Hub").GetComponentsInChildren<Transform>(true).Where(t=>t.name=="R1 wall skirting"&&t.position.z>30)) {r.RegisterObjectModification(t);t.position=new Vector3(-8.25f,t.position.y,t.position.z);t.localScale=new Vector3(4.2f,t.localScale.y,t.localScale.z);}
 EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());result.Log("Lightwell enclosure and real hub sightline authored");}
}
