using System;

namespace UniTAS.Patcher.Models.Utils;

public readonly struct Either<TLeft, TRight>
{
    private readonly TLeft _left;
    private readonly TRight _right;

    public Either(TLeft left)
    {
        _left = left;
        _right = default!;
        IsLeft = true;
    }

    public Either(TRight right)
    {
        _left = default!;
        _right = right;
        IsLeft = false;
    }

    public bool IsLeft { get; }
    public bool IsRight => !IsLeft;

    public TLeft Left => IsLeft ? _left : throw new InvalidOperationException("Either is not left");
    public TRight Right => !IsLeft ? _right : throw new InvalidOperationException("Either is not right");
}