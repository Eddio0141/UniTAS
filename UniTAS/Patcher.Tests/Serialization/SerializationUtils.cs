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

    public class TestClassWithStrings
    {
        public static string String1 = "1";
        public static string String2 = "2";
#pragma warning disable CS8618
        public static string String3;
#pragma warning restore CS8618
    }
}