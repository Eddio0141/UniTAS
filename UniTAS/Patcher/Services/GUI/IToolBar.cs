using System;

namespace UniTAS.Patcher.Services.GUI;

public interface IToolBar
{
    bool Show { get; }
    event Action<bool> OnShowChange;
}