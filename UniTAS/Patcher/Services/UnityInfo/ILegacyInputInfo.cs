using UniTAS.Patcher.Models.UnityInfo;

namespace UniTAS.Patcher.Services.UnityInfo;

public interface ILegacyInputInfo
{
    void AddAxis(LegacyInputAxis axis);
}