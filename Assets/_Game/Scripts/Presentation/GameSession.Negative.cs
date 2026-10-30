using EchoProtocol.Campaign;
using UnityEngine;

namespace EchoProtocol.Presentation
{
    public sealed partial class GameSession
    {
        [Header("El Negativo / opt-in presentation")]
        [SerializeField] bool negativePresentation;
        [SerializeField] Camera negativeMenuCamera;
        Quaternion negativeMenuRestRotation;
        bool negativeMenuCameraWasActive;
        public bool NegativePresentation => negativePresentation;

        void UpdateNegativeMenuCamera()
        {
            if(!negativeMenuCamera)return;
            bool active=negativePresentation&&!started;
            if(active&&!negativeMenuCameraWasActive)negativeMenuRestRotation=negativeMenuCamera.transform.localRotation;
            negativeMenuCamera.enabled=active;
            // Only the independent title camera breathes. Player pose, recording,
            // world simulation and the free gameplay camera are never touched.
            if(active)negativeMenuCamera.transform.localRotation=negativeMenuRestRotation*Quaternion.Euler(0,Mathf.Sin(Time.unscaledTime*.12f)*.45f,0);
            else if(negativeMenuCameraWasActive)negativeMenuCamera.transform.localRotation=negativeMenuRestRotation;
            negativeMenuCameraWasActive=active;
        }

        void DrawNegativeBackdrop()
        {
            var ui=identity;
            bool titlePage=!started&&!showControls&&!help&&!folioSettings&&!credits&&!confirmRestart&&string.IsNullOrEmpty(documentTitle);
            if(!titlePage){ui.Fill(new Rect(-2000,-2000,5280,4720),new Color(.008f,.007f,.005f,.88f));return;}
            // Leave the architecture visible on the right; no full-screen card.
            ui.Fill(new Rect(-2000,-2000,5280,4720),new Color(0,0,0,.13f));
            for(int i=0;i<48;i++)
            {
                float alpha=.87f*(1-Mathf.SmoothStep(0,1,i/47f));
                ui.Fill(new Rect(i*22,0,23,720),new Color(.005f,.004f,.003f,alpha));
            }
            ui.Fill(new Rect(0,672,1280,48),new Color(0,0,0,.34f));
        }

        void DrawNegativeMain()
        {
            var ui=identity;
            if(confirmRestart){DrawNegativeRestart();return;}
            if(ending){DrawNegativeEnding();return;}
            if(started){DrawNegativePause();return;}
            ui.Tag(new Rect(70,45,650,24),"ECHO PROTOCOL / EDICIÓN EL NEGATIVO",ui.Muted);
            GUI.Label(new Rect(63,87,880,92),"EL",ui.Display(74));
            GUI.Label(new Rect(60,160,1110,145),"NEGATIVO",ui.Display(112));
            ui.Fill(new Rect(74,308,40,3),ui.Accent);
            GUI.Label(new Rect(74,330,535,53),"Tu registro abre el paso.\nVos tenés que cruzar.",ui.Text(19));
            float y=398;
            if(ui.Button(new Rect(74,y,514,56),"negative-enter","ENTRAR EN LA CÁMARA","Enter",true,campaign&&campaign.CoverEncounter))
                RequestSessionResume(()=>campaign.BeginCoverPreview());
            y+=67;
            if(ui.Button(new Rect(74,y,514,40),"begin","Recorrido completo"))RequestSessionResume();
            y+=49;
            if(campaign&&campaign.HasSavedCheckpoint)
            {
                if(ui.Button(new Rect(74,y,514,40),"saved","Continuar checkpoint"))RequestSessionResume(()=>campaign.RestoreCheckpoint());
                y+=49;
            }
            if(ui.Button(new Rect(74,y,252,40),"settings","Escucha y ajustes"))folioSettings=true;
            if(ui.Button(new Rect(336,y,252,40),"how-to","Cómo jugar","H"))showControls=true;
            y+=49;
            if(ui.Button(new Rect(74,y,252,34),"credits","Créditos","C"))credits=true;
            if(ui.Button(new Rect(336,y,252,34),"quit","Salir"))Quit();
            GUI.Label(new Rect(74,688,1050,21),"Auriculares a volumen cómodo.     Tab / ↑ ↓ navegar · Enter elegir",ui.Text(12,false,ui.Muted));
            ui.Tag(new Rect(1055,688,170,20),"01 / LA CÁMARA",ui.Accent);
        }

