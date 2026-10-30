using UnityEngine;

namespace EchoProtocol.Presentation
{
    public sealed partial class GameSession
    {
        bool showControls;

        // All teaching reads the live owners; it never advances a puzzle or a take.
        string CurrentNextAction => protocolGuidance && protocolGuidance.isActiveAndEnabled
            ? protocolGuidance.NextAction : campaign ? campaign.Objective : "Acercate al primer lector y mirá su aro iluminado.";
        string CurrentStageTitle => protocolGuidance && protocolGuidance.isActiveAndEnabled
            ? protocolGuidance.StageTitle : "La sesión";
        string RecordingStatus => campaign && campaign.IsReturnRecordingSuspended ? "Fin de registro · R inactivo"
            : !loop.enabled || !recorder.IsRecording ? "Registro suspendido"
            : recorder.IsArmed ? "Movete para empezar a grabar"
            : $"Grabando  {recorder.ElapsedTime:00.0} / {recorder.MaxRecordingDuration:00} s";

        void DrawTitleIntroduction()
        {
            var ui=identity;
            ui.Tag(new Rect(812,142,366,24),"JUGÁS JUNTO A TU PROPIO ECO");
            GUI.Label(new Rect(810,181,374,72),"Tu recorrido.\nOtra oportunidad.",ui.Display(34,true));
            DrawLearningRow(812,284,"1","GRABÁ", "Al moverte se guardan tu recorrido, tu orientación y tus pausas.");
            DrawLearningRow(812,379,"2","PULSÁ R · RECALL", "Volvés al origen. Tu Echo empieza a repetir esa toma.");
            DrawLearningRow(812,474,"3","COOPERÁ", "Mientras él repite, vos podés hacer lo que falta del otro lado.");
            GUI.Label(new Rect(812,589,366,43),"WASD: moverte · mouse: mirar\nE: interactuar · H: cómo jugar",ui.Text(15,false,ui.Muted));
        }

        void DrawLearningRow(float x,float y,string number,string heading,string body,float width=366)
        {
            var ui=identity;
            ui.Fill(new Rect(x,y,29,29),new Color(.14f,.23f,.22f));
            GUI.Label(new Rect(x,y+3,29,25),number,ui.Text(17,true,ui.Accent,TextAnchor.UpperCenter));
            ui.Tag(new Rect(x+42,y+4,width-42,24),heading);
            var style=ui.Text(17);
            float h=style.CalcHeight(new GUIContent(body),width-42);
            GUI.Label(new Rect(x+42,y+31,width-42,h),body,style);
        }

        void DrawPausedNextAction()
        {
            var ui=identity;
            ui.Tag(new Rect(812,149,366,24),"AL VOLVER A LA SESIÓN");
            GUI.Label(new Rect(810,191,366,84),CurrentStageTitle,ui.Display(34));
            var style=ui.Text(21);
            float h=style.CalcHeight(new GUIContent(CurrentNextAction),366);
            GUI.Label(new Rect(812,294,366,h),CurrentNextAction,style);
            if(ui.Button(new Rect(812,470,366,50),"pause-help","Necesito una pista","H"))
            {returnToPauseFromNotes=true;help=true;}
            ui.Line(812,548,366);
            GUI.Label(new Rect(812,569,366,65),"El tiempo y tu Echo están en pausa.\nPodés leer sin perder la toma.",ui.Text(17,false,ui.Muted));
        }

        void DrawFolioControls()
        {
            var ui=identity;
            ui.Tag(new Rect(88,123,1104,24),"CÓMO JUGAR / PODÉS VOLVER ACÁ DESDE PAUSA");
            GUI.Label(new Rect(84,157,1104,78),"Un recorrido. Dos presencias.",ui.Display(54));
            ui.Panel(new Rect(88,257,536,309));
            DrawLearningRow(110,279,"1","HACÉ TU RECORRIDO", "Se graba al empezar a caminar. Quedarte quieto también forma parte de la toma.",492);
            DrawLearningRow(110,374,"2","VOLVÉ CON R", "Volvés al origen con un Echo que repite lo que grabaste, incluidas tus pausas.",492);
            DrawLearningRow(110,469,"3","HACÉ LA OTRA PARTE", "Tu Echo ocupa un lugar mientras vos actuás en otro. Una nueva R reemplaza la toma anterior.",492);
            ui.Panel(new Rect(650,257,542,309));
            ControlRow(672,279,"WASD","Moverte");
            ControlRow(672,323,"MOUSE","Mirar / orientar a tu personaje");
            ControlRow(672,367,"E","Interactuar; mantenelo si se indica");
            ControlRow(672,411,"R","Volver al origen con tu Echo");
            ControlRow(672,455,"H","Ayuda y próximo paso · pausa el tiempo");
            ControlRow(672,499,"ESC","Pausa y ajustes");
            if(ui.Button(new Rect(88,592,536,50),"close-controls",started?"Volver a pausa":"Volver al inicio","H / Esc",true))showControls=false;
            GUI.Label(new Rect(650,592,542,54),"Los lectores responden a dónde mirás: buscá su aro iluminado. No necesitan E.",ui.Text(17,false,ui.Accent));
            GUI.Label(new Rect(88,677,1104,24),"Auriculares a volumen cómodo. Parlantes y dinámica reducida disponibles en ajustes.",ui.Text(14,false,ui.Muted));
        }

