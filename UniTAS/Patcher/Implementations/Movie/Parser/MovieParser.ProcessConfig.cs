using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Implementations.UnitySafeWrappers;
using UniTAS.Patcher.Models.Movie;

namespace UniTAS.Patcher.Implementations.Movie.Parser;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public partial class MovieParser
{
    private (bool, PropertiesModel) ProcessConfig(Script script)
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
                GetSeed(configTable),
                GetWindowState(configTable)
            ),
            GetUpdateType(configTable));

        return (IsGlobalScope(configTable), properties);
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
                    logger.LogWarning(
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
                logger.LogWarning(
                    $"{startTimeVariable} is not defined, using default value of {startTime.ToString(CultureInfo.InvariantCulture)}");
                break;
        }

        return startTime;
    }

    private float GetFrameTime(Table table)
    {
        var selected = SelectAndWarnConflictingVariables(table, ["frametime", "fps"]);
        var selectedValue = selected.Item1;
        var selectedVariable = selected.Item2;

        var valueParsed = selectedValue.CastToNumber();
        if (valueParsed is null)
        {
            logger.LogWarning($"Could not parse {selectedVariable} as a number, using default value of 100 fps");
            return 1f / 100f;
        }

        var frameTime = selectedVariable == "fps" ? 1f / valueParsed.Value : valueParsed.Value;
        if (frameTime <= 0)
        {
            logger.LogWarning("Frame time must be greater than 0, using default value of 100 fps");
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
            logger.LogWarning("Could not parse seed as a number, using default value of 0");
            return 0;
        }

        return (long)valueParsed.Value;
    }

    private UpdateType GetUpdateType(Table table)
    {
        var selectedValue = table.Get("update_type");
        const UpdateType fallback = UpdateType.Update;

        var valueParsed = selectedValue.CastToString();
        if (valueParsed is null)
        {
            logger.LogWarning(
                $"Could not parse update_type as a string, using default value of {fallback}");
            return fallback;
        }

        UpdateType updateType;
        try
        {
            updateType = (UpdateType)Enum.Parse(typeof(UpdateType), valueParsed, true);
        }
        catch (Exception)
        {
            logger.LogWarning(
                $"Could not parse update_type as a valid variant, using default value of {fallback}");
            return fallback;
        }

        return updateType;
    }

    private WindowState GetWindowState(Table table)
    {
        var window = table.Get("window");

        const int fallbackWidth = 1920;
        const int fallbackHeight = 1080;
        const int fallbackResClosestDefault = 100;
        var fallbackRr = unityInstanceWrapFactory.CreateNew<RefreshRateWrap>();
        fallbackRr.Rate = 60;

        if (window.Type != DataType.Table)
        {
            logger.LogWarning(
                window.Type == DataType.Nil
                    ? $"`window` config is nil, falling back to resolution of {fallbackWidth}x{fallbackHeight}@{fallbackRr.Rate}hz with no extra screen resolutions"
                    : $"`window` isn't a table, falling back to resolution of {fallbackWidth}x{fallbackHeight}@{fallbackRr.Rate}hz with no extra screen resolutions");

            return new WindowState(unityInstanceWrapFactory,
                new ResolutionWrapper(fallbackWidth, fallbackHeight, fallbackRr),
                [],
                fallbackResClosestDefault);
        }

        var windowTable = window.Table;

        var width = (int)(windowTable.Get("width").CastToNumber() ?? fallbackWidth);
        var height = (int)(windowTable.Get("height").CastToNumber() ?? fallbackHeight);
        var fallbackResClosest =
            (int)(windowTable.Get("fallback_res_closest").CastToNumber() ?? fallbackResClosestDefault);

        if (width <= 0 || height <= 0)
        {
            logger.LogWarning(
                $"the screen resolution value can't be 0 or lower, using default value of {fallbackWidth}x{fallbackHeight}");
            width = fallbackWidth;
            height = fallbackHeight;
        }

        if (fallbackResClosest < 1)
        {
            logger.LogWarning(
                $"the fallback resolution value can't be 0 or lower, using default value of {fallbackResClosestDefault}");
            fallbackResClosest = fallbackResClosestDefault;
        }

        // fallback is 0hz (unlimited for unity)
        var rr = RefreshRateParser(windowTable.Get("refresh_rate"), out var rrParsed)
            ? rrParsed
            : fallbackRr;

        var resolutionsRaw = windowTable.Get("resolutions");
        var resolutions = new List<ResolutionWrapper>();
        if (resolutionsRaw.Type == DataType.Table)
        {
            var resolutionRawValues = resolutionsRaw.Table.Values;
            foreach (var resolution in resolutionRawValues)
            {
                if (resolution.Type != DataType.Table)
                {
                    logger.LogWarning(
                        "entry of `resolutions` isn't a table containing `width`, `height`, and `refresh_rate` values");
                }

                var resolutionTable = resolution.Table;
                var resWidthRaw = resolutionTable.Get("width");
                var resHeightRaw = resolutionTable.Get("height");
                var refreshRateRaw = resolutionTable.Get("refresh_rate");
                var resW = resWidthRaw.CastToNumber();
                var resH = resHeightRaw.CastToNumber();

                if (resW == null)
                {
                    logger.LogWarning(
                        $"in `resolutions`, the width value {resWidthRaw} isn't a number, ignoring this entry");
                    continue;
                }

                if ((uint)resW.Value <= 0)
                {
                    logger.LogWarning($"in `resolutions`, the width value {resW} is invalid, ignoring this entry");
                    continue;
                }

                if (resH == null)
                {
                    logger.LogWarning(
                        $"in `resolutions`, the height value {resHeightRaw} isn't a number, ignoring this entry");
                    continue;
                }

                if ((uint)resH.Value <= 0)
                {
                    logger.LogWarning($"in `resolutions`, the height value {resH} is invalid, ignoring this entry");
                    continue;
                }

                if (!RefreshRateParser(refreshRateRaw, out var refreshRateParsed))
                {
                    continue;
                }

                resolutions.Add(new ResolutionWrapper((int)resW.Value, (int)resH.Value, refreshRateParsed));
            }
        }
        else if (resolutionsRaw.Type != DataType.Nil)
        {
            logger.LogWarning("`resolutions` isn't an array, no additional supported screen resolutions are added");
        }

        return new WindowState(unityInstanceWrapFactory, new ResolutionWrapper(width, height, rr),
            resolutions.ToArray(), fallbackResClosest);
    }

    private bool RefreshRateParser(DynValue entry, out RefreshRateWrap refreshRate)
    {
        var refreshRateNum = entry.CastToNumber();
        if (refreshRateNum.HasValue)
        {
            refreshRate = new RefreshRateWrap(null)
            {
                Rate = refreshRateNum.Value
            };
        }
        else
        {
            // is it specifying the raw values
            if (entry.Type != DataType.Table)
            {
                logger.LogWarning(
                    $"the refresh rate `{entry}` isn't either a number or a table containing " +
                    "`numerator` and `denominator` (or `n` and `d`), ignoring this entry");
                refreshRate = null;
                return false;
            }

            var refreshRateTable = entry.Table;

            var (nRaw, _) = SelectAndWarnConflictingVariables(refreshRateTable, ["n", "numerator"]);
            var n = nRaw.CastToNumber();
            if (n == null)
            {
                logger.LogWarning(
                    $"the numerator value {nRaw} isn't a number, ignoring this entry");
                refreshRate = null;
                return false;
            }

            var (dRaw, _) = SelectAndWarnConflictingVariables(refreshRateTable, ["d", "denominator"]);
            var d = dRaw.CastToNumber();
            if (d == null)
            {
                logger.LogWarning(
                    $"in `resolutions`, the denominator value {dRaw} isn't a number, ignoring this entry");
                refreshRate = null;
                return false;
            }

            refreshRate = unityInstanceWrapFactory.CreateNew<RefreshRateWrap>();
            refreshRate.Denominator = (uint)d;
            refreshRate.Numerator = (uint)n;
        }

        if (refreshRate.Rate < 0)
        {
            logger.LogWarning(
                $"the refresh rate {refreshRate.Rate} is invalid, it has to be 0hz (0 is valid) or higher, ignoring this entry");
            refreshRate = null;
            return false;
        }

        return true;
    }

    private (DynValue, string) SelectAndWarnConflictingVariables(Table table, List<string> variables)
    {
        var selected = DynValue.Nil;
        var selectedString = string.Empty;

        foreach (var variable in variables)
        {
            var value = table.Get(variable);
            if (value.Type == DataType.Nil) continue;

            if (selected.Type != DataType.Nil)
            {
                logger.LogWarning($"{selectedString} and {variable} are both defined, using {selectedString}");
                continue;
            }

            selected = value;
            selectedString = variable;
        }

        return (selected, selectedString);
    }
}