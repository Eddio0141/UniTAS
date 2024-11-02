using UniTAS.Patcher.Implementations.GUI.Windows;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models;

namespace UniTAS.Patcher.Services.GUI;

public interface IWindowFactory
{
    T Create<T>() where T : Window;
    ObjectTrackerInstanceWindow Create(UnityObjectIdentifier identifier);
    ObjectSearchConfigWindow Create(UnityObjectIdentifier.SearchSettings searchSettings);
    ObjectPickerSearchSettings Create(ObjectPickerWindow.SearchSettings searchSettings);
}