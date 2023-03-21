using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBuffColorDisplay : EntityDisplayComponent
{
    public SpriteRenderer[] srs;
    public Color defaultColor = Color.white;

    public override void OnInit(Entity entity)
    {

    }

    public override void OnFixedUpdate(Entity entity)
    {
        bool foundBuff = false;
        Color buffColor = Color.white;
        int highestPriorityBuff = int.MinValue;

        foreach (var buff in entity.AppliedBuffsProperties)
        {
            if (buff.Value.IsApplyingColor && buff.Value.DisplayColorPriority > highestPriorityBuff)
            {
                highestPriorityBuff = buff.Value.DisplayColorPriority;
                buffColor = buff.Value.DisplayColor;
                foundBuff = true;
            }
        }

        if (foundBuff)
        {
            for (int i = 0; i < srs.Length; i++)
            {
                srs[i].color = buffColor;
            }
        }
        else
        {
            for (int i = 0; i < srs.Length; i++)
            {
                srs[i].color = defaultColor;
            }
        }
    }
}
