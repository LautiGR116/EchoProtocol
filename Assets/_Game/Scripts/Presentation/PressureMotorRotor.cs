using EchoProtocol.Campaign;
using UnityEngine;

namespace EchoProtocol.Presentation
{
    // Only presents the existing motor envelope. Isolation owns all state and timing.
    [DefaultExecutionOrder(210), DisallowMultipleComponent]
    public sealed class PressureMotorRotor : MonoBehaviour
    {
        [SerializeField] private PressureIsolation isolation;
        [SerializeField] private GameSession session;
        [SerializeField] private Transform rotor;
        [SerializeField] private Vector3 localAxis = Vector3.right;
        [SerializeField, Min(0f)] private float degreesPerSecond = 1440f;

        private Quaternion restRotation;
        private float angle;

        private void Awake()
        {
            if (!isolation || !session || !rotor || localAxis.sqrMagnitude < .001f)
            {
                Debug.LogError("PressureMotorRotor requires explicit isolation, session, rotor and axis references.", this);
                enabled = false;
                return;
            }
            localAxis.Normalize();
            restRotation = rotor.localRotation;
        }

        private void LateUpdate()
        {
            if (!session.HasStarted || session.IsPaused || Time.timeScale <= 0f ||
                !isolation.isActiveAndEnabled || isolation.MotorLevel01 <= 0f) return;
            angle = Mathf.Repeat(angle + degreesPerSecond * isolation.MotorLevel01 * Time.deltaTime, 360f);
            rotor.localRotation = restRotation * Quaternion.AngleAxis(angle, localAxis);
        }
    }
}
