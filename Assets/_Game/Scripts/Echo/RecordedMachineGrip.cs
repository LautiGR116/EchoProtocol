namespace EchoProtocol.Echo
{
    // Only the configured machine's spring-loaded grip is captured. No audio,
    // inventory, world snapshot or inferred interaction is part of this channel.
    public readonly struct RecordedMachineGrip
    {
        public readonly float Timestamp;
        public readonly int Target;
        public readonly bool Held;
        public RecordedMachineGrip(float timestamp, int target, bool held)
        { Timestamp = timestamp; Target = target; Held = held; }
    }
}
