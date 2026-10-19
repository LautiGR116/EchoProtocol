using EchoProtocol.Echo;
using EchoProtocol.Campaign;
using EchoProtocol.Horror;
using EchoProtocol.Interaction;
using EchoProtocol.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace EchoProtocol.Presentation
{
    [DefaultExecutionOrder(-200)]
    public sealed partial class GameSession : MonoBehaviour
    {
        [SerializeField] private FirstPersonController player;
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private LoopController loop;
        [SerializeField] private EchoRecorder recorder;
        [SerializeField] private SessionAudio audioOutput;
        [SerializeField] private ProtocolFinale finale;
        [SerializeField] private CampaignDirector campaign;
        [SerializeField] private HouseMemory houseMemory;
        [SerializeField] private FinalDoorMemory finalDoorMemory;
        [SerializeField] private ProtocolGuidance protocolGuidance;
        [SerializeField] private InteractionFocusHUD interactionFocusHud;
        [SerializeField] private ContextualHints contextualHints;
        [SerializeField] private FolioUI identity;
        [SerializeField] private string editionLabel="EDICIÓN 01";
        private bool started, paused = true, ending, credits, help, confirmRestart;
        private bool returnToPauseFromNotes;
        private bool restorePlayer, restoreInteractor, restoreLoop;
        private float volume = .7f;
        private float sessionStartedAt;
        private GUIStyle title, text, small, button, accent;
        private string documentTitle, documentBody;
        public bool IsPaused => paused;
        public bool HasStarted => started;
        public bool IsEnding => ending;
        private void Start()
        {
            if (!player || !interactor || !loop || !recorder || !audioOutput)
            { Debug.LogError("GameSession requires all explicit scene references.", this); enabled=false; return; }
            volume=PlayerPrefs.GetFloat("EP.Volume", .7f);
            player.UsesSessionPause=true;
            audioOutput.ReducedDynamics=PlayerPrefs.GetInt("EP.Reduced",0)==1;
            audioOutput.Speakers=PlayerPrefs.GetInt("EP.Speakers",0)==1;
            // Start after the components' Awake/OnEnable have initialized input.
            paused=false; SetPaused(true); AudioListener.volume=volume;
        }
        private void Update()
        {
            var k=Keyboard.current;
            if(k==null)return;
            if(campaign&&campaign.CoverEncounter&&campaign.CoverEncounter.Failed){if(identity)identity.Navigate();return;}
            if(identity && paused)
            {
                identity.Navigate();
                if(folioSettings && k.escapeKey.wasPressedThisFrame){folioSettings=false;return;}
                if(confirmRestart && k.escapeKey.wasPressedThisFrame){confirmRestart=false;return;}
                if(!string.IsNullOrEmpty(documentTitle))
                {
                    if(k.downArrowKey.wasPressedThisFrame)documentScroll+=62;
                    if(k.upArrowKey.wasPressedThisFrame)documentScroll-=62;
                    if(k.pageDownKey.wasPressedThisFrame)documentScroll+=280;
                    if(k.pageUpKey.wasPressedThisFrame)documentScroll-=280;
                    if(k.homeKey.wasPressedThisFrame)documentScroll=0;
                    if(k.endKey.wasPressedThisFrame)documentScroll=float.MaxValue;
                }
                if(help)
                {
                    if(k.downArrowKey.wasPressedThisFrame)noteScroll+=60;
                    if(k.upArrowKey.wasPressedThisFrame)noteScroll-=60;
                    if(k.pageDownKey.wasPressedThisFrame)noteScroll+=180;
                    if(k.pageUpKey.wasPressedThisFrame)noteScroll-=180;
                }
            }
            if(started && !ending && !folioSettings && string.IsNullOrEmpty(documentTitle) && !credits && !confirmRestart
                && k.hKey.wasPressedThisFrame)
            {
                if(help)CloseSessionNotes();
                else { returnToPauseFromNotes=paused; help=true; SetPaused(true); }
                return;
            }
            if(help)
            {
                if(contextualHints && k.jKey.wasPressedThisFrame)contextualHints.RequestMore();
                if(k.escapeKey.wasPressedThisFrame)CloseSessionNotes();
                return;
            }
            if(paused && string.IsNullOrEmpty(documentTitle) && !confirmRestart && k.cKey.wasPressedThisFrame)credits=!credits;
            if(paused && credits && k.escapeKey.wasPressedThisFrame){credits=false;return;}
            if(k.escapeKey.wasPressedThisFrame && started && !ending) { credits=false; confirmRestart=false; documentTitle=null; SetPaused(!paused); }
            if(!identity && paused && !credits && !confirmRestart && k.enterKey.wasPressedThisFrame && !ending) BeginOrResume();
        }
        public void BeginOrResume() { if(ending || campaign&&campaign.CoverEncounter&&campaign.CoverEncounter.Failed)return; documentTitle=null; help=false; if(!started)sessionStartedAt=Time.time; started=true; SetPaused(false); }
        public void ShowDocument(string heading, string body)
        { documentTitle=heading; documentBody=body; folioDocumentKey=null;documentScroll=0; SetPaused(true); }
        public void SetPaused(bool value)
        {
            if(paused==value)return;
            paused=value;
            if(value)
            {
                restorePlayer=player.enabled; restoreInteractor=interactor.enabled; restoreLoop=loop.enabled;
                player.enabled=false; interactor.enabled=false; loop.enabled=false;
            }
            else
            {
                player.enabled=restorePlayer; interactor.enabled=restoreInteractor; loop.enabled=restoreLoop;
            }
            Time.timeScale=value?0f:1f; AudioListener.pause=value;
            Cursor.lockState=value?CursorLockMode.None:CursorLockMode.Locked; Cursor.visible=value;
        }
        public void Finish() { ending=true; credits=false; SetPaused(true); }
        private void OnApplicationFocus(bool focused) { if(!focused && started && !paused && !ending)SetPaused(true); }
        private void OnDestroy() { Time.timeScale=1f; AudioListener.pause=false; AudioListener.volume=1f; }
        private void SaveSettings()
        {
            AudioListener.volume=volume;
            PlayerPrefs.SetFloat("EP.Volume",volume);
            PlayerPrefs.SetInt("EP.Reduced",audioOutput.ReducedDynamics?1:0);
            PlayerPrefs.SetInt("EP.Speakers",audioOutput.Speakers?1:0);
            PlayerPrefs.Save();
        }
        private void Styles()
        {
            if(title!=null)return;
            title=new GUIStyle(GUI.skin.label){fontSize=48,fontStyle=FontStyle.Bold,wordWrap=true};
            text=new GUIStyle(GUI.skin.label){fontSize=19,wordWrap=true};
            small=new GUIStyle(text){fontSize=14};
            accent=new GUIStyle(small){fontStyle=FontStyle.Bold};
            title.normal.textColor=new Color(.9f,.94f,.92f); text.normal.textColor=new Color(.76f,.82f,.81f);
            small.normal.textColor=new Color(.57f,.68f,.68f); accent.normal.textColor=new Color(.4f,.84f,.8f);
            button=new GUIStyle(){font=GUI.skin.font,fontSize=18,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(20,10,5,5),border=new RectOffset(0,0,0,0)};
            button.normal.background=Swatch(new Color(.12f,.17f,.18f));button.hover.background=Swatch(new Color(.18f,.29f,.28f));button.active.background=Swatch(new Color(.24f,.37f,.34f));
            button.normal.textColor=new Color(.83f,.85f,.78f);button.hover.textColor=Color.white;button.active.textColor=Color.white;
            // A fresh style avoids inheriting the Editor skin's platform-specific button textures.
            button.focused.background=button.normal.background;button.focused.textColor=button.normal.textColor;
        }
        private Texture2D Swatch(Color c){var t=new Texture2D(1,1);t.SetPixel(0,0,c);t.Apply();return t;}
        private void OnGUI()
        {
            if(identity && identity.isActiveAndEnabled){DrawFolio();return;}
            Styles(); GUI.depth=-1200;
            Matrix4x4 old=GUI.matrix; float s=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*s)/2,(Screen.height-720*s)/2),Quaternion.identity,Vector3.one*s);
            if(paused)DrawMenu(); else {DrawHUD(); if(finalDoorMemory&&finalDoorMemory.Fade>0){var tint=GUI.color;GUI.color=new Color(0,0,0,finalDoorMemory.Fade);GUI.DrawTexture(new Rect(-2000,-2000,5280,4720),Texture2D.whiteTexture);GUI.color=tint;}} GUI.matrix=old;
        }
        private void DrawMenu()
        {
            var prev=GUI.color; GUI.color=new Color(.009f,.016f,.021f,1f); GUI.DrawTexture(new Rect(-2000,-2000,5280,4720),Texture2D.whiteTexture); GUI.color=prev;
            if(help) { DrawSessionNotes(); return; }
            if(!string.IsNullOrEmpty(documentTitle))
            {
                var tint=GUI.color;GUI.color=new Color(.76f,.73f,.65f);GUI.DrawTexture(new Rect(165,42,950,628),Texture2D.whiteTexture);GUI.color=tint;
                var heading=new GUIStyle(title){fontSize=34};heading.normal.textColor=new Color(.13f,.15f,.14f);
                var body=new GUIStyle(text){fontSize=20};body.normal.textColor=new Color(.16f,.17f,.15f);
                var label=new GUIStyle(small);label.normal.textColor=new Color(.36f,.37f,.31f);
                GUI.Label(new Rect(220,76,820,28),"ARCHIVO 6B / DOCUMENTO APORTADO",label);
                GUI.Label(new Rect(220,125,820,88),documentTitle,heading);
                GUI.Label(new Rect(220,231,820,310),documentBody,body);
                if(GUI.Button(new Rect(220,583,440,44),"Dejar documento   /   Esc",button))BeginOrResume();
                return;
            }
            GUI.Label(new Rect(80,64,1100,28),editionLabel,accent);
            GUI.Label(new Rect(80,112,620,120),credits?"Créditos":ending?"Original\nconservado.":started?"Pausa":"ECHO\nPROTOCOL",title);
            if(credits)
            {
                var creditBody=new GUIStyle(text){fontSize=18};
                GUI.Label(new Rect(80,250,530,24),"PROYECTO",accent);
                GUI.Label(new Rect(80,286,530,62),"Diseño y desarrollo: Proyecto Echo Protocol\nAsistencia de producción: OpenAI Codex",creditBody);
                GUI.Label(new Rect(80,373,530,24),"SONIDOS CC0",accent);
                GUI.Label(new Rect(80,407,530,158),"HECKFRICKER · JoelMcDaniel\nAlaskaRobotics · Rolly-SFX\nse2001 · Sadiquecat · SamuelGremaud\nFOSSarts · ftpalad · SoundDesignForYou\nloopo · JohanHauskater · cookies+policy\nahriik · SPAudiobooks · Archeos\nqubodup · AntumDeluge · Brian MacIntosh",creditBody);
                GUI.Label(new Rect(700,250,500,24),"ARTE Y SUPERFICIES",accent);
                GUI.Label(new Rect(700,286,500,204),"Modelos y derivados: Blender.\nAnatomía adulta CC0: MakeHuman Community.\nPatio y papel floral: OpenAI ImageGen.\n\nSuperficies CC0 / Poly Haven:\nRob Tuytel · Amal Kumar\nCharlotte Baglioni · Dimitrios Savva",creditBody);
                GUI.Label(new Rect(700,501,500,24),"TECNOLOGÍA",accent);
                GUI.Label(new Rect(700,535,500,48),"Unity 6 · URP · Input System · ProBuilder",creditBody);
                if(GUI.Button(new Rect(80,606,400,48),"Volver   /   Esc",button))credits=false;
                GUI.Label(new Rect(80,674,1120,24),"Fuentes, licencias y tratamientos completos: CREDITS.txt junto a la aplicación.",small);
                return;
            }
            GUI.Label(new Rect(80,256,530,110),ending?(finalDoorMemory?"La toma quedó entera.\nLa llave quedó en portería.\n"+(houseMemory&&houseMemory.HallLightLeftOn?"El recibidor quedó encendido.":"El recibidor quedó a oscuras."):"La sesión ha terminado.\nLa declaración permanece junto al registro."):started?"La sesión está detenida.\nTu registro continúa cuando vuelvas.":"Una llave. Dos declaraciones.\nVolviste para revisar la tuya.",text);
            if(confirmRestart)
            {
                GUI.Label(new Rect(80,392,530,60),"Reiniciar descarta esta sesión completa.",text);
                if(GUI.Button(new Rect(80,464,450,48),"Sí, empezar de nuevo",button))Restart();
                if(GUI.Button(new Rect(80,524,450,48),"Volver",button))confirmRestart=false;
            }
            else
            {
                if(!ending && GUI.Button(new Rect(80,392,450,52),started?"Continuar   /   Esc":"Iniciar sesión   /   Enter",button))BeginOrResume();
                if(!started && campaign && campaign.HasSavedCheckpoint && GUI.Button(new Rect(80,456,450,44),"Continuar / "+campaign.SavedChapterTitle,button)){BeginOrResume();campaign.RestoreCheckpoint();}
                if((started||ending) && GUI.Button(new Rect(80,456,450,44),"Nueva sesión",button))confirmRestart=true;
                if(GUI.Button(new Rect(80,512,450,44),"Créditos   /   C",button))credits=true;
                if(GUI.Button(new Rect(80,568,450,44),"Salir",button))Quit();
            }
            GUI.Label(new Rect(738,176,420,32),"ESCUCHA Y CONTROLES",accent);
            GUI.Label(new Rect(738,220,430,74),"Auriculares a volumen cómodo.\nTambién podés jugar con parlantes.\nNo hace falta subir el volumen.",text);
            GUI.Label(new Rect(738,310,430,24),"Volumen general",small);
            GUI.changed=false; volume=GUI.HorizontalSlider(new Rect(742,350,390,24),volume,0f,1f);
            audioOutput.ReducedDynamics=GUI.Toggle(new Rect(738,388,420,30),audioOutput.ReducedDynamics,(audioOutput.ReducedDynamics?"[x] ":"[ ] ")+"Reducir impactos sonoros",text);
            audioOutput.Speakers=GUI.Toggle(new Rect(738,426,420,30),audioOutput.Speakers,(audioOutput.Speakers?"[x] ":"[ ] ")+"Parlantes / mezcla mono",text);
            if(contextualHints)contextualHints.AutomaticEnabled=GUI.Toggle(new Rect(738,464,430,30),contextualHints.AutomaticEnabled,(contextualHints.AutomaticEnabled?"[x] ":"[ ] ")+"Pistas automáticas graduales",text);
            if(GUI.changed)SaveSettings();
            GUI.Label(new Rect(738,513,450,120),"WASD   moverte      Mouse   mirar\nE   interactuar          R   RECALL\nH   ayuda                  Esc   pausa\n\n"+(recorder.RecordsBodyPose?"Registro: recorrido, locomoción y cabeza.":"Registro: posición y orientación corporal."),small);
            GUI.Label(new Rect(80,664,1110,28),"EL REGISTRO NO RECUERDA POR VOS.",small);
        }
        private void DrawHUD()
        {
            if((finale && finale.HasCommitted)||(campaign && campaign.HideHudForPresence))return;
            // New scenes use the explicit focus HUD; preserved scenes retain
            // this fallback, consuming the same cached interaction snapshot.
            if (!interactionFocusHud)
            {
                var isolation=campaign?campaign.IsolationControl:null;
                var t=interactor.FocusedTarget;
                if(isolation&&isolation.IsHolding)
                    GUI.Label(new Rect(535,406,590,32),"Mantener E · aislar la instalación",accent);
                else if(interactor.HasUsableFocus && !(campaign && campaign.IsWorking))
                    GUI.Label(new Rect(535,406,590,32),t is PressureIsolation?"Mantener E · aislar la instalación":"[E]  "+(t is CampaignStation cs?cs.Prompt:t is HouseInteraction hi?hi.Prompt:t is FinalDoorInteraction fi?fi.Prompt:"Interactuar"),accent);
            }
            if(campaign && campaign.IsWorking)
            {
                GUI.Label(new Rect(462,468,420,30),"Mantené el control a la vista",small);
                var tint=GUI.color;GUI.color=new Color(.055f,.08f,.08f,.95f);GUI.DrawTexture(new Rect(462,501,350,3),Texture2D.whiteTexture);
                GUI.color=new Color(.36f,.76f,.67f);GUI.DrawTexture(new Rect(462,501,350*campaign.WorkFraction,3),Texture2D.whiteTexture);GUI.color=tint;
            }
            GUI.Label(new Rect(42,30,520,30),campaign&&campaign.IsReturnRecordingSuspended?"FIN DE REGISTRO · R INACTIVO":!loop.enabled?"REGISTRO SUSPENDIDO":recorder.IsArmed?"REGISTRO EN ESPERA":$"REGISTRANDO  {recorder.ElapsedTime:00.0} / {recorder.MaxRecordingDuration:00} s",small);
            if(loop.ActiveEcho!=null&&!loop.ActiveEcho.IsComplete)
                GUI.Label(new Rect(42,56,500,30),$"ECHO  {loop.ActiveEcho.ElapsedTime:00.0} / {loop.ActiveEcho.Recording.Duration:00.0} s",accent);
            if(recorder.HasRecordingStarted && recorder.MaxRecordingDuration-recorder.ElapsedTime<8)
                GUI.Label(new Rect(430,90,650,32),"El registro termina en breve. RECALL automático.",text);
            if((!contextualHints || !contextualHints.IsVisible) && (Time.time-sessionStartedAt<20 || help))GUI.Label(new Rect(42,659,600,28),loop.enabled?"R  RECALL     ·     E  INTERACTUAR     ·     H  AYUDA":"E  INTERACTUAR     ·     ESC  PAUSA",small);
            if((!contextualHints || !contextualHints.IsVisible) && houseMemory && houseMemory.Active && houseMemory.PowerLocation==HousePowerLocation.Carried)
                GUI.Label(new Rect(42,615,500,28),"RESERVA DE LUZ EN LA MANO",small);
        }
        private void DrawSessionNotes()
        {
            string context=protocolGuidance&&protocolGuidance.isActiveAndEnabled?protocolGuidance.Context
                :finalDoorMemory&&finalDoorMemory.Active?finalDoorMemory.Objective+"\n\n"+finalDoorMemory.StatusText
                :campaign&&(campaign.Chapter>=CampaignChapter.Descent||campaign.IsReturnRecordingSuspended)?campaign.Objective+"\n\n"+campaign.StatusText
                :"El aro cian confirma una lectura de mirada.\nEl registro conserva posición, orientación y pausas.";
            if(contextualHints)context=contextualHints.CurrentNote;
            string recallNote="R vuelve al último origen y sustituye la toma anterior.";
            if(campaign&&campaign.UsesPressureIsolation)
                recallNote=finalDoorMemory&&finalDoorMemory.Active
                    ?(finalDoorMemory.OriginalPreserved?"El original está conservado. El registro terminó.":finalDoorMemory.UsesArrivalPresentation&&finalDoorMemory.HasWitnessed?"La llegada está observada. Conservá esta toma en la pantalla de portería.":"R vuelve a 6B y sustituye la toma que estás revisando.")
                    :!restoreLoop?"El registro permanece suspendido hasta la próxima revisión.":recallNote;
            GUI.Label(new Rect(240,120,800,32),"RECALL / NOTAS DE LA SESIÓN",accent);
            float contextHeight=text.CalcHeight(new GUIContent(context),800);
            GUI.Label(new Rect(240,188,800,contextHeight),context,text);
            float noteY=188+contextHeight+38;
            float noteHeight=small.CalcHeight(new GUIContent(recallNote),800);
            GUI.Label(new Rect(240,noteY,800,noteHeight),recallNote,small);
            if(contextualHints && contextualHints.ManualLevel<2 && GUI.Button(new Rect(240,465,800,48),contextualHints.ManualLevel==0?"Otra pista sobre la relación   /   J":"Mostrar pasos de resolución   /   J",button))contextualHints.RequestMore();
            GUI.Label(new Rect(240,548,800,36),"La sesión está en pausa. Tu toma y el Echo esperan.",small);
            if(GUI.Button(new Rect(240,596,530,48),returnToPauseFromNotes?"Volver a pausa   /   H o Esc":"Volver a la sesión   /   H o Esc",button))CloseSessionNotes();
        }
        private void CloseSessionNotes()
        {
            help=false;
            if(!returnToPauseFromNotes)BeginOrResume();
        }
        private void Restart() { if(campaign)campaign.ClearCheckpointSave();Time.timeScale=1; AudioListener.pause=false; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#elif UNITY_WEBGL
            SetPaused(true);
            EchoWebExit();
#else
            Application.Quit();
#endif
        }
    }
}
