using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnityFix;

[Singleton]
public class ClearAudioOnRestart : IOnPreGameRestart
{
    public void OnPreGameRestart()
    {
        var audioSources = ObjectUtils.FindObjectsOfType<AudioSource>();
        foreach (var audioSource in audioSources)
        {
            audioSource.Stop();
        }
    }
}