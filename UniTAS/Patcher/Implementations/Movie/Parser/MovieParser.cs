using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
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
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Movie.Parser;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[Register]
public partial class MovieParser : IMovieParser
{
    private readonly IMovieLogger _logger;
    private readonly IEngineModuleClassesFactory _engineModuleClassesFactory;
    private readonly IContainer _container;
    private readonly MovieProxyType[] _movieProxyTypes;

    public MovieParser(IMovieLogger logger, IEngineModuleClassesFactory engineModuleClassesFactory,
        IContainer container, MovieProxyType[] movieProxyTypes)
    {
        _logger = logger;
        _engineModuleClassesFactory = engineModuleClassesFactory;
        _container = container;
        _movieProxyTypes = movieProxyTypes;
    }

    public Tuple<IMovieEngine, PropertiesModel> Parse(string input)
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

    private Tuple<Script, MovieEngine> SetupScript(MovieEngine movieEngine = null, PropertiesModel properties = null)
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
                ScriptLoader = new FileSystemScriptLoader(),
                DebugInput = _ => null
            }
        };

        if (movieEngine == null)
        {
            script.Options.DebugPrint = _ => { };
        }
        else
        {
            script.Options.DebugPrint = s => _logger.LogInfo(s);
        }

        if (movieEngine == null)
        {
            var args = new ExplicitArguments();
            args.Set(script);
            movieEngine = _container.GetInstance<MovieEngine>(args);
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
            AddUnityTypes();
        }

        AddEngineMethods(movieEngine);
        AddCustomTypes(script);

        return Tuple.New(script, movieEngine);
    }

    private void AddEngineMethods(IMovieEngine engine)
    {
        var script = engine.Script;

        var engineMethodClasses = _engineModuleClassesFactory.GetAll(engine);

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
        foreach (var movieProxyType in _movieProxyTypes)
        {
            UserData.RegisterProxyType(movieProxyType);
        }
    }

    private static readonly Assembly[] UnityTypesIgnore =
        { typeof(MovieParser).Assembly, typeof(BepInEx.Paths).Assembly };

    private static void AddUnityTypes()
    {
        if (UnitTestUtils.IsTesting) return;

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

            var desc = (StandardUserDataDescriptor)UserData.RegisterType(monoBehaviour, InteropAccessMode.HideMembers);

            var fields = AccessTools.GetDeclaredFields(monoBehaviour);
            foreach (var field in fields)
            {
                desc.AddMember(field.Name, new ReadOnlyFieldDescriptor(field, InteropAccessMode.Default));
            }
        }
    }

    private static void AddFrameAdvance(Script script)
    {
        // manually modify movie.frame_advance
        const string frameAdvance = @"
movie.frame_advance = function(frames)
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