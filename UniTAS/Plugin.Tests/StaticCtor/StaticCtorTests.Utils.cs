using System.Diagnostics.CodeAnalysis;

namespace UniTAS.Plugin.Tests.StaticCtor;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public partial class StaticCtorTests
{
    private class StaticCtorTest
    {
        public static int StaticField = Assign(nameof(StaticField), 5);
        public static int StaticField2;

        static StaticCtorTest()
        {
            Console.WriteLine("StaticCtorTest");
            StaticField2 = Assign(nameof(StaticField2), StaticField);
        }

        private static int Assign(string msg, int value)
        {
            Console.WriteLine(msg);
            return value;
        }
    }

    private class StaticCtorTestBase
    {
        public static int StaticFieldBase = Assign(nameof(StaticFieldBase), 5);
        public static int StaticFieldBase2 = Assign(nameof(StaticFieldBase2), 10);

        static StaticCtorTestBase()
        {
            Console.WriteLine("StaticCtorTest Base");
        }

        private static int Assign(string msg, int value)
        {
            Console.WriteLine(msg);
            return value;
        }
    }

    private class StaticCtorTestBase2
    {
        public static int StaticFieldBase = Assign(nameof(StaticFieldBase), 5);
        public static int StaticFieldBase2 = Assign(nameof(StaticFieldBase2), 10);

        static StaticCtorTestBase2()
        {
            Console.WriteLine("StaticCtorTest Base");
        }

        private static int Assign(string msg, int value)
        {
            Console.WriteLine(msg);
            return value;
        }
    }

    private class StaticCtorTestDerived : StaticCtorTestBase
    {
        public static int StaticFieldDerived = Assign(nameof(StaticFieldDerived), 15);
        public static int StaticFieldDerived2 = Assign(nameof(StaticFieldDerived2), 20);

        static StaticCtorTestDerived()
        {
            Console.WriteLine("StaticCtorTest Derived");
        }

        private static int Assign(string msg, int value)
        {
            Console.WriteLine(msg);
            return value;
        }
    }

    private class StaticCtorTestDerived2 : StaticCtorTestBase2
    {
        public static int StaticField2Derived = Assign(nameof(StaticField2Derived), 15);
        public static int StaticField2Derived2 = Assign(nameof(StaticField2Derived2), 20);

        static StaticCtorTestDerived2()
        {
            // access StaticCtorTestBase
#pragma warning disable CS1717
            StaticFieldBase = StaticFieldBase;
#pragma warning restore CS1717
            Console.WriteLine("Accessing StaticCtorTestBase");
            Console.WriteLine("StaticCtorTest Derived 2");
        }

        private static int Assign(string msg, int value)
        {
            Console.WriteLine(msg);
            return value;
        }
    }

    private class StaticCtorMethodTest
    {
        public static int StaticField = Assign(nameof(StaticField), 5);

        static StaticCtorMethodTest()
        {
            Console.WriteLine(nameof(StaticCtorMethodTest));
        }

        public static int StaticMethod()
        {
            Console.WriteLine(nameof(StaticMethod));
            return 0;
        }

        private static int Assign(string msg, int value)
        {
            Console.WriteLine(msg);
            return value;
        }
    }
}