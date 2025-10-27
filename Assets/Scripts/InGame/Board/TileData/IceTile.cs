using UnityEngine;

public class IceTile : TileData
{
    public override void OnBlockEnter(BlockData block, Vector2Int pos, Vector2Int direction, TileType color, float moveTime)
    {
        base.OnBlockEnter(block, pos, direction, color, moveTime);
        // BlockMoveController.MoveBlocks()에서 로직 대체함. 블록 이동 로직과 관련이 깊기 때문
    }
}
