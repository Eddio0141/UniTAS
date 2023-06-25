namespace UniTAS.Patcher.Services;

public interface ILiveScripting
{
    /// <summary>
    /// Evaluates the given code.
    /// </summary>
    /// <param name="code"></param>
    void Evaluate(string code);
}