namespace UniTAS.Patcher.Implementations.Customization;

public partial class Config
{
    public static class Sections
    {
        public static class Debug
        {
            public static class FunctionCallTrace
            {
                public const string SectionName = $"{nameof(Debug)}.FunctionCallTrace";
                public const string Enable = "Enable";
                public const string Methods = "Methods";
            }
        }

        public static class Remote
        {
            public const string SectionName = $"{nameof(Remote)}";
            public const string Enable = "Enable";
            public const string Address = "Address";
            public const string Port = "Port";
        }
    }
}