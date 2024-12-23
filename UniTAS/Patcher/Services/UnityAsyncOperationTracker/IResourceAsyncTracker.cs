using System;
using UnityEngine;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface IResourceAsyncTracker
{
    void ResourceLoadAsync(AsyncOperation op, string path, Type type);
}