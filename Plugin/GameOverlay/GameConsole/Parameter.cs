using System.Collections.Generic;
using System.Linq;

namespace UniTASPlugin.GameOverlay.GameConsole;

public class Parameter
{
    public ParameterType ParamType { get; }

    readonly string valueString;
    readonly int valueInt;
    readonly float valueFloat;
    readonly bool valueBool;
    readonly Parameter[] valueList;

    Parameter(string value)
    {
        ParamType = ParameterType.String;
        valueString = value.Substring(1, value.Length - 2);
    }

    Parameter(int value)
    {
        ParamType = ParameterType.Int;
        valueInt = value;
    }

    Parameter(float value)
    {
        ParamType = ParameterType.Float;
        valueFloat = value;
    }

    Parameter(bool value)
    {
        ParamType = ParameterType.Bool;
        valueBool = value;
    }

    Parameter(Parameter[] value)
    {
        ParamType = ParameterType.List;
        valueList = value;
    }

    public bool GetString(out string value)
    {
        if (ParamType == ParameterType.String)
        {
            value = valueString;
            return true;
        }
        value = null;
        return false;
    }

    public bool GetInt(out int value)
    {
        if (ParamType == ParameterType.Int)
        {
            value = valueInt;
            return true;
        }
        value = 0;
        return false;
    }

    public bool GetFloat(out float value)
    {
        if (ParamType == ParameterType.Float)
        {
            value = valueFloat;
            return true;
        }
        value = 0;
        return false;
    }

    public bool GetBool(out bool value)
    {
        if (ParamType == ParameterType.Bool)
        {
            value = valueBool;
            return true;
        }
        value = false;
        return false;
    }

    public bool GetList(out Parameter[] value)
    {
        if (ParamType == ParameterType.List)
        {
            value = valueList;
            return true;
        }
        value = null;
        return false;
    }

    public override string ToString()
    {
        return ParamType switch
        {
            ParameterType.String => valueString,
            ParameterType.Int => valueInt.ToString(),
            ParameterType.Float => valueFloat.ToString(),
            ParameterType.Bool => valueBool.ToString(),
            ParameterType.List => $"[{string.Join(", ", valueList.Select(v => v.ToString()).ToArray())}]",
            _ => null,
        };
    }

    public static bool FromString(string value, out Parameter parameter)
    {
        if (value.StartsWith("\"") && value.EndsWith("\""))
        {
            parameter = new Parameter(value);
            return true;
        }
        if (int.TryParse(value, out var intValue))
        {
            parameter = new Parameter(intValue);
            return true;
        }
        if (float.TryParse(value, out var floatValue))
        {
            parameter = new Parameter(floatValue);
            return true;
        }
        if (bool.TryParse(value, out var boolValue))
        {
            parameter = new Parameter(boolValue);
            return true;
        }
        if (value.StartsWith("[") && value.EndsWith("]"))
        {
            var listString = value.Substring(1, value.Length - 2);
            // we do not allow recursive lists
            if (listString.Contains("[") || listString.Contains("]"))
            {
                parameter = null;
                return false;
            }
            var list = new List<Parameter>();
            var listStringSplit = listString.Split(',');
            ParameterType? listType = null;
            foreach (var listStringSplitItem in listStringSplit)
            {
                if (!FromString(listStringSplitItem.Trim(), out var listParam))
                {
                    parameter = null;
                    return false;
                }
                if (listType == null)
                {
                    listType = listParam.ParamType;
                }
                else if (listType != listParam.ParamType)
                {
                    parameter = null;
                    return false;
                }
                list.Add(listParam);
            }
            parameter = new Parameter(list.ToArray());
            return true;
        }

        parameter = null;
        return false;
    }
}

public enum ParameterType
{
    String,
    Int,
    Float,
    Bool,
    List,
}
