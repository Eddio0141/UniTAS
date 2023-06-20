using UnityEngine;

namespace UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;

public interface ITextureWrapper
{
    void LoadImage(Texture2D texture, string path);
}