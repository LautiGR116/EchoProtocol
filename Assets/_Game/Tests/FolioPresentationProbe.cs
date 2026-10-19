#if UNITY_EDITOR || ECHO_NATIVE_VERIFY
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using EchoProtocol.Presentation;
using EchoProtocol.Campaign;
using EchoProtocol.Player;
using EchoProtocol.Horror;

// Explicit presentation fixtures, separate from continuous no-fixture campaign verification.
public sealed class FolioPresentationProbe:MonoBehaviour
{
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    GameSession session;FolioUI theme;CampaignDirector campaign;Keyboard keyboard;Mouse mouse;
    Key[] keys=Array.Empty<Key>();string output,error;int checks;bool oldSuppress;
    readonly Dictionary<string,string> prefs=new Dictionary<string,string>();
    readonly List<Shot> shots=new List<Shot>();
    public string Status{get;private set;}="running";
    [Serializable] sealed class Shot{public string file,page;public int width,height,frame;public float time;}
    [Serializable] sealed class Report{public string status,error,scope;public int checks;public Shot[] screenshots;public bool preferencesRestored;}
    public void Run(string path){output=path;StartCoroutine(Guarded(Execute()));}
    T Field<T>(object o,string n)=>(T)o.GetType().GetField(n,Private).GetValue(o);
    void Set(object o,string n,object v)=>o.GetType().GetField(n,Private).SetValue(o,v);
    void Check(bool value,string label){File.AppendAllText(Path.Combine(output,"checks.txt"),(value?"PASS ":"FAIL ")+label+"\n");checks++;if(!value)throw new Exception(label);}
    void Input(){InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));InputSystem.QueueStateEvent(mouse,new MouseState{position=new Vector2(5,5)});}
    IEnumerator Press(Key key){keys=new[]{key};for(int i=0;i<3;i++)yield return null;keys=Array.Empty<Key>();for(int i=0;i<3;i++)yield return null;}
    IEnumerator ShotAt(string name){yield return new WaitForSecondsRealtime(.25f);yield return new WaitForEndOfFrame();string path=Path.Combine(output,name+".png");ScreenCapture.CaptureScreenshot(path);float deadline=Time.realtimeSinceStartup+3f;while(!File.Exists(path)&&Time.realtimeSinceStartup<deadline)yield return null;Check(File.Exists(path),"native screenshot "+name);shots.Add(new Shot{file=name+".png",page=session.GraphicPage,width=Screen.width,height=Screen.height,frame=Time.frameCount,time=Time.realtimeSinceStartup});}
    IEnumerator Before(string name){theme.enabled=false;yield return ShotAt("before-"+name);theme.enabled=true;yield return null;}
    IEnumerator Guarded(IEnumerator routine){var stack=new Stack<IEnumerator>();stack.Push(routine);while(stack.Count>0){object next;try{if(!stack.Peek().MoveNext()){stack.Pop();continue;}next=stack.Peek().Current;}catch(Exception e){Finish(false,e.ToString());yield break;}if(next is IEnumerator nested){stack.Push(nested);continue;}yield return next;}Finish(true,"");}
    IEnumerator Execute()
    {
        session=FindFirstObjectByType<GameSession>();theme=FindFirstObjectByType<FolioUI>();campaign=FindFirstObjectByType<CampaignDirector>();Check(session&&theme&&campaign,"explicit scene identity and campaign");
        string save=Field<string>(campaign,"checkpointKey");
        foreach(string key in new[]{"EP.Volume","EP.Reduced","EP.Speakers","EP.AutomaticHints",save}){prefs[key]=PlayerPrefs.HasKey(key)?(key=="EP.Volume"?PlayerPrefs.GetFloat(key).ToString(System.Globalization.CultureInfo.InvariantCulture):PlayerPrefs.GetInt(key).ToString()):null;}
        PlayerPrefs.DeleteKey(save);oldSuppress=campaign.SuppressSaves;campaign.SuppressSaves=true;
        keyboard=InputSystem.AddDevice<Keyboard>("FolioPresentationKeyboard");mouse=InputSystem.AddDevice<Mouse>("FolioPresentationMouse");InputSystem.onBeforeUpdate+=Input;
        var player=FindFirstObjectByType<FirstPersonController>();var hints=FindFirstObjectByType<ContextualHints>();var audio=FindFirstObjectByType<SessionAudio>();
        foreach(var size in new[]{new Vector2Int(1280,800),new Vector2Int(1440,810),new Vector2Int(1280,720)})
        {
            Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(1.5f);
            Check(Screen.width==size.x&&Screen.height==size.y,"actual native viewport "+size);Check(Application.isFocused,"native window stays focused");string prefix=size.x+"x"+size.y+"-";
            if(session.HasStarted){session.SetPaused(true);Set(session,"started",false);Set(session,"ending",false);}
            yield return ShotAt(prefix+"title");
            if(size.y==720){yield return Before("title");PlayerPrefs.SetInt(save,5);yield return ShotAt("title-saved-checkpoint");Check(campaign.HasSavedCheckpoint,"saved continuation visible");PlayerPrefs.DeleteKey(save);}
            yield return Press(Key.DownArrow);Check(theme.FocusedControl=="settings","title keyboard focus moves once");yield return Press(Key.Enter);Check(session.GraphicPage=="settings"&&session.IsPaused,"settings Enter opens without starting");
            float volume=Field<float>(session,"volume");yield return Press(Key.RightArrow);Check(Mathf.Approximately(Field<float>(session,"volume"),Mathf.Min(1,volume+.05f)),"keyboard volume step");yield return Press(Key.LeftArrow);
            yield return Press(Key.Tab);bool reduced=audio.ReducedDynamics;yield return Press(Key.Enter);Check(audio.ReducedDynamics!=reduced,"reduced dynamics toggles exactly once");
            yield return Press(Key.Tab);bool mono=audio.Speakers;yield return Press(Key.Space);Check(audio.Speakers!=mono,"speaker toggle via Space");
            yield return Press(Key.Tab);bool automatic=hints.AutomaticEnabled;yield return Press(Key.Enter);Check(hints.AutomaticEnabled!=automatic,"automatic hints toggle");
            yield return ShotAt(prefix+"settings");if(size.y==720)yield return Before("settings");
            yield return Press(Key.Escape);Check(session.GraphicPage=="title"&&session.IsPaused,"settings Esc keeps title paused");
            yield return Press(Key.C);Check(session.GraphicPage=="credits","credits shortcut");yield return ShotAt(prefix+"credits");yield return Press(Key.Escape);
            yield return Press(Key.Enter);Check(!session.IsPaused&&session.HasStarted,"title Enter starts once");yield return Press(Key.Escape);Check(session.GraphicPage=="pause","actual pause");yield return ShotAt(prefix+"pause");if(size.y==720)yield return Before("pause");
            // Focus last control via Shift+Tab, cancel restart without resuming.
            keys=new[]{Key.LeftShift,Key.Tab};for(int i=0;i<3;i++)yield return null;keys=Array.Empty<Key>();for(int i=0;i<3;i++)yield return null;
            Check(theme.FocusedControl=="restart","reverse Tab wraps to restart");yield return Press(Key.Enter);Check(session.GraphicPage=="restart","restart confirmation");yield return ShotAt(prefix+"restart");yield return Press(Key.Escape);Check(session.GraphicPage=="pause"&&session.IsPaused,"restart cancel stays paused");yield return Press(Key.Escape);
            string letter="A.,\n\nLa dejé en portería. Dijiste que no habías vuelto, pero te quedaste con ella toda la semana.\n\nNo necesito otra copia del recorrido. Necesito que me digas por qué esperaste del otro lado.\n\nSi vas a entrar otra vez, dejá encendida la luz del recibidor.\n\nMara";
            session.ShowDocument("La llave de 6B",letter);yield return ShotAt(prefix+"letter");if(size.y==720)yield return Before("letter");
            float time=Time.time;Vector3 position=player.transform.position;yield return new WaitForSecondsRealtime(.3f);Check(Mathf.Approximately(time,Time.time)&&player.transform.position==position&&AudioListener.pause,"document pauses time player and audio");
            yield return Press(Key.End);Check(Mathf.Abs(session.DocumentScroll-Mathf.Max(0,session.DocumentContentHeight-332))<1,"letter completely fits or ending reachable through End");yield return ShotAt(prefix+"letter-end");yield return Press(Key.Home);Check(session.DocumentScroll==0,"Home returns to letter beginning");yield return Press(Key.Escape);Check(!session.IsPaused,"document Esc resumes");
            string consent="A. Vera / solicitud de revisión\n\nEl protocolo conserva posición y orientación corporal. No conserva pensamientos, voces ni imágenes de lo que el sujeto miró.\n\n«Mara me preguntó por qué no abrí. Le dije que ya me había ido. No quiero demostrar que no estuve. Quiero saber por qué recuerdo haberme ido.»\n\nSolicitud manuscrita: mantener el registro original incluso si el solicitante pide borrarlo durante la revisión.";
            session.ShowDocument("Consentimiento / última sesión",consent);yield return ShotAt(prefix+"consent");yield return Press(Key.PageDown);Check(Mathf.Abs(session.DocumentScroll-Mathf.Min(280,Mathf.Max(0,session.DocumentContentHeight-332)))<1,"institutional PageDown clamps to content");yield return Press(Key.End);Check(Mathf.Abs(session.DocumentScroll-Mathf.Max(0,session.DocumentContentHeight-332))<1,"last institutional line reachable");yield return ShotAt(prefix+"consent-end");yield return Press(Key.Enter);Check(!session.IsPaused,"document Enter closes once");
            session.ShowDocument("Archivo / prueba explícita de lectura",string.Join("\n\n",Enumerable.Repeat(consent,6))+"\n\nFIN DEL DOCUMENTO / Ñ, áéíóú, ¿por qué?, ¡Sí!, → ← ↑ ↓.");yield return ShotAt(prefix+"stress-begin");yield return Press(Key.End);Check(session.DocumentScroll>1000,"multi-page fixture scroll reaches final paragraph");yield return ShotAt(prefix+"stress-end");yield return Press(Key.Escape);
            yield return Press(Key.H);Check(session.GraphicPage=="notes"&&session.IsPaused,"H opens session notes");yield return Press(Key.J);yield return ShotAt(prefix+"notes");yield return Press(Key.J);yield return Press(Key.Enter);Check(!session.IsPaused,"removed more-note control transfers focus to close; Enter resumes");yield return Press(Key.H);Check(session.IsPaused,"notes can reopen at final hint level");yield return Press(Key.H);Check(!session.IsPaused,"H closes notes and resumes");
            // Explicit HUD fixture at the actual reception document collider. No puzzle completion claim.
            var station=FindObjectsByType<CampaignStation>(FindObjectsSortMode.None).FirstOrDefault(s=>s.Kind==StationKind.IntroLetter);
            if(station){var target=station.GetComponent<Collider>().bounds.center;Vector3 pose=target+Vector3.back*1.7f;pose.y=player.transform.position.y;Vector3 eye=pose+Vector3.up*1.65f;Vector3 direction=target-eye;player.ResetPose(pose,Quaternion.Euler(0,Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg,0));float pitch=-Mathf.Asin(direction.normalized.y)*Mathf.Rad2Deg;Set(player,"pitch",pitch);player.GetComponentInChildren<Camera>().transform.localRotation=Quaternion.Euler(pitch,0,0);}
            yield return new WaitForSecondsRealtime(.3f);
            var subtitle=Field<SD1SubtitleDisplay>(theme,"subtitles");subtitle.Show("«No me detuve frente a esa puerta.» La declaración fue escrita después de la primera sesión.",15);
            hints.AutomaticEnabled=true;Set(hints,"<VisibleText>k__BackingField","El registro conserva el recorrido y sus pausas. Volvé a escuchar desde el otro lado; la puerta necesita que vos y el Echo estén en lugares distintos.");Set(hints,"visibleRemaining",15f);
            yield return ShotAt(prefix+"hud");if(size.y==720)yield return Before("hud");
            Set(hints,"visibleRemaining",0f);subtitle.Show("",0);session.SetPaused(true);
        }
        session.Finish();yield return ShotAt("1280x720-ending");Check(session.GraphicPage=="ending","ending UI fixture displayed");
    }
    void Finish(bool passed,string why){Restore();Status=passed?"PASS":"FAIL";File.WriteAllText(Path.Combine(output,"result.json"),JsonUtility.ToJson(new Report{status=Status,error=why,checks=checks,scope="Native UI keyboard and resolution verification. Explicit document, saved-menu, HUD placement and ending fixtures; not a continuous gameplay run. Before views use the unchanged legacy renderer in equivalent state.",screenshots=shots.ToArray(),preferencesRestored=true},true));}
    void Restore(){if(keyboard!=null){InputSystem.onBeforeUpdate-=Input;if(keyboard.added)InputSystem.RemoveDevice(keyboard);keyboard=null;}if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);if(campaign)campaign.SuppressSaves=oldSuppress;foreach(var p in prefs){if(p.Value==null)PlayerPrefs.DeleteKey(p.Key);else if(p.Key=="EP.Volume")PlayerPrefs.SetFloat(p.Key,float.Parse(p.Value,System.Globalization.CultureInfo.InvariantCulture));else PlayerPrefs.SetInt(p.Key,int.Parse(p.Value));}PlayerPrefs.Save();}
    void OnDestroy(){Restore();}
}
#endif
