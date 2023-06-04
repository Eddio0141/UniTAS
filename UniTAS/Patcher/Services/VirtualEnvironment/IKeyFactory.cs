using UniTAS.Patcher.Models.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Patcher.Services.VirtualEnvironment;

public interface IKeyFactory
{
    Key CreateKey(string key);
    Key CreateKey(KeyCode key);
}