namespace Patcher.Tests.Serialization;

public static class SerializationUtils
{
    public class TestClassWithInts
    {
        public static int Int1 = 1;
#pragma warning disable CS0414
        private static int _int2 = 2;
#pragma warning restore CS0414
        public int Int3 = 3;
    }

    public class InstanceLoop
    {
        public static InstanceLoop Instance = new();
    }

    public class ReferenceType
    {
        public int Value;
    }

    public class ReferencingType
    {
#pragma warning disable CS8618
        public static ReferenceType ReferenceType;
#pragma warning restore CS8618
    }

    public class ReferencingType2
    {
#pragma warning disable CS8618
        public static ReferenceType ReferenceType;
#pragma warning restore CS8618
    }
}