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
}