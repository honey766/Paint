using UnityEngine;

public class CustomTools
{    public static Quaternion GetRotationByDirection(Vector2Int direction)
    {
        switch (direction)
        {
            case Vector2Int v when v == Vector2Int.up:
                return Quaternion.identity;
            case Vector2Int v when v == Vector2Int.right:
                return Quaternion.Euler(new Vector3(0, 0, -90));
            case Vector2Int v when v == Vector2Int.down:
                return Quaternion.Euler(new Vector3(0, 0, 180));
            case Vector2Int v when v == Vector2Int.left:
                return Quaternion.Euler(new Vector3(0, 0, 90));
        }
        return Quaternion.identity;
    }
}