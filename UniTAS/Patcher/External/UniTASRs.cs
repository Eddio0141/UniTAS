using System;
using System.Runtime.InteropServices;

namespace UniTAS.Patcher.External;

public static class UniTasRs
{
    [DllImport("unitas_rs")]
    public static extern void init();

    [DllImport("unitas_rs")]
    public static extern void last_update_set_callback(Action callback);
}