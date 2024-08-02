// ReSharper disable UnusedMember.Global
namespace UniTAS.Patcher.Models.UnityInfo;

public readonly struct LegacyInputAxis(
    string name,
    string negativeButton,
    string positiveButton,
    string altNegativeButton,
    string altPositiveButton,
    float gravity,
    float dead,
    float sensitivity,
    bool snap,
    bool invert,
    AxisType type,
    AxisChoice axis,
    JoyNum joyNum)
{
    // name of the axis
    public readonly string Name = name;

    // button to press to go negative value
    public readonly string NegativeButton = negativeButton;

    // button to press to go positive value
    public readonly string PositiveButton = positiveButton;

    // button to press to go negative value (alt)
    public readonly string AltNegativeButton = altNegativeButton;

    // button to press to go positive value (alt)
    public readonly string AltPositiveButton = altPositiveButton;

    // speed (in units / sec) that the output value will fall towards neutral when device at rest
    public readonly float Gravity = gravity;

    // size of the analog dead zone (all analog device values within this range map to neutral)
    public readonly float Dead = dead;

    // speed to move towards target value for digital devices (in units / sec)
    public readonly float Sensitivity = sensitivity;

    // if we have input in opposite direction of current, do we jump to neutral and continue from there
    public readonly bool Snap = snap;

    // flip pos and neg values
    public readonly bool Invert = invert;

    // axis type
    public readonly AxisType Type = type;

    // which axis this one is on
    public readonly AxisChoice Axis = axis;

    // either gets this from all joysticks or a specific one
    public readonly JoyNum JoyNum = joyNum;
}

public enum AxisType
{
    KeyOrMouseButton,
    MouseMovement,
    JoystickAxis,
    WindowMovement
}

public enum JoyNum
{
    AllJoysticks,
    Joystick1,
    Joystick2,
    Joystick3,
    Joystick4,
    Joystick5,
    Joystick6,
    Joystick7,
    Joystick8,
    Joystick9,
    Joystick10,
    Joystick11,
    Joystick12,
    Joystick13,
    Joystick14,
    Joystick15,
    Joystick16
}

public enum AxisChoice
{
    XAxis,
    YAxis,
    Axis3,
    Axis4,
    Axis5,
    Axis6,
    Axis7,
    Axis8,
    Axis9,
    Axis10,
    Axis11,
    Axis12,
    Axis13,
    Axis14,
    Axis15,
    Axis16,
    Axis17,
    Axis18,
    Axis19,
    Axis20,
    Axis21,
    Axis22,
    Axis23,
    Axis24,
    Axis25,
    Axis26,
    Axis27,
    Axis28
}