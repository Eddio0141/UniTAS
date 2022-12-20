using FluentAssertions;
using UniTASPlugin.Movie.EngineMethods;
using UniTASPlugin.Movie.Exceptions.ParseExceptions;
using UniTASPlugin.Movie.LowLevel.OpCodes;
using UniTASPlugin.Movie.LowLevel.OpCodes.Jump;
using UniTASPlugin.Movie.LowLevel.OpCodes.Logic;
using UniTASPlugin.Movie.LowLevel.OpCodes.Maths;
using UniTASPlugin.Movie.LowLevel.OpCodes.Method;
using UniTASPlugin.Movie.LowLevel.OpCodes.RegisterSet;
using UniTASPlugin.Movie.LowLevel.OpCodes.Scope;
using UniTASPlugin.Movie.LowLevel.OpCodes.StackOp;
using UniTASPlugin.Movie.LowLevel.OpCodes.Tuple;
using UniTASPlugin.Movie.LowLevel.Register;
using UniTASPlugin.Movie.MovieModels.Script;
using UniTASPlugin.Movie.Parsers.MovieScriptParser;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPluginTest.MovieParsing;

public class MovieParserUnitTests
{
    private static ScriptModel Setup(string input)
    {
        var parser = new MovieScriptParser(new[] { new PrintExternalMethod() });
        var methods = parser.Parse(input).ToList();
        var mainMethod = methods.First(x => x.Name == null);
        var definedMethods = methods.Where(x => x.Name != null);
        return new(mainMethod, definedMethods);
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
            new OpCode[]
            {
                new PopArgOpCode(RegisterType.Temp0),
                new SetVariableOpCode(RegisterType.Temp0, "arg2"),
                new PopArgOpCode(RegisterType.Temp0),
                new SetVariableOpCode(RegisterType.Temp0, "arg1")
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

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(4)),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(10)),
            new VarToRegisterOpCode(RegisterType.Temp1, "value"),
            new AddOpCode(RegisterType.Temp0, RegisterType.Temp1, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(3)),
            new VarToRegisterOpCode(RegisterType.Temp1, "value"),
            new SubOpCode(RegisterType.Temp0, RegisterType.Temp1, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(100)),
            new VarToRegisterOpCode(RegisterType.Temp1, "value"),
            new MultOpCode(RegisterType.Temp0, RegisterType.Temp1, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(4)),
            new VarToRegisterOpCode(RegisterType.Temp1, "value"),
            new DivOpCode(RegisterType.Temp0, RegisterType.Temp1, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(5)),
            new VarToRegisterOpCode(RegisterType.Temp1, "value"),
            new ModOpCode(RegisterType.Temp0, RegisterType.Temp1, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp0, "value")
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

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(10)),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp0, new FloatValueType(10f)),
            new SetVariableOpCode(RegisterType.Temp0, "value2"),
            new ConstToRegisterOpCode(RegisterType.Temp0, new StringValueType("hello world!")),
            new SetVariableOpCode(RegisterType.Temp0, "value3"),
            new ConstToRegisterOpCode(RegisterType.Temp0, new BoolValueType(true)),
            new SetVariableOpCode(RegisterType.Temp0, "value4"),
            new ConstToRegisterOpCode(RegisterType.Temp0, new BoolValueType(false)),
            new SetVariableOpCode(RegisterType.Temp0, "value5")
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

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(1)),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(2)),
            new AddOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new MoveOpCode(RegisterType.Temp0, RegisterType.Temp2),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(3)),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(4)),
            new MultOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(5)),
            new DivOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new SubOpCode(RegisterType.Temp0, RegisterType.Temp2, RegisterType.Temp0),
            new FlipNegativeOpCode(RegisterType.Temp0, RegisterType.Temp0),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(6)),
            new ModOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new SetVariableOpCode(RegisterType.Temp0, "value")
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

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(0)),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new VarToRegisterOpCode(RegisterType.Temp0, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(0)),
            new EqualOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new JumpIfFalse(6, RegisterType.Temp0),
            new EnterScopeOpCode(),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(1)),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new ExitScopeOpCode(),
            new JumpOpCode(23),
            new VarToRegisterOpCode(RegisterType.Temp0, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(1)),
            new EqualOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new JumpIfFalse(6, RegisterType.Temp0),
            new EnterScopeOpCode(),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(2)),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new ExitScopeOpCode(),
            new JumpOpCode(14),
            new VarToRegisterOpCode(RegisterType.Temp0, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(2)),
            new EqualOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new JumpIfFalse(6, RegisterType.Temp0),
            new EnterScopeOpCode(),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(3)),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new ExitScopeOpCode(),
            new JumpOpCode(5),
            new EnterScopeOpCode(),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(4)),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
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

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(0)),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new EnterScopeOpCode(),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(1)),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new ExitScopeOpCode(),
            new EnterScopeOpCode(),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(2)),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new ExitScopeOpCode()
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

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(5)),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new VarToRegisterOpCode(RegisterType.Temp0, "value"),
            new JumpIfEqZero(10, RegisterType.Temp0),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(1)),
            new SubOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new PushStackOpCode(RegisterType.Temp0),
            new EnterScopeOpCode(),
            new GotoMethodOpCode("method"),
            new FrameAdvanceOpCode(),
            new ExitScopeOpCode(),
            new PopStackOpCode(RegisterType.Temp0),
            new JumpOpCode(-9)
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void Loop2()
    {
        var script = Setup(@"$loop_index = 0
loop 10 {
    print($loop_index)
    $loop_index += 1
}");

        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(0)),
            new SetVariableOpCode(RegisterType.Temp0, "loop_index"),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(10)),
            new JumpIfEqZero(15, RegisterType.Temp0),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(1)),
            new SubOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new PushStackOpCode(RegisterType.Temp0),
            new EnterScopeOpCode(),
            new VarToRegisterOpCode(RegisterType.Temp0, "loop_index"),
            new PushArgOpCode(RegisterType.Temp0),
            new GotoMethodOpCode("print"),
            new VarToRegisterOpCode(RegisterType.Temp0, "loop_index"),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(1)),
            new AddOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new SetVariableOpCode(RegisterType.Temp0, "loop_index"),
            new ExitScopeOpCode(),
            new PopStackOpCode(RegisterType.Temp0),
            new JumpOpCode(-14)
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void Loop3()
    {
        var script = Setup("loop 2 { ; }");

        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(2)),
            new JumpIfEqZero(9, RegisterType.Temp0),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(1)),
            new SubOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new PushStackOpCode(RegisterType.Temp0),
            new EnterScopeOpCode(),
            new FrameAdvanceOpCode(),
            new ExitScopeOpCode(),
            new PopStackOpCode(RegisterType.Temp0),
            new JumpOpCode(-8)
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void LoopWithIf()
    {
        var script = Setup(@"loop 5 {
    if true {
        break
    }
}");

        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            // loop 5
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(5)),
            new JumpIfEqZero(16, RegisterType.Temp0),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(1)),
            new SubOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new PushStackOpCode(RegisterType.Temp0),
            new EnterScopeOpCode(),
            // if true
            new ConstToRegisterOpCode(RegisterType.Temp0, new BoolValueType(true)),
            new JumpIfFalse(7, RegisterType.Temp0),
            new EnterScopeOpCode(),
            // break
            new ExitScopeOpCode(),
            new ExitScopeOpCode(),
            new PopStackOpCode(RegisterType.Temp0),
            new JumpOpCode(5),
            new ExitScopeOpCode(),
            // loop end
            new ExitScopeOpCode(),
            new PopStackOpCode(RegisterType.Temp0),
            new JumpOpCode(-15)
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
    } else {
        method()
    }
}");

        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            // $value = 5
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(5)),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new VarToRegisterOpCode(RegisterType.Temp0, "value"),
            // loop $value
            new JumpIfEqZero(33, RegisterType.Temp0),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(1)),
            new SubOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new PushStackOpCode(RegisterType.Temp0),
            new EnterScopeOpCode(),
            // if
            new VarToRegisterOpCode(RegisterType.Temp0, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(3)),
            new EqualOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new JumpIfFalse(8, RegisterType.Temp0),
            new EnterScopeOpCode(),
            // break
            new ExitScopeOpCode(),
            new ExitScopeOpCode(),
            new PopStackOpCode(RegisterType.Temp0),
            new JumpOpCode(20),
            new ExitScopeOpCode(),
            new JumpOpCode(15),
            // else if
            new VarToRegisterOpCode(RegisterType.Temp0, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(4)),
            new EqualOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new JumpIfFalse(8, RegisterType.Temp0),
            new EnterScopeOpCode(),
            new ExitScopeOpCode(),
            new ExitScopeOpCode(),
            new PopStackOpCode(RegisterType.Temp0),
            new JumpOpCode(-24),
            new JumpOpCode(5),
            new ExitScopeOpCode(),
            // else
            new EnterScopeOpCode(),
            new GotoMethodOpCode("method"),
            new ExitScopeOpCode(),
            // loop end
            new ExitScopeOpCode(),
            new PopStackOpCode(RegisterType.Temp0),
            new JumpOpCode(-32)
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void Return()
    {
        var script = Setup(@"fn method(){ return 5 }
$value = method(1)");

        var definedMethod = script.Methods[0];

        var actual = new ScriptMethodModel("method", new OpCode[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(5)),
            new MoveOpCode(RegisterType.Temp0, RegisterType.Ret),
            new ReturnOpCode()
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void MainReturn()
    {
        var script = Setup("return");

        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            new ReturnOpCode()
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void TupleDeconstruction()
    {
        var script = Setup(@"fn method() { return (10, 20) }
$value = (50, ""test"", -1.0)
$(value2, value3) = $value
$(_, value4, value5) = $value
$(_, value6) = (10, 10)
$(value7, value8) = method()");

        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            // $value = (50, "test", -1.0)
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(50)),
            new ClearTupleOpCode(RegisterType.Temp1),
            new PushTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new ConstToRegisterOpCode(RegisterType.Temp1, new StringValueType("test")),
            new PushTupleOpCode(RegisterType.Temp0, RegisterType.Temp1),
            new ConstToRegisterOpCode(RegisterType.Temp1, new FloatValueType(-1f)),
            new PushTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            // $(value2, value3) = $value
            new VarToRegisterOpCode(RegisterType.Temp0, "value"),
            new PopTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp1, "value2"),
            new PopTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp1, "value3"),
            // $(_, value4, value5) = $value
            new VarToRegisterOpCode(RegisterType.Temp0, "value"),
            new PopTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new PopTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp1, "value4"),
            new PopTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp1, "value5"),
            // $(_, value6) = (10, 10)
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(10)),
            new ClearTupleOpCode(RegisterType.Temp1),
            new PushTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(10)),
            new PushTupleOpCode(RegisterType.Temp0, RegisterType.Temp1),
            // set
            new PopTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new PopTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp1, "value6"),
            // $(value7, value8) = method()
            new GotoMethodOpCode("method"),
            new PopTupleOpCode(RegisterType.Temp0, RegisterType.Ret),
            new SetVariableOpCode(RegisterType.Temp0, "value7"),
            new PopTupleOpCode(RegisterType.Temp0, RegisterType.Ret),
            new SetVariableOpCode(RegisterType.Temp0, "value8")
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void Comments()
    {
        var script = Setup(@"$value = ""thingy""
/* AAA; ""fake string to make you angry""
\"" "" BBB;
"" no
$value = ""thingy2""
;;;;
*/
$value2 = ""\""this is another thingy"" // you can't see this!
$value3 = (10, /*""thing"",,*/ ""thing2"")");

        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp0, new StringValueType("thingy")),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            new ConstToRegisterOpCode(RegisterType.Temp0, new StringValueType("\"this is another thing")),
            new SetVariableOpCode(RegisterType.Temp0, "value2"),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(10)),
            new ClearTupleOpCode(RegisterType.Temp1),
            new PushTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new ConstToRegisterOpCode(RegisterType.Temp0, new StringValueType("thing2")),
            new PushTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp1, "value3")
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void Strings()
    {
        var script = Setup("$value = \"th\\\"i\\\"ngy\"");

        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp0, new StringValueType("th\"i\"ngy")),
            new SetVariableOpCode(RegisterType.Temp0, "value")
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void UndefinedMethod()
    {
        var setup = () => Setup("$value = this_method_does_not_exist()");

        setup.Should().Throw<UsingUndefinedMethodException>();
    }

    [Fact]
    public void ExternalMethod()
    {
        Setup("print(\"hello world!\")");
    }

    [Fact]
    public void MinusOne()
    {
        var script = Setup("$value = -(1)");
        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(-1)),
            new FlipNegativeOpCode(RegisterType.Temp0, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp0, "value")
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void TypeCasting()
    {
        var script = Setup(@"$value = (int)""10""
$value2 = (float)$value
$value3 = (bool)1
$value4 = (bool)""false""
$value5 = (string)$value");

        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            // $value = (int)"10"
            new ConstToRegisterOpCode(RegisterType.Temp0, new StringValueType("10")),
            new CastOpCode(BasicValueType.Int, RegisterType.Temp0, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp0, "value"),
            // $value2 = (float)$value
            new VarToRegisterOpCode(RegisterType.Temp0, "value"),
            new CastOpCode(BasicValueType.Float, RegisterType.Temp0, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp0, "value2"),
            // $value3 = (bool)1
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(1)),
            new CastOpCode(BasicValueType.Bool, RegisterType.Temp0, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp0, "value3"),
            // $value4 = (bool)"false"
            new ConstToRegisterOpCode(RegisterType.Temp0, new StringValueType("false")),
            new CastOpCode(BasicValueType.Bool, RegisterType.Temp0, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp0, "value4"),
            // $value5 = (string)$value
            new VarToRegisterOpCode(RegisterType.Temp0, "value"),
            new CastOpCode(BasicValueType.String, RegisterType.Temp0, RegisterType.Temp0),
            new SetVariableOpCode(RegisterType.Temp0, "value5")
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void ReturnValueThrow()
    {
        var setup = () => Setup("fn method() { if true { return (0, 1) } return }");
        setup.Should().Throw<MethodReturnCountNotMatchingException>();

        setup = () => Setup("fn method() { if true { return 0 } return (1, 2) }");
        setup.Should().Throw<MethodReturnCountNotMatchingException>();

        setup = () => Setup("fn method() { if true { return (0, 1) } return (1, 2, 5) }");
        setup.Should().Throw<MethodReturnCountNotMatchingException>();

        Setup("return (0, 1); return 5");
    }

    [Fact]
    public void ExternMethodWrongReturn()
    {
        var setup = () => Setup("$value = print(\"hello world!\")");
        setup.Should().Throw<MethodHasNoReturnValueException>();
    }

    [Fact]
    public void UsingLoopActionsOutside()
    {
        var setup = () => Setup("continue");
        setup.Should().Throw<UsingLoopActionOutsideOfLoopException>();
        setup = () => Setup("fn method() { continue } loop 5 { method() }");
        setup.Should().Throw<UsingLoopActionOutsideOfLoopException>();
        setup = () => Setup("break");
        setup.Should().Throw<UsingLoopActionOutsideOfLoopException>();
    }

    [Fact]
    public void Tuple()
    {
        var script = Setup(@"fn get_args(one, two, three) { }
get_args((10, 20), ""third item"", (40, 50))");
        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            // 10, 20
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(10)),
            new ClearTupleOpCode(RegisterType.Temp1),
            new PushTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(20)),
            new PushTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new PushArgOpCode(RegisterType.Temp1),
            // third item
            new ConstToRegisterOpCode(RegisterType.Temp0, new StringValueType("third item")),
            new PushArgOpCode(RegisterType.Temp0),
            // 40, 50
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(40)),
            new ClearTupleOpCode(RegisterType.Temp1),
            new PushTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(50)),
            new PushTupleOpCode(RegisterType.Temp1, RegisterType.Temp0),
            new PushArgOpCode(RegisterType.Temp1),
            // goto
            new GotoMethodOpCode("get_args")
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void PipeSplit()
    {
        var script =
            Setup(@"fn register(arg1, arg2) { }
$concurrent1 = register(""concurrent"", true) | $concurrent2 = register(""concurrent"", false)");
        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            // "concurrent", true
            new ConstToRegisterOpCode(RegisterType.Temp0, new StringValueType("concurrent")),
            new PushArgOpCode(RegisterType.Temp0),
            new ConstToRegisterOpCode(RegisterType.Temp0, new BoolValueType(true)),
            new PushArgOpCode(RegisterType.Temp0),
            new PushStackOpCode(RegisterType.Temp0),
            new PushStackOpCode(RegisterType.Temp1),
            new PushStackOpCode(RegisterType.Temp2),
            new GotoMethodOpCode("register"),
            new PopStackOpCode(RegisterType.Temp0),
            new PopStackOpCode(RegisterType.Temp1),
            new PopStackOpCode(RegisterType.Temp2),
            // $concurrent1 = 
            new SetVariableOpCode(RegisterType.Ret, "concurrent1"),
            // "concurrent", false
            new ConstToRegisterOpCode(RegisterType.Temp0, new StringValueType("concurrent")),
            new PushArgOpCode(RegisterType.Temp0),
            new ConstToRegisterOpCode(RegisterType.Temp0, new BoolValueType(false)),
            new PushArgOpCode(RegisterType.Temp0),
            new PushStackOpCode(RegisterType.Temp0),
            new PushStackOpCode(RegisterType.Temp1),
            new PushStackOpCode(RegisterType.Temp2),
            new GotoMethodOpCode("register"),
            new PopStackOpCode(RegisterType.Temp0),
            new PopStackOpCode(RegisterType.Temp1),
            new PopStackOpCode(RegisterType.Temp2),
            // $concurrent2 = 
            new SetVariableOpCode(RegisterType.Ret, "concurrent2")
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void HugeLoops()
    {
        var script =
            Setup(@"fn get_args(arg1) { }
loop 500 { ; }
get_args(""checkpoint 1"");
loop 500 { ; }
get_args(""checkpoint 2"")");
        var definedMethod = script.MainMethod;

        var actual = new ScriptMethodModel(null, new OpCode[]
        {
            // loop 500 { ; }
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(500)),
            new JumpIfEqZero(9, RegisterType.Temp0),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(1)),
            new SubOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new PushStackOpCode(RegisterType.Temp0),
            new EnterScopeOpCode(),
            new FrameAdvanceOpCode(),
            new ExitScopeOpCode(),
            new PopStackOpCode(RegisterType.Temp0),
            new JumpOpCode(-8),
            // get_args("checkpoint 1")
            new ConstToRegisterOpCode(RegisterType.Temp0, new StringValueType("checkpoint 1")),
            new PushArgOpCode(RegisterType.Temp0),
            new GotoMethodOpCode("get_args"),
            new FrameAdvanceOpCode(),
            // loop 500 { ; }
            new ConstToRegisterOpCode(RegisterType.Temp0, new IntValueType(500)),
            new JumpIfEqZero(9, RegisterType.Temp0),
            new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(1)),
            new SubOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1),
            new PushStackOpCode(RegisterType.Temp0),
            new EnterScopeOpCode(),
            new FrameAdvanceOpCode(),
            new ExitScopeOpCode(),
            new PopStackOpCode(RegisterType.Temp0),
            new JumpOpCode(-8),
            // get_args("checkpoint 2")
            new ConstToRegisterOpCode(RegisterType.Temp0, new StringValueType("checkpoint 2")),
            new PushArgOpCode(RegisterType.Temp0),
            new GotoMethodOpCode("get_args")
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }
}