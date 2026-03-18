using System;
using System.Runtime.InteropServices;

namespace UniTAS.Patcher.External;

public static class UniTasRs
{
    [DllImport("unitas_rs")]
    public static extern void last_update_set_callback(Action callback);

    [DllImport("unitas_rs")]
    public static extern void toggle_reverse_invoker(bool enable);

    [DllImport("unitas_rs")]
    public static extern void set_frame_time(double frameTime);

    [DllImport("unitas_rs")]
    public static extern void update_actual();

    [DllImport("unitas_rs")]
    public static extern void restart(ulong secs, uint nano_secs);
}
