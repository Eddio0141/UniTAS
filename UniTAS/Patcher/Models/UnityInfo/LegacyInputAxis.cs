namespace UniTAS.Patcher.Models.UnityInfo;

public readonly struct LegacyInputAxis
{
    // name of the axis
    public readonly string Name;

    // button to press to go negative value
    public readonly string NegativeButton;

    // button to press to go positive value
    public readonly string PositiveButton;

    // button to press to go negative value (alt)
    public readonly string AltNegativeButton;

    // button to press to go positive value (alt)
    public readonly string AltPositiveButton;

    // speed (in units / sec) that the output value will fall towards neutral when device at rest
    public readonly float Gravity;

    // size of the analog dead zone (all analog device values within this range map to neutral)
    public readonly float Dead;

    // speed to move towards target value for digital devices (in units / sec)
    public readonly float Sensitivity;

    // if we have input in opposite direction of current, do we jump to neutral and continue from there
    public readonly bool Snap;

    // flip pos and neg values
    public readonly bool Invert;

    // axis type
    public readonly AxisType Type;

    // which axis this one is on
    public readonly AxisChoice Axis;

    // either gets this from all joysticks or a specific one
    public readonly JoyNum JoyNum;

    public LegacyInputAxis(string name, string negativeButton, string positiveButton, string altNegativeButton,
        string altPositiveButton, float gravity, float dead, float sensitivity, bool snap, bool invert, AxisType type,
        AxisChoice axis, JoyNum joyNum)
    {
        Name = name;
        NegativeButton = negativeButton;
        PositiveButton = positiveButton;
        AltNegativeButton = altNegativeButton;
        AltPositiveButton = altPositiveButton;
        Gravity = gravity;
        Dead = dead;
        Sensitivity = sensitivity;
        Snap = snap;
        Invert = invert;
        Type = type;
        Axis = axis;
        JoyNum = joyNum;
    }
}