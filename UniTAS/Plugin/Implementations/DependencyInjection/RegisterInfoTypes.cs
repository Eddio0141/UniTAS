using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Models.DependencyInjection;

namespace UniTAS.Plugin.Implementations.DependencyInjection;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class DiscoverAndRegister
{
    private abstract class RegisterInfoBase
    {
        protected RegisterInfoBase(RegisterPriority priority)
        {
            Priority = (int)priority;
        }

        public int Priority { get; }
    }

    // register single type
    private class RegisterInfo : RegisterInfoBase
    {
        public Type Type { get; }
        public bool IncludeDifferentAssembly { get; }
        public RegisterAttribute RegisterAttribute { get; }
        public bool IsSingleton { get; }

        public RegisterInfo(Type type, bool includeDifferentAssembly, RegisterAttribute registerAttribute,
            bool isSingleton) : base(registerAttribute?.Priority ?? RegisterPriority.Default)
        {
            Type = type;
            IncludeDifferentAssembly = includeDifferentAssembly;
            RegisterAttribute = registerAttribute;
            IsSingleton = isSingleton;
        }
    }

    private class RegisterAllInfo : RegisterInfoBase
    {
        public Type Type { get; }
        public Type InnerType { get; }

        public RegisterAllInfo(Type type, Type innerType, RegisterAttribute registerAttribute) : base(registerAttribute
            .Priority)
        {
            Type = type;
            InnerType = innerType;
        }
    }
}