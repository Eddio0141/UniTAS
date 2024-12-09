using System.IO;
using AssetsTools.NET.Extra;
using BepInEx;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.UnityInfo;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityInfo;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

namespace UniTAS.Patcher.Implementations.UnityInfo;

[Singleton]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class LegacyInputInfo
{
    /// Grabs legacy input info from globalgamemanagers
    public LegacyInputInfo(IAssetsManager assetsManager, ILogger logger,
        IAxisStateEnvLegacySystem axisStateEnvLegacySystem)
    {
        var globalGameManagersPath = Directory.GetParent(Paths.ManagedPath)?.FullName;
        if (globalGameManagersPath == null)
        {
            logger.LogError("Failed to get globalgamemanagers path");
            return;
        }

        globalGameManagersPath = Path.Combine(globalGameManagersPath, "globalgamemanagers");

        if (!File.Exists(globalGameManagersPath))
        {
            logger.LogInfo("Failed to find globalgamemanagers file");
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

        var inputManagerAssets = file.GetAssetsOfType(AssetClassID.InputManager);

        if (inputManagerAssets.Count == 0)
        {
            logger.LogError("Failed to find input manager asset");
            return;
        }

        var inputManagerAsset = inputManagerAssets[0];
        var inputManager = manager.GetBaseField(assetsFileInstance, inputManagerAsset);

        var axes = inputManager["m_Axes.Array"];

        foreach (var axis in axes)
        {
            // getting those shouldn't fail, unless the new version of Unity changed the format

            var name = axis["m_Name"].AsString;
            var negativeButton = axis["negativeButton"].AsString;
            var positiveButton = axis["positiveButton"].AsString;
            var altNegativeButton = axis["altNegativeButton"].AsString;
            var altPositiveButton = axis["altPositiveButton"].AsString;
            var gravity = axis["gravity"].AsFloat;
            var dead = axis["dead"].AsFloat;
            var sensitivity = axis["sensitivity"].AsFloat;
            var snap = axis["snap"].AsBool;
            var invert = axis["invert"].AsBool;
            var typeRaw = axis["type"].AsInt;
            var axisNumRaw = axis["axis"].AsInt;
            var joyNumRaw = axis["joyNum"].AsInt;

            var type = (AxisType)typeRaw;
            var axisNum = (AxisChoice)axisNumRaw;
            var joyNum = (JoyNum)joyNumRaw;

            logger.LogDebug(
                $"Found legacy input axis: {name}, positive button: `{positiveButton}` (alt `{altPositiveButton}`), " +
                $"negative button: `{negativeButton}` (alt `{altNegativeButton}`), gravity: {gravity}, dead zone: {dead}, " +
                $"sensitivity: {sensitivity}, snap: {snap}, invert: {invert}, type: {type}, axis: {axisNum}, joystick num: {joyNum}"
            );

            var legacyInputAxis = new LegacyInputAxis(name, negativeButton, positiveButton, altNegativeButton,
                altPositiveButton, gravity, dead, sensitivity, snap, invert, type, axisNum, joyNum);

            axisStateEnvLegacySystem.AddAxis(legacyInputAxis);
        }
    }
}