        void DrawNegativePause()
        {
            var ui=identity;
            ui.Tag(new Rect(74,54,650,23),"EL NEGATIVO / SESIÓN DETENIDA");
            GUI.Label(new Rect(67,108,1050,130),"EN PAUSA",ui.Display(90));
            GUI.Label(new Rect(74,253,650,30),"Tu toma también está detenida.",ui.Text(20,false,ui.Muted));
            float y=320;
            if(ui.Button(new Rect(74,y,514,54),"begin","VOLVER A LA CÁMARA","Esc",true))RequestSessionResume();
            y+=65;
            var cover=campaign?campaign.CoverEncounter:null;
            if(cover&&cover.CanRestartPreparedTake)
            {
                if(ui.Button(new Rect(74,y,514,44),"cover-reuse","Reusar toma de la bomba"))RequestSessionResume(()=>cover.RestartPreparedTake());
                y+=55;
            }
            if(ui.Button(new Rect(74,y,252,44),"settings","Escucha y ajustes"))folioSettings=true;
            if(ui.Button(new Rect(336,y,252,44),"how-to","Cómo jugar"))showControls=true;
            y+=55;
            if(ui.Button(new Rect(74,y,514,44),"pause-help","Ayuda con el próximo paso","H")){returnToPauseFromNotes=true;help=true;}
            y+=55;
            if(ui.Button(new Rect(74,y,252,40),"restart","Nueva sesión"))confirmRestart=true;
            if(ui.Button(new Rect(336,y,252,40),"quit","Salir"))Quit();
            ui.Tag(new Rect(714,326,470,22),CurrentStageTitle.ToUpperInvariant(),ui.Muted);
            var style=ui.Text(23);
            GUI.Label(new Rect(714,368,470,style.CalcHeight(new GUIContent(CurrentNextAction),470)+8),CurrentNextAction,style);
            GUI.Label(new Rect(74,686,1050,24),"Tab / Shift+Tab · ↑ ↓ · Enter / Espacio",ui.Text(13,false,ui.Muted));
        }

        void DrawNegativeRestart()
        {
            var ui=identity;
            ui.Tag(new Rect(74,54,1110,24),"EL NEGATIVO / NUEVA SESIÓN");
            GUI.Label(new Rect(67,111,1110,210),"EMPEZAR\nDE NUEVO",ui.Display(74));
            GUI.Label(new Rect(74,355,850,70),"Se descartarán esta sesión y su checkpoint.\nLos ajustes se conservarán.",ui.Text(23));
            if(ui.Button(new Rect(74,469,574,54),"restart-no","CONSERVAR SESIÓN","Esc",true))confirmRestart=false;
            if(ui.Button(new Rect(74,539,574,48),"restart-yes","Descartar y empezar de nuevo"))Restart();
        }

        void DrawNegativeEnding()
        {
            var ui=identity;
            ui.Tag(new Rect(74,54,1110,24),"EL NEGATIVO / ORIGINAL CONSERVADO");
            GUI.Label(new Rect(67,111,1110,210),"LA TOMA\nQUEDÓ ENTERA.",ui.Display(72));
            GUI.Label(new Rect(74,355,850,70),"La llave quedó en portería.\n"+(houseMemory&&houseMemory.HallLightLeftOn?"El recibidor quedó encendido.":"El recibidor quedó a oscuras."),ui.Text(23));
            if(ui.Button(new Rect(74,469,574,54),"credits","CRÉDITOS","C",true))credits=true;
            if(ui.Button(new Rect(74,539,280,48),"restart","Nueva sesión"))confirmRestart=true;
            if(ui.Button(new Rect(368,539,280,48),"quit","Salir"))Quit();
        }

        void DrawNegativeControls()
        {
            var ui=identity;
            ui.Tag(new Rect(74,54,1050,23),"EL NEGATIVO / CÓMO JUGAR");
            GUI.Label(new Rect(67,102,1110,110),"PREPARÁ TU AYUDA.",ui.Display(64));
            NegativeRule(74,252,"01","GRABÁ EN EL MANDO","Mantené E. Tus pausas y el mando quedan en la toma.");
            NegativeRule(74,352,"02","VOLVÉ CON R","Regresás al origen. Tu Echo repite esa misma toma.");
            NegativeRule(74,452,"03","OCUPATE DEL OTRO LADO","Trabajá con el ruido; cruzá cuando baje la presión.");
            NegativeControl(720,254,"WASD","Moverte");
            NegativeControl(720,300,"MOUSE","Mirar");
            NegativeControl(720,346,"E","Usar / mantener cuando se indica");
            NegativeControl(720,392,"R","Volver con una nueva toma");
            NegativeControl(720,438,"H","Ayuda · pausa el tiempo");
            NegativeControl(720,484,"ESC","Pausa y ajustes");
            if(ui.Button(new Rect(74,593,514,50),"close-controls","VOLVER","H / Esc",true))showControls=false;
            GUI.Label(new Rect(720,589,470,60),"Una nueva R reemplaza el Echo anterior. El registro conserva sólo lo que capturó.",ui.Text(16,false,ui.Muted));
            GUI.Label(new Rect(74,686,1120,24),"Auriculares a volumen cómodo. Parlantes y dinámica reducida en ajustes.",ui.Text(13,false,ui.Muted));
        }

        void NegativeRule(float x,float y,string number,string titleText,string body)
        {
            var ui=identity;
            GUI.Label(new Rect(x,y,65,60),number,ui.Text(38,true,ui.Accent));
            GUI.Label(new Rect(x+84,y+2,500,28),titleText,ui.Text(19,true));
            GUI.Label(new Rect(x+84,y+35,475,54),body,ui.Text(17,false,ui.Muted));
        }

        void NegativeControl(float x,float y,string key,string action)
        {
            var ui=identity;
            ui.Outline(new Rect(x,y,75,30),ui.Muted);
            GUI.Label(new Rect(x,y+5,75,25),key,ui.Text(13,true,ui.Paper,TextAnchor.UpperCenter));
            GUI.Label(new Rect(x+91,y+3,385,36),action,ui.Text(16));
        }

        void DrawNegativeFailure()
        {
            var ui=identity;var cover=campaign.CoverEncounter;
            ui.Tag(new Rect(74,54,1110,24),"EL NEGATIVO / INTENTO INTERRUMPIDO",ui.Accent);
            GUI.Label(new Rect(67,111,1110,130),"TE ALCANZÓ.",ui.Display(86));
            GUI.Label(new Rect(74,277,650,100),cover.TakeStatus,ui.Text(24));
            float y=421;
            if(cover.CanReplayLastTake)
            {
                if(ui.Button(new Rect(74,y,574,54),"cover-retry","REINTENTAR LA MISMA TOMA","Enter",true))cover.Retry(true);
                y+=68;
            }
            if(ui.Button(new Rect(74,y,574,48),"cover-record","Grabar otra toma desde el mando"))cover.Retry(false);
            if(ui.Button(new Rect(74,y+62,260,42),"cover-quit","Salir"))Quit();
            ui.Tag(new Rect(792,287,406,24),"EL REGISTRO SIGUE ENTERO");
            GUI.Label(new Rect(792,333,406,180),"Rompé la línea de visión.\n\nEl tabique te oculta. El ruido de la bomba puede desviar su atención.",ui.Text(21,false,ui.Muted));
        }

        void NegativeAction(out string stage,out string key,out string action,out float progress)
        {
            stage=CurrentStageTitle.ToUpperInvariant();key="";action=CurrentNextAction;progress=-1;
            var cover=campaign?campaign.CoverEncounter:null;
            if(!cover||!cover.Active||cover.Complete)return;
            if(cover.ExitReleased){stage="03 / SALIR";action=cover.EscapeInstruction;return;}
            if(cover.IsPreparingTake)
            {
                stage="01 / PREPARÁ TU TOMA";progress=cover.PreparationFraction;
                if(cover.CurrentTakeReady)
                {
                    stage="02 / VOLVÉ AL ORIGEN";progress=-1;
                    key=cover.Machine.PlayerHeld?"SOLTAR E":"R";
                    action=cover.Machine.PlayerHeld?"Terminá de grabar el mando":"Poné tu Echo en marcha";return;
                }
                if(cover.CanRestartPreparedTake&&loop.ActiveEcho&&loop.ActiveEcho.IsComplete)
                {key="ESC";action="Reusá la toma de la bomba desde pausa";progress=-1;return;}
                if(cover.PreparationPhase==1)
                {
                    key=progress<1?"ESPERAR":"MANTENER E";
                    action=progress<1?"Dejá descargar la bomba":"Grabá el ruido del regreso";return;
                }
                bool enough=progress>=1;
                key=enough?"SOLTAR E":"MANTENER E";
                action=enough?cover.PreparationPhase==0?"Dejá caer la presión":"Cerrá la toma para volver con R"
                    :cover.PreparationPhase==0?"Grabá el primer arranque en el mando":"Grabá el ruido del regreso";
                return;
            }
            stage="03 / COOPERÁ CON TU ECHO";
            if(!cover.BoltReleased)
            {
                if(cover.Covered){key="MANTENER E";action="Aflojá la manivela junto a la reja";progress=cover.WorkFraction;}
                else if(cover.HasUpcomingPumpRun||cover.Machine.ReplayHeld){key="ESPERAR";action="Acercate a la manivela; esperá el ruido";}
                else {key="R";action="Volvé al mando y prepará otra toma";}
            }
            else if(cover.Pressure01>.15f){key="ESPERAR";action="La presión debe bajar para cruzar";}
            else {key="MANTENER E";action="Cruzá y usá la palanca del fondo";progress=cover.WorkFraction;}
        }

