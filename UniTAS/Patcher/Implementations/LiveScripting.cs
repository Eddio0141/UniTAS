using System;
using HarmonyLib;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Interop.RegistrationPolicies;
using MoonSharp.Interpreter.Loaders;
using UniTAS.Patcher.Implementations.Proxies;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class LiveScripting : ILiveScripting
{
    private readonly ILogger _logger;

    private class CustomInteropRegistrationPolicy : IRegistrationPolicy
    {
        private static readonly IRegistrationPolicy Default = new DefaultRegistrationPolicy();

        public IUserDataDescriptor HandleRegistration(IUserDataDescriptor newDescriptor,
            IUserDataDescriptor oldDescriptor)
        {
            return Default.HandleRegistration(newDescriptor, oldDescriptor);
        }

        public bool AllowTypeAutoRegistration(Type type)
        {
            var unitasAssembly = typeof(LiveScripting).Assembly;
            return Equals(type.Assembly, unitasAssembly);
        }
    }

    public LiveScripting(ILogger logger)
    {
        UserData.RegistrationPolicy = new CustomInteropRegistrationPolicy();
        // TODO: movie proxies should be registered here instead
        // TODO: proxies must be gathered with some attribute
        UserData.RegisterProxyType<TraverseProxy, Traverse>(x => new TraverseProxy(x));
        _logger = logger;
    }

    public Script NewScript()
    {
        var script = new Script(CoreModules.Preset_Complete)
        {
            Options =
            {
                // do NOT use unity loader
                ScriptLoader = new FileSystemScriptLoader(),
                DebugInput = _ => null,
                DebugPrint = _logger.LogInfo,
            }
        };

        return script;
    }
}