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
    public static extern void toggle_reverse_invoker(bool enable);


    [DllImport("unitas_rs")]
    public static extern void movie_start(string fs_path, string[] fs_passthrough, nuint fs_passthrough_count);

    [DllImport("unitas_rs")]
    public static extern void movie_end();

    [DllImport("unitas_rs")]
    public static extern bool extract_zip(string target, string dest);
}
