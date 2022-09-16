using System;

namespace Core.UnityHelpers.Types;

public class Args
{
    public object Instance { get; private set; }
    public object[] Arguments { get; private set; }

    public Args(object instance, object[] arguments)
    {
        Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }

    public Args(object[] arguments)
    {
        Instance = null;
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }
}
