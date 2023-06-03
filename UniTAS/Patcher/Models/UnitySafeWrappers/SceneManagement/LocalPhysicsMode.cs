using System;

namespace UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;

[Flags]
public enum LocalPhysicsMode
{
    None,
    Physics2D,
    Physics3D
}