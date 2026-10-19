using UnityEngine;
namespace EchoProtocol.Presentation
{
    // A local reader practical follows the existing first-act circuit and fade.
    [ExecuteAlways, DefaultExecutionOrder(100)]
    public sealed class LinkedTaskLight : MonoBehaviour
    {
        [SerializeField] private Light circuitSource;
        [SerializeField] private Light taskLight;
        [SerializeField, Min(0f)] private float sourceNominalIntensity = 1f;
        [SerializeField, Min(0f)] private float taskIntensity = 1f;
        private void LateUpdate()
        {
            if (!circuitSource || !taskLight) return;
            taskLight.enabled = circuitSource.enabled && circuitSource.gameObject.activeInHierarchy;
            taskLight.intensity = taskIntensity * Mathf.Clamp01(circuitSource.intensity / Mathf.Max(.001f, sourceNominalIntensity));
        }
    }
}
