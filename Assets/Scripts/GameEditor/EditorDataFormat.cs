using UnityEngine;

public static class EditorDataFormat
{
    public static int EncodeDirectedSpray(int paintCount, Vector2Int direction, bool doPaintReverse)
    {
        int sign = paintCount >= 0 ? 1 : -1;
        int absTileCount = Mathf.Abs(paintCount);

        int dirCode;
        switch ((direction.x, direction.y))
        {
            case (0, 1): dirCode = 0; break;
            case (1, 0): dirCode = 1; break;
            case (0, -1): dirCode = 2; break;
            case (-1, 0): dirCode = 3; break;
            default:
                dirCode = 0;
                Logger.LogWarning($"EncodeDirectedSpray: Unknown direction {direction}, defaulting to Up");
                break;
        }

        int result = absTileCount
                   + dirCode * 100_000_000
                   + (doPaintReverse ? 1_000_000_000 : 0);

        return result * sign;
    }

    public static void DecodeDirectedSpray(int encodedValue, out int paintCount, out Vector2Int direction, out bool doPaintReverse)
    {
        int sign = encodedValue >= 0 ? 1 : -1;
        encodedValue = Mathf.Abs(encodedValue);

        paintCount = encodedValue % 100_000_000 * sign;
        int code = encodedValue / 100_000_000;

        doPaintReverse = code >= 10;
        int dirCode = code % 10;

        direction = dirCode switch
        {
            0 => Vector2Int.up,
            1 => Vector2Int.right,
            2 => Vector2Int.down,
            3 => Vector2Int.left,
            _ => Vector2Int.up
        };
    }
}