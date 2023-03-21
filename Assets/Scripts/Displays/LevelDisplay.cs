using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDisplay : EntityDisplayComponent
{
    public TMPro.TextMeshPro LevelText;
    private int currLevel = -1;
    public override void OnInit(Entity entity)
    {
    }

    public override void OnFixedUpdate(Entity entity)
    {
        if (currLevel != entity.Level)
        {
            currLevel = entity.Level;
            LevelText.text = entity.Level.ToString();
        }
    }
}
