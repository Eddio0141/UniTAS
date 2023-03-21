namespace UniTAS.Plugin.Interfaces.DependencyInjection;

/// <summary>
/// Explicitly exclude a class from being registered when unit testing
/// </summary>
public class ExcludeRegisterIfTestingAttribute : DependencyInjectionAttribute
{
}