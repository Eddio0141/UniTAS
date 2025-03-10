using System;
using UniTAS.Patcher.Models.UnityInfo;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Models.VirtualEnvironment;

public class LegacyInputAxisState(LegacyInputAxis axis)
{
    public float Value { get; private set; }
    public float ValueRaw { get; private set; }
    public LegacyInputAxis Axis { get; } = axis;

    public void ResetState()
    {
        Value = 0f;
        ValueRaw = 0f;
        _moveDir = AxisMoveDirection.Neutral;
        _moveDirPrev = AxisMoveDirection.Neutral;
        _mousePrevPos = 0f;
        _mousePos = 0f;
    }

    public Vector2 MousePos
    {
        set
        {
            if (Axis.Type != AxisType.MouseMovement)
            {
                return;
            }

            _mousePos = Axis.Axis switch
            {
                AxisChoice.XAxis => value.x,
                AxisChoice.YAxis => value.y,
                _ => 0f
            };
        }
    }

    public void MouseMoveRelative(Vector2 pos)
    {
        if (Axis.Type != AxisType.MouseMovement)
        {
            return;
        }

        _mousePos = Axis.Axis switch
        {
            AxisChoice.XAxis => _mousePos + pos.x,
            AxisChoice.YAxis => _mousePos + pos.y,
            _ => 0f
        };
    }

    private AxisMoveDirection _moveDir;
    private AxisMoveDirection _moveDirPrev;

    // in case type is mouse movement
    private float _mousePrevPos;
    private float _mousePos;

    public void KeyDown(KeyCode key)
    {
        if (Axis.Type != AxisType.KeyOrMouseButton)
        {
            return;
        }

        var negative = Axis.NegativeButton;
        var positive = Axis.PositiveButton;
        var negativeAlt = Axis.AltNegativeButton;
        var positiveAlt = Axis.AltPositiveButton;

        var name = InputSystemUtils.KeyCodeToStringVariant(key);

        if (name == negative || name == negativeAlt)
        {
            _moveDir = Axis.Invert ? AxisMoveDirection.Positive : AxisMoveDirection.Negative;
        }
        else if (name == positive || name == positiveAlt)
        {
            _moveDir = Axis.Invert ? AxisMoveDirection.Negative : AxisMoveDirection.Positive;
        }
    }

    public void KeyUp(KeyCode key)
    {
        if (Axis.Type != AxisType.KeyOrMouseButton)
        {
            return;
        }

        var negative = Axis.NegativeButton;
        var positive = Axis.PositiveButton;
        var negativeAlt = Axis.AltNegativeButton;
        var positiveAlt = Axis.AltPositiveButton;

        var name = InputSystemUtils.KeyCodeToStringVariant(key);

        if (name == negative || name == negativeAlt || name == positive || name == positiveAlt)
        {
            _moveDir = AxisMoveDirection.Neutral;
        }
    }

    /// <summary>
    /// For joystick axis or mouse wheel
    /// </summary>
    public void SetAxis(float value)
    {
        if ((Axis.Type == AxisType.MouseMovement && Axis.Axis != AxisChoice.Axis3) ||
            Axis.Type != AxisType.JoystickAxis)
        {
            return;
        }

        value *= Axis.Sensitivity;
        if (Axis.Invert)
        {
            value = -value;
        }

        // either way value is clamped
        value = Mathf.Clamp(value, -1f, 1f);

        // gravity does nothing
        // snap does nothing

        // value and value raw are the same for joystick axis
        Value = value;
        ValueRaw = value;
    }

    public void FlushBufferedInputs()
    {
        // note: the inversion for axis is handled outside of this class, but the inversion for mouse movement is handled here

        if (Axis.Type == AxisType.MouseMovement)
        {
            // mouse movement is handled differently
            // 
            // snap, gravity, dead zone doesn't do anything
            // sensitivity is literally a multiplier
            // invert actually inverts

            var diff = (_mousePos - _mousePrevPos) * Axis.Sensitivity;
            if (Axis.Invert)
            {
                diff = -diff;
            }

            ValueRaw = diff;
            Value = diff;

            _mousePrevPos = _mousePos;
            return;
        }

        // TODO window movement
        // TODO negative and positive at the same time
        // TODO unit tests
        if (Axis.Type == AxisType.WindowMovement)
        {
            throw new NotImplementedException();
        }

        if (Axis.Type != AxisType.KeyOrMouseButton) return;

        // below is for buttons
        var dt = Time.deltaTime;

        switch (_moveDir)
        {
            case AxisMoveDirection.Positive:
            {
                // handle smoothing
                if (_moveDirPrev == AxisMoveDirection.Negative && Axis.Snap)
                {
                    // snap actually resets to 0 for a frame
                    ValueRaw = 0f;
                    Value = 0f;
                }
                else
                {
                    ValueRaw = 1f;
                    Value = Math.Min(Value + Axis.Sensitivity * dt, 1f);
                }

                break;
            }
            case AxisMoveDirection.Negative:
            {
                // handle smoothing
                if (_moveDirPrev == AxisMoveDirection.Positive && Axis.Snap)
                {
                    // snap actually resets to 0 for a frame
                    ValueRaw = 0f;
                    Value = 0f;
                }
                else
                {
                    ValueRaw = -1f;
                    Value = Math.Max(Value - Axis.Sensitivity * dt, -1f);
                }

                break;
            }
            case AxisMoveDirection.Neutral:
            {
                // handle smoothing
                // also yes negative gravity works its stupid
                ValueRaw = 0f;
                Value = Mathf.MoveTowards(Value, 0f, Axis.Gravity * dt);

                // dead zone only applies to neutral
                if (Math.Abs(Value) < Axis.Dead)
                {
                    Value = 0f;
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        _moveDirPrev = _moveDir;
    }

    public bool JoyNumEquals(JoyNum joyNum)
    {
        var axisJoyNum = Axis.JoyNum;
        return axisJoyNum == joyNum || axisJoyNum == JoyNum.AllJoysticks;
    }
}

public enum AxisMoveDirection
{
    Positive,
    Negative,
    Neutral
}