using System.Runtime.InteropServices;

namespace UniTAS.Patcher.External;

public static class Test
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int Callback(int value);

    [DllImport("unitas_rs")]
    public static extern void set_callback(Callback cb);

    [DllImport("unitas_rs")]
    public static extern void hello_world();
}