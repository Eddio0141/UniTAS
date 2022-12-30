using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UniTASPlugin.Interfaces.StartEvent;
using UniTASPlugin.Interfaces.Update;
using PatchProcessor = UniTASPlugin.Patches.PatchProcessor.PatchProcessor;
#if TRACE
using UniTASPlugin.Patches.Modules.FileSystemControlModules.FilePatchModule;
#endif

namespace UniTASPlugin;

// ReSharper disable once ClassNeverInstantiated.Global
public class PluginWrapper
{
    private bool _updated;
    private bool _calledPreUpdate;

    private readonly IOnUpdate[] _onUpdates;
    private readonly IOnFixedUpdate[] _onFixedUpdates;
    private readonly IOnAwake[] _onAwakes;
    private readonly IOnStart[] _onStarts;
    private readonly IOnEnable[] _onEnables;
    private readonly IOnPreUpdates[] _onPreUpdates;

    public PluginWrapper(IEnumerable<IOnUpdate> onUpdates, IEnumerable<IOnFixedUpdate> onFixedUpdates,
        IEnumerable<IOnAwake> onAwakes, IEnumerable<IOnStart> onStarts, IEnumerable<IOnEnable> onEnables,
        IEnumerable<IOnPreUpdates> onPreUpdates,
        IEnumerable<PatchProcessor> patchProcessors)
    {
        _onFixedUpdates = onFixedUpdates.ToArray();
        _onAwakes = onAwakes.ToArray();
        _onStarts = onStarts.ToArray();
        _onEnables = onEnables.ToArray();
        _onPreUpdates = onPreUpdates.ToArray();
        _onUpdates = onUpdates.ToArray();

        Harmony harmony = new($"{MyPluginInfo.PLUGIN_GUID}HarmonyPatch");

        var sortedPatches = patchProcessors.SelectMany(x => x.ProcessModules()).OrderByDescending(x => x.Key)
            .Select(x => x.Value);
        foreach (var patch in sortedPatches)
        {
            harmony.PatchAll(patch);
        }
    }

    // calls awake before any other script
    public void Awake()
    {
        foreach (var onAwake in _onAwakes)
        {
            onAwake.Awake();
        }
    }

    // calls onEnable before any other script
    public void OnEnable()
    {
        foreach (var onEnable in _onEnables)
        {
            onEnable.OnEnable();
        }
    }

    // calls start before any other script
    public void Start()
    {
        foreach (var onStart in _onStarts)
        {
            onStart.Start();
        }
    }

    public void Update()
    {
        if (_updated) return;
        _updated = true;

        CallOnPreUpdate();

        foreach (var update in _onUpdates)
        {
            update.Update();
        }

        //Overlay.Update();
        //GameCapture.Update();
    }

#if TRACE
    private static void MonoIOPatchModuleTracePrints()
    {
        var logCount = MonoIOPatchModule.Log.Count;
        for (var i = 0; i < logCount; i++)
        {
            var log = MonoIOPatchModule.Log[i];
            Plugin.Log.LogDebug(log);
        }

        MonoIOPatchModule.Log.RemoveRange(0, logCount);
    }
#endif

    // right now I don't call this update before other scripts so I don't need to check if it was already called
    public void LateUpdate()
    {
        _updated = false;
        _calledPreUpdate = false;
        GameTracker.LateUpdate();
    }

    public void PreFixedUpdate()
    {
        CallOnPreUpdate();
    }

    // right now I don't call this update before other scripts so I don't need to check if it was already called
    public void FixedUpdate()
    {
        foreach (var update in _onFixedUpdates)
        {
            update.FixedUpdate();
        }
    }

    private void CallOnPreUpdate()
    {
        if (_calledPreUpdate) return;
        _calledPreUpdate = true;

#if TRACE
        MonoIOPatchModuleTracePrints();
#endif

        foreach (var onPreUpdate in _onPreUpdates)
        {
            onPreUpdate.PreUpdate();
        }
    }
}