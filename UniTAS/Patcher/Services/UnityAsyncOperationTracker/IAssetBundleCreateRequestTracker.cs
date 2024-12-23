using System.IO;
using UnityEngine;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface IAssetBundleCreateRequestTracker
{
    void NewAssetBundleCreateRequest(AsyncOperation op, string path, uint crc, ulong offset);
    void NewAssetBundleCreateRequest(AsyncOperation op, byte[] binary, uint crc);
    void NewAssetBundleCreateRequest(AsyncOperation op, Stream stream, uint crc, uint managedReadBufferSize);
    AssetBundle GetAssetBundleCreateRequest(AsyncOperation asyncOperation);
}