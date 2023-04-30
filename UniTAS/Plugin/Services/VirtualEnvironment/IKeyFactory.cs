using UniTAS.Plugin.Models.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Plugin.Services.VirtualEnvironment;

public interface IKeyFactory
{
    Key CreateKey(string key);
    Key CreateKey(KeyCode key);
}