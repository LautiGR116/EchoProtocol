using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EchoProtocol.Presentation
{
    // Preserve the original Unicode text; wrap with the selected font and keep
    // the final line's full height inside the scrollable extent.
    public sealed class FolioDocumentLayout
    {
        readonly List<string> lines=new List<string>();
        readonly List<float> positions=new List<float>();
        float lineHeight;
        public float Height {get;private set;}
        public int LineCount=>lines.Count;

        public void Set(string text,GUIStyle style,float width,float leading=31)
        {
            lines.Clear();positions.Clear();float y=0;
            lineHeight=Mathf.Max(leading,style.lineHeight);
            foreach(string paragraph in (text??string.Empty).Replace("\r\n","\n").Replace('\r','\n').Split('\n'))
            {
                if(paragraph.Length==0){y+=lineHeight*.55f;continue;}
                string line="";
                foreach(Match match in Regex.Matches(paragraph,@"\S+|\s+"))
                {
                    string token=match.Value;
                    if(style.CalcSize(new GUIContent(line+token)).x<=width){line+=token;continue;}
                    if(line.Length>0){AddLine(line,ref y);line="";}
                    if(style.CalcSize(new GUIContent(token)).x<=width){line=token;continue;}
                    // An unusually long identifier still remains readable. Split
                    // only at grapheme boundaries, never inside an accented glyph.
                    var elements=StringInfo.GetTextElementEnumerator(token);
                    while(elements.MoveNext())
                    {
                        string element=elements.GetTextElement();
                        if(line.Length>0&&style.CalcSize(new GUIContent(line+element)).x>width)
                        {AddLine(line,ref y);line="";}
                        line+=element;
                    }
                }
                if(line.Length>0)AddLine(line,ref y);
            }
            Height=y+12;
        }

        void AddLine(string line,ref float y)
        {lines.Add(line);positions.Add(y);y+=lineHeight;}

        public void Draw(Rect viewport,float scroll,GUIStyle style,float topInset=0)
        {
            GUI.BeginGroup(viewport);
            for(int i=0;i<lines.Count;i++)
            {
                float y=positions[i]+topInset-scroll;
                if(y>-lineHeight&&y<viewport.height)
                    GUI.Label(new Rect(0,y,viewport.width,lineHeight),lines[i],style);
            }
            GUI.EndGroup();
        }
    }
}
