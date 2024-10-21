using System;
using UniTAS.Patcher.Interfaces.GUI;
using UnityEngine;

namespace UniTAS.Patcher.Services.GUI;

public interface IDropdownList : IGUIComponent
{
    bool DropdownButtons(Rect position, (string, Action)[] buttons);
}