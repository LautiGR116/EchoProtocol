using UnityEngine;

namespace EchoProtocol.Horror
{
    [DisallowMultipleComponent]
    public sealed class SD1SubtitleDisplay : MonoBehaviour
    {
        [SerializeField] private int fontSize = 28;
        [SerializeField] private Color textColor = new Color(0.9f, 0.92f, 0.9f, 1f);
        [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.8f);

        private GUIStyle textStyle;
        private GUIStyle shadowStyle;
        private string currentText;
        private float hideAtTime;

        public bool IsShowing => !string.IsNullOrEmpty(currentText)
            && Time.time < hideAtTime;
        public string VisibleText => IsShowing ? currentText : "";

        public void Show(string subtitle, float duration)
        {
            currentText = subtitle;
            hideAtTime = Time.time + Mathf.Max(0f, duration);
        }

        private void OnGUI()
        {
            if(EchoProtocol.Presentation.FolioUI.Current)return;
            if (!IsShowing)
            {
                return;
            }

            EnsureStyles();
            float width = Mathf.Min(Screen.width * 0.8f, 900f);
            var rect = new Rect(
                (Screen.width - width) * 0.5f,
                Screen.height * 0.82f,
                width,
                fontSize * 2.2f);
            var shadowRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height);
            GUI.Label(shadowRect, currentText, shadowStyle);
            GUI.Label(rect, currentText, textStyle);
        }

        private void EnsureStyles()
        {
            if (textStyle != null)
            {
                return;
            }

            textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                fontStyle = FontStyle.Normal
            };
            textStyle.normal.textColor = textColor;

            shadowStyle = new GUIStyle(textStyle);
            shadowStyle.normal.textColor = shadowColor;
        }

        private void OnValidate()
        {
            fontSize = Mathf.Max(12, fontSize);
            textStyle = null;
            shadowStyle = null;
        }
    }
}
