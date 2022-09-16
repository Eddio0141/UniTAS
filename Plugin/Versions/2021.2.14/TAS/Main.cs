using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniTASPlugin.TAS;

public static class Main
{    

    public static void RunMovie(Movie movie)
    {
        CurrentFrameNum = 0;
        currentFramebulkIndex = 0;
        currentFramebulkFrameIndex = 1;

        CurrentMovie = movie;

        if (CurrentMovie.Framebulks.Count > 0)
        {
            var firstFb = CurrentMovie.Framebulks[0];

            Input.Main.Clear();
            UnityEngine.Time.captureDeltaTime = firstFb.Frametime;
            GameControl(firstFb);

            if (currentFramebulkFrameIndex >= firstFb.FrameCount)
            {
                currentFramebulkFrameIndex = 0;
                currentFramebulkIndex++;
            }
        }

        pendingMovieStartFixedUpdate = true;
        Plugin.Log.LogInfo("Starting movie, pending FixedUpdate call");
    }

    static void RunMoviePending()
    {
        Running = true;
        SoftRestart(CurrentMovie.Seed);
        Plugin.Log.LogInfo($"Movie start: {CurrentMovie}");
    }
}
