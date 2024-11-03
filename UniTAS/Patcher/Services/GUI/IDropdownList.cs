using System;
using UniTAS.Patcher.Interfaces.GUI;
using UnityEngine;

namespace UniTAS.Patcher.Services.GUI;

public interface IDropdownList : IGUIComponent
{
    /// <summary>
    /// Creates a dropdown list with named buttons
    /// </summary>
    /// <param name="position">Position of where the dropdown list should go</param>
    /// <param name="buttons">Buttons with (name, callback) in order</param>
    /// <returns>True if clicked on any button</returns>
    bool DropdownButtons(Rect position, (string, Action)[] buttons);
}