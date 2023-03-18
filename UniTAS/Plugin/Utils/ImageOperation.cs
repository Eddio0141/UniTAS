using System;

namespace UniTAS.Plugin.Utils;

public static class ImageOperation
{
    /// <summary>
    /// Resizes the image to a different resolution while maintaining aspect ratio
    /// Fills in the rest of the image with black
    /// </summary>
    public static byte[] Resize(byte[] original, int originalWidth, int originalHeight, int targetWidth,
        int targetHeight)
    {
        if (originalWidth == 0 || originalHeight == 0 || targetWidth == 0 || targetHeight == 0)
        {
            throw new ArgumentException("Width and height must be greater than 0");
        }

        // original format
        // RGB
        // left to right, bottom to top, row by row
        var ratioX = (double)targetWidth / originalWidth;
        var ratioY = (double)targetHeight / originalHeight;
        var ratio = Math.Min(ratioX, ratioY);

        var resized = new byte[targetWidth * targetHeight * 3];

        var newWidth = (int)(originalWidth * ratio);
        var newHeight = (int)(originalHeight * ratio);

        var newXOffset = newWidth < targetWidth ? (targetWidth - newWidth) / 2 : 0;
        var newYOffset = newHeight < targetHeight ? (targetHeight - newHeight) / 2 : 0;

        var originalXChange = (double)originalWidth / newWidth;
        var originalYChange = (double)originalHeight / newHeight;

        for (var y = 0; y < newHeight; y++)
        {
            for (var x = 0; x < newWidth; x++)
            {
                var originalIndex = ((int)(y * originalYChange) * originalWidth + (int)(x * originalXChange)) * 3;
                var newIndex = ((y + newYOffset) * targetWidth + x + newXOffset) * 3;

                resized[newIndex] = original[originalIndex];
                resized[newIndex + 1] = original[originalIndex + 1];
                resized[newIndex + 2] = original[originalIndex + 2];
            }
        }

        return resized;
    }
}