using System;

namespace UniTAS.Patcher.Extensions;

public static class DoubleExtensions
{
    public static int DecimalPlaces(this double n)
    {
        n = Math.Abs(n);
        n -= (int)n;
        var decimalPlaces = 0;
        while (n > 0)
        {
            decimalPlaces++;
            n *= 10;
            n -= (int)n;
        }

        return decimalPlaces;
    }
}