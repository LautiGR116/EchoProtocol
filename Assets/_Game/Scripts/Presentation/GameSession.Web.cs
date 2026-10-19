#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
using UnityEngine;

namespace EchoProtocol.Presentation
{
    public sealed partial class GameSession
    {
        private bool webPointerWasLocked;

        [DllImport("__Internal")]
        private static extern void EchoWebExit();

        [DllImport("__Internal")]
        private static extern int EchoWebPointerState();

        private void LateUpdate()
        {
            // Cursor.lockState can keep reporting Unity's requested state after
            // the browser consumes Escape. Read the actual DOM capture state.
            int pointerState = EchoWebPointerState();
            bool locked = pointerState > 0;
            if (pointerState == 2 && started && paused && !ending) BeginOrResume();
            // Browsers may consume Escape before Unity receives the key. Losing
            // pointer capture must still stop movement, the take and pursuit.
            if ((pointerState < 0 || webPointerWasLocked && !locked) && started && !paused && !ending)
                SetPaused(true);
            webPointerWasLocked = locked;
        }
    }
}
#endif
