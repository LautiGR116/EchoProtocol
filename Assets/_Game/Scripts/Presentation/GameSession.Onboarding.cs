using UnityEngine;
using EchoProtocol.Campaign;

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
            ui.Tag(new Rect(812,142,366,24),"UN JUGADOR. DOS PRESENCIAS.");
            GUI.Label(new Rect(810,178,374,75),"Primero vos.\nDespués, los dos.",ui.Display(34,true));
            DrawTeachingDiagram(new Rect(812,271,366,96),0);
            DrawTeachingDiagram(new Rect(812,377,366,96),1);
            DrawTeachingDiagram(new Rect(812,483,366,96),2);
            GUI.Label(new Rect(812,592,366,20),"ESQUEMA ILUSTRATIVO / NO ES GAMEPLAY",ui.Text(10,true,ui.Muted));
            GUI.Label(new Rect(812,619,366,23),"WASD moverte · mouse mirar · E usar",ui.Text(14,false,ui.Muted));
        }

        void DrawTeachingDiagram(Rect area,int step)
        {
            var ui=identity;
            float x=area.x,y=area.y,w=area.width;
            ui.Panel(area);
            ui.Tag(new Rect(x+13,y+9,28,20),(step+1).ToString("00"),ui.Muted);
            ui.Tag(new Rect(x+47,y+9,w-60,20),step==0?"GRABÁ AL MOVERTE":step==1?"R / VOLVÉ AL ORIGEN":"REPARTÍ LAS DOS TAREAS");
            float routeY=y+51;
            if(step==0)
            {
                ui.RoleMark(new Rect(x+21,routeY-7,14,14),false);
                ui.RecordedRoute(x+49,routeY,w-131);
                ui.Outline(new Rect(x+w-68,routeY-11,23,23),ui.Muted);
                ui.Fill(new Rect(x+w-61,routeY-4,9,9),ui.Paper);
                GUI.Label(new Rect(x+15,y+70,67,18),"VOS",ui.Text(11,true));
                GUI.Label(new Rect(x+83,y+70,w-98,18),"RECORRIDO + MIRADA + PAUSAS",ui.Text(11,false,ui.Muted,TextAnchor.UpperRight));
            }
            else if(step==1)
            {
                ui.RoleMark(new Rect(x+21,routeY-7,14,14),false);
                ui.Outline(new Rect(x+14,routeY-14,28,28),ui.Muted);
                ui.RecordedRoute(x+56,routeY,w-135);
                ui.RoleMark(new Rect(x+w-66,routeY-9,18,18),true);
                GUI.Label(new Rect(x+15,y+70,130,18),"VOS / ORIGEN",ui.Text(11,true));
                GUI.Label(new Rect(x+147,y+70,w-162,18),"ECHO / REPITE TU TOMA",ui.Text(11,true,ui.Accent,TextAnchor.UpperRight));
            }
            else
            {
                float mid=x+w*.5f;
                ui.RoleMark(new Rect(x+21,routeY-9,18,18),true);
                ui.Line(x+51,routeY,w*.5f-79,ui.Accent);
                ui.Outline(new Rect(mid-13,routeY-14,26,28),ui.Muted);
                ui.Fill(new Rect(mid-7,routeY-9,3,18),ui.Accent);
                ui.Fill(new Rect(mid+4,routeY-9,3,18),ui.Paper);
                ui.Line(mid+28,routeY,w*.5f-79,ui.Paper);
                ui.RoleMark(new Rect(x+w-37,routeY-7,14,14),false);
                GUI.Label(new Rect(x+15,y+70,w*.5f-15,18),"ECHO / UN PUESTO",ui.Text(11,true,ui.Accent));
                GUI.Label(new Rect(mid,y+70,w*.5f-15,18),"VOS / EL OTRO",ui.Text(11,true,null,TextAnchor.UpperRight));
            }
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
            DrawTeachingDiagram(new Rect(88,257,536,96),0);
            DrawTeachingDiagram(new Rect(88,363,536,96),1);
            DrawTeachingDiagram(new Rect(88,469,536,96),2);
            ui.Panel(new Rect(650,257,542,309));
            ControlRow(672,279,"WASD","Moverte");
            ControlRow(672,323,"MOUSE","Mirar / orientar a tu personaje");
            ControlRow(672,367,"E","Interactuar; mantenelo si se indica");
            ControlRow(672,411,"R","Volver al origen con tu Echo");
            ControlRow(672,455,"H","Ayuda y próximo paso · pausa el tiempo");
            ControlRow(672,499,"ESC","Pausa y ajustes");
            if(ui.Button(new Rect(88,592,536,50),"close-controls",started?"Volver a pausa":"Volver al inicio","H / Esc",true))showControls=false;
            GUI.Label(new Rect(650,592,542,54),"Una nueva R reemplaza tu Echo. Los lectores se activan al mirarlos: no necesitan E.",ui.Text(17,false,ui.Accent));
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
            var cover=campaign&&campaign.Chapter==CampaignChapter.Pressure?campaign.CoverEncounter:null;
            string action=cover&&cover.IsPreparingTake?cover.PreparationDetail:CurrentNextAction;
            string heading=cover&&cover.IsPreparingTake?cover.PreparationTitle:CurrentStageTitle;
            var style=ui.Text(18);
            float h=style.CalcHeight(new GUIContent(action),326);
            bool plan=protocolGuidance.ShowPlan && protocolGuidance.PlanSteps!=null && protocolGuidance.PlanSteps.Length>0;
            ui.Panel(new Rect(884,26,364,h+(plan?116:65)));
            ui.Tag(new Rect(903,38,326,20),heading.ToUpperInvariant());
            GUI.Label(new Rect(903,67,326,h+4),action,style);
            if(plan)DrawPlanStrip(new Rect(903,80+h,326,42));
        }

        void DrawPlanStrip(Rect area)
        {
            var ui=identity;var steps=protocolGuidance.PlanSteps;
            int current=Mathf.Clamp(protocolGuidance.PlanStep,0,steps.Length);
            if(steps.Length>4)
            {
                string label=current==steps.Length?"PLAN COMPLETO":$"{current+1}/{steps.Length}  {steps[current]}";
                ui.Tag(area,label);
                return;
            }
            float width=(area.width-9*(steps.Length-1))/steps.Length;
            for(int i=0;i<steps.Length;i++)
            {
                float x=area.x+i*(width+9);
                bool done=i<current,active=i==current;
                ui.Fill(new Rect(x,area.y,width,3),new Color(.16f,.24f,.23f));
                if(done)ui.Fill(new Rect(x,area.y,width,3),ui.Accent);
                else if(active)ui.Fill(new Rect(x,area.y,width*Mathf.Clamp01(protocolGuidance.PlanProgress01),3),ui.Paper);
                if(active)ui.Fill(new Rect(x,area.y-3,3,9),ui.Paper);
                GUI.Label(new Rect(x,area.y+9,width,31),steps[i],ui.Text(12,active,done?ui.Accent:active?ui.Paper:ui.Muted));
            }
        }

        void DrawEscapeActionHud()
        {
            string action=campaign.CoverEncounter.EscapeInstruction;
            if(string.IsNullOrEmpty(action))return;
            var ui=identity;var style=ui.Text(18,true);
            float h=style.CalcHeight(new GUIContent(action),280);
            ui.Panel(new Rect(930,26,318,h+58));
            ui.Tag(new Rect(949,38,280,20),"AHORA");
            GUI.Label(new Rect(949,63,280,h+4),action,style);
        }

        float DrawRecordingHud()
        {
            var ui=identity;var echo=loop.ActiveEcho;
            bool available=loop.enabled && recorder.IsRecording;
            float height=echo?169:103;
            ui.Panel(new Rect(32,26,292,height));
            ui.RoleMark(new Rect(48,42,12,12),false);
            ui.Tag(new Rect(72,38,95,21),"VOS",ui.Paper);
            GUI.Label(new Rect(147,38,160,21),available&&!recorder.IsArmed?$"{recorder.ElapsedTime:00.0} / {recorder.MaxRecordingDuration:00} s":"",ui.Text(13,true,ui.Paper,TextAnchor.UpperRight));
            string status=!available?RecordingStatus:recorder.IsArmed?"WASD / empezá tu recorrido":"Recorrido, mirada y pausas";
            GUI.Label(new Rect(48,65,259,21),status,ui.Text(14,false,ui.Muted));
            if(available)
            {
                ui.Fill(new Rect(48,91,259,2),new Color(.13f,.22f,.22f));
                ui.Fill(new Rect(48,91,259*Mathf.Clamp01(recorder.ElapsedTime/Mathf.Max(.01f,recorder.MaxRecordingDuration)),2),ui.Paper);
            }
            if(echo)
            {
                ui.RoleMark(new Rect(46,106,16,16),true);
                ui.Tag(new Rect(72,103,95,21),"ECHO");
                GUI.Label(new Rect(147,103,160,21),$"{echo.ElapsedTime:00.0} / {echo.Recording.Duration:00.0} s",ui.Text(13,true,ui.Accent,TextAnchor.UpperRight));
                GUI.Label(new Rect(48,130,259,21),echo.IsComplete?"Toma terminada":"Repite tu toma anterior",ui.Text(14,false,ui.Accent));
                ui.Fill(new Rect(48,156,259,2),new Color(.13f,.22f,.22f));
                ui.Fill(new Rect(48,156,259*Mathf.Clamp01(echo.ElapsedTime/Mathf.Max(.01f,echo.Recording.Duration)),2),ui.Accent);
            }
            GUI.Label(new Rect(48,26+height-25,259,20),available?(recorder.IsArmed?"R disponible después de caminar":"R / volver con esta toma"):"REGISTRO SUSPENDIDO",ui.Text(12,true,ui.Muted));
            return 26+height;
        }
    }
}
