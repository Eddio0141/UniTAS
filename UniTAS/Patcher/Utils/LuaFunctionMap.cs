using UnityEngine;

namespace UniTAS.Patcher.Utils;

public static class LuaFunctionMap
{
    public static class Key
    {
        private static readonly string keyClassName = nameof(Implementations.Movie.Engine.Modules.Key).ToLowerInvariant();
        public static string Hold(KeyCode key)
        {
            var holdfunc = nameof(Implementations.Movie.Engine.Modules.Key.Hold).ToLowerInvariant();
            var keyname = key.ToString();
            return $"{keyClassName}.{holdfunc}(\"{keyname}\")";
        }
        public static string Release(KeyCode key)
        {
            var releasefunc = nameof(Implementations.Movie.Engine.Modules.Key.Release).ToLowerInvariant();
            var keyname = key.ToString();
            return $"{keyClassName}.{releasefunc}(\"{keyname}\")";
        }
        public static string Clear()    
        {
            var clearfunc = nameof(Implementations.Movie.Engine.Modules.Key.Clear).ToLowerInvariant();
            return $"{keyClassName}.{clearfunc}()";
        }
    }

    public static class Mouse
    {
        private static readonly string mouseClassName = nameof(Implementations.Movie.Engine.Modules.Mouse).ToLowerInvariant();
        public static string Move(float x, float y)
        {
            var moveFunc = nameof(Implementations.Movie.Engine.Modules.Mouse.Move).ToLowerInvariant();
            return $"{mouseClassName}.{moveFunc}({x}, {y})";
        }
        public static string Move_rel(float x, float y)
        {
            var moveRelFunc = nameof(Implementations.Movie.Engine.Modules.Mouse.Move_rel).ToLowerInvariant();
            return $"{mouseClassName}.{moveRelFunc}({x}, {y})";
        }
        public static string Left(bool hold = true)
        {
            var leftFunc = nameof(Implementations.Movie.Engine.Modules.Mouse.Left).ToLowerInvariant();
            return $"{mouseClassName}.{leftFunc}({(hold ? "" : "false")})";
        }
        public static string Right(bool hold = true)
        {
            var rightFunc = nameof(Implementations.Movie.Engine.Modules.Mouse.Right).ToLowerInvariant();
            return $"{mouseClassName}.{rightFunc}({(hold ? "" : "false")})";
        }
        public static string Middle(bool hold = true)
        {
            var middleFunc = nameof(Implementations.Movie.Engine.Modules.Mouse.Middle).ToLowerInvariant();
            return $"{mouseClassName}.{middleFunc}({(hold ? "" : "false")})";
        }
        public static string Set_scroll(float x, float y)
        {
            var setScrollFunc = nameof(Implementations.Movie.Engine.Modules.Mouse.Set_scroll).ToLowerInvariant();
            return $"{mouseClassName}.{setScrollFunc}({x}, {y})";
        }
    }

    public static string FrameAdvance(int count = 1)
    {
        const string frameAdvanceFunc = Implementations.Movie.Parser.MovieParser.frameAdvanceFullName;
        return $"{frameAdvanceFunc}({(count == 1 ? "" : count)})";
    }
}