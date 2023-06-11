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
        public string String1;

        public ReferenceType(string string1)
        {
            String1 = string1;
        }
    }
}