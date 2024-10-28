using UniTAS.Patcher.Models;
using UnityEngine;

namespace UniTAS.Patcher.Services;

public interface IUnityObjectIdentifierFactory
{
    UnityObjectIdentifier NewUnityObjectIdentifier(Object o);
}