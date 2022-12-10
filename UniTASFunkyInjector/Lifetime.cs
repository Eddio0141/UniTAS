namespace UniTASFunkyInjector;

/// <summary>
/// The lifetime of a registration.
/// </summary>
public enum Lifetime
{
    /// <summary>
    /// The registration is created once and reused.
    /// </summary>
    Singleton,

    /// <summary>
    /// The registration is created every time it is resolved.
    /// </summary>
    Transient
}