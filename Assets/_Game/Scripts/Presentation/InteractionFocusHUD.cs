using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using EchoProtocol.Horror;
using EchoProtocol.Interaction;
using EchoProtocol.Puzzle;
using UnityEngine;

namespace EchoProtocol.Presentation
{
    [DisallowMultipleComponent]
    public sealed class InteractionFocusHUD : MonoBehaviour
    {
        [SerializeField] private GameSession session;
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private Camera view;
        [SerializeField] private CampaignDirector campaign;
        [SerializeField] private PressureIsolation isolation;
        GUIStyle label;
        public IInteractable DisplayedTarget { get; private set; }
        public string DisplayedPrompt { get; private set; } = "";
        void OnGUI()
        {
            if(FolioUI.Current)return;
            DisplayedTarget = null; DisplayedPrompt = "";
            if (!session || session.IsPaused || session.IsEnding || !interactor || !interactor.isActiveAndEnabled
                || (campaign && campaign.HideHudForPresence)) return;
            if (label == null) label = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            Vector2 offset = new Vector2((Screen.width - 1280f * scale) * .5f, (Screen.height - 720f * scale) * .5f);
            var oldMatrix = GUI.matrix; var oldColor = GUI.color; int oldDepth = GUI.depth;
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one * scale); GUI.depth = -1000;
            var returnControl=interactor.FocusedTarget as ReturnWitnessControl;
            bool holdingReturn=returnControl && returnControl.IsHolding;
            var coverHolding=campaign&&campaign.CoverEncounter?campaign.CoverEncounter.HoldingInteraction:null;
            bool holding = (isolation && isolation.IsHolding)||holdingReturn||coverHolding!=null;
            bool available = interactor.HasUsableFocus;
            GUI.color = available || holding ? new Color(.73f, .9f, .84f, .95f) : new Color(.74f, .8f, .78f, .55f);
            GUI.DrawTexture(new Rect(638.5f, 358.5f, 3, 3), Texture2D.whiteTexture);
            if (available || holding)
            {
                DisplayedTarget = coverHolding??(holdingReturn ? (IInteractable)returnControl : holding ? isolation : interactor.FocusedTarget);
                string action = Prompt(DisplayedTarget);
                DisplayedPrompt = DisplayedTarget is PressureIsolation || DisplayedTarget is ReturnWitnessControl || DisplayedTarget is RecordedMachineControl || DisplayedTarget is MachineCoverInteraction ? "MANTENER E  ·  " + action : "E  ·  " + action;
                GUI.color = new Color(.012f, .021f, .024f, .94f);
                float width = Mathf.Clamp(label.CalcSize(new GUIContent(DisplayedPrompt)).x + 44, 260, 640);
                GUI.DrawTexture(new Rect(640 - width * .5f, 558, width, 44), Texture2D.whiteTexture);
                GUI.color = Color.white; label.normal.textColor = new Color(.89f, .94f, .90f);
                GUI.Label(new Rect(654 - width * .5f, 561, width - 28, 38), DisplayedPrompt, label);
                if (holding)
                {
                    GUI.color = new Color(.46f, .79f, .69f);
                    GUI.DrawTexture(new Rect(654 - width * .5f, 600, (width - 28) * (coverHolding is MachineCoverInteraction cw?cw.Fraction:coverHolding!=null?1:holdingReturn?returnControl.Fraction:isolation.HandleFraction), 3), Texture2D.whiteTexture);
                }
                else if (view)
                {
                    Vector3 screen = view.WorldToScreenPoint(interactor.FocusPoint);
                    Vector2 p = (new Vector2(screen.x, Screen.height - screen.y) - offset) / scale;
                    GUI.color = new Color(.65f, .86f, .77f, .9f);
                    const float r = 8;
                    GUI.DrawTexture(new Rect(p.x-r, p.y-r, 5, 1),Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(p.x-r, p.y-r, 1, 5),Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(p.x+r-5, p.y+r, 5, 1),Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(p.x+r, p.y+r-4, 1, 5),Texture2D.whiteTexture);
                }
            }
            GUI.matrix = oldMatrix; GUI.color = oldColor; GUI.depth = oldDepth;
        }
        static string Prompt(IInteractable target)
        {
            if (target is RecordedMachineControl machine) return machine.Prompt;
            if (target is MachineCoverInteraction work) return work.Prompt;
            if (target is PressureIsolation) return "Aislar la instalación";
            if (target is HouseInteraction house) return house.Prompt;
            if (target is FinalDoorInteraction final) return final.Prompt;
            if (target is CampaignStation station) return station.Kind == StationKind.IntroLetter ? "Leer la carta de recepción" : station.Prompt;
            if (target is VS1RecallAnchor) return "Fijar el origen de registro";
            if (target is TemporalRelayTerminal relay) return relay.Prompt;
            if (target is ReturnWitnessControl returning) return returning.Prompt;
            return "Interactuar";
        }
        public void DrawFolio(FolioUI ui,Rect area)
        {
            DisplayedTarget=null;DisplayedPrompt="";
            if(!session || session.IsPaused || session.IsEnding || !interactor || !interactor.isActiveAndEnabled || (campaign && campaign.HideHudForPresence))return;
            var returning=interactor.FocusedTarget as ReturnWitnessControl;
            bool holdReturn=returning&&returning.IsHolding;
            var coverHolding=campaign&&campaign.CoverEncounter?campaign.CoverEncounter.HoldingInteraction:null;
            bool hold=(isolation&&isolation.IsHolding)||holdReturn||coverHolding!=null;
            bool available=interactor.HasUsableFocus;
            ui.Fill(new Rect(638.5f,358.5f,3,3),available||hold?ui.Paper:ui.Muted);
            if(!available&&!hold)return;
            DisplayedTarget=coverHolding??(holdReturn?(IInteractable)returning:hold?isolation:interactor.FocusedTarget);
            bool sustained=DisplayedTarget is PressureIsolation||DisplayedTarget is ReturnWitnessControl||DisplayedTarget is RecordedMachineControl||DisplayedTarget is MachineCoverInteraction;
            DisplayedPrompt=(sustained?"MANTENER E  ·  ":"E  ·  ")+Prompt(DisplayedTarget);
            ui.Panel(area);
            ui.Fill(new Rect(area.x+15,area.y+13,32,32),ui.Accent);
            GUI.Label(new Rect(area.x+15,area.y+15,32,28),"E",ui.Text(19,true,ui.Ink,TextAnchor.MiddleCenter));
            GUI.Label(new Rect(area.x+61,area.y+10,area.width-79,area.height-15),(sustained?"Mantener · ":"")+Prompt(DisplayedTarget),ui.Text(20,false,null,TextAnchor.MiddleLeft));
            if(hold)ui.Fill(new Rect(area.x+61,area.yMax-4,(area.width-79)*(coverHolding is MachineCoverInteraction cw?cw.Fraction:coverHolding!=null?1:holdReturn?returning.Fraction:isolation.HandleFraction),2),ui.Accent);
            if(available&&!hold&&view)
            {
                Vector3 point=view.WorldToScreenPoint(interactor.FocusPoint);float scale=ui.Scale;
                Vector2 offset=new Vector2((Screen.width-1280*scale)*.5f,(Screen.height-720*scale)*.5f);
                Vector2 p=(new Vector2(point.x,Screen.height-point.y)-offset)/scale;
                ui.Fill(new Rect(p.x-8,p.y-8,5,1),ui.Accent);ui.Fill(new Rect(p.x-8,p.y-8,1,5),ui.Accent);
                ui.Fill(new Rect(p.x+3,p.y+8,5,1),ui.Accent);ui.Fill(new Rect(p.x+8,p.y+4,1,5),ui.Accent);
            }
        }
    }
}
