using System;
using System.Collections.Generic;
using System.Linq;
using UniTAS.Patcher.Implementations.UnitySafeWrappers;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Services.UnitySafeWrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Models.Movie;

public class WindowState(
    IUnityInstanceWrapFactory unityInstanceWrapFactory,
    ResolutionWrapper currentResolution,
    ResolutionWrapper[] extraResolutions,
    int extraResolutionClosest)
{
    // https://en.wikipedia.org/wiki/Display_aspect_ratio
    // covers common ones
    private static readonly HashSet<(int, int)> GenerateRatios =
        [(1, 1), (5, 4), (4, 3), (3, 2), (8, 5), (16, 9), (64, 27), (32, 9)];

    public void SetWindowEnv(IWindowEnv windowEnv)
    {
        using var _ = Bench.Measure();

        windowEnv.CurrentResolution = CurrentResolution;
        var width = CurrentResolution.Width;
        var height = CurrentResolution.Height;

        var ratio = CalculateAspectRatio(windowEnv.CurrentResolution.Width, windowEnv.CurrentResolution.Height);
        var extraResolutions = new HashSet<(int width, int height)>();

        IEnumerable<(int, int)> ratiosIter = GenerateRatios;
        if (!GenerateRatios.Contains(ratio))
            ratiosIter = ratiosIter.Concat([ratio]);
        
        var calcBench = Bench.Measure();

        var maxWidth = width - width % ExtraResolutionClosest;
        var maxHeight = height - height % ExtraResolutionClosest;

        foreach (var (ratioW, ratioH) in ratiosIter)
        {
            // I know the two loops are the same, I don't care
            for (var i = ExtraResolutionClosest; i <= maxWidth; i += ExtraResolutionClosest)
            {
                var ratioMult = (int)Math.Ceiling(i / (double)ratioW);
                var closestH = ratioMult * ratioH;
                if (closestH > maxHeight) break;
                var closestW = ratioMult * ratioW;
                extraResolutions.Add((closestW, closestH));
            }

            for (var i = ExtraResolutionClosest; i <= maxHeight; i += ExtraResolutionClosest)
            {
                var ratioMult = (int)Math.Ceiling(i / (double)ratioH);
                var closestW = ratioMult * ratioW;
                if (closestW > maxWidth) break;
                var closestH = ratioMult * ratioH;
                extraResolutions.Add((closestW, closestH));
            }
        }
        
        calcBench.Dispose();

        extraResolutions.Remove((width, height));

        var allExtraRes = new List<ResolutionWrapper>();

        foreach (var res in ExtraResolutions)
        {
            extraResolutions.Remove((res.Width, res.Height));
            allExtraRes.Add(res);
        }

        var rrDefault = unityInstanceWrapFactory.CreateNew<RefreshRateWrap>(60.0);
        
        var resCreateBench = Bench.Measure();

        foreach (var (w, h) in extraResolutions)
        {
            allExtraRes.Add(new(w, h, rrDefault));
        }
        
        resCreateBench.Dispose();

        windowEnv.ExtraSupportedResolutions =
            allExtraRes.OrderBy(rr => rr.Width).ThenBy(rr => rr.Width * rr.Height).ToArray();
    }

    private static (int width, int height) CalculateAspectRatio(int originalWidth, int originalHeight)
    {
        var gcd = MathUtils.GreatestCommonDivisor(originalWidth, originalHeight);
        return (originalWidth / gcd, originalHeight / gcd);
    }

    public ResolutionWrapper CurrentResolution { get; } = currentResolution;
    public ResolutionWrapper[] ExtraResolutions { get; } = extraResolutions;

    public int ExtraResolutionClosest { get; } = Math.Max(extraResolutionClosest, 1);
}