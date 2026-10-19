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
        public float DocumentContentHeight => documentLayout.Height;
        public string DocumentHeading => documentTitle;
        void DrawFolio()
        {
            var oldMatrix=GUI.matrix;var oldColor=GUI.color;int depth=GUI.depth;
            GUI.matrix=identity.Matrix;GUI.depth=-1200;GUI.color=Color.white;
            if(paused)
            {
                identity.ReconcileTabModifiers(Event.current);
                identity.BeginMenu(GraphicPage);identity.Background();
                if(Event.current.type==EventType.KeyDown && (Event.current.keyCode==KeyCode.Return || Event.current.keyCode==KeyCode.KeypadEnter || Event.current.keyCode==KeyCode.Space || Event.current.keyCode==KeyCode.Tab || Event.current.keyCode==KeyCode.UpArrow || Event.current.keyCode==KeyCode.DownArrow || Event.current.keyCode==KeyCode.LeftArrow || Event.current.keyCode==KeyCode.RightArrow))Event.current.Use();
                GUI.color=new Color(1,1,1,identity.MenuOpacity);
                if(campaign&&campaign.CoverEncounter&&campaign.CoverEncounter.Failed)DrawCoverFailure();
                else if(help)DrawFolioNotes();else if(!string.IsNullOrEmpty(documentTitle))DrawFolioDocument();
                else if(credits)DrawFolioCredits();else if(folioSettings)DrawFolioSettings();else DrawFolioMain();
            }
            else
            {
                DrawFolioHud();
                if(finalDoorMemory&&finalDoorMemory.Fade>0)identity.Fill(new Rect(-2000,-2000,5280,4720),new Color(0,0,0,finalDoorMemory.Fade));
                if(campaign&&campaign.DepartureFade>0)identity.Fill(new Rect(-2000,-2000,5280,4720),new Color(0,0,0,campaign.DepartureFade));
            }
            GUI.matrix=oldMatrix;GUI.color=oldColor;GUI.depth=depth;
        }
        void DrawFolioMain()
        {
            var ui=identity;
            ui.Tag(new Rect(88,121,680,24),ending?"CONSERVACIÓN / REGISTRO ORIGINAL":started?"SESIÓN EN CURSO":"PROTOCOLO DE REVISIÓN   /   EDICIÓN 01");
            GUI.Label(new Rect(84,158,690,202),ending?"Original\nconservado.":started?"La sesión\nestá detenida.":"ECHO\nPROTOCOL",ui.Display(started||ending?62:82));
            float y=414;
            if(confirmRestart)
            {
                GUI.Label(new Rect(88,362,620,42),"Reiniciar descarta esta sesión completa.",ui.Text(20));
                if(ui.Button(new Rect(88,y,530,54),"restart-yes","Sí, empezar de nuevo","Enter",true))Restart();
                if(ui.Button(new Rect(88,y+64,530,48),"restart-no","Volver","Esc"))confirmRestart=false;
            }
            else
            {
                if(!ending&&ui.Button(new Rect(88,y,530,54),"begin",started?"Continuar":"Iniciar sesión","Enter",true))BeginOrResume();
                y+=64;
                if(!started&&campaign&&campaign.CoverEncounter)
                {if(ui.Button(new Rect(88,y,530,40),"cover-preview","Jugar · La máquina que te cubre")) {BeginOrResume();campaign.BeginCoverPreview();}y+=44;}
                if(!started&&campaign&&campaign.HasSavedCheckpoint)
                {if(ui.Button(new Rect(88,y,530,44),"saved","Continuar · "+campaign.SavedChapterTitle)) {BeginOrResume();campaign.RestoreCheckpoint();}y+=52;}
                if(ui.Button(new Rect(88,y,530,44),"settings","Escucha y ajustes"))folioSettings=true;y+=52;
                if(ui.Button(new Rect(88,y,250,44),"credits","Créditos","C"))credits=true;
                if(ui.Button(new Rect(356,y,262,44),"quit","Salir"))Quit();
                if((started||ending)&&ui.Button(new Rect(88,y+52,530,36),"restart","Nueva sesión"))confirmRestart=true;
            }
            ui.Line(762,130,1);
            GUI.Label(new Rect(812,155,366,126),ending?"La toma quedó entera.\nLa llave quedó en portería.":started?"Tu registro continúa\ncuando vuelvas.":"Una llave.\nDos declaraciones.",ui.Display(35,true));
            GUI.Label(new Rect(816,304,346,74),ending?(houseMemory&&houseMemory.HallLightLeftOn?"El recibidor quedó encendido.":"El recibidor quedó a oscuras."):started?"La sesión está en pausa. El tiempo y la reproducción esperan.":"Volviste para revisar la tuya.",ui.Text(19,false,ui.Muted));
            ui.DrawPaper(new Rect(812,420,366,164),true);
            ui.Tag(new Rect(836,443,315,24),ending?"DEVOLUCIÓN / CONFIRMADA":"DOCUMENTO PERSONAL",ui.Seal);
            GUI.Label(new Rect(836,480,312,65),ending?"El original permanece.":"La dejé en portería.\nMara",ui.Display(28,true,ui.Ink));
            ui.Tag(new Rect(88,678,720,24),"EL REGISTRO NO RECUERDA POR VOS.",ui.Muted);
            GUI.Label(new Rect(918,678,278,24),"Tab / flechas · Enter",ui.Text(13,false,ui.Muted,TextAnchor.UpperRight));
        }
        void DrawFolioSettings()
        {
            var ui=identity;GUI.Label(new Rect(84,125,660,92),"Escucha y ajustes",ui.Display(58));
            GUI.Label(new Rect(88,226,440,134),"Auriculares a volumen cómodo.\nTambién podés jugar con parlantes.\nNo hace falta subir el volumen.",ui.Text(21));
            ui.Tag(new Rect(88,402,420,25),"CONTROLES");
            GUI.Label(new Rect(88,443,420,135),"WASD   moverte\nMouse   mirar\nE   interactuar     R   RECALL\nH   ayuda              Esc   pausa",ui.Text(19));
            GUI.Label(new Rect(680,244,420,30),"Volumen general",ui.Text(21,true));
            GUI.Label(new Rect(1080,244,90,30),Mathf.RoundToInt(volume*100)+" %",ui.Text(20,true,null,TextAnchor.UpperRight));
            float prior=volume;bool reduced=audioOutput.ReducedDynamics,mono=audioOutput.Speakers,automatic=contextualHints&&contextualHints.AutomaticEnabled;
            volume=ui.Slider(new Rect(680,294,490,30),"volume",volume);
            audioOutput.ReducedDynamics=ui.Toggle(new Rect(680,360,490,56),"reduced","Reducir impactos sonoros",reduced);
            audioOutput.Speakers=ui.Toggle(new Rect(680,434,490,56),"mono","Parlantes / mezcla mono",mono);
            if(contextualHints)contextualHints.AutomaticEnabled=ui.Toggle(new Rect(680,508,490,56),"hints","Pistas automáticas graduales",automatic);
            if(volume!=prior||reduced!=audioOutput.ReducedDynamics||mono!=audioOutput.Speakers||automatic!=(contextualHints&&contextualHints.AutomaticEnabled))SaveSettings();
            if(ui.Button(new Rect(88,598,440,46),"back-settings","Volver","Esc"))folioSettings=false;
            ui.Tag(new Rect(680,609,500,24),"← → AJUSTAR   /   ENTER CAMBIAR",ui.Muted);
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
            ui.Tag(new Rect(88,244,330,24),"PROYECTO");
            GUI.Label(new Rect(88,285,320,160),"Diseño y desarrollo\nProyecto Echo Protocol\n\nAsistencia de producción\nOpenAI Codex",ui.Text(18));
            ui.Tag(new Rect(454,244,350,24),"SONIDOS / CC0");
            GUI.Label(new Rect(454,285,350,265),"HECKFRICKER · JoelMcDaniel\nAlaskaRobotics · Rolly-SFX\nse2001 · Sadiquecat\nSamuelGremaud · FOSSarts\nftpalad · SoundDesignForYou\nloopo · JohanHauskater\ncookies+policy · ahriik\nSPAudiobooks · Archeos\nqubodup · AntumDeluge\nBrian MacIntosh",ui.Text(17));
            ui.Tag(new Rect(854,244,340,24),"ARTE / TIPOGRAFÍA");
            GUI.Label(new Rect(854,285,330,275),"Blender · MakeHuman Community\nOpenAI ImageGen\nPoly Haven / superficies CC0\nRob Tuytel · Amal Kumar\nCharlotte Baglioni\nDimitrios Savva\n\nCormorant / Christian Thalmann\nIBM Plex / Mike Abbink\ny Bold Monday · SIL OFL 1.1",ui.Text(17));
            if(ui.Button(new Rect(88,591,380,50),"back-credits","Volver","Esc"))credits=false;
            GUI.Label(new Rect(502,601,690,46),"Unity 6 · URP · Input System · ProBuilder\nFuentes y tratamientos completos: CREDITS.txt",ui.Text(14,false,ui.Muted));
        }
        bool IsPersonalDocument()=>documentTitle!=null&&(documentTitle.Contains("llave")||documentTitle.Contains("carta")||documentTitle.Contains("recibidor"));
        void DrawFolioDocument()
        {
            var ui=identity;bool personal=IsPersonalDocument();
            ui.DrawPaper(new Rect(178,32,924,650),personal);
            ui.Tag(new Rect(230,66,795,26),personal?"CORRESPONDENCIA PERSONAL   /   MARA":"ARCHIVO 6B   /   DOCUMENTO APORTADO",personal?ui.Seal:ui.Ink);
            ui.Line(230,103,820,personal?ui.Seal:new Color(.35f,.47f,.43f));
            GUI.Label(new Rect(229,124,804,90),documentTitle,ui.Display(44,false,ui.Ink));
            var body=ui.Editorial(22,false,personal);
            string key=documentTitle+"|"+documentBody+"|"+personal+"|"+body.font.GetInstanceID();
            if(key!=folioDocumentKey){folioDocumentKey=key;documentLayout.Set(documentBody,body,778);documentScroll=0;}
            var viewport=new Rect(230,232,778,332);documentScroll=ui.Scroll(viewport,documentScroll,documentLayout.Height);
            documentLayout.Draw(viewport,documentScroll,body);
            ui.Line(230,585,820,new Color(.58f,.58f,.50f));
            if(ui.Button(new Rect(230,606,390,47),"document-close","Dejar documento","Esc",true))BeginOrResume();
            GUI.Label(new Rect(678,607,363,48),documentLayout.Height>viewport.height?"Rueda / ↑ ↓ · leer\n"+Mathf.RoundToInt(documentScroll/Mathf.Max(1,documentLayout.Height-viewport.height)*100)+" % del recorrido":"Documento completo\nLa sesión está en pausa",ui.Text(14,false,ui.Ink,TextAnchor.UpperRight));
        }
        void DrawFolioNotes()
        {
            var ui=identity;GUI.Label(new Rect(84,126,830,84),"Notas de la sesión",ui.Display(58));
            string note=contextualHints?contextualHints.CurrentNote:protocolGuidance?protocolGuidance.Context:campaign?campaign.Objective:"El registro conserva el recorrido y sus pausas.";
            ui.Tag(new Rect(88,232,800,24),"RECALL   /   LECTURA DEL ESTADO ACTUAL");
            var style=ui.Text(22);
            if(note!=folioNoteKey){folioNoteKey=note;noteLayout.Set(note,style,985,30);noteScroll=0;}
            var viewport=new Rect(88,277,985,182);noteScroll=ui.Scroll(viewport,noteScroll,noteLayout.Height);noteLayout.Draw(viewport,noteScroll,style);
            string recall=finalDoorMemory&&finalDoorMemory.Active?(finalDoorMemory.OriginalPreserved?"El original está conservado. El registro terminó.":finalDoorMemory.UsesArrivalPresentation&&finalDoorMemory.HasWitnessed?"La llegada está observada. Conservá esta toma en la pantalla de portería.":"R vuelve a 6B y sustituye la toma que estás revisando."):!restoreLoop?"El registro permanece suspendido hasta la próxima revisión.":"R vuelve al último origen y sustituye la toma anterior.";
            GUI.Label(new Rect(88,472,985,26),recall,ui.Text(16,false,ui.Muted));
            if(contextualHints&&contextualHints.ManualLevel<2&&ui.Button(new Rect(88,504,940,50),"more-note",contextualHints.ManualLevel==0?"Otra pista sobre la relación":"Mostrar pasos de resolución","J"))contextualHints.RequestMore();
            if(ui.Button(new Rect(88,592,530,50),"close-notes",returnToPauseFromNotes?"Volver a pausa":"Volver a la sesión","H / Esc",true))CloseSessionNotes();
            ui.Tag(new Rect(665,604,500,24),"TU TOMA Y EL ECHO ESPERAN.",ui.Muted);
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
