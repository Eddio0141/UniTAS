using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameInfo;
using UniTASPlugin.GameRestart;
using UniTASPlugin.LegacySafeWrappers;
using UniTASPlugin.Logger;
using UniTASPlugin.Patches.PatchProcessor;
using UniTASPlugin.ReverseInvoker;
using UnityEngine;
using PatchProcessor = UniTASPlugin.Patches.PatchProcessor.PatchProcessor;

namespace UniTASPlugin;

// ReSharper disable once ClassNeverInstantiated.Global
public class PluginWrapper
{
    public PluginWrapper(IEnumerable<PatchProcessor> patchProcessors, IEnumerable<IOnGameRestart> onGameRestarts,
        IReverseInvokerFactory reverseInvokerFactory, IGameInfo gameInfo, ILogger logger,
        IVirtualEnvironmentFactory virtualEnvironmentFactory)
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

        // init random seed
        // TODO make this happen after 
        var env = virtualEnvironmentFactory.GetVirtualEnv();
        RandomWrap.InitState((int)env.Seed);

        logger.LogInfo($"System time: {DateTime.Now}");
        logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");

        var rev = reverseInvokerFactory.GetReverseInvoker();
        var actualTime = rev.Invoke(() => DateTime.Now);

        foreach (var onGameRestart in onGameRestarts)
        {
            onGameRestart.OnGameRestart(actualTime);
        }

        var sortedPatches = patchProcessors
            .Where(x => x is not OnPluginInitProcessor)
            .SelectMany(x => x.ProcessModules())
            .OrderByDescending(x => x.Key)
            .Select(x => x.Value);
        foreach (var patch in sortedPatches)
        {
            Trace.Write($"Patching {patch} post game restart");
            Plugin.Harmony.PatchAll(patch);
        }

        // this needs to be ran after the patches are applied
        Plugin.StartEndOfFrameLoop();

        // this is to make sure Path fields are property set, not sure if theres a better place to put this
        // AccessTools.Constructor(typeof(System.IO.Path), searchForStatic: true).Invoke(null, null);
    }

// #if TRACE
//     private static void MonoIOPatchModuleTracePrints()
//     {
//         var logCount = MonoIOPatchModule.Log.Count;
//         for (var i = 0; i < logCount; i++)
//         {
//             var log = MonoIOPatchModule.Log[i];
//             Trace.Write(log);
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