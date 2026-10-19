using System.Collections.Generic;
using UnityEngine;

namespace EchoProtocol.Presentation
{
    // Text remains real Unicode. Fixed leading and paragraph gaps are measured
    // with the exact selected font; long documents are clipped only by the scroll viewport.
    public sealed class FolioDocumentLayout
    {
        readonly List<string> lines=new List<string>();
        readonly List<float> positions=new List<float>();
        public float Height {get;private set;}
        public int LineCount=>lines.Count;
        public void Set(string text,GUIStyle style,float width,float leading=31)
        {
            lines.Clear();positions.Clear();float y=0;
            foreach(string paragraph in text.Replace("\r","").Split('\n'))
            {
                if(paragraph.Length==0){y+=17;continue;}
                string line="";
                foreach(string word in paragraph.Split(' '))
                {
                    string trial=line.Length==0?word:line+" "+word;
                    if(style.CalcSize(new GUIContent(trial)).x>width&&line.Length>0){lines.Add(line);positions.Add(y);y+=leading;line=word;}else line=trial;
                }
                lines.Add(line);positions.Add(y);y+=leading;
            }
            Height=y+12;
        }
        public void Draw(Rect viewport,float scroll,GUIStyle style)
        {
            GUI.BeginGroup(viewport);
            for(int i=0;i<lines.Count;i++){float y=positions[i]-scroll;if(y>-38&&y<viewport.height)GUI.Label(new Rect(0,y,viewport.width,38),lines[i],style);}
            GUI.EndGroup();
        }
    }
}
