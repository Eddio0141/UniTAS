using System.Collections.Generic;
using UniTAS.Patcher.Models.VirtualEnvironment;

namespace UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;

public interface IKeyboardStateNew
{
    HashSet<NewKeyCodeWrap> HeldKeys { get; }
}
