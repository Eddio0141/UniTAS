using UnityEngine;

namespace UniTAS.Patcher.Models.GUI;

public readonly struct AnchoredOffset
{
    public int AnchorX { get; }
    public int AnchorY { get; }
    public int OffsetX { get; }
    public int OffsetY { get; }

    /// <summary>
    /// Creates a new AnchoredOffset.
    /// </summary>
    /// <param name="anchorX">Anchor X position. 0 is left, 1 is right.</param>
    /// <param name="anchorY">Anchor Y position. 0 is top, 1 is bottom.</param>
    /// <param name="offsetX">Offset X position.</param>
    /// <param name="offsetY">Offset Y position.</param>
    public AnchoredOffset(int anchorX, int anchorY, int offsetX, int offsetY)
    {
        AnchorX = anchorX;
        AnchorY = anchorY;
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    /// <summary>
    /// Gets the actual X position based on screen width.
    /// </summary>
    public int X => Screen.width * AnchorX + OffsetX;

    /// <summary>
    /// Gets the actual Y position based on screen height.
    /// </summary>
    public int Y => Screen.height * AnchorY + OffsetY;
}