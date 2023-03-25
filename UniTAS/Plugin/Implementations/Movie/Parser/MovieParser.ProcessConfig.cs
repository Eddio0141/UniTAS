using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Models.Movie;
using UniTAS.Plugin.Utils;

namespace UniTAS.Plugin.Implementations.Movie.Parser;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public partial class MovieParser
{
    private Tuple<bool, PropertiesModel> ProcessConfig(Script script)
    {
        const string configVariable = "MOVIE_CONFIG";

        var configValue = script.Globals.Get(configVariable);
        if (configValue.Type == DataType.Nil)
        {
            configValue = DynValue.NewTable(script);
            script.Globals.Set(configVariable, configValue);
        }

        var configTable = configValue.Table;

        var properties = new PropertiesModel(new(
            GetStartTime(configTable),
            GetFrameTime(configTable),
            GetSeed(configTable)
        ));

        return Tuple.New(IsGlobalScope(configTable), properties);
    }

    private static bool IsGlobalScope(Table configTable)
    {
        return configTable.Get("is_global_scope").CastToBool();
    }

    private DateTime GetStartTime(Table table)
    {
        const string startTimeVariable = "start_time";

        var startTimeRaw = table.Get(startTimeVariable);
        DateTime startTime;
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (startTimeRaw.Type)
        {
            case DataType.String:
            {
                // culture invariant
                if (!DateTime.TryParse(startTimeRaw.String, CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal,
                        out startTime))
                {
                    startTime = default;
                    _logger.LogWarning(
                        $"{startTimeVariable} is invalid, using default time of {startTime.ToString(CultureInfo.InvariantCulture)}");
                }

                break;
            }
            case DataType.Number:
                // parse as ticks
                startTime = new((long)startTimeRaw.Number);
                break;
            default:
                startTime = default;
                _logger.LogWarning(
                    $"{startTimeVariable} is not defined, using default value of {startTime.ToString(CultureInfo.InvariantCulture)}");
                break;
        }

        return startTime;
    }

    private float GetFrameTime(Table table)
    {
        var selected = SelectAndWarnConflictingVariables(table, new() { "frametime", "fps" });
        var selectedValue = selected.Item1;
        var selectedVariable = selected.Item2;

        var valueParsed = selectedValue.CastToNumber();
        if (valueParsed is null)
        {
            _logger.LogWarning($"Could not parse {selectedVariable} as a number, using default value of 100 fps");
            return 1f / 100f;
        }

        var frameTime = selectedVariable == "fps" ? 1f / valueParsed.Value : valueParsed.Value;
        if (frameTime <= 0)
        {
            _logger.LogWarning("Frame time must be greater than 0, using default value of 100 fps");
            return 1f / 100f;
        }

        return (float)frameTime;
    }

    private long GetSeed(Table table)
    {
        var selectedValue = table.Get("seed");

        var valueParsed = selectedValue.CastToNumber();
        if (valueParsed is null)
        {
            _logger.LogWarning($"Could not parse seed as a number, using default value of 0");
            return 0;
        }

        return (long)valueParsed.Value;
    }

    private Tuple<DynValue, string> SelectAndWarnConflictingVariables(Table table, List<string> variables)
    {
        var selected = DynValue.Nil;
        var selectedString = string.Empty;

        foreach (var variable in variables)
        {
            var value = table.Get(variable);
            if (value.Type == DataType.Nil) continue;

            if (selected.Type != DataType.Nil)
            {
                _logger.LogWarning($"{selectedString} and {variable} are both defined, using {selectedString}");
                continue;
            }

            selected = value;
            selectedString = variable;
        }

        return Tuple.New(selected, selectedString);
    }
}