using System;

namespace UniTAS.Plugin.Services;

public interface IGameRestart
{
    void SoftRestart(DateTime time);
}