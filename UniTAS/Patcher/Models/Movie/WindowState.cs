using System;
using System.Collections.Generic;
using System.Linq;
using UniTAS.Patcher.Implementations.UnitySafeWrappers;
using UniTAS.Patcher.Services.UnitySafeWrappers;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Models.Movie;

public class WindowState(
    IUnityInstanceWrapFactory unityInstanceWrapFactory,
    IResolutionWrapper currentResolution,
    IResolutionWrapper[] extraResolutions,
    int extraResolutionClosest)
{
    // https://en.wikipedia.org/wiki/Display_aspect_ratio
    // covers common ones
    private static readonly (int, int)[] GenerateRatios =
        [(1, 1), (5, 4), (4, 3), (3, 2), (8, 5), (16, 9), (64, 27), (32, 9)];

    public void SetWindowEnv(IWindowEnv windowEnv)
    {
        windowEnv.CurrentResolution = CurrentResolution;
        var width = CurrentResolution.Width;
        var height = CurrentResolution.Height;

        var ratio = CalculateAspectRatio(windowEnv.CurrentResolution.Width, windowEnv.CurrentResolution.Height);
        var extraResolutions = new HashSet<(int width, int height)>();

        foreach (var (ratioW, ratioH) in GenerateRatios.Concat([ratio]))
        {
            // I know the two loops are the same, I don't care
            var maxWidth = width - width % ExtraResolutionClosest;
            for (var i = 1; i <= maxWidth; i++)
            {
                var ratioMult = (int)Math.Ceiling(i * ExtraResolutionClosest / (double)ratioW);
                var closestW = ratioMult * ratioW;
                var closestH = ratioMult * ratioH;
                extraResolutions.Add((closestW, closestH));
            }

            var maxHeight = height - height % ExtraResolutionClosest;
            for (var i = 1; i <= maxHeight; i++)
            {
                var ratioMult = (int)Math.Ceiling(i * ExtraResolutionClosest / (double)ratioH);
                var closestW = ratioMult * ratioW;
                var closestH = ratioMult * ratioH;
                extraResolutions.Add((closestW, closestH));
            }
        }

        extraResolutions.Remove((width, height));

        var allExtraRes = new List<IResolutionWrapper>();

        foreach (var res in ExtraResolutions)
        {
            extraResolutions.Remove((res.Width, res.Height));
            allExtraRes.Add(res);
        }

        var rrDefault = unityInstanceWrapFactory.CreateNew<RefreshRateWrap>(60.0);

        allExtraRes.AddRange(extraResolutions.Select(r =>
            unityInstanceWrapFactory.CreateNew<IResolutionWrapper>(r.width, r.height, rrDefault)));

        windowEnv.ExtraSupportedResolutions =
            allExtraRes.OrderBy(rr => rr.Width).ThenBy(rr => rr.Width * rr.Height).ToArray();
    }

    private static (int width, int height) CalculateAspectRatio(int originalWidth, int originalHeight)
    {
        var gcd = MathUtils.GreatestCommonDivisor(originalWidth, originalHeight);
        return (originalWidth / gcd, originalHeight / gcd);
    }

    public IResolutionWrapper CurrentResolution { get; } = currentResolution;
    public IResolutionWrapper[] ExtraResolutions { get; } = extraResolutions;

    public int ExtraResolutionClosest { get; } = Math.Max(extraResolutionClosest, 1);
}