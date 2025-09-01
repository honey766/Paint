using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.Rendering;

public class PlayerController : SingletonBehaviour<PlayerController>
{
    [Tooltip("타일 한 칸을 건너는 데 걸리는 시간")]
    public float moveTimePerTile;

    private TileColor myColor;
    private int curI, curJ; // 현재 플레이어가 위치한 좌표
    private int destI, destJ; // 최종 이동 장소로서 예약된 좌표 (플레이어 이동 중에는 현재좌표 != 도착좌표)
    private Queue<Vector2Int> moveQueue = new Queue<Vector2Int>();
    private bool isMoving; // 현재 이동 중인지

    // 캐싱
    private WaitForSeconds waitMoveTimeHalf;


    private void Start()
    {
        waitMoveTimeHalf = new WaitForSeconds(moveTimePerTile / 2f);
        InitPlayer(1, 2);
    }

    public void InitPlayer(int i, int j)
    {
        myColor = TileColor.None;
        curI = destI = i;
        curJ = destJ = j;
        transform.position = Board.Instance.GetTilePos(i, j);
    }

    // (i, j) 좌표로 이동 시도
    public void TryMoveTo(int i, int j)
    {
        if (!CanMoveTo(i, j)) return;

        // 같은 행, 열 중 어디인지에 따라 분기
        if (destI == i) // j 방향(가로 이동)
        {
            int step = (j > destJ) ? 1 : -1;
            for (int col = destJ + step; col != j + step; col += step)
                moveQueue.Enqueue(new Vector2Int(destI, col));
        }
        else if (destJ == j) // i 방향(세로 이동)
        {
            int step = (i > destI) ? 1 : -1;
            for (int row = destI + step; row != i + step; row += step)
                moveQueue.Enqueue(new Vector2Int(row, destJ));
        }

        destI = i;
        destJ = j;

        // 이동 중이 아니라면 이동 시작
        if (!isMoving) StartCoroutine(Move());
    }

    // 플레이어가 (destI, destJ) => (i, j)로 갈 수 있는지 검사
    private bool CanMoveTo(int i, int j)
    {
        if (destI != i && destJ != j) return false; // 대각선 방향은 이동X

        // 플레이어와 같은 색의 타일은 이동 불가능
        TileColor playerColor = myColor;
        TileColor changeColor;

        // 같은 행, 열 중 어디인지에 따라 분기
        if (destI == i) // j 방향(가로 이동)
        {
            int step = (j > destJ) ? 1 : -1;
            for (int col = destJ + step; col != j + step; col += step)
            { 
                changeColor = Board.Instance.IsChangeTile(i, col);
                if (changeColor != TileColor.None) // 색 바꾸는 타일
                    playerColor = changeColor;
                else if (playerColor != TileColor.None && Board.Instance.HasColor(i, col, playerColor))
                    return false;
            }
        }
        else if (destJ == j) // i 방향(세로 이동)
        {
            int step = (i > destI) ? 1 : -1;
            for (int row = destI + step; row != i + step; row += step)
            {
                changeColor = Board.Instance.IsChangeTile(row, j);
                if (changeColor != TileColor.None) // 색 바꾸는 타일
                    playerColor = changeColor;
                else if (playerColor != TileColor.None && Board.Instance.HasColor(row, j, playerColor))
                    return false;
            }
        }

        return true;
    }

    public void ChangePlayerColor(TileColor color)
    {
        myColor = color;
    }

    private IEnumerator Move()
    {
        isMoving = true;
        TileColor changeColor;

        while (moveQueue.Count > 0)
        {
            Vector2Int nextPos = moveQueue.Dequeue(); // 큐에서 하나 꺼내기
            Vector2 nextRealPos = Board.Instance.GetTilePos(nextPos.x, nextPos.y);
            transform.DOMove(nextRealPos, moveTimePerTile).SetEase(Ease.Linear);
            yield return waitMoveTimeHalf;

            // 타일 색칠 or 플레이어 색 변화
            changeColor = Board.Instance.IsChangeTile(nextPos.x, nextPos.y);
            if (changeColor != TileColor.None) // 색 바꾸는 타일
                myColor = changeColor;
            else if (myColor != TileColor.None) // 타일 색칠
                Board.Instance.ColorTile(nextPos.x, nextPos.y, myColor);
            yield return waitMoveTimeHalf;
        }

        isMoving = false;
    }
}
