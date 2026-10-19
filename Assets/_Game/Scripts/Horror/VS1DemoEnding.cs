using EchoProtocol.Echo;
using EchoProtocol.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace EchoProtocol.Horror
{
    [DisallowMultipleComponent]
    public sealed class VS1DemoEnding : MonoBehaviour
    {
        [SerializeField] private VS1DenialEvidence evidence;
        [SerializeField] private SD1SubtitleDisplay thought;
        [SerializeField] private FirstPersonController playerController;
        [SerializeField] private LoopController loop;
        [SerializeField, Min(0.1f)] private float fadeSeconds = 0.65f;

        private bool witnessed;
        private bool fading;
        private float fadeStartedAt;
        private int selectedOption;
        private GUIStyle titleStyle;
        private GUIStyle buttonStyle;

        public bool IsFading => fading;
        public bool IsMenuVisible => fading && Time.unscaledTime - fadeStartedAt >= fadeSeconds;

        private void Awake()
        {
            if (evidence != null && thought != null && playerController != null && loop != null)
                return;

            Debug.LogError("VS1DemoEnding needs explicit D1, thought, Player and loop references.", this);
            enabled = false;
        }

        private void Update()
        {
            if (!witnessed)
            {
                if (!evidence.HasWitnessed) return;
                witnessed = true;
                // Keep the delivered thought on screen for its full authored duration.
                playerController.enabled = false;
                loop.enabled = false;
            }

            if (!fading)
            {
                if (thought.IsShowing) return;
                fading = true;
                fadeStartedAt = Time.unscaledTime;
                return;
            }

            if (!IsMenuVisible || Keyboard.current == null) return;
            if (Keyboard.current.upArrowKey.wasPressedThisFrame
                || Keyboard.current.downArrowKey.wasPressedThisFrame)
                selectedOption = 1 - selectedOption;
            if (Keyboard.current.enterKey.wasPressedThisFrame
                || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                Choose(selectedOption);
        }

        private void OnGUI()
        {
            if (!fading) return;
            GUI.depth = -1000;
            float alpha = Mathf.Clamp01((Time.unscaledTime - fadeStartedAt) / fadeSeconds);
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, alpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previousColor;
            if (!IsMenuVisible) return;

            EnsureStyles();
            float width = Mathf.Min(420f, Screen.width * 0.8f);
            float left = (Screen.width - width) * 0.5f;
            float middle = Screen.height * 0.5f;
            GUI.Label(new Rect(left, middle - 105f, width, 65f), "FIN DE LA DEMO", titleStyle);
            if (GUI.Button(new Rect(left, middle + 5f, width, 48f),
                selectedOption == 0 ? "> Reiniciar" : "Reiniciar", buttonStyle)) Choose(0);
            if (GUI.Button(new Rect(left, middle + 65f, width, 48f),
                selectedOption == 1 ? "> Salir" : "Salir", buttonStyle)) Choose(1);
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 32,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = Color.white;
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22
            };
        }

        private void Choose(int option)
        {
            if (!IsMenuVisible) return;
            if (option == 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnValidate() => fadeSeconds = Mathf.Max(0.1f, fadeSeconds);
    }
}
