using System;

namespace UniTAS.Plugin.Extensions;

public static class IntExtensions
{
    // TODO test
    public static int RoundUpToNearestPowerOfTwo(this int value)
    {
        var power = 1;
        while (power < value)
        {
            power <<= 1;

            // if we overflowed, throw an exception
            if (power < 0)
            {
                throw new OverflowException();
            }
        }

        return power;
    }
}