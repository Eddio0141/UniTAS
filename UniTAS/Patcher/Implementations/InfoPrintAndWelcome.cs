using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Singleton(RegisterPriority.InfoPrintAndWelcome)]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class InfoPrintAndWelcome
{
    public InfoPrintAndWelcome(IGameInfo gameInfo, ILogger logger)
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

        logger.LogInfo($"System time: {DateTime.Now}");
        logger.LogInfo($"UniTAS {MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }
}