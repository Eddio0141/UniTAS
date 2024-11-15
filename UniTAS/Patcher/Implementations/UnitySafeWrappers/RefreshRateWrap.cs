using System;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.UnitySafeWrappers;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

public class RefreshRateWrap : UnityInstanceWrap
{
    public uint Numerator
    {
        get => _numerator;
        set
        {
            if (_numerator == value) return;
            _numerator = value;
            _rate = (double)_numerator / _denominator;
            NumeratorField?.SetValue(Instance, value);
        }
    }

    public uint Denominator
    {
        get => _denominator;
        set
        {
            if (_denominator == value) return;
            _denominator = value;
            _rate = (double)_numerator / _denominator;
            DenominatorField?.SetValue(Instance, value);
        }
    }

    public double Rate
    {
        get => _rate;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_rate == value) return;
            _rate = value;

            // whatever it should be good enough
            Denominator = (uint)Math.Pow(10, value.DecimalPlaces());
            Numerator = (uint)(value * Denominator);
        }
    }

    protected override Type WrappedType { get; } = AccessTools.TypeByName("UnityEngine.RefreshRate");

    public RefreshRateWrap(object instance) : base(instance)
    {
        if (NumeratorField == null) return;
        _numerator = (uint)NumeratorField.GetValue(Instance);
        _denominator = (uint)DenominatorField.GetValue(Instance);
    }

    private static readonly Type RefreshRateType = AccessTools.TypeByName("UnityEngine.RefreshRate");
    private static readonly FieldInfo NumeratorField = AccessTools.Field(RefreshRateType, "numerator");
    private static readonly FieldInfo DenominatorField = AccessTools.Field(RefreshRateType, "denominator");
    private double _rate;
    private uint _numerator;
    private uint _denominator;
}