using System;
using UniTAS.Patcher.Models.UnityInfo;
using UnityEngine;

namespace UniTAS.Patcher.Models.VirtualEnvironment;

public class LegacyInputAxisState
{
    public float Value { get; private set; }
    public float ValueRaw { get; private set; }
    public LegacyInputAxis Axis { get; }
    public float MousePos { get; set; }

    private AxisMoveDirection _moveDir;
    private AxisMoveDirection _moveDirPrev;

    // in case type is mouse movement
    private float _mousePrevPos;

    public void Update()
    {
        // note: the inversion for axis is handled outside of this class, but the inversion for mouse movement is handled here

        if (Axis.Type == AxisType.MouseMovement)
        {
            // mouse movement is handled differently
            // 
            // snap, gravity, dead zone doesn't do anything
            // sensitivity is literally a multiplier
            // invert actually inverts

            var diff = (MousePos - _mousePrevPos) * Axis.Sensitivity;
            if (Axis.Invert)
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
        // TODO test on unity 3.4 ~ present
        if (Axis.Type == AxisType.WindowMovement)
        {
            throw new NotImplementedException();
        }

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
        _mousePrevPos = MousePos;
    }
}

public enum AxisMoveDirection
{
    Positive,
    Negative,
    Neutral
}