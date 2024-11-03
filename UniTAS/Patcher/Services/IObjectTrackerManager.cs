using System.Collections.ObjectModel;
using UniTAS.Patcher.Implementations.GUI.Windows;
using UniTAS.Patcher.Models;

namespace UniTAS.Patcher.Services;

public interface IObjectTrackerManager
{
    void AddNew();
    ReadOnlyCollection<(UnityObjectIdentifier, ObjectTrackerInstanceWindow)> Trackers { get; }
}