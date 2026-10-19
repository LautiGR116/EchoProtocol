using UnityEngine;

namespace EchoProtocol.Presentation
{
    internal static class WebPlatform
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // The browser owns display pacing; typing outside the focused
            // game canvas must remain available to itch.io's page.
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
            WebGLInput.captureAllKeyboardInput = false;
        }
#endif
    }
}
