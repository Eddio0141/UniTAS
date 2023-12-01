using System;
using UniTAS.Patcher.Models.UnityInfo;
using UnityEngine;

namespace UniTAS.Patcher.Models.VirtualEnvironment;

public class LegacyInputAxisState
{
    public float Value { get; private set; }
    public float ValueRaw { get; private set; }
    private readonly LegacyInputAxis _axis;

    public Vector2 MousePos
    {
        set
        {
            if (_axis.Type != AxisType.MouseMovement)
            {
                return;
            }

            // TODO what if axis choice is not mouse x or mouse y
            _mousePos = _axis.Axis switch
            {
                AxisChoice.XAxis => value.x,
                AxisChoice.YAxis => value.y,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private AxisMoveDirection _moveDir;
    private AxisMoveDirection _moveDirPrev;

    // in case type is mouse movement
    private float _mousePrevPos;
    private float _mousePos;

    public LegacyInputAxisState(LegacyInputAxis axis)
    {
        _axis = axis;
    }

    public void KeyDown(string name)
    {
        if (_axis.Type != AxisType.KeyOrMouseButton)
        {
            return;
        }

        var negative = _axis.NegativeButton;
        var positive = _axis.PositiveButton;
        var negativeAlt = _axis.AltNegativeButton;
        var positiveAlt = _axis.AltPositiveButton;

        if (name == negative || name == negativeAlt)
        {
            _moveDir = _axis.Invert ? AxisMoveDirection.Positive : AxisMoveDirection.Negative;
        }
        else if (name == positive || name == positiveAlt)
        {
            _moveDir = _axis.Invert ? AxisMoveDirection.Negative : AxisMoveDirection.Positive;
        }
    }

    public void KeyUp(string name)
    {
        if (_axis.Type != AxisType.KeyOrMouseButton)
        {
            return;
        }

        var negative = _axis.NegativeButton;
        var positive = _axis.PositiveButton;
        var negativeAlt = _axis.AltNegativeButton;
        var positiveAlt = _axis.AltPositiveButton;

        if (name == negative || name == negativeAlt || name == positive || name == positiveAlt)
        {
            _moveDir = AxisMoveDirection.Neutral;
        }
    }

    public void FlushBufferedInputs()
    {
        // note: the inversion for axis is handled outside of this class, but the inversion for mouse movement is handled here

        if (_axis.Type == AxisType.MouseMovement)
        {
            // mouse movement is handled differently
            // 
            // snap, gravity, dead zone doesn't do anything
            // sensitivity is literally a multiplier
            // invert actually inverts

            var diff = (_mousePos - _mousePrevPos) * _axis.Sensitivity;
            if (_axis.Invert)
            {
                diff = -diff;
            }

            ValueRaw = diff;
            Value = diff;
            return;
        }

        // TODO window movement
        // TODO negative and positive at the same time
        // TODO unit tests
        if (_axis.Type == AxisType.WindowMovement)
        {
            throw new NotImplementedException();
        }

        var dt = Time.deltaTime;

        switch (_moveDir)
        {
            case AxisMoveDirection.Positive:
            {
                // handle smoothing
                if (_moveDirPrev == AxisMoveDirection.Negative && _axis.Snap)
                {
                    // snap actually resets to 0 for a frame
                    ValueRaw = 0f;
                    Value = 0f;
                }
                else
                {
                    ValueRaw = 1f;
                    Value = Math.Min(Value + _axis.Sensitivity * dt, 1f);
                }

                break;
            }
            case AxisMoveDirection.Negative:
            {
                // handle smoothing
                if (_moveDirPrev == AxisMoveDirection.Positive && _axis.Snap)
                {
                    // snap actually resets to 0 for a frame
                    ValueRaw = 0f;
                    Value = 0f;
                }
                else
                {
                    ValueRaw = -1f;
                    Value = Math.Max(Value - _axis.Sensitivity * dt, -1f);
                }

                break;
            }
            case AxisMoveDirection.Neutral:
            {
                // handle smoothing
                // also yes negative gravity works its stupid
                ValueRaw = 0f;
                Value = Mathf.MoveTowards(Value, 0f, _axis.Gravity * dt);

                // dead zone only applies to neutral
                if (Math.Abs(Value) < _axis.Dead)
                {
                    Value = 0f;
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        _moveDirPrev = _moveDir;
        _mousePrevPos = _mousePos;
    }
}

public enum AxisMoveDirection
{
    Positive,
    Negative,
    Neutral
}