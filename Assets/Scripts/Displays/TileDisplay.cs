using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileDisplay : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public void OnInit(Tile tile)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        var alpha = spriteRenderer.color.a;
        HighlightDefault(tile.Type, alpha);
    }

    public void OnUpdate(Tile tile)
    {
        var alpha = spriteRenderer.color.a;
        spriteRenderer.color = TdUtils.GetTileColor(tile.Type);
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
    }

    public void HighlightDefault(TileType type, float alpha = 1)
    {
        spriteRenderer.color = TdUtils.GetTileColor(type);
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
    }

    public void HighlightColor(Color color)
    {
        spriteRenderer.color = color;
    }
}
