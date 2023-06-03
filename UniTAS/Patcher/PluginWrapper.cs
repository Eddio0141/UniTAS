using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UniTAS.Patcher.Implementations.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Services.Logging;
using UnityEngine;
using PatchProcessor = UniTAS.Patcher.Interfaces.Patches.PatchProcessor.PatchProcessor;

namespace UniTAS.Patcher;

// ReSharper disable once ClassNeverInstantiated.Global
public class PluginWrapper
{
    public PluginWrapper(IEnumerable<PatchProcessor> patchProcessors, IGameInfo gameInfo, ILogger logger,
        IWindowFactory windowFactory)
    {
        logger.LogInfo($"Internally found unity version: {gameInfo.UnityVersion}");
        logger.LogInfo($"Game product name: {gameInfo.ProductName}");
        logger.LogDebug($"Mscorlib version: {gameInfo.MscorlibVersion}");
        logger.LogDebug($"Netstandard version: {gameInfo.NetStandardVersion}");
        // TODO complete fixing this
        var companyNameProperty = Traverse.Create(typeof(Application)).Property("companyName");
        if (companyNameProperty.PropertyExists())
            logger.LogInfo(
                $"Game company name: {companyNameProperty.GetValue<string>()}"); //product name: {Application.productName}, version: {Application.version}");

        // TODO all axis names for help

        var sortedPatches = patchProcessors
            .SelectMany(x => x.ProcessModules())
            .OrderByDescending(x => x.Key)
            .Select(x => x.Value);
        foreach (var patch in sortedPatches)
        {
            logger.LogDebug($"Patching {patch} post game restart");
            Plugin.Harmony.PatchAll(patch);
        }

        // this needs to be ran after the patches are applied
        Plugin.StartEndOfFrameLoop();

        // this is to make sure Path fields are property set, not sure if theres a better place to put this
        // AccessTools.Constructor(typeof(System.IO.Path), searchForStatic: true).Invoke(null, null);

        windowFactory.Create<MainMenu>().Show();

        logger.LogInfo($"System time: {DateTime.Now}");
        logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
    }

// #if TRACE
//     private static void MonoIOPatchModuleTracePrints()
//     {
//         var logCount = MonoIOPatchModule.Log.Count;
//         for (var i = 0; i < logCount; i++)
//         {
//             var log = MonoIOPatchModule.Log[i];
//             _logger.LogDebug(log);
//         }
//
//         MonoIOPatchModule.Log.RemoveRange(0, logCount);
//     }
// #endif
//     
//
//     private void CallOnPreUpdate()
//     {
//         if (_calledPreUpdate) return;
//         _calledPreUpdate = true;
//
// #if TRACE
//         MonoIOPatchModuleTracePrints();
// #endif
//
//         foreach (var onPreUpdate in _onPreUpdates)
//         {
//             onPreUpdate.PreUpdate();
//         }
//     }
}