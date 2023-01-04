using System;
using System.Collections.Generic;
using UniTASPlugin.GameInfo;
using UniTASPlugin.Logger;
using UniTASPlugin.Patches.PatchGroups;
using UniTASPlugin.Patches.PatchTypes;

namespace UniTASPlugin.Patches.PatchProcessor;

// ReSharper disable once UnusedType.Global
public class UnityGroupPatchProcessor : GroupPatchProcessor
{
    private readonly IGameInfo _gameInfo;

    public UnityGroupPatchProcessor(ILogger logger, IGameInfo gameInfo) : base(logger)
    {
        _gameInfo = gameInfo;
    }

    protected override IEnumerable<int> ChoosePatch(ModulePatchType patchType, PatchGroup[] patchGroups)
    {
        var version = VersionStringToNumber(_gameInfo.UnityVersion);

        if (patchType.PatchAllGroups)
        {
            for (var i = 0; i < patchGroups.Length; i++)
            {
                var group = patchGroups[i];
                var groupUnityPatch = (UnityPatchGroup)group;

                var versionStart = VersionStringToNumber(groupUnityPatch.RangeStart);
                var versionEnd = VersionStringToNumber(groupUnityPatch.RangeEnd, ulong.MaxValue);

                // patch group version check
                if (versionStart <= version && version <= versionEnd)
                {
                    yield return i;
                }
            }

            yield break;
        }

        for (var i = 0; i < patchGroups.Length; i++)
        {
            var patchGroup = patchGroups[i];
            var unityPatchGroup = (UnityPatchGroup)patchGroup;

            var versionStart = VersionStringToNumber(unityPatchGroup.RangeStart);
            var versionEnd = VersionStringToNumber(unityPatchGroup.RangeEnd, ulong.MaxValue);

            if (versionStart > version || version > versionEnd) continue;

            yield return i;
            yield break;
        }
    }

    protected override Type TargetPatchType => typeof(UnityPatch);
}