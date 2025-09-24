using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockMoveController : SingletonBehaviour<BlockMoveController>
{
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

    public bool CanMove(Vector2Int curPos, Vector2Int direction)
    {
        // Logger.Log($"========");
        // foreach (var block in blocks)
        // {
        //     if (block.Value.Type == TileType.Player)
        //         Logger.Log($"Player pos : {block.Key}");
        // }
        while (true)
        {
            curPos += direction;
            if (board.ContainsKey(curPos))
            {
                if (!blocks.TryGetValue(curPos, out BlockData block))
                    return true;
            }
            else
            {
                return false;
            }
        }
    }

    // 밀 수 있는 상태를 가정
    public void MoveBlocks(BlockData originBlock, Vector2Int curPos, Vector2Int direction)
    {
        if (blocks.TryGetValue(curPos, out BlockData block) && block == originBlock)
            blocks.Remove(curPos);
        else Logger.LogWarning($"내 자리에 내가 없어요; {curPos}, {block}, {direction}, origin:{originBlock}");
        Vector2Int tempPos = curPos + direction;
        BlockData prevBlock = originBlock;
        int pushCnt = 1;

        // 1회 push
        while (true)
        {
            if (board.ContainsKey(tempPos) && blocks.TryGetValue(tempPos, out block) &&
                block.slidingDirection != direction)
            {
                blocks[tempPos] = prevBlock;
                block.MoveAnimation(tempPos + direction);
                pushCnt++;
            }
            else
            {
                blocks[tempPos] = prevBlock;
                break;
            }
            tempPos += direction;
            prevBlock = block;
        }
            
        // ice타일 검사
        tempPos = curPos;
        for (int i = 0; i < pushCnt; i++)
        {
            tempPos += direction;
            if (board[tempPos].Type == TileType.Ice)
            {
                block = blocks[tempPos];
                if (block.slidingDirection != direction)
                {
                    if (block.Type == TileType.Player)
                        PlayerController.Instance.ClearMoveQueue();
                    block.StartSliding(tempPos, direction);
                }
            }
        }
    }

    // private void BlockEnterIce(Vector2Int curPos, Vector2Int direction, bool isPlayer)
    // {
    //     if (isPlayer)// 예약된 이동 취소
    //         PlayerController.Instance.ClearMoveQueue();
    //     Vector2Int originPos = curPos;
    //     BlockData originBlock = blocks[curPos];
    //     originBlock.isSliding = true;

    //     // curPos는 타일 끝까지 이동 후, 블록 수만큼 다시 뒤로 후퇴함. 이 떄 curPos는 플레이어가 위치할 최종 위치
    //     int tileCnt = 0, blockCnt = 0;
    //     curPos += direction;
    //     while (board.ContainsKey(curPos))
    //     {
    //         tileCnt++;
    //         if (blocks.TryGetValue(curPos, out BlockData block))
    //         {
    //             blockCnt++;
    //             block.isSliding = true;
    //         }
    //         curPos += direction;
    //     }
    //     curPos -= direction * (blockCnt + 1);

    //     if (isPlayer)
    //         PlayerController.Instance.TryMoveTo(curPos.x, curPos.y, true);
    //     else
    //         originBlock.StartSliding(originPos, direction, tileCnt - blockCnt);
    // }
}