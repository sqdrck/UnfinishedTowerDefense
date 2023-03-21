using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorDisplay : EntityDisplayComponent
{
    public Transform sprite;
    public void SetColor(Color color)
    {
        sprite.GetComponent<SpriteRenderer>().color = color;
    }

    public override void OnInit(Entity entity)
    {
        SetColor(new Color(entity.Color.x, entity.Color.y, entity.Color.z, entity.Color.w));
    }

    public override void OnFixedUpdate(Entity entity)
    {

    }
}
