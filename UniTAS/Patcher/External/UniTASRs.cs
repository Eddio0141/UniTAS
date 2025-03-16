using System;
using System.Runtime.InteropServices;

namespace UniTAS.Patcher.External;

public static class UniTasRs
{
    [DllImport("unitas_rs")]
    public static extern void init();

    [DllImport("unitas_rs")]
    public static extern void last_update_set_callback(Action callback);

    [DllImport("unitas_rs")]
    public static extern void update_actual(double deltaTime);
    
    [DllImport("unitas_rs")]
    public static extern void fixed_update_actual(double fixedDeltaTime);
    
    [DllImport("unitas_rs")]
    public static extern bool restart(ulong secs, uint nanoSecs);

    [DllImport("unitas_rs")]
    public static extern void toggle_reverse_invoker(bool enable);
}