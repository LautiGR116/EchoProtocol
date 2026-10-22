using UnityEngine;

namespace EchoProtocol.Presentation
{
    public sealed partial class GameSession
    {
        const string ReadingPreferenceKey="EP.LargeReadingText";
        bool readingTextEnlarged;
        float documentReadingHeight;
        string folioNoteText;

        void LoadReadingPreferences()
        {readingTextEnlarged=PlayerPrefs.GetInt(ReadingPreferenceKey,0)==1;}

        void SetReadingTextEnlarged(bool value)
        {
            if(readingTextEnlarged==value)return;
            readingTextEnlarged=value;
            PlayerPrefs.SetInt(ReadingPreferenceKey,value?1:0);
            PlayerPrefs.Save();
        }

        bool IsPersonalDocument()=>documentTitle!=null&&(documentTitle.Contains("llave")||documentTitle.Contains("carta")||documentTitle.Contains("recibidor"));

        void DrawFolioDocument()
        {
            var ui=identity;bool personal=IsPersonalDocument();
            ui.DrawPaper(new Rect(178,32,924,650),personal);
            ui.Tag(new Rect(230,66,795,26),personal?"CORRESPONDENCIA PERSONAL   /   MARA":"ARCHIVO 6B   /   DOCUMENTO APORTADO",personal?ui.Seal:ui.Ink);
            ui.Line(230,103,820,personal?ui.Seal:new Color(.35f,.47f,.43f));

            var viewport=new Rect(230,123,778,437);
            var heading=ui.Display(44,false,ui.Ink);
            float headingHeight=heading.CalcHeight(new GUIContent(documentTitle),viewport.width);
            float bodyStart=headingHeight+24;
            var body=ui.Editorial(readingTextEnlarged?27:22,false,personal);
            string key=documentTitle+"|"+documentBody+"|"+personal+"|"+body.font.GetInstanceID()+"|"+body.fontSize+"|"+viewport.width;
            if(key!=folioDocumentKey)
            {
                // ShowDocument clears the key for a new document; size changes
                // preserve progress through the current text, including its end.
                float fraction=folioDocumentKey==null?0:documentScroll/Mathf.Max(1,documentReadingHeight-viewport.height);
                folioDocumentKey=key;
                documentLayout.Set(documentBody,body,viewport.width,readingTextEnlarged?39:32);
                documentReadingHeight=bodyStart+documentLayout.Height;
                documentScroll=Mathf.Clamp01(fraction)*Mathf.Max(0,documentReadingHeight-viewport.height);
            }
            documentScroll=ui.Scroll(viewport,documentScroll,documentReadingHeight,"document-reading");
            GUI.BeginGroup(viewport);
            GUI.Label(new Rect(0,-documentScroll,viewport.width,headingHeight),documentTitle,heading);
            GUI.EndGroup();
            documentLayout.Draw(viewport,documentScroll,body,bodyStart);

            ui.Line(230,577,820,new Color(.58f,.58f,.50f));
            if(ui.Button(new Rect(230,599,365,50),"document-close","Dejar documento","Esc",true))CloseDocument();
            if(ui.Button(new Rect(617,599,433,50),"document-size",readingTextEnlarged?"Texto de lectura: ampliado":"Texto de lectura: normal"))SetReadingTextEnlarged(!readingTextEnlarged);
            GUI.Label(new Rect(230,657,820,21),ReadingStatus(ui,"document-reading",documentScroll,documentReadingHeight,viewport.height),ui.Text(13,false,ui.Ink));
        }

        void DrawFolioNotes()
        {
            var ui=identity;GUI.Label(new Rect(84,126,830,84),"Notas de la sesión",ui.Display(58));
            string note=contextualHints?contextualHints.CurrentNote:protocolGuidance?protocolGuidance.Context:campaign?campaign.Objective:"El registro conserva el recorrido y sus pausas.";
            ui.Tag(new Rect(88,224,800,24),"RECALL   /   LECTURA DEL ESTADO ACTUAL");
            var style=ui.Text(readingTextEnlarged?27:22);
            var viewport=new Rect(88,265,985,190);
            string key=note+"|"+style.font.GetInstanceID()+"|"+style.fontSize+"|"+viewport.width;
            if(key!=folioNoteKey)
            {
                float fraction=note==folioNoteText?noteScroll/Mathf.Max(1,noteLayout.Height-viewport.height):0;
                folioNoteText=note;folioNoteKey=key;
                noteLayout.Set(note,style,viewport.width,readingTextEnlarged?39:32);
                noteScroll=Mathf.Clamp01(fraction)*Mathf.Max(0,noteLayout.Height-viewport.height);
            }
            noteScroll=ui.Scroll(viewport,noteScroll,noteLayout.Height,"notes-reading");
            noteLayout.Draw(viewport,noteScroll,style);
            string recall=finalDoorMemory&&finalDoorMemory.Active?(finalDoorMemory.OriginalPreserved?"El original está conservado. El registro terminó.":finalDoorMemory.UsesArrivalPresentation&&finalDoorMemory.HasWitnessed?"La llegada está observada. Conservá esta toma en la pantalla de portería.":"R vuelve a 6B y sustituye la toma que estás revisando."):!restoreLoop?"El registro permanece suspendido hasta la próxima revisión.":"R vuelve al último origen y sustituye la toma anterior.";
            GUI.Label(new Rect(88,470,1080,48),recall,ui.Text(16,false,ui.Muted));
            if(contextualHints&&contextualHints.ManualLevel<2&&ui.Button(new Rect(88,523,940,48),"more-note",contextualHints.ManualLevel==0?"Otra pista sobre la relación":"Mostrar pasos de resolución","J"))contextualHints.RequestMore();
            if(ui.Button(new Rect(88,594,530,50),"close-notes",returnToPauseFromNotes?"Volver a pausa":"Volver a la sesión","H / Esc",true))CloseSessionNotes();
            if(ui.Button(new Rect(642,594,526,50),"notes-size",readingTextEnlarged?"Texto de lectura: ampliado":"Texto de lectura: normal"))SetReadingTextEnlarged(!readingTextEnlarged);
            GUI.Label(new Rect(88,674,1104,24),ReadingStatus(ui,"notes-reading",noteScroll,noteLayout.Height,viewport.height),ui.Text(13,false,ui.Muted));
        }

        static string ReadingStatus(FolioUI ui,string id,float scroll,float height,float viewportHeight)
        {
            string position=height>viewportHeight?Mathf.RoundToInt(scroll/Mathf.Max(1,height-viewportHeight)*100)+" % del recorrido":"Texto completo";
            return ui.FocusedControl==id?position+"   ·   ↑ ↓ / Re Pág / Av Pág / Inicio / Fin   ·   Tab: controles":position+"   ·   Rueda: leer   ·   Tab: volver al texto";
        }
    }
}
