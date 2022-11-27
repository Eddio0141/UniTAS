using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using FluentAssertions;
using UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser;
using UniTASPlugin.Movie.Models.Script;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Jump;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Loop;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Maths;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Method;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.RegisterSet;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Scope;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.StackOp;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPluginTest.MovieParsing;

public class DefaultMovieScriptParserUnitTests
{
    private static ScriptModel Setup(string input)
    {
        var inputStream = new AntlrInputStream(input);
        var speakLexer = new MovieScriptDefaultGrammarLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(speakLexer);
        var speakParser = new MovieScriptDefaultGrammarParser(commonTokenStream);
        var program = speakParser.script();
        var listener = new DefaultGrammarListenerCompiler();
        ParseTreeWalker.Default.Walk(listener, program);
        var methods = listener.Compile().ToList();
        var mainMethod = methods.First(x => x.Name == null);
        var definedMethods = methods.Where(x => x.Name != null);
        return new ScriptModel(mainMethod, definedMethods);
    }

    [Fact]
    public void AssignTupleOrExpression()
    {
        Setup("$value = ((\"nested\", 5), ((1.0, -2), \"nested 2\", (10 + 5) * (3)))");
    }

    [Fact]
    public void NewMethod()
    {
        var script = Setup("fn method(arg1, arg2) { }");
        var definedMethod = script.Methods[0];

        var actual = new ScriptMethodModel("method",
            new OpCodeBase[]
            {
                new PopArgOpCode(RegisterType.Temp),
                new SetVariableOpCode(RegisterType.Temp, "arg2"),
                new PopArgOpCode(RegisterType.Temp),
                new SetVariableOpCode(RegisterType.Temp, "arg1"),
            });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void AssignVariable()
    {
        var script = Setup(@"$value = 4
$value += 10
$value -= 3
$value *= 100
$value /= 4
$value %= 5");
        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCodeBase[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(4)),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(10)),
            new VarToRegisterOpCode(RegisterType.Temp2, "value"),
            new AddOpCode(RegisterType.Temp, RegisterType.Temp2, RegisterType.Temp),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(3)),
            new VarToRegisterOpCode(RegisterType.Temp2, "value"),
            new SubOpCode(RegisterType.Temp, RegisterType.Temp2, RegisterType.Temp),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(100)),
            new VarToRegisterOpCode(RegisterType.Temp2, "value"),
            new MultOpCode(RegisterType.Temp, RegisterType.Temp2, RegisterType.Temp),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(4)),
            new VarToRegisterOpCode(RegisterType.Temp2, "value"),
            new DivOpCode(RegisterType.Temp, RegisterType.Temp2, RegisterType.Temp),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(5)),
            new VarToRegisterOpCode(RegisterType.Temp2, "value"),
            new ModOpCode(RegisterType.Temp, RegisterType.Temp2, RegisterType.Temp),
            new SetVariableOpCode(RegisterType.Temp, "value"),
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void VariableAssignTypes()
    {
        var script = Setup(@"$value = 10
$value2 = 10.0
$value3 = ""hello world!""
$value4 = true
$value5 = false");
        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCodeBase[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(10)),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp, new FloatValueType(10f)),
            new SetVariableOpCode(RegisterType.Temp, "value2"),
            new ConstToRegisterOpCode(RegisterType.Temp, new StringValueType("hello world!")),
            new SetVariableOpCode(RegisterType.Temp, "value3"),
            new ConstToRegisterOpCode(RegisterType.Temp, new BoolValueType(true)),
            new SetVariableOpCode(RegisterType.Temp, "value4"),
            new ConstToRegisterOpCode(RegisterType.Temp, new BoolValueType(false)),
            new SetVariableOpCode(RegisterType.Temp, "value5"),
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void ExpressionCalculation()
    {
        /*
         * -((1 + 2) - 3 * 4 / 5) % 6
         * %, flipSign, (, -, (, +, 1, 2, ), /, *, 3, (, +, 4, 5, ), 6, ), 7
         * %, flipSign, -, +, 1, 2, /, *, 3, +, 4, 5, 6, 7
         */

        var script = Setup("$value = -((1 + 2) - 3 * 4 / 5) % 6");
        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCodeBase[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(1)),
            new ConstToRegisterOpCode(RegisterType.Temp2, new IntValueType(2)),
            new AddOpCode(RegisterType.Temp, RegisterType.Temp, RegisterType.Temp2),
            new MoveOpCode(RegisterType.Temp, RegisterType.Temp3),
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(3)),
            new ConstToRegisterOpCode(RegisterType.Temp2, new IntValueType(4)),
            new MultOpCode(RegisterType.Temp, RegisterType.Temp, RegisterType.Temp2),
            new ConstToRegisterOpCode(RegisterType.Temp2, new IntValueType(5)),
            new DivOpCode(RegisterType.Temp, RegisterType.Temp, RegisterType.Temp2),
            new SubOpCode(RegisterType.Temp, RegisterType.Temp3, RegisterType.Temp),
            new FlipNegativeOpCode(RegisterType.Temp, RegisterType.Temp),
            new ConstToRegisterOpCode(RegisterType.Temp2, new IntValueType(6)),
            new ModOpCode(RegisterType.Temp, RegisterType.Temp, RegisterType.Temp2),
            new SetVariableOpCode(RegisterType.Temp, "value")
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void BracketsExpressionChain()
    {
        Setup("$value = 1+(2*3+(4+(5+(6+(7+(8))))))");
    }

    [Fact]
    public void ExpressionMethodCall()
    {
        Setup("fn method(arg1, arg2) { } $value = 5+(method(1 * 9, 1+(2+3)*4) * 3)");
    }

    [Fact]
    public void MethodCallChain()
    {
        Setup(
            "fn method(arg){} fn method2(arg){} fn method3(arg){} fn method4(arg){} fn method5(arg){} fn method6(arg){} method(method2(method3(method4(method5(method6((5 + 3) * (3 + 3)))))))");
    }

    [Fact]
    public void MethodCallChain2()
    {
        Setup(
            "fn method(arg){} fn method2(arg){} method(method2((5 + 3) * (3 + 3)))");
    }

    [Fact]
    public void MethodCalculations()
    {
        Setup(@"fn method(arg) { }
$value = method((5 + method(4 * 9 + method(222))) * (4 + 5))");
    }

    [Fact]
    public void IfElse()
    {
        var script = Setup(@"$value = 0
if $value == 0 {
    $value = 1
} else if $value == 1 {
    $value = 2
} else if $value == 2 {
    $value = 3
} else {
    $value = 4
}");
        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCodeBase[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(0)),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new VarToRegisterOpCode(RegisterType.Temp, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp2, new IntValueType(0)),
            new EqualOpCode(RegisterType.Temp, RegisterType.Temp, RegisterType.Temp2),
            new JumpIfFalse(6, RegisterType.Temp),
            new EnterScopeOpCode(),
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(1)),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new ExitScopeOpCode(),
            new JumpOpCode(23),
            new VarToRegisterOpCode(RegisterType.Temp, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp2, new IntValueType(1)),
            new EqualOpCode(RegisterType.Temp, RegisterType.Temp, RegisterType.Temp2),
            new JumpIfFalse(6, RegisterType.Temp),
            new EnterScopeOpCode(),
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(2)),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new ExitScopeOpCode(),
            new JumpOpCode(14),
            new VarToRegisterOpCode(RegisterType.Temp, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp2, new IntValueType(2)),
            new EqualOpCode(RegisterType.Temp, RegisterType.Temp, RegisterType.Temp2),
            new JumpIfFalse(6, RegisterType.Temp),
            new EnterScopeOpCode(),
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(3)),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new ExitScopeOpCode(),
            new JumpOpCode(5),
            new EnterScopeOpCode(),
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(4)),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new ExitScopeOpCode()
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void Scopes()
    {
        var script = Setup(@"$value = 0
{
    $value = 1
}
{
    $value = 2
}");
        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCodeBase[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(0)),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new EnterScopeOpCode(),
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(1)),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new ExitScopeOpCode(),
            new EnterScopeOpCode(),
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(2)),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new ExitScopeOpCode(),
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void Loop()
    {
        var script = Setup(@"fn method(){}
$value = 5
loop $value {
    method();
}");

        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCodeBase[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(5)),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new VarToRegisterOpCode(RegisterType.Temp, "value"),
            new JumpIfEqZero(10, RegisterType.Temp),
            new ConstToRegisterOpCode(RegisterType.Temp2, new IntValueType(1)),
            new SubOpCode(RegisterType.Temp, RegisterType.Temp, RegisterType.Temp2),
            new PushStackOpCode(RegisterType.Temp),
            new EnterScopeOpCode(),
            new GotoMethodOpCode("method"),
            new FrameAdvanceOpCode(),
            new ExitScopeOpCode(),
            new PopStackOpCode(RegisterType.Temp),
            new JumpOpCode(-9)
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void LoopBreakContinue()
    {
        var script = Setup(@"fn method(){}
$value = 5
loop $value {
    if $value == 3 {
        break
    } else if $value == 4 {
        continue
    }
}");

        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCodeBase[]
        {
            // $value = 5
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(5)),
            new SetVariableOpCode(RegisterType.Temp, "value"),
            new VarToRegisterOpCode(RegisterType.Temp, "value"),
            // loop $value
            new JumpIfEqZero(23, RegisterType.Temp),
            new ConstToRegisterOpCode(RegisterType.Temp2, new IntValueType(1)),
            new SubOpCode(RegisterType.Temp, RegisterType.Temp, RegisterType.Temp2),
            new PushStackOpCode(RegisterType.Temp),
            new EnterScopeOpCode(),
            // if
            new VarToRegisterOpCode(RegisterType.Temp, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp2, new IntValueType(3)),
            new EqualOpCode(RegisterType.Temp, RegisterType.Temp, RegisterType.Temp2),
            new JumpIfFalse(12, RegisterType.Temp),
            new EnterScopeOpCode(),
            new BreakOpCode(),
            new ExitScopeOpCode(),
            new JumpOpCode(8),
            // else if
            new VarToRegisterOpCode(RegisterType.Temp, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp2, new IntValueType(4)),
            new EqualOpCode(RegisterType.Temp, RegisterType.Temp, RegisterType.Temp2),
            new JumpIfFalse(4, RegisterType.Temp),
            new EnterScopeOpCode(),
            new ContinueOpCode(),
            new ExitScopeOpCode(),
            // loop end
            new ExitScopeOpCode(),
            new PopStackOpCode(RegisterType.Temp),
            new JumpOpCode(-22)
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void Return()
    {
        var script = Setup(@"fn method(){ return 5 }
$value = method()");

        var definedMethod = script.Methods[0];

        var actual = new ScriptMethodModel("method", new OpCodeBase[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp, new IntValueType(5)),
            new MoveOpCode(RegisterType.Temp, RegisterType.Ret),
            new ReturnOpCode()
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void MainReturn()
    {
        var script = Setup("return");

        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCodeBase[]
        {
            new ReturnOpCode()
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }
}