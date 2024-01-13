using System;
using System.IO;
using HarmonyLib;
using UniTAS.Patcher.Exceptions;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

[Singleton]
public class TextureWrapper : ITextureWrapper
{
    private readonly Func<Texture2D, byte[], bool> _texture2dLoadImage;
    private readonly Func<Texture2D, byte[], bool, bool> _imageConversionLoadImage;

    public TextureWrapper()
    {
        var texture2dLoadImage = AccessTools.Method(typeof(Texture2D), "LoadImage", [typeof(byte[])]);
        if (texture2dLoadImage != null)
        {
            _texture2dLoadImage = AccessTools.MethodDelegate<Func<Texture2D, byte[], bool>>(texture2dLoadImage);
            return;
        }

        _imageConversionLoadImage =
            AccessTools.MethodDelegate<Func<Texture2D, byte[], bool, bool>>(
                AccessTools.Method("UnityEngine.ImageConversion:LoadImage",
                    [typeof(Texture2D), typeof(byte[]), typeof(bool)]));
    }

    public void LoadImage(Texture2D texture, string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Texture image not found at {path}");
        }

        var bytes = File.ReadAllBytes(path);

        bool success;
        if (_texture2dLoadImage != null)
        {
            success = _texture2dLoadImage.Invoke(texture, bytes);
            texture.Apply();
        }
        else
        {
            success = _imageConversionLoadImage.Invoke(texture, bytes, false);
        }

        if (!success)
        {
            throw new TextureLoadFailException($"Texture2D.LoadImage failed to load image from {path}");
        }
    }
}