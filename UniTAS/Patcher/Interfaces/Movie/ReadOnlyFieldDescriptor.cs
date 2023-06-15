using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Interop.Converters;

namespace UniTAS.Patcher.Interfaces.Movie;

public class ReadOnlyFieldDescriptor : IMemberDescriptor, IOptimizableDescriptor
{
    /// <summary>
    /// Gets the FieldInfo got by reflection
    /// </summary>
    private readonly FieldInfo _fieldInfo;

    /// <summary>
    /// Gets the <see cref="InteropAccessMode" />
    /// </summary>
    private readonly InteropAccessMode _accessMode;

    /// <summary>
    /// Gets a value indicating whether the described property is static.
    /// </summary>
    public bool IsStatic { get; }

    /// <summary>
    /// Gets the name of the property
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is a constant 
    /// </summary>
    private readonly bool _isConst;

    private readonly object _constValue;

    private Func<object, object> _optimizedGetter;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyMemberDescriptor"/> class.
    /// </summary>
    /// <param name="fieldInfo">The FieldInfo.</param>
    /// <param name="accessMode">The <see cref="InteropAccessMode" /> </param>
    public ReadOnlyFieldDescriptor(FieldInfo fieldInfo, InteropAccessMode accessMode)
    {
        if (Script.GlobalOptions.Platform.IsRunningOnAOT())
            accessMode = InteropAccessMode.Reflection;

        _fieldInfo = fieldInfo;
        _accessMode = accessMode;
        Name = fieldInfo.Name;
        IsStatic = _fieldInfo.IsStatic;

        if (_fieldInfo.IsLiteral)
        {
            _isConst = true;
            _constValue = _fieldInfo.GetValue(null);
        }

        if (_accessMode == InteropAccessMode.Preoptimized)
        {
            OptimizeGetter();
        }
    }


    /// <summary>
    /// Gets the value of the property
    /// </summary>
    /// <param name="script">The script.</param>
    /// <param name="obj">The object.</param>
    /// <returns></returns>
    public DynValue GetValue(Script script, object obj)
    {
        this.CheckAccess(MemberDescriptorAccess.CanRead, obj);

        // optimization+workaround of Unity bug.. 
        if (_isConst)
            return ClrToScriptConversions.ObjectToDynValue(script, _constValue);

        if (_accessMode == InteropAccessMode.LazyOptimized && _optimizedGetter == null)
            OptimizeGetter();

        var result = _optimizedGetter != null ? _optimizedGetter(obj) : _fieldInfo.GetValue(obj);

        return ClrToScriptConversions.ObjectToDynValue(script, result);
    }

    private void OptimizeGetter()
    {
        if (_isConst)
            return;

        if (IsStatic)
        {
            var paramExp = Expression.Parameter(typeof(object), "dummy");
            var propAccess = Expression.Field(null, _fieldInfo);
            var castPropAccess = Expression.Convert(propAccess, typeof(object));
            var lambda = Expression.Lambda<Func<object, object>>(castPropAccess, paramExp);
            Interlocked.Exchange(ref _optimizedGetter, lambda.Compile());
        }
        else
        {
            var paramExp = Expression.Parameter(typeof(object), "obj");
            var castParamExp = Expression.Convert(paramExp, _fieldInfo.DeclaringType!);
            var propAccess = Expression.Field(castParamExp, _fieldInfo);
            var castPropAccess = Expression.Convert(propAccess, typeof(object));
            var lambda = Expression.Lambda<Func<object, object>>(castPropAccess, paramExp);
            Interlocked.Exchange(ref _optimizedGetter, lambda.Compile());
        }
    }

    /// <summary>
    /// Sets the value of the property
    /// </summary>
    /// <param name="script">The script.</param>
    /// <param name="obj">The object.</param>
    /// <param name="v">The value to set.</param>
    public void SetValue(Script script, object obj, DynValue v)
    {
        throw new ScriptRuntimeException(
            $"You cannot set the field of {_fieldInfo.DeclaringType?.Name ?? "unknown type"}.{Name} as script can only have read access to unity types");
    }


    /// <summary>
    /// Gets the types of access supported by this member
    /// </summary>
    public MemberDescriptorAccess MemberAccess => MemberDescriptorAccess.CanRead;

    void IOptimizableDescriptor.Optimize()
    {
        if (_optimizedGetter == null)
            OptimizeGetter();
    }
}