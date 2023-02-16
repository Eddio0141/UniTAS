using System;

namespace UniTASPlugin.Interfaces;

/// <summary>
/// An interface that you can use to subscribe to MonoBehaviour events.
/// </summary>
public interface IUpdateEvents
{
    event Action OnGUIEvent;
}