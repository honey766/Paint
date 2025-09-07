using UnityEngine;

public class EditorTileDrawer : MonoBehaviour
{
    [SerializeField] GameObject startText;
    [SerializeField] GameObject tile;
    [SerializeField] Transform tileParent;
    [SerializeField] GameObject color1Paint, color2Paint;
    [SerializeField] Transform paintParent;
    [SerializeField] Color gray, color1, color2, color12;

    public GameObject AddTile(Vector2Int pos)
    {
        return Instantiate(tile, (Vector2)pos, Quaternion.identity, tileParent);
    }

    public void ChangeTileColor(EditorTileInfo tileInfo)
    {
        SpriteRenderer spriter = tileInfo.tile.GetComponent<SpriteRenderer>();
        switch (tileInfo.color)
        {
            case TileColor.None:
                spriter.color = gray;
                break;
            case TileColor.Color1:
                spriter.color = color1;
                break;
            case TileColor.Color2:
                spriter.color = color2;
                break;
            case TileColor.Color1 | TileColor.Color2:
                spriter.color = color12;
                break;
        }
    }

    public void DeleteTile(GameObject tile)
    {
        Destroy(tile);
    }

    public void SetStartText(Vector2Int pos)
    {
        if (!startText.activeSelf)
            startText.SetActive(true);
        startText.transform.position = (Vector2)pos;
    }

    public void DeleteStartText()
    {
        startText.SetActive(false);
    }

    public GameObject AddPaint(Vector2Int pos, bool isColor1)
    {
        if (isColor1)
            return Instantiate(color1Paint, (Vector2)pos, Quaternion.identity, paintParent);
        else
            return Instantiate(color2Paint, (Vector2)pos, Quaternion.identity, paintParent);
    }
}
