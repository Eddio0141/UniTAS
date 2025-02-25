using System.Runtime.InteropServices;

namespace UniTAS.Patcher.External;

public static class UniTasRs
{
    // [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    // public delegate int Callback(int value);

    [DllImport("unitas_rs")]
    public static extern void init();
}