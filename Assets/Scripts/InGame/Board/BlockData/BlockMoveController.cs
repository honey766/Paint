using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockMoveController : SingletonBehaviour<BlockMoveController>
{
    [SerializeField] private TileClickEvent tileClickScript;
    private Dictionary<Vector2Int, TileData> board; // 보드 상태
    private Dictionary<Vector2Int, BlockData> blocks; // 블록 위치와 상태

    protected override void Init()
    {
        m_IsDestroyOnLoad = true;
        base.Init();
    }

    public void InitBoard()
    {
        board = Board.Instance.board;
        blocks = Board.Instance.blocks;
    }

    // public bool CanMove(Vector2Int curPos, Vector2Int direction)
    // {
    //     while (true)
    //     {
    //         curPos += direction;
    //         if (board.ContainsKey(curPos))
    //         {
    //             if (!blocks.TryGetValue(curPos, out BlockData block))
    //                 return true;
    //         }
    //         else
    //         {
    //             return false;
    //         }
    //     }
    // }

    // // 밀 수 있는 상태를 가정
    // public void MoveBlocks(BlockData originBlock, Vector2Int curPos, Vector2Int direction)
    // {
    //     if (blocks.TryGetValue(curPos, out BlockData block) && block == originBlock)
    //         blocks.Remove(curPos);
    //     else Logger.LogWarning($"내 자리에 내가 없어요; {curPos}, {block}, {direction}, origin:{originBlock}");
    //     Vector2Int tempPos = curPos + direction;
    //     BlockData prevBlock = originBlock;
    //     int pushCnt = 1;

    //     // 1회 push
    //     while (true)
    //     {
    //         if (board.ContainsKey(tempPos) && blocks.TryGetValue(tempPos, out block) &&
    //             block.slidingDirection != direction)
    //         {
    //             blocks[tempPos] = prevBlock;
    //             block.MoveAnimation(tempPos + direction);
    //             pushCnt++;
    //         }
    //         else
    //         {
    //             blocks[tempPos] = prevBlock;
    //             break;
    //         }
    //         tempPos += direction;
    //         prevBlock = block;
    //     }

    //     // ice타일 검사
    //     tempPos = curPos;
    //     for (int i = 0; i < pushCnt; i++)
    //     {
    //         tempPos += direction;
    //         if (board[tempPos].Type == TileType.Ice)
    //         {
    //             block = blocks[tempPos];
    //             if (block.slidingDirection != direction)
    //             {
    //                 if (block.Type == TileType.Player)
    //                 {
    //                     tileClickScript.lastTile = null;
    //                     PlayerController.Instance.ClearMoveQueue();
    //                 }
    //                 block.StartSliding(tempPos, direction);
    //             }
    //         }
    //     }
    // }

    public bool CanMove(Vector2Int curPos, Vector2Int direction)
    {
        curPos += direction;

        // 타일이 없다면 이동 불가
        if (!board.ContainsKey(curPos))
            return false;
        // 블록이 없다면 이동 가능
        if (!(blocks.TryGetValue(curPos, out BlockData block) && !IsFakePlayerBlock(curPos, block)))
            return true;

        // 한 칸 더 갔을 때 블록이 있다면 이동 불가능
        curPos += direction;
        if (!board.ContainsKey(curPos))
            return false;
        return !(blocks.TryGetValue(curPos, out block) && !IsFakePlayerBlock(curPos, block));
    }

    // 밀 수 있는 상태를 가정
    public void MoveBlocks(BlockData originBlock, Vector2Int curPos, Vector2Int direction)
    {
        if (blocks.TryGetValue(curPos, out BlockData block) && block == originBlock)
            blocks.Remove(curPos);
        else Logger.LogWarning($"내 자리에 내가 없어요; {curPos}, {block}, {direction}, origin:{originBlock}");

        Vector2Int tempPos = curPos + direction;
        int blockCount = 1;

        if (blocks.TryGetValue(tempPos, out block) && !IsFakePlayerBlock(tempPos, block) && block.slidingDirection != direction)
        {
            block.MoveAnimation(tempPos + direction);
            blocks[tempPos + direction] = block;
            blockCount++;
        }
        blocks[tempPos] = originBlock;

        // ice타일 검사
        // tempPos = curPos;
        // for (int i = 0; i < blockCount; i++)
        // {
        //     tempPos += direction;
        //     if (board[tempPos].Type != TileType.Ice)
        //         return;

        //     block = blocks[tempPos];
        //     if (block.slidingDirection != direction)
        //     {
        //         if (block.Type == TileType.Player)
        //         {
        //             tileClickScript.lastTile = null;
        //             PlayerController.Instance.ClearMoveQueue();
        //         }
        //         block.StartSliding(tempPos, direction);
        //     }
        // }
    }
    
    // 혹시라도 잘못되어 현재 플레이어가 위치한 곳이 아닌 곳에 플레이어 블록이라고 남아있는지 체크
    private bool IsFakePlayerBlock(Vector2Int pos, BlockData block)
    {
        if (block.Type == TileType.Player && pos != PlayerController.Instance.curPos)
        {
            blocks.Remove(pos);
            return true;
        }
        return false;
    }
}