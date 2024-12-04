using System;
using UniTAS.Patcher.Extensions;

namespace Patcher.Tests.Extensions;

public class MethodDelegateTests
{
    private struct TestStruct
    {
        public int Value;
        public void Increment() => Value++;
        public string ReturnValue() => Value.ToString();
    }

    private delegate void TestStructRefDelegate(ref TestStruct testStruct);

    private delegate void TestStructObjectRefDelegate(ref object testStruct);

    [Fact]
    public void StructRefDelegateMutate()
    {
        var increment = typeof(TestStruct).GetMethod(nameof(TestStruct.Increment))!;

        var incrementDel = increment.MethodDelegate<TestStructRefDelegate>(delegateArgs:
            [typeof(TestStruct).MakeByRefType()]);
        var instance = new TestStruct();
        incrementDel(ref instance);
        Assert.Equal(1, instance.Value);

        var incrementDel2 = increment.MethodDelegate<TestStructObjectRefDelegate>(delegateArgs:
            [typeof(object).MakeByRefType()]);
        var instanceWrap = (object)instance;
        incrementDel2(ref instanceWrap);
        Assert.Equal(2, ((TestStruct)instanceWrap).Value);
    }

    [Fact]
    public void StructDelegateInvoke()
    {
        var instance = new TestStruct();
        instance.Value++;
        
        var returnValue = typeof(TestStruct).GetMethod(nameof(TestStruct.ReturnValue))!;
        var returnValueDel = returnValue.MethodDelegate<Func<object, string>>();
        Assert.Equal("1", returnValueDel(instance));
    }
}