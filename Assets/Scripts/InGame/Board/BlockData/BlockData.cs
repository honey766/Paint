using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public abstract class BlockData : MonoBehaviour
{
    public abstract TileType Type { get; protected set; }
    public abstract bool HasMutableColor { get; protected set; }
    public abstract bool HasColor { get; protected set; }
    public abstract TileType Color { get; protected set; }
    public abstract bool IsTransparent { get; protected set; }

    [HideInInspector] public Vector2Int slidingDirection; // Ice블럭을 밟아서 슬라이드중일 때 방향 (default : zero)
    public Vector2Int curPos;
    protected float moveTime = 0.1f;
    protected Queue<Vector2Int> moveQueue = new Queue<Vector2Int>();
    protected WaitForSeconds moveWaitForSeconds;
    protected Coroutine moveCoroutine;
    protected SpriteRenderer mySpriter;

    protected virtual void Awake()
    {
        moveWaitForSeconds = new WaitForSeconds(moveTime);
        mySpriter = GetComponent<SpriteRenderer>();
    }

    public virtual void Initialize(BoardSOTileData boardSOTileData)
    {
        curPos = boardSOTileData.pos;
        slidingDirection = Vector2Int.zero;
        // AdjustAlphaBasedOnTileBelow(Board.Instance.board[boardSOTileData.pos]);
    }

    #region Color
    protected abstract void ApplyColorChange(TileType color);
    public void ChangeColor(TileType color)
    {
        if (!HasMutableColor)
        {
            Logger.LogWarning($"{Type} has not mutableColor but trying to change {Type}'s Color");
            return;
        }
        ApplyColorChange(color);
    }
    /// <summary>
    /// 블럭 밑에 있는 특수타일을 가리는 경우, 알파값을 조절해서 보이도록 함
    /// </summary>
    public virtual void AdjustAlphaBasedOnTileBelow(TileData tile)
    {
        if (IsTransparent || Type == TileType.Player)
            return;
        if (HaveToChangeAlpha(tile))
        {
            mySpriter.DOFade(0.85f, 0.15f);
        }
        else
        {
            mySpriter.DOFade(1f, 0.15f);
        }
    }
    protected bool HaveToChangeAlpha(TileData tile)
    {
        HashSet<TileType> alphaBasedTile = new HashSet<TileType>
        {
            TileType.Color1Paint, TileType.Color2Paint, TileType.ReversePaint, TileType.WhitePaint,
            TileType.Spray, TileType.DirectedSpray
        };
        return alphaBasedTile.Contains(tile.Type);
    }
    #endregion
    
    #region Move
    public void StartSliding(Vector2Int startPos, Vector2Int slidingDirection)
    {
        StartCoroutine(StartSlidingAfterSeconds(startPos, slidingDirection));
    }

    protected IEnumerator StartSlidingAfterSeconds(Vector2Int startPos, Vector2Int slidingDirection)
    {
        this.slidingDirection = slidingDirection;
        yield return moveWaitForSeconds;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(StartSlidingCoroutine(startPos, slidingDirection));
    }

    protected virtual IEnumerator StartSlidingCoroutine(Vector2Int curPos, Vector2Int slidingDirection)
    {
        while (BlockMoveController.Instance.CanMove(curPos, slidingDirection))
        {
            BlockMoveController.Instance.MoveBlocks(this, curPos, slidingDirection);
            curPos += slidingDirection;
            bool gameClear = MoveAnimation(curPos);
            if (gameClear) break;
            yield return moveWaitForSeconds;
        }
        this.slidingDirection = Vector2Int.zero;
    }

    public bool MoveAnimation(Vector2Int nextPos, bool isRedo = false)
    {
        AudioManager.Instance.PlaySfx(SfxType.PushBlock);
        Vector2 nextRealPos = Board.Instance.GetTilePos(nextPos.x, nextPos.y);
        transform.DOMove(nextRealPos, moveTime).SetEase(Ease.Linear);
        if (!isRedo)
            Board.Instance.board[nextPos].OnBlockEnter(this, nextPos, nextPos - curPos, Color, moveTime);
        curPos = nextPos;
        return Board.Instance.CheckGameClear();
    }
    #endregion
}