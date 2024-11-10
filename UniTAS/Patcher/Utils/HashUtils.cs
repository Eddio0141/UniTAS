using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UniTAS.Patcher.Utils;

public static class HashUtils
{
    public class ReferenceComparer : IEqualityComparer<object>
    {
        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}