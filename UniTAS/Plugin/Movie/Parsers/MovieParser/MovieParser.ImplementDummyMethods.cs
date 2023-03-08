using System;
using System.Linq;
using HarmonyLib;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using UniTAS.Plugin.Movie.Engine;

namespace UniTAS.Plugin.Movie.Parsers.MovieParser;

public partial class MovieParser
{
    /// <summary>
    /// Implement dummy methods for the engine, so script can call them but doesn't accidentally call the real methods
    /// </summary>
    private void ImplementDummyMethods(IMovieEngine engine)
    {
        var script = engine.Script;

        var engineMethodClasses = _engineMethodClassesFactory.GetAll(engine);

        foreach (var methodClass in engineMethodClasses)
        {
            RegisterClassInner(script, methodClass.GetType());
        }

        foreach (var moduleType in _moduleTypes)
        {
            RegisterModuleInner(script, moduleType);
        }

        // we need frame advance at least
        AddFrameAdvance(script);
    }

    private static void RegisterClassInner(Script script, Type type)
    {
        var className = type.Name.ToLowerInvariant();
        var classTable = DynValue.NewTable(script);
        script.Globals[className] = classTable;

        // methods
        var methods = AccessTools.GetDeclaredMethods(type);
        foreach (var method in methods)
        {
            var moonSharpHiddenAttribute = method.GetCustomAttributes(typeof(MoonSharpHiddenAttribute), false)
                .FirstOrDefault();

            if (moonSharpHiddenAttribute is not null) continue;

            var moonSharpVisibleAttributeRaw = method.GetCustomAttributes(typeof(MoonSharpVisibleAttribute), false)
                .FirstOrDefault();

            if (!(method.IsPublic ||
                  moonSharpVisibleAttributeRaw is MoonSharpVisibleAttribute { Visible: true })) continue;

            var methodName = method.Name.ToLowerInvariant();
            classTable.Table.Set(methodName, DynValue.NewCallback((_, _) => DynValue.Nil));
        }
    }

    private static void RegisterModuleInner(Script script, Type type)
    {
        var moonsharpModuleAttributeRaw =
            type.GetCustomAttributes(typeof(MoonSharpModuleAttribute), false).FirstOrDefault();

        var nameSpace = moonsharpModuleAttributeRaw is not MoonSharpModuleAttribute moonsharpModuleAttribute
            ? null
            : moonsharpModuleAttribute.Namespace;

        var moduleTable = DynValue.NewTable(script);
        script.Globals[nameSpace] = moduleTable;

        // methods
        var methods = AccessTools.GetDeclaredMethods(type);
        foreach (var method in methods)
        {
            var moonSharpModuleMethodAttributeRaw =
                method.GetCustomAttributes(typeof(MoonSharpModuleMethodAttribute), false).FirstOrDefault();

            if (moonSharpModuleMethodAttributeRaw is not MoonSharpModuleMethodAttribute moonSharpModuleMethodAttribute)
                continue;

            var methodName = moonSharpModuleMethodAttribute.Name?.ToLowerInvariant() ?? method.Name.ToLowerInvariant();

            moduleTable.Table.Set(methodName, DynValue.NewCallback((_, _) => DynValue.Nil));
        }
    }
}