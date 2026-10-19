using System.Collections.Generic;
using EchoProtocol.Echo;
using UnityEngine;

namespace EchoProtocol.Presentation
{
    // Read-only guide. Progress and attempts come from the configured puzzle;
    // time accrues only while the real session is playable, never in documents.
    [DisallowMultipleComponent]
    public sealed class ContextualHints : MonoBehaviour
    {
        [SerializeField] private GameSession session;
        [SerializeField] private ProtocolGuidance guidance;
        [SerializeField] private LoopController loop;
        [SerializeField, Min(10)] private float firstDelay = 45;
        [SerializeField, Min(10)] private float nextDelay = 65;
        private sealed class History
        {
            public int automatic, manual, attemptsAtEntry;
            public readonly HashSet<string> progress = new HashSet<string>();
        }
        private readonly Dictionary<string, History> histories = new Dictionary<string, History>();
        private History current;
        private string key = "";
        private string lastProgress = "";
        private float stalled, visibleRemaining;
        private bool automaticEnabled;
        private GUIStyle text, hintTag;
        public bool AutomaticEnabled
        {
            get => automaticEnabled;
            set { if(automaticEnabled==value)return; automaticEnabled=value; if (!value) visibleRemaining=0; PlayerPrefs.SetInt("EP.AutomaticHints",value?1:0); }
        }
        public string StageKey => key;
        public float NoProgressSeconds => stalled;
        public float ActiveSeconds { get; private set; }
        public int AutomaticCount { get; private set; }
        public int ExplicitRequests { get; private set; }
        public int ManualLevel => current == null ? 0 : current.manual;
        public string VisibleText { get; private set; } = "";
        public string CurrentNote => guidance ? guidance.Hint(ManualLevel) : "";
        public bool IsVisible => automaticEnabled && visibleRemaining>0 && session && !session.IsPaused && !session.IsEnding;
        private void Awake()
        {
            automaticEnabled=PlayerPrefs.GetInt("EP.AutomaticHints",1)==1;
            if (!session || !guidance || !loop) { Debug.LogError("ContextualHints requires explicit session, guidance and loop.",this);enabled=false; }
        }
        private void Update()
        {
            if (!session.HasStarted || session.IsPaused || session.IsEnding || !guidance.CanOfferHint) return;
            string next=guidance.StageKey;
            if(next!=key || current==null)
            {
                key=next;
                if(!histories.TryGetValue(key,out current)) histories[key]=current=new History { attemptsAtEntry=loop.CompletedLoopCount+guidance.RejectedAttempts };
                stalled=0;visibleRemaining=0;lastProgress="";
            }
            // A repeated retry can reach a state already seen in this session.
            // Its previous instruction is still stale and must leave the HUD.
            string progress=guidance.ProgressStamp;
            if(progress!=lastProgress) { lastProgress=progress;current.progress.Add(progress);stalled=0;visibleRemaining=0; }
            float dt=Time.deltaTime;ActiveSeconds+=dt;stalled+=dt;visibleRemaining=Mathf.Max(0,visibleRemaining-dt);
            if(!automaticEnabled || !guidance.IsPuzzleStage || current.automatic>=2) return;
            int attempts=Mathf.Max(0,loop.CompletedLoopCount+guidance.RejectedAttempts-current.attemptsAtEntry);
            float delay=current.automatic==0?firstDelay:nextDelay;
            if(attempts>=2+current.automatic)delay=Mathf.Min(delay,25+current.automatic*10);
            if(stalled<delay)return;
            int level=current.automatic++;
            VisibleText=guidance.Hint(level);visibleRemaining=11;stalled=0;AutomaticCount++;
            current.manual=Mathf.Max(current.manual,level);
        }
        // Only called from the deliberate button/J within paused session notes.
        public void RequestMore()
        {
            if(!session.IsPaused || current==null)return;
            current.manual=Mathf.Min(2,current.manual+1);ExplicitRequests++;
            current.automatic=Mathf.Max(current.automatic,Mathf.Min(2,current.manual+1));
            visibleRemaining=0;
        }
        private void OnGUI()
        {
            if(FolioUI.Current)return;
            if(!IsVisible)return;
            if(text==null)
            {
                text=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true};text.normal.textColor=new Color(.84f,.89f,.85f);
                hintTag=new GUIStyle(text){fontSize=12,fontStyle=FontStyle.Bold};hintTag.normal.textColor=new Color(.47f,.72f,.67f);
            }
            var old=GUI.matrix;var tint=GUI.color;var depth=GUI.depth;GUI.depth=-1050;
            float s=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*s)/2,(Screen.height-720*s)/2),Quaternion.identity,Vector3.one*s);
            float height=text.CalcHeight(new GUIContent(VisibleText),670);
            GUI.color=new Color(.012f,.024f,.026f,.95f);GUI.DrawTexture(new Rect(300,625-height,710,height+59),Texture2D.whiteTexture);GUI.color=Color.white;
            GUI.Label(new Rect(320,633-height,670,20),"NOTA DE SESIÓN    ·    H / AMPLIAR    ·    DESACTIVABLE EN PAUSA",hintTag);
            GUI.Label(new Rect(320,655-height,670,height),VisibleText,text);
            GUI.matrix=old;GUI.color=tint;GUI.depth=depth;
        }
    }
}
