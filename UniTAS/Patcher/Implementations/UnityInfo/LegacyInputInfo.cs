using System.IO;
using AssetsTools.NET.Extra;
using BepInEx;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityInfo;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.UnityInfo;

[Singleton]
[ForceInstantiate]
public class LegacyInputInfo : ILegacyInputInfo
{
    public LegacyInputInfo(IAssetsManager assetsManager, ILogger logger)
    {
        var globalGameManagersPath = Directory.GetParent(Paths.ManagedPath)?.FullName;
        if (globalGameManagersPath == null)
        {
            logger.LogError("Failed to get globalGameManagers path");
            return;
        }

        globalGameManagersPath = Path.Combine(globalGameManagersPath, "globalgamemanagers");

        if (!File.Exists(globalGameManagersPath))
        {
            logger.LogError("Failed to find globalGameManagers file");
            return;
        }

        var manager = assetsManager.Instance;
        if (manager == null)
        {
            logger.LogError("Failed to get assets manager instance");
            return;
        }

        var assetsFileInstance = manager.LoadAssetsFile(globalGameManagersPath, true);
        var file = assetsFileInstance.file;
        manager.LoadClassDatabaseFromPackage(file.Metadata.UnityVersion);

        var inputManagerAsset = file.GetAssetsOfType(AssetClassID.InputManager)[0];
        var inputManager = manager.GetBaseField(assetsFileInstance, inputManagerAsset);

        var axes = inputManager["m_Axes.Array"];

        foreach (var axis in axes)
        {
            var name = axis["m_Name"].Value?.AsString;
            // button to press to go negative value
            var negativeButton = axis["negativeButton"].Value?.AsString;
            // button to press to go positive value
            var positiveButton = axis["positiveButton"].Value?.AsString;
            // button to press to go negative value (alt)
            var altNegativeButton = axis["altNegativeButton"].Value?.AsString;
            // button to press to go positive value (alt)
            var altPositiveButton = axis["altPositiveButton"].Value?.AsString;
            // speed (in units / sec) that the output value will fall towards neutral when device at rest
            var gravity = axis["gravity"].Value?.AsFloat;
            // size of the analog dead zone (all analog device values within this range map to neutral)
            var dead = axis["dead"].Value?.AsFloat;
            // speed to move towards target value for digital devices (in units / sec)
            var sensitivity = axis["sensitivity"].Value?.AsFloat;
            // if we have input in opposite direction of current, do we jump to neutral and continue from there
            var snap = axis["snap"].Value?.AsBool;
            // flip pos and neg values
            var invert = axis["invert"].Value?.AsBool;
            // TODO translate to enum
            // 0 = Key or Mouse Button
            // 1 = Mouse Movement
            // 2 = Joystick Axis
            var type = axis["type"].Value?.AsInt;
            // TODO translate to enum
            // 0 = X axis
            // 1 = Y axis
            // 2 = 3rd axis (Joysticks and ScrollWheel)
            // 3 = 4th axis Joystick
            // 4 = 5th axis Joystick
            // ...
            // 27 = 28th axis Joystick
            var axisNum = axis["axis"].Value?.AsInt;

            // TODO translate to enum
            // 0 = Get motion from all joysticks
            // 1 = Joystick 1
            // 2 = Joystick 2
            // ...
            // 16 = Joystick 16
            var joyNum = axis["joyNum"].Value?.AsInt;

            StaticLogger.Log.LogDebug("info:");
            StaticLogger.Log.LogDebug("name: " + name);
            StaticLogger.Log.LogDebug("negativeButton: " + negativeButton);
            StaticLogger.Log.LogDebug("positiveButton: " + positiveButton);
            StaticLogger.Log.LogDebug("altNegativeButton: " + altNegativeButton);
            StaticLogger.Log.LogDebug("altPositiveButton: " + altPositiveButton);
            StaticLogger.Log.LogDebug("gravity: " + gravity);
            StaticLogger.Log.LogDebug("dead: " + dead);
            StaticLogger.Log.LogDebug("sensitivity: " + sensitivity);
            StaticLogger.Log.LogDebug("snap: " + snap);
            StaticLogger.Log.LogDebug("invert: " + invert);
            StaticLogger.Log.LogDebug("type: " + type);
            StaticLogger.Log.LogDebug("axisNum: " + axisNum);
            StaticLogger.Log.LogDebug("joyNum: " + joyNum);
        }
    }
}