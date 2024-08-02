public class DuplicateWindowIDException : System.Exception
{
    public DuplicateWindowIDException() { }
    public DuplicateWindowIDException(string message) : base(message) { }
    public DuplicateWindowIDException(string message, System.Exception inner) : base(message, inner) { }
    protected DuplicateWindowIDException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
