namespace UniTASPlugin.Movie.MovieRunner.EngineMethods.Exceptions;

public class MethodArgCountNotMatching : RegisterExternalMethodException
{
    public MethodArgCountNotMatching(string expected, string actual) : base(
        $"Registering method arg count not matching, expected: {expected}, actual: {actual}")
    {
    }
}