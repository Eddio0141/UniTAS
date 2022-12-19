using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Movie.MovieRunner.EngineMethods.Exceptions;
using UniTASPlugin.Movie.MovieRunner.LowLevel;
using UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes;
using UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.Method;
using UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.RegisterSet;
using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;
using UniTASPlugin.Movie.MovieRunner.MovieModels.Script;
using UniTASPlugin.Movie.MovieRunner.ValueTypes;

namespace UniTASPlugin.Movie.MovieRunner;

public partial class ScriptEngineMovieRunner
{
    public int RegisterConcurrentMethod(string methodName, bool preUpdate,
        IEnumerable<IEnumerable<ValueType>> defaultArgs)
    {
        if (methodName == null) return -1;

        var foundDefinedMethod = _mainScript.Methods.ToList().Find(x => x.Name == methodName);
        var externFound = foundDefinedMethod == null ? _externalMethods.ToList().Find(x => x.Name == methodName) : null;

        if (foundDefinedMethod == null && externFound == null)
        {
            return -1;
        }

        // check if arg count match
        var argCount = foundDefinedMethod == null
            ? externFound.ArgCount
            : foundDefinedMethod.OpCodes.Count(x => x is PopArgOpCode);

        var argTuples = defaultArgs as IEnumerable<ValueType>[] ?? defaultArgs.ToArray();
        if (argCount != argTuples.Count())
        {
            throw new MethodArgCountNotMatching(argCount.ToString(), argTuples.Count().ToString());
        }

        ScriptModel runnerScript;
        if (foundDefinedMethod != null)
        {
            var mainMethod = new List<OpCode>();
            foreach (var argTuple in argTuples)
            {
                foreach (var arg in argTuple)
                {
                    mainMethod.Add(new ConstToRegisterOpCode(RegisterType.Temp0, arg));
                    mainMethod.Add(new PushArgOpCode(RegisterType.Temp0));
                }
            }

            mainMethod.Add(new GotoMethodOpCode(foundDefinedMethod.Name));

            runnerScript = new(new(null, mainMethod),
                new[] { foundDefinedMethod });
        }
        else
        {
            var wrapperMethod = new List<OpCode>();
            foreach (var argTuple in argTuples)
            {
                foreach (var arg in argTuple)
                {
                    wrapperMethod.Add(new ConstToRegisterOpCode(RegisterType.Temp0, arg));
                    wrapperMethod.Add(new PushArgOpCode(RegisterType.Temp0));
                }
            }

            wrapperMethod.Add(new GotoMethodOpCode(externFound.Name));

            wrapperMethod.Add(new GotoMethodOpCode(externFound.Name));
            runnerScript = new(new(null, wrapperMethod), new ScriptMethodModel[0]);
        }

        var engine = new ScriptEngineLowLevelEngine(runnerScript, _externalMethods);

        if (preUpdate)
        {
            _concurrentRunnersPreUpdate.Add(engine);
            // actually run it since otherwise it will skip a frame
            engine.ExecUntilStop(this);
            if (engine.FinishedExecuting)
            {
                engine.Reset();
            }
        }
        else
        {
            _concurrentRunnersPostUpdate.Add(engine);
        }

        return preUpdate
            ? _concurrentRunnersPreUpdate.Last().GetHashCode()
            : _concurrentRunnersPostUpdate.Last().GetHashCode();
    }

    public void UnregisterConcurrentMethod(int hashCode, bool preUpdate)
    {
        if (preUpdate)
        {
            var foundRunner = _concurrentRunnersPreUpdate.FindIndex(x => x.GetHashCode() == hashCode);
            if (foundRunner < 0) return;
            _concurrentRunnersPreUpdate.RemoveAt(foundRunner);
        }
        else
        {
            var foundRunner = _concurrentRunnersPostUpdate.FindIndex(x => x.GetHashCode() == hashCode);
            if (foundRunner < 0) return;
            _concurrentRunnersPostUpdate.RemoveAt(foundRunner);
        }
    }

    private void ConcurrentRunnersPreUpdate()
    {
        foreach (var runner in _concurrentRunnersPreUpdate)
        {
            runner.ExecUntilStop(this);
            if (runner.FinishedExecuting)
            {
                runner.Reset();
            }
        }
    }

    private void ConcurrentRunnersPostUpdate()
    {
        foreach (var runner in _concurrentRunnersPostUpdate)
        {
            runner.ExecUntilStop(this);
            if (runner.FinishedExecuting)
            {
                runner.Reset();
            }
        }
    }
}