using System.Diagnostics.CodeAnalysis;
using BepInEx;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using StructureMap;
using StructureMap.Pipeline;
using UniTAS.Patcher.Exceptions.Movie.Parser;
using UniTAS.Patcher.Implementations.Movie.Engine;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Models.Movie;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Services.UnitySafeWrappers;
#if !UNIT_TESTS
using UnityEngine;
using MoonSharp.Interpreter.Interop;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
#endif

namespace UniTAS.Patcher.Implementations.Movie.Parser;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[Register]
public partial class MovieParser(
    IMovieLogger movieLogger,
#if !UNIT_TESTS
    ILogger logger,
#endif
    IEngineModuleClassesFactory engineModuleClassesFactory,
    IContainer container,
    MovieProxyType[] movieProxyTypes,
    IUnityInstanceWrapFactory unityInstanceWrapFactory)
    : IMovieParser
{
    public (IMovieEngine, PropertiesModel) Parse(string input)
    {
        var scriptAndMovieEngine = SetupScript();
        var script = scriptAndMovieEngine.Item1;
        var movieEngine = scriptAndMovieEngine.Item2;

        var wrappedInput = WrapInput(input);
        ImplementDummyMethods(movieEngine);
        var engineCoroutine = script.DoString(wrappedInput);

        // yield once to see if we are using global scope
        engineCoroutine = script.CreateCoroutine(engineCoroutine);
        engineCoroutine.Coroutine.Resume();

        var processedConfig = ProcessConfig(script);
        var useGlobalScope = processedConfig.Item1;
        var properties = processedConfig.Item2;
        if (useGlobalScope)
        {
            script = SetupScript(movieEngine, properties).Item1;
            engineCoroutine = script.DoString(input);

            // because we are using global scope, we expect a function to be returned
            if (engineCoroutine.Type != DataType.Function)
            {
                throw new NotReturningFunctionException("Expected a function to be returned from the global scope");
            }
        }
        else
        {
            script = SetupScript(movieEngine, properties).Item1;
            engineCoroutine = script.DoString(wrappedInput);
        }

        // reset the engine
        movieEngine.InitCoroutine(engineCoroutine);

        return new(movieEngine, properties);
    }

    private static bool _registeredGlobals;

    private (Script, MovieEngine) SetupScript(MovieEngine movieEngine = null,
        PropertiesModel properties = null)
    {
        // TODO add OS_Time, OS_System, IO later for manipulating state of the vm
        var script = new Script(CoreModules.Basic | CoreModules.GlobalConsts | CoreModules.TableIterators |
                                CoreModules.Metatables | CoreModules.String | CoreModules.LoadMethods |
                                CoreModules.Table | CoreModules.ErrorHandling | CoreModules.Math |
                                CoreModules.Coroutine | CoreModules.Bit32 | CoreModules.Debug | CoreModules.Dynamic)
        {
            Options =
            {
                // do NOT use unity loader
                ScriptLoader = new FileSystemScriptLoader
                    { ModulePaths = [$"{Paths.GameRootPath}/?.lua", $"{Paths.GameRootPath}/?"] },
                DebugInput = _ => null
            }
        };

        if (movieEngine == null)
        {
            script.Options.DebugPrint = _ => { };
        }
        else
        {
            script.Options.DebugPrint = s => movieLogger.LogInfo(s);
        }

        if (movieEngine == null)
        {
            var args = new ExplicitArguments();
            args.Set(script);
            movieEngine = container.GetInstance<MovieEngine>(args);
        }
        else
        {
            movieEngine.Script = script;
        }

        if (properties != null)
        {
            movieEngine.Properties = properties;
        }

        if (!_registeredGlobals)
        {
            _registeredGlobals = true;
            UserData.RegisterAssembly(typeof(MovieParser).Assembly);
            AddProxyTypes();
#if !UNIT_TESTS
            AddUnityTypes();
#endif
        }

        AddEngineMethods(movieEngine);
        AddCustomTypes(script);

        return new(script, movieEngine);
    }

    private void AddEngineMethods(IMovieEngine engine)
    {
        var script = engine.Script;

        var engineMethodClasses = engineModuleClassesFactory.GetAll(engine);

        foreach (var methodClass in engineMethodClasses)
        {
            UserData.RegisterType(methodClass.GetType());
            var className = methodClass.GetType().Name.ToLowerInvariant();
            script.Globals[className] = methodClass;
        }

        RegisterModuleTypes(engine);
    }

    private static void AddCustomTypes(Script script)
    {
        AddFrameAdvance(script);
    }

    private void AddProxyTypes()
    {
        foreach (var movieProxyType in movieProxyTypes)
        {
            UserData.RegisterProxyType(movieProxyType);
        }
    }

#if !UNIT_TESTS
    private static readonly Assembly[] UnityTypesIgnore = [typeof(MovieParser).Assembly, typeof(Paths).Assembly];

    private void AddUnityTypes()
    {
        // only add types that isn't going to affect unity
        UserData.RegisterType<Vector3>();
        UserData.RegisterType<Matrix4x4>();

        // add unity scripts
        var monoBehaviours =
            AppDomain.CurrentDomain.GetAssemblies().Where(x => !UnityTypesIgnore.Any(a => Equals(x, a)))
                .SelectMany(AccessTools.GetTypesFromAssembly)
                .Where(x => x.IsSubclassOf(typeof(MonoBehaviour)) && !x.IsAbstract);

        foreach (var monoBehaviour in monoBehaviours)
        {
            // idk why there is duplicate types but distinct doesn't seem to work
            if (UserData.IsTypeRegistered(monoBehaviour)) continue;

            var userDataDesc = UserData.RegisterType(monoBehaviour, InteropAccessMode.HideMembers);
            if (userDataDesc is not StandardUserDataDescriptor desc)
            {
                movieLogger.LogWarning(
                    $"Failed to register type: {monoBehaviour.FullName}, you won't be able to access it in the script");
                continue;
            }

            var fields = monoBehaviour.GetFields(AccessTools.all);
            foreach (var field in fields)
            {
                try
                {
                    desc.AddMember(field.Name, new ReadOnlyFieldDescriptor(field, InteropAccessMode.Default));
                }
                catch (Exception e)
                {
                    logger.LogWarning($"something went wrong while calling desc.AddMember, ignoring: {e}");
                }
            }
        }
    }
#endif

    internal const string frameAdvanceFullName = "movie.frame_advance";

    private static void AddFrameAdvance(Script script)
    {
        // manually modify movie.frame_advance
        const string frameAdvance = $@"
{frameAdvanceFullName} = function(frames)
    if type(frames) ~= 'number' then
        frames = 1
    end
    for i = 1, frames do
        coroutine.yield()
    end
end";

        script.DoString(frameAdvance);
    }

    private static string WrapInput(string input)
    {
        return $"return function()\n{input}\nend";
    }
}