using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Movie.MovieModels.Properties;
using UniTAS.Plugin.Utils;

namespace UniTAS.Plugin.Movie.Parsers.MovieParser;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public partial class MovieParser
{
    private PropertiesModel ProcessProperties(Script script)
    {
        var properties = new PropertiesModel(new(
            GetStartTime(script),
            GetFrameTime(script)
        ));

        return properties;
    }

    private DateTime GetStartTime(Script script)
    {
        var startTimeRaw = script.Globals.Get("START_TIME");
        DateTime startTime;
        if (startTimeRaw.Type == DataType.String)
        {
            // culture invariant
            if (!DateTime.TryParse(startTimeRaw.String, null, System.Globalization.DateTimeStyles.AssumeUniversal,
                    out startTime))
            {
                startTime = default;
                _logger.LogWarning($"START_TIME is invalid, using default time of {startTime}");
            }
        }
        else if (startTimeRaw.Type == DataType.Number)
        {
            // parse as ticks
            startTime = new((long)startTimeRaw.Number);
        }
        else
        {
            startTime = default;
            _logger.LogWarning($"START_TIME is not defined, using default value of {startTime}");
        }

        return startTime;
    }

    private float GetFrameTime(Script script)
    {
        var selected = SelectAndWarnConflictingVariables(script, new() { "frametime", "ft", "fps" });
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

    private Tuple<DynValue, string> SelectAndWarnConflictingVariables(Script script, List<string> variables)
    {
        var selected = DynValue.Nil;
        var selectedString = string.Empty;

        foreach (var variable in variables)
        {
            var value = script.Globals.Get(variable);
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