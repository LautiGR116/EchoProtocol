using UnityEngine;
using EchoProtocol.Campaign;

namespace EchoProtocol.Presentation
{
    public sealed partial class GameSession
    {
        bool folioSettings;
        float documentScroll;
        string folioDocumentKey;
        readonly FolioDocumentLayout documentLayout=new FolioDocumentLayout();
        readonly FolioDocumentLayout noteLayout=new FolioDocumentLayout();
        string folioNoteKey;
        float noteScroll;
        public string GraphicPage => campaign&&campaign.CoverEncounter&&campaign.CoverEncounter.Failed?"caught":!paused?"HUD":help?"notes":!string.IsNullOrEmpty(documentTitle)?"document":credits?"credits":folioSettings?"settings":confirmRestart?"restart":ending?"ending":started?"pause":"title";
        public float DocumentScroll => documentScroll;
        public float DocumentContentHeight => documentReadingHeight;
        public string DocumentHeading => documentTitle;
        void DrawFolio()
        {
            var oldMatrix=GUI.matrix;var oldColor=GUI.color;int depth=GUI.depth;bool oldEnabled=GUI.enabled;
            GUI.matrix=identity.Matrix;GUI.depth=-1200;GUI.color=Color.white;
            if(paused)
            {
                identity.ReconcileTabModifiers(Event.current);
                identity.BeginMenu(GraphicPage);identity.Background();
                GUI.enabled=oldEnabled&&!resumeRequested;
                if(Event.current.type==EventType.KeyDown && (Event.current.keyCode==KeyCode.Return || Event.current.keyCode==KeyCode.KeypadEnter || Event.current.keyCode==KeyCode.Space || Event.current.keyCode==KeyCode.Tab || Event.current.keyCode==KeyCode.UpArrow || Event.current.keyCode==KeyCode.DownArrow || Event.current.keyCode==KeyCode.LeftArrow || Event.current.keyCode==KeyCode.RightArrow || Event.current.keyCode==KeyCode.PageUp || Event.current.keyCode==KeyCode.PageDown || Event.current.keyCode==KeyCode.Home || Event.current.keyCode==KeyCode.End))Event.current.Use();
                GUI.color=new Color(1,1,1,identity.MenuOpacity);
                if(campaign&&campaign.CoverEncounter&&campaign.CoverEncounter.Failed)DrawCoverFailure();
                else if(help)DrawFolioNotes();else if(!string.IsNullOrEmpty(documentTitle))DrawFolioDocument();
                else if(credits)DrawFolioCredits();else if(folioSettings)DrawFolioSettings();else DrawFolioMain();
                if(resumeRequested)
                {
                    identity.Fill(new Rect(762,674,434,30),new Color(.031f,.046f,.052f));
                    GUI.Label(new Rect(762,678,434,24),"Soltá las teclas para volver a la sesión",identity.Text(16,false,identity.Accent,TextAnchor.UpperRight));
                }
            }
            else
            {
                DrawFolioHud();
                if(finalDoorMemory&&finalDoorMemory.Fade>0)identity.Fill(new Rect(-2000,-2000,5280,4720),new Color(0,0,0,finalDoorMemory.Fade));
                if(campaign&&campaign.DepartureFade>0)identity.Fill(new Rect(-2000,-2000,5280,4720),new Color(0,0,0,campaign.DepartureFade));
            }
            GUI.matrix=oldMatrix;GUI.color=oldColor;GUI.depth=depth;GUI.enabled=oldEnabled;
        }
        void DrawFolioMain()
        {
            var ui=identity;
            ui.Tag(new Rect(88,121,680,24),confirmRestart?"NUEVA SESIÓN / CONFIRMACIÓN":ending?"CONSERVACIÓN / REGISTRO ORIGINAL":started?"SESIÓN EN CURSO":"PROTOCOLO DE REVISIÓN   /   EDICIÓN 01");
            GUI.Label(new Rect(84,154,690,172),confirmRestart?"¿Empezar\nde nuevo?":ending?"Original\nconservado.":started?"La sesión\nestá detenida.":"ECHO\nPROTOCOL",ui.Display(confirmRestart||started||ending?60:72));
            if(confirmRestart)
            {
                GUI.Label(new Rect(88,342,610,64),"Se descartarán la sesión actual y su checkpoint.\nLos ajustes se conservarán.",ui.Text(20));
                if(ui.Button(new Rect(88,435,610,54),"restart-no","Conservar sesión","Esc",true))confirmRestart=false;
                if(ui.Button(new Rect(88,503,610,54),"restart-yes","Descartar y empezar de nuevo"))Restart();
            }
            else
            {
                bool saved=!started&&campaign&&campaign.HasSavedCheckpoint;
                bool preview=!started&&campaign&&campaign.CoverEncounter;
                bool restart=started||ending;
                // Anchor the complete action stack above the footer. Optional actions
                // receive their own measured row instead of pushing later rows down.
                float savedHeight=saved?Mathf.Max(64,ui.Text(20,true).CalcHeight(new GUIContent("Continuar checkpoint\n"+campaign.SavedChapterTitle),566)+16):0;
                float height=44; // final row: credits and exit
                if(!ending)height+=64;
                if(saved)height+=savedHeight+10;
                if(preview)height+=58;
                height+=58; // settings
                if(restart)height+=54;
                float y=Mathf.Min(402,638-height);
                if(!ending)
                {
                    if(ui.Button(new Rect(88,y,610,54),"begin",started?"Reanudar sesión":"Iniciar desde el comienzo","Enter",true))RequestSessionResume();
                    y+=64;
                }
                if(saved)
                {
                    if(ui.Button(new Rect(88,y,610,savedHeight),"saved","Continuar checkpoint\n"+campaign.SavedChapterTitle))RequestSessionResume(()=>campaign.RestoreCheckpoint());
                    y+=savedHeight+10;
                }
                if(preview)
                {
                    if(ui.Button(new Rect(88,y,610,48),"cover-preview","Probar la bomba · acceso directo"))RequestSessionResume(()=>campaign.BeginCoverPreview());
                    y+=58;
                }
                if(ui.Button(new Rect(88,y,610,48),"settings","Escucha y ajustes"))folioSettings=true;
                y+=58;
                if(ui.Button(new Rect(88,y,292,44),"credits","Créditos","C"))credits=true;
                if(ui.Button(new Rect(398,y,300,44),"quit","Salir"))Quit();
                if(restart&&ui.Button(new Rect(88,y+54,610,44),"restart","Nueva sesión"))confirmRestart=true;
            }
            ui.Fill(new Rect(762,130,1,478),new Color(.26f,.32f,.31f));
            GUI.Label(new Rect(812,155,366,126),ending?"La toma quedó entera.\nLa llave quedó en portería.":started?"Tu registro continúa\ncuando vuelvas.":"Una llave.\nDos declaraciones.",ui.Display(35,true));
            GUI.Label(new Rect(816,304,346,88),ending?(houseMemory&&houseMemory.HallLightLeftOn?"El recibidor quedó encendido.":"El recibidor quedó a oscuras."):started?"La sesión está en pausa. El tiempo y la reproducción esperan.":"Volviste para revisar la tuya.\nLa campaña empieza en portería.",ui.Text(19,false,ui.Muted));
            ui.DrawPaper(new Rect(812,440,366,164),true);
            ui.Tag(new Rect(836,463,315,24),ending?"DEVOLUCIÓN / CONFIRMADA":"DOCUMENTO PERSONAL",ui.Seal);
            GUI.Label(new Rect(836,500,312,65),ending?"El original permanece.":"La dejé en portería.\nMara",ui.Display(28,true,ui.Ink));
            ui.Tag(new Rect(88,678,650,24),"EL REGISTRO NO RECUERDA POR VOS.",ui.Muted);
            GUI.Label(new Rect(762,678,434,24),"Tab / Shift+Tab · ↑ ↓ · Enter / Espacio",ui.Text(14,false,ui.Muted,TextAnchor.UpperRight));
        }
        void DrawFolioSettings()
        {
            var ui=identity;
            GUI.Label(new Rect(84,125,660,82),"Escucha y ajustes",ui.Display(58));
            GUI.Label(new Rect(88,230,450,120),"Auriculares a volumen cómodo.\nTambién podés jugar con parlantes.\nNo hace falta subir el volumen.",ui.Text(21));
            ui.Tag(new Rect(88,402,450,25),"CONTROLES DE LA SESIÓN");
            GUI.Label(new Rect(88,443,450,135),"WASD   moverte\nMouse   mirar\nE   interactuar     R   RECALL\nH   ayuda              Esc   pausa",ui.Text(19));
            float prior=volume;
            bool reduced=audioOutput.ReducedDynamics,mono=audioOutput.Speakers,automatic=contextualHints&&contextualHints.AutomaticEnabled;
            GUI.Label(new Rect(650,215,400,28),"Volumen general",ui.Text(21,true));
            volume=ui.Slider(new Rect(650,258,530,28),"volume",volume);
            GUI.Label(new Rect(1070,215,110,28),Mathf.RoundToInt(volume*100)+" %",ui.Text(20,true,null,TextAnchor.UpperRight));
            audioOutput.ReducedDynamics=ui.Toggle(new Rect(650,310,530,46),"reduced","Dinámica reducida",reduced);
            GUI.Label(new Rect(672,361,508,26),"Atenúa los picos de sonido más intensos.",ui.Text(16,false,ui.Muted));
            audioOutput.Speakers=ui.Toggle(new Rect(650,390,530,46),"mono","Parlantes / mezcla mono",mono);
            GUI.Label(new Rect(672,441,508,26),"Combina los canales; no conserva la dirección estéreo.",ui.Text(16,false,ui.Muted));
            if(contextualHints)
            {
                contextualHints.AutomaticEnabled=ui.Toggle(new Rect(650,470,530,46),"hints","Pistas automáticas",automatic);
                GUI.Label(new Rect(672,521,508,26),"Muestra pistas graduales. La ayuda H sigue disponible.",ui.Text(16,false,ui.Muted));
            }
            bool enlarged=ui.Toggle(new Rect(650,550,530,46),"large-reading","Texto de lectura ampliado",readingTextEnlarged);
            if(enlarged!=readingTextEnlarged)SetReadingTextEnlarged(enlarged);
            GUI.Label(new Rect(672,601,508,26),"Aumenta sólo el texto de documentos y notas.",ui.Text(16,false,ui.Muted));
            if(volume!=prior||reduced!=audioOutput.ReducedDynamics||mono!=audioOutput.Speakers||automatic!=(contextualHints&&contextualHints.AutomaticEnabled))SaveSettings();
            if(ui.Button(new Rect(88,598,440,46),"back-settings",ending?"Volver al cierre":started?"Volver a pausa":"Volver al inicio","Esc"))folioSettings=false;
            GUI.Label(new Rect(650,678,530,24),"← → volumen · Enter / Espacio cambiar · Esc volver",ui.Text(14,false,ui.Muted));
        }
        void DrawCoverFailure()
        {
            var ui=identity;var cover=campaign.CoverEncounter;
            ui.Tag(new Rect(88,142,700,24),"INTENTO INTERRUMPIDO / EL REGISTRO SIGUE ENTERO");
            GUI.Label(new Rect(84,192,850,150),"Te alcanzó\nen el pasillo.",ui.Display(66));
            GUI.Label(new Rect(88,363,660,92),"El ruido lo atrae hacia la bomba. Si te ve, puede perseguirte. Usá el paso lateral mientras tu Echo mantenga la máquina en marcha.",ui.Text(22));
            if(loop.LastRecording!=null&&ui.Button(new Rect(88,471,570,54),"cover-retry","Reintentar con la misma toma","Enter",true))cover.Retry(true);
            if(ui.Button(new Rect(88,541,570,48),"cover-record","Volver al mando y grabar otra toma"))cover.Retry(false);
            if(ui.Button(new Rect(88,609,260,42),"cover-quit","Salir"))Quit();
        }
        void DrawFolioCredits()
        {
            var ui=identity;GUI.Label(new Rect(84,126,700,82),"Créditos",ui.Display(64));
            ui.Tag(new Rect(88,228,330,24),"PROYECTO");
            GUI.Label(new Rect(88,270,320,190),"Diseño y desarrollo\nProyecto Echo Protocol\n\nAsistencia de producción\nOpenAI Codex",ui.Text(18));
            ui.Tag(new Rect(454,228,350,24),"SONIDOS / CC0");
            GUI.Label(new Rect(454,270,350,288),"HECKFRICKER · JoelMcDaniel\nAlaskaRobotics · Rolly-SFX\nse2001 · Sadiquecat\nSamuelGremaud · FOSSarts\nftpalad · SoundDesignForYou\nloopo · JohanHauskater\ncookies+policy · ahriik\nSPAudiobooks · Archeos\nqubodup · AntumDeluge\nBrian MacIntosh",ui.Text(18));
            ui.Tag(new Rect(854,228,340,24),"ARTE / TIPOGRAFÍA");
            GUI.Label(new Rect(854,270,340,288),"Blender · MakeHuman Community\nOpenAI ImageGen\nPoly Haven / superficies CC0\nRob Tuytel · Amal Kumar\nCharlotte Baglioni\nDimitrios Savva\n\nCormorant / Christian Thalmann\nIBM Plex / Mike Abbink\ny Bold Monday · SIL OFL 1.1",ui.Text(18));
            if(ui.Button(new Rect(88,591,380,50),"back-credits","Volver","Esc"))credits=false;
            GUI.Label(new Rect(502,601,690,46),"Unity 6 · URP · Input System · ProBuilder\nFuentes y tratamientos completos: CREDITS.txt",ui.Text(14,false,ui.Muted));
        }
        void DrawFolioHud()
        {
            if(campaign&&campaign.CoverEncounter&&campaign.CoverEncounter.HideEncounterHud)return;
            var ui=identity;bool hidden=(finale&&finale.HasCommitted)||(campaign&&campaign.HideHudForPresence);
            bool recallWarning=loop.enabled&&recorder.HasRecordingStarted&&recorder.MaxRecordingDuration-recorder.ElapsedTime<8;
            float stack=668;string subtitle=ui.Subtitle;
            if(!string.IsNullOrEmpty(subtitle))
            {var style=ui.Text(23,false,null,TextAnchor.MiddleCenter);float h=style.CalcHeight(new GUIContent(subtitle),850)+28;stack-=h;ui.Panel(new Rect(200,stack,880,h));GUI.Label(new Rect(215,stack+12,850,h-24),subtitle,style);stack-=14;}
            if(hidden)return;
            if(campaign&&campaign.CoverEncounter&&campaign.Chapter==CampaignChapter.Pressure)
            {
                var cover=campaign.CoverEncounter;
                var style=ui.Text(17);float h=style.CalcHeight(new GUIContent(cover.Objective),350);
                ui.Panel(new Rect(846,26,402,h+91));ui.Tag(new Rect(865,38,362,20),"SUBSUELO / SERVICIO");
                GUI.Label(new Rect(865,65,350,h+4),cover.Objective,style);
                GUI.Label(new Rect(865,73+h,350,34),cover.StatusText,ui.Text(12,true,ui.Accent));
            }
            // An immediate state/subtitle has priority over the optional note.
            // H still opens the full note; the interaction remains actionable.
            if(contextualHints&&contextualHints.IsVisible&&string.IsNullOrEmpty(subtitle)
                &&!(campaign&&campaign.CoverEncounter&&campaign.CoverEncounter.ExitReleased&&!campaign.CoverEncounter.Complete))
            {
                var style=ui.Text(18);float h=style.CalcHeight(new GUIContent(contextualHints.VisibleText),790)+54;stack-=h;
                ui.Panel(new Rect(225,stack,830,h));ui.Tag(new Rect(245,stack+11,790,21),"NOTA DE SESIÓN   /   H PARA AMPLIAR");
                GUI.Label(new Rect(245,stack+36,790,h-42),contextualHints.VisibleText,style);stack-=14;
            }
            if(interactionFocusHud)interactionFocusHud.DrawFolio(ui,new Rect(335,Mathf.Min(568,stack-65),610,58));
            if(campaign&&campaign.IsWorking)
            {float y=Mathf.Min(568,stack-65);ui.Fill(new Rect(350,y+61,580,3),new Color(.10f,.18f,.17f));ui.Fill(new Rect(350,y+61,580*campaign.WorkFraction,3),ui.Accent);}
            ui.Panel(new Rect(32,26,355,loop.ActiveEcho&&!loop.ActiveEcho.IsComplete?90:60));
            ui.Tag(new Rect(48,37,319,20),"REGISTRO / SESIÓN ACTUAL");
            string status=campaign&&campaign.IsReturnRecordingSuspended?"Fin de registro · R inactivo":!loop.enabled?"Registro suspendido":recorder.IsArmed?"Registro en espera":$"Registrando  {recorder.ElapsedTime:00.0} / {recorder.MaxRecordingDuration:00} s";
            GUI.Label(new Rect(48,59,319,25),status,ui.Text(16));
            if(loop.ActiveEcho&&!loop.ActiveEcho.IsComplete)GUI.Label(new Rect(48,84,319,25),$"Echo  {loop.ActiveEcho.ElapsedTime:00.0} / {loop.ActiveEcho.Recording.Duration:00.0} s",ui.Text(16,true,ui.Accent));
            if(recallWarning)
            {
                float y=loop.ActiveEcho&&!loop.ActiveEcho.IsComplete?128:98;
                if(houseMemory&&houseMemory.Active&&houseMemory.PowerLocation==HousePowerLocation.Carried)y=Mathf.Max(180,y);
                var style=ui.Text(16);const string message="El registro termina en breve. RECALL automático.";float h=style.CalcHeight(new GUIContent(message),319)+26;
                ui.Panel(new Rect(32,y,355,h));GUI.Label(new Rect(48,y+12,319,h-20),message,style);
            }
            if(houseMemory&&houseMemory.Active&&houseMemory.PowerLocation==HousePowerLocation.Carried)
            {ui.Panel(new Rect(32,130,355,38));ui.Tag(new Rect(48,141,320,24),"RESERVA DE LUZ EN LA MANO",ui.Paper);}
            if(stack==668&&Time.time-sessionStartedAt<20)GUI.Label(new Rect(34,680,650,23),loop.enabled?"R / RECALL     E / INTERACTUAR     H / AYUDA":"E / INTERACTUAR     ESC / PAUSA",ui.Text(13,true));
        }
    }
}