        void DrawNegativeHud()
        {
            var ui=identity;var cover=campaign?campaign.CoverEncounter:null;
            string subtitle=ui.Subtitle;
            if(!string.IsNullOrEmpty(subtitle))
            {
                var style=ui.Text(22,false,null,TextAnchor.MiddleCenter);
                float h=style.CalcHeight(new GUIContent(subtitle),850)+22;
                ui.Panel(new Rect(200,586-h,880,h));
                GUI.Label(new Rect(215,597-h,850,h-20),subtitle,style);
            }
            if((finale&&finale.HasCommitted)||(campaign&&campaign.HideHudForPresence))return;
            NegativeAction(out string stage,out string key,out string action,out float progress);
            bool escape=cover&&cover.HideEncounterHud;
            // A single verb/target remains outside the centre of the frame.
            for(int i=0;i<18;i++)ui.Fill(new Rect(i*44,28,45,132),new Color(0,0,0,.62f*(1-i/18f)));
            ui.Tag(new Rect(64,45,710,21),stage,ui.Muted);
            float actionX=64;
            if(key.Length>0)
            {
                ui.Outline(new Rect(64,80,112,29),ui.Accent);
                GUI.Label(new Rect(68,84,104,22),key,ui.Text(12,true,ui.Accent,TextAnchor.UpperCenter));
                actionX=194;
            }
            var actionStyle=ui.Text(20,true);
            GUI.Label(new Rect(actionX,79,escape?650:610,72),action,actionStyle);
            if(progress>=0&&!escape)
            {
                ui.Fill(new Rect(64,133,340,2),new Color(.96f,.94f,.87f,.24f));
                ui.Fill(new Rect(64,133,340*Mathf.Clamp01(progress),2),ui.Accent);
            }
            if(escape)return;
            if(interactionFocusHud)interactionFocusHud.DrawFolio(ui,new Rect(382,596,516,50));
            DrawNegativeTimeline();
        }

        void DrawNegativeTimeline()
        {
            var ui=identity;var echo=loop.ActiveEcho;
            ui.Fill(new Rect(0,653,1280,67),new Color(0,0,0,.64f));
            bool available=loop.enabled&&recorder.IsRecording;
            ui.RoleMark(new Rect(37,670,9,9),false);
            string own=!available?"REGISTRO SUSPENDIDO":recorder.IsArmed?"VOS / CAMINÁ PARA GRABAR":$"VOS / REC  {recorder.ElapsedTime:00.0} / {recorder.MaxRecordingDuration:00} s";
            GUI.Label(new Rect(59,666,460,18),own,ui.Text(12,true));
            GUI.Label(new Rect(59,686,544,15),available&&!recorder.IsArmed?"R / NUEVA TOMA     H / AYUDA     ESC / PAUSA":"H / AYUDA     ESC / PAUSA",ui.Text(10,false,ui.Muted));
            ui.Fill(new Rect(36,706,544,2),new Color(.96f,.94f,.87f,.23f));
            if(available)ui.Fill(new Rect(36,706,544*Mathf.Clamp01(recorder.ElapsedTime/Mathf.Max(.01f,recorder.MaxRecordingDuration)),2),ui.Paper);
            if(echo)
            {
                ui.RoleMark(new Rect(661,667,14,14),true);
                GUI.Label(new Rect(688,666,490,22),echo.IsComplete?"ECHO / TOMA TERMINADA":$"ECHO / REPLAY  {echo.ElapsedTime:00.0} / {echo.Recording.Duration:00.0} s",ui.Text(12,true,ui.Accent));
                ui.Fill(new Rect(660,706,584,2),new Color(.96f,.65f,.29f,.23f));
                ui.Fill(new Rect(660,706,584*Mathf.Clamp01(echo.ElapsedTime/Mathf.Max(.01f,echo.Recording.Duration)),2),ui.Accent);
            }
            else GUI.Label(new Rect(660,666,584,22),available?"R / VOLVER CON LA TOMA     H / AYUDA     ESC / PAUSA":"H / AYUDA     ESC / PAUSA",ui.Text(12,false,ui.Muted,TextAnchor.UpperRight));
            if(available&&recorder.HasRecordingStarted&&recorder.MaxRecordingDuration-recorder.ElapsedTime<8)
                GUI.Label(new Rect(36,628,570,22),"El registro termina en breve / RECALL automático",ui.Text(13,true,ui.Accent));
            if(houseMemory&&houseMemory.Active&&houseMemory.PowerLocation==HousePowerLocation.Carried)
                GUI.Label(new Rect(36,602,310,22),"RESERVA DE LUZ EN LA MANO",ui.Text(12,true,ui.Accent));
        }
    }
}
