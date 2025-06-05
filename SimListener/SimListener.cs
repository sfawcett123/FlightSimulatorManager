
namespace SimListener
{
    internal class SimListener : IEquatable<SimListener?>
    {
        public Definition eDef = Definition.Dummy;
        public Request eRequest = Request.Dummy;
        public string? Parameter { get; set; }
        public string? Measure { get; set; }
        public bool bIsString = false;
        public bool bPending = true;
        public bool bStillPending = false;
        public string? Value { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as SimListener);
        }

        public bool Equals(SimListener? other)
        {
            return other is not null &&
                   Parameter == other.Parameter &&
                   Measure == other.Measure;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Parameter);
        }

        public static bool operator ==(SimListener? left, SimListener? right)
        {
            return EqualityComparer<SimListener>.Default.Equals(left, right);
        }

        public static bool operator !=(SimListener? left, SimListener? right)
        {
            return !(left == right);
        }
    }
}