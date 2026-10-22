using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoProtocol.Presentation
{
    // Scene-owned graphic language. Gameplay owners supply all displayed state.
    [DefaultExecutionOrder(-300), DisallowMultipleComponent]
    public sealed class FolioUI : MonoBehaviour
    {
        public static FolioUI Current { get; private set; }
        [SerializeField] private Font displayFont, narrativeItalic, functionalFont, functionalMedium, editorialFont, editorialItalic;
        [SerializeField] private Texture2D domesticPaper, archivePaper;
        [SerializeField] private EchoProtocol.Horror.SD1SubtitleDisplay subtitles;
        readonly Dictionary<string, GUIStyle> styles = new Dictionary<string, GUIStyle>();
        readonly List<Texture2D> swatches = new List<Texture2D>();
        readonly List<string> controls = new List<string>();
        readonly HashSet<string> readers = new HashSet<string>();
        readonly HashSet<string> sliders = new HashSet<string>();
        readonly Dictionary<string,string> menuFocus = new Dictionary<string,string>();
        string screenKey, focused, activate;
        string scrollTarget;
        int adjustment, scrollLines, scrollPages, scrollEdge, activationFrame=-1, scrollFrame=-1, tabFrame=-1, nativeShiftFrame=-2;
        string tabOrigin;
        bool tabReverse;
        float enteredAt;
        public float MenuOpacity => .82f+.18f*Mathf.Clamp01((Time.unscaledTime-enteredAt)/.12f);
        public Color Ink => new Color(.055f,.073f,.080f);
        public Color Paper => new Color(.91f,.88f,.79f);
        public Color Muted => new Color(.59f,.65f,.63f);
        public Color Accent => new Color(.63f,.79f,.73f);
        public Color Seal => new Color(.60f,.33f,.26f);
        public Font FunctionalFont => functionalFont;
        public Font DisplayFont => displayFont;
        public Font EditorialFont => editorialFont;
        public string FocusedControl => focused;
        public string Subtitle => subtitles && subtitles.IsShowing ? subtitles.VisibleText : "";
        public float Scale => Mathf.Min(Screen.width/1280f,Screen.height/720f);
        public Matrix4x4 Matrix => Matrix4x4.TRS(new Vector3((Screen.width-1280*Scale)*.5f,(Screen.height-720*Scale)*.5f),Quaternion.identity,Vector3.one*Scale);
        void Awake()
        {
            if (!displayFont || !functionalFont || !functionalMedium || !editorialFont || !narrativeItalic || !editorialItalic || !domesticPaper || !archivePaper)
            { Debug.LogError("FolioUI requires all explicit typefaces and paper surfaces.",this);enabled=false;return; }
            Current=this;
        }
        void OnDestroy() { if(Current==this)Current=null; foreach(var t in swatches)if(t)Destroy(t); }
        void OnEnable() {if(Application.isPlaying && displayFont && functionalFont)Current=this;}
        void OnDisable() {if(Current==this)Current=null;}
        public void InvalidateStyles() { styles.Clear(); foreach(var t in swatches)if(t)Destroy(t);swatches.Clear(); }
        public GUIStyle Text(int size=20,bool medium=false,Color? color=null,TextAnchor align=TextAnchor.UpperLeft)
            => Style(medium?functionalMedium:functionalFont,size,color??Paper,align);
        public GUIStyle Display(int size=66,bool italic=false,Color? color=null)
            => Style(italic?narrativeItalic:displayFont,size,color??Paper,TextAnchor.UpperLeft);
        public GUIStyle Editorial(int size=22,bool italic=false,bool domestic=true)
            => Style(domestic?(italic?editorialItalic:editorialFont):functionalFont,size,Ink,TextAnchor.UpperLeft);
        GUIStyle Style(Font font,int size,Color color,TextAnchor align)
        {
            string key=font.GetInstanceID()+"/"+size+"/"+color.ToString()+"/"+align;
            if(styles.TryGetValue(key,out var s))return s;
            s=new GUIStyle {font=font,fontSize=size,alignment=align,wordWrap=true,richText=false,clipping=TextClipping.Clip,padding=new RectOffset(0,0,0,0)};
            s.normal.textColor=color; styles[key]=s;return s;
        }
        public void Fill(Rect rect,Color color) {var old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;}
        public void Line(float x,float y,float width,Color? color=null) => Fill(new Rect(x,y,width,1),color??new Color(.26f,.32f,.31f));
        public void Tag(Rect rect,string text,Color? color=null) => GUI.Label(rect,text,Text(13,true,color??Accent));
        public void Panel(Rect rect) { Fill(rect,new Color(.034f,.055f,.060f,1));Line(rect.x,rect.y,rect.width,new Color(.31f,.43f,.40f)); }
        public void Background()
        {
            Fill(new Rect(-2000,-2000,5280,4720),new Color(.031f,.046f,.052f));
            for(int y=28;y<720;y+=48)Line(28,y,1224,new Color(.25f,.32f,.30f,.065f));
            Fill(new Rect(60,42,1,636),new Color(.25f,.32f,.30f));
            Line(88,94,1104); Line(88,656,1104);
            Tag(new Rect(88,54,720,24),"ECHO / ARCHIVO DE SESIONES");
            Tag(new Rect(1000,54,200,24),"6B   /   03:17",Muted);
        }
        public void BeginMenu(string key)
        {
            if(screenKey!=key)ChangeMenu(key);
            controls.Clear();readers.Clear();sliders.Clear();
        }
        void ChangeMenu(string key)
        {
            if(screenKey!=null&&focused!=null)menuFocus[screenKey]=focused;
            screenKey=key;focused=null;
            // Confirmations start on the safe action; fresh readers start on
            // their text, so their first arrow press always means reading.
            if(key!="restart"&&key!="document"&&key!="notes")menuFocus.TryGetValue(key,out focused);
            activate=null;adjustment=0;scrollTarget=null;enteredAt=Time.unscaledTime;
            controls.Clear();readers.Clear();sliders.Clear();
        }
        void Register(string id,bool enabled=true) {if(enabled){controls.Add(id);if(focused==null)focused=id;}}
        public void Navigate(string currentPage=null)
        {
            activate=null;adjustment=0;scrollTarget=null;
            if(currentPage!=null&&screenKey!=currentPage){ChangeMenu(currentPage);return;}
            var k=Keyboard.current;if(k==null||controls.Count==0)return;
            int index=controls.IndexOf(focused);if(index<0){index=0;focused=controls[0];activate=null;}
            bool reverse=nativeShiftFrame==Time.frameCount||k.shiftKey.isPressed||k.leftShiftKey.wasPressedThisFrame||k.rightShiftKey.wasPressedThisFrame;
            bool reading=readers.Contains(focused),slider=sliders.Contains(focused);
            int move=k.tabKey.wasPressedThisFrame?(reverse?-1:1):!reading&&k.downArrowKey.wasPressedThisFrame?1:!reading&&k.upArrowKey.wasPressedThisFrame?-1:
                !reading&&!slider&&k.rightArrowKey.wasPressedThisFrame?1:!reading&&!slider&&k.leftArrowKey.wasPressedThisFrame?-1:0;
            if(k.tabKey.wasPressedThisFrame){tabFrame=Time.frameCount;tabOrigin=focused;tabReverse=reverse;}
            if(move!=0){focused=controls[(index+move+controls.Count)%controls.Count];return;}
            if(reading)
            {
                scrollTarget=focused;scrollFrame=Time.frameCount;
                scrollLines=k.downArrowKey.wasPressedThisFrame?1:k.upArrowKey.wasPressedThisFrame?-1:0;
                scrollPages=k.pageDownKey.wasPressedThisFrame?1:k.pageUpKey.wasPressedThisFrame?-1:0;
                scrollEdge=k.homeKey.wasPressedThisFrame?-1:k.endKey.wasPressedThisFrame?1:0;
            }
            else if(k.enterKey.wasPressedThisFrame||k.numpadEnterKey.wasPressedThisFrame||k.spaceKey.wasPressedThisFrame)
            {activate=focused;activationFrame=Time.frameCount;}
            if(slider)adjustment=k.rightArrowKey.wasPressedThisFrame?1:k.leftArrowKey.wasPressedThisFrame?-1:0;
        }
        // Native IMGUI carries modifier flags even when a platform input event does
        // not expose a separate Shift key transition. Reconcile the same Tab once.
        public void ReconcileTabModifiers(Event e)
        {
            if(e.type==EventType.KeyDown&&e.shift)nativeShiftFrame=Time.frameCount;
            if(e.type!=EventType.KeyDown||(e.keyCode!=KeyCode.Tab&&e.keyCode!=KeyCode.None)||!e.shift||tabReverse||tabFrame!=Time.frameCount)return;
            int index=controls.IndexOf(tabOrigin);
            if(index>=0){focused=controls[(index-1+controls.Count)%controls.Count];activate=null;tabReverse=true;}
        }
        bool Consume(string id) {if(activate!=id||activationFrame!=Time.frameCount)return false;activate=null;return true;}
        GUIStyle ButtonStyle(bool focus,bool primary,bool enabled)
        {
            string key="button/"+focus+"/"+primary+"/"+enabled;
            if(styles.TryGetValue(key,out var s))return s;
            s=new GUIStyle(Text(20,true)){alignment=TextAnchor.MiddleLeft,padding=new RectOffset(22,16,0,0),wordWrap=false};
            s.normal.background=Swatch(primary?new Color(.77f,.83f,.76f):focus?new Color(.17f,.26f,.25f):new Color(.070f,.105f,.111f));
            s.hover.background=Swatch(new Color(.24f,.34f,.31f));s.active.background=Swatch(new Color(.36f,.43f,.36f));s.focused.background=s.normal.background;
            s.normal.textColor=enabled?(primary?Ink:Paper):Muted;s.hover.textColor=Color.white;s.active.textColor=Color.white;s.focused.textColor=s.normal.textColor;
            styles[key]=s;return s;
        }
        Texture2D Swatch(Color color){var t=new Texture2D(1,1,TextureFormat.RGBA32,false);t.SetPixel(0,0,color);t.Apply();swatches.Add(t);return t;}
        public bool Button(Rect rect,string id,string label,string shortcut="",bool primary=false,bool enabled=true)
        {
            enabled=enabled&&GUI.enabled;Register(id,enabled);bool wasEnabled=GUI.enabled;GUI.enabled=enabled;
            bool hover=rect.Contains(Event.current.mousePosition);
            if(enabled&&hover&&Event.current.type==EventType.MouseDown){focused=id;activate=null;}
            bool click=GUI.Button(rect,GUIContent.none,ButtonStyle(focused==id,primary,enabled));GUI.enabled=wasEnabled;
            Color foreground=enabled?(hover?Color.white:primary?Ink:Paper):Muted;
            var shortcutStyle=Text(13,true,primary?Ink:Muted,TextAnchor.MiddleRight);
            float shortcutWidth=shortcut.Length==0?0:shortcutStyle.CalcSize(new GUIContent(shortcut)).x+22;
            var labelRect=new Rect(rect.x+22,rect.y+4,rect.width-44-shortcutWidth,rect.height-8);
            GUI.Label(labelRect,label,Text(20,true,foreground,TextAnchor.MiddleLeft));
            if(focused==id&&enabled)Fill(new Rect(rect.x-5,rect.y,2,rect.height),Accent);
            if(shortcut.Length>0)GUI.Label(new Rect(rect.xMax-shortcutWidth-22,rect.y+4,shortcutWidth,rect.height-8),shortcut,shortcutStyle);
            if(click){focused=id;activate=null;}return enabled&&(click||Consume(id));
        }
        public bool Toggle(Rect rect,string id,string label,bool value)
        {
            bool pressed=Button(rect,id,label,value?"ACTIVO":"INACTIVO");
            Fill(new Rect(rect.xMax-13,rect.center.y-2.5f,5,5),value?Accent:Muted);return pressed?!value:value;
        }
        public float Slider(Rect rect,string id,float value)
        {
            Register(id);sliders.Add(id);if(focused==id&&adjustment!=0){value=Mathf.Clamp01(value+adjustment*.05f);adjustment=0;}
            var track=Text(1);var thumb=Text(1); // Dedicated cached styles below, no skin-dependent track.
            if(!styles.TryGetValue("sliderTrack",out track)){track=new GUIStyle{normal={background=Swatch(new Color(.16f,.23f,.23f))},fixedHeight=3,margin=new RectOffset(0,0,12,12)};styles["sliderTrack"]=track;}
            if(!styles.TryGetValue("sliderThumb",out thumb)){thumb=new GUIStyle{normal={background=Swatch(Paper)},hover={background=Swatch(Accent)},active={background=Swatch(Accent)},fixedWidth=14,fixedHeight=26};styles["sliderThumb"]=thumb;}
            if(Event.current.type==EventType.MouseDown&&rect.Contains(Event.current.mousePosition))focused=id;
            value=GUI.HorizontalSlider(rect,value,0,1,track,thumb);
            if(focused==id)Line(rect.x,rect.yMax+5,rect.width,Accent);return value;
        }
        public float Scroll(Rect viewport,float value,float totalHeight,string id=null)
        {
            float maximum=Mathf.Max(0,totalHeight-viewport.height);
            var scrollbar=new Rect(viewport.xMax+8,viewport.y,22,viewport.height);
            if(id!=null)
            {
                Register(id);readers.Add(id);
                if(Event.current.type==EventType.MouseDown&&(viewport.Contains(Event.current.mousePosition)||scrollbar.Contains(Event.current.mousePosition)))focused=id;
                if(scrollTarget==id&&scrollFrame==Time.frameCount)
                {
                    value+=scrollLines*62+scrollPages*Mathf.Max(62,viewport.height-42);
                    if(scrollEdge!=0)value=scrollEdge<0?0:maximum;
                    scrollTarget=null;
                }
                if(focused==id)Fill(new Rect(viewport.x-8,viewport.y,2,viewport.height),screenKey=="document"?Seal:Accent);
            }
            if(Event.current.type==EventType.ScrollWheel&&(viewport.Contains(Event.current.mousePosition)||scrollbar.Contains(Event.current.mousePosition)))
            {value+=Event.current.delta.y*28;if(id!=null)focused=id;Event.current.Use();}
            value=Mathf.Clamp(value,0,maximum);
            if(maximum>0)
            {
                var track=Style(functionalFont,1,new Color(.7f,.7f,.7f),TextAnchor.UpperLeft);
                if(!styles.TryGetValue("scrollTrack",out track)){track=new GUIStyle{normal={background=Swatch(new Color(.5f,.49f,.43f,.25f))},fixedWidth=4,margin=new RectOffset(7,7,0,0)};styles["scrollTrack"]=track;}
                if(!styles.TryGetValue("scrollThumb",out var thumb)){thumb=new GUIStyle{normal={background=Swatch(new Color(.37f,.40f,.35f))},hover={background=Swatch(Seal)},active={background=Swatch(Seal)},fixedWidth=12,fixedHeight=36};styles["scrollThumb"]=thumb;}
                value=GUI.VerticalSlider(scrollbar,value,0,maximum,track,thumb);
            }
            return Mathf.Clamp(value,0,maximum);
        }
        public void DrawPaper(Rect rect,bool domestic)
        {
            Fill(new Rect(rect.x+7,rect.y+9,rect.width,rect.height),new Color(0,0,0,.3f));
            GUI.DrawTexture(rect,domestic?domesticPaper:archivePaper,ScaleMode.StretchToFill);
            Fill(new Rect(rect.x,rect.y,3,rect.height),domestic?Seal:new Color(.21f,.38f,.35f));
        }
        // Authored low-contrast fibers; no baked letters, seals or generated words.
        public static Texture2D MakePaper(bool domestic)
        {
            const int n=512;var t=new Texture2D(n,n,TextureFormat.RGB24,false);var pixels=new Color[n*n];uint seed=domestic?71291u:16037u;
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                seed=1664525*seed+1013904223;float grain=((seed>>16)/65535f-.5f)*.015f;
                float fiber=Mathf.Sin(x*.77f+y*1.37f)*Mathf.Sin(y*.21f+x*.035f)*.004f;
                float crease=domestic?Mathf.Exp(-Mathf.Abs(y-n*.56f)/2)*-.023f+Mathf.Exp(-Mathf.Abs(y-n*.56f-3)/1.5f)*.011f:0;
                float edge=domestic?Mathf.Pow(Mathf.Abs((x-n*.5f)/(n*.5f)),8)*-.018f:0;
                pixels[y*n+x]=(domestic?new Color(.91f,.884f,.807f):new Color(.90f,.921f,.898f))+new Color(grain+fiber+crease+edge,grain+fiber+crease+edge,grain+fiber+crease+edge);
            }
            t.SetPixels(pixels);t.Apply();return t;
        }
    }
}