        void ControlRow(float x,float y,string key,string action)
        {
            var ui=identity;
            ui.Fill(new Rect(x,y,75,30),new Color(.14f,.23f,.22f));
            GUI.Label(new Rect(x,y+5,75,25),key,ui.Text(14,true,ui.Accent,TextAnchor.UpperCenter));
            GUI.Label(new Rect(x+94,y+3,414,34),action,ui.Text(17));
        }

        void DrawBasicControls()
        {
            GUI.Label(new Rect(80,110,1120,90),"Cómo jugar",title);
            GUI.Label(new Rect(80,240,1120,230),"1. Movete: se graban tu recorrido, tu orientación y tus pausas.\n\n2. Pulsá R: volvés al origen y tu Echo repite esa toma.\n\n3. Mientras él repite, vos podés hacer la otra parte.",text);
            GUI.Label(new Rect(80,495,1120,76),"WASD: moverte · Mouse: mirar · E: interactuar · R: volver con tu Echo\nH: ayuda y próximo paso · Esc: pausa\nLos lectores responden a dónde mirás, sin pulsar E.",small);
            if(GUI.Button(new Rect(80,596,530,50),"Volver   /   H o Esc",button))showControls=false;
        }

        void DrawNextActionHud()
        {
            if(!protocolGuidance || !protocolGuidance.isActiveAndEnabled || !protocolGuidance.CanOfferHint)return;
            var ui=identity;
            var style=ui.Text(18);
            float h=style.CalcHeight(new GUIContent(CurrentNextAction),350);
            ui.Panel(new Rect(846,26,402,h+65));
            ui.Tag(new Rect(865,38,362,20),CurrentStageTitle.ToUpperInvariant());
            GUI.Label(new Rect(865,67,350,h+4),CurrentNextAction,style);
        }

        void DrawEscapeActionHud()
        {
            string action=campaign.CoverEncounter.EscapeInstruction;
            if(string.IsNullOrEmpty(action))return;
            var ui=identity;var style=ui.Text(18,true);
            float h=style.CalcHeight(new GUIContent(action),350);
            ui.Panel(new Rect(846,26,402,h+58));
            ui.Tag(new Rect(865,38,350,20),"AHORA");
            GUI.Label(new Rect(865,63,350,h+4),action,style);
        }

        float DrawRecordingHud()
        {
            var ui=identity;var echo=loop.ActiveEcho;
            bool available=loop.enabled && recorder.IsRecording;
            bool teaching=available && protocolGuidance && protocolGuidance.StageKey=="P1" && loop.CompletedLoopCount<2;
            string lesson=!teaching?"":echo
                ? echo.IsComplete ? "Tu Echo llegó al final. Si necesitás más tiempo, grabá una pausa más larga."
                    : "Tu Echo repite la toma anterior. Vos podés hacer la otra parte."
                : recorder.IsArmed ? "WASD para moverte. Mouse para mirar. El registro empieza cuando caminás."
                    : "Tus pausas también se graban. R vuelve al origen y pone tu Echo en marcha.";
            var lessonStyle=ui.Text(16,false,ui.Accent);
            float lessonHeight=string.IsNullOrEmpty(lesson)?0:lessonStyle.CalcHeight(new GUIContent(lesson),319)+20;
            float height=(available?94:68)+(echo?68:0)+lessonHeight;
            ui.Panel(new Rect(32,26,355,height));
            ui.Tag(new Rect(48,37,319,20),available?"VOS / TOMA EN CURSO":"REGISTRO / SESIÓN ACTUAL");
            GUI.Label(new Rect(48,61,319,25),RecordingStatus,ui.Text(16));
            if(available)
            {
                ui.Fill(new Rect(48,96,319,3),new Color(.13f,.22f,.22f));
                ui.Fill(new Rect(48,96,319*Mathf.Clamp01(recorder.ElapsedTime/recorder.MaxRecordingDuration),3),ui.Accent);
                GUI.Label(new Rect(48,102,319,17),recorder.IsArmed?"R disponible después de caminar":"R / volver con esta toma",ui.Text(12,false,ui.Muted));
            }
            float y=available?130:101;
            if(echo)
            {
                ui.Tag(new Rect(48,y,319,20),"ECHO / TOMA ANTERIOR");
                GUI.Label(new Rect(48,y+23,319,25),echo.IsComplete?"Llegó al final de la toma":$"Reproduciendo  {echo.ElapsedTime:00.0} / {echo.Recording.Duration:00.0} s",ui.Text(16,true,ui.Accent));
                ui.Fill(new Rect(48,y+51,319,3),new Color(.13f,.22f,.22f));
                ui.Fill(new Rect(48,y+51,319*Mathf.Clamp01(echo.ElapsedTime/Mathf.Max(.01f,echo.Recording.Duration)),3),ui.Paper);
            }
            if(lessonHeight>0)GUI.Label(new Rect(48,26+height-lessonHeight+8,319,lessonHeight-8),lesson,lessonStyle);
            return 26+height;
        }
    }
}
