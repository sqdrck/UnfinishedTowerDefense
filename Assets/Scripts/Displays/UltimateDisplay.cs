using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UltimateDisplay : EntityDisplayComponent
{
    [SerializeField]
    private Color defaultTowerUltimateColor;
    [SerializeField]
    private Color accessibleTowerUltimateColor;
    [SerializeField]
    private SpriteRenderer towerFillSr;
    private Material fillMat;

    public override void OnInit(Entity entity)
    {
        fillMat = new Material(Shader.Find("Custom/RadialFill"));
        towerFillSr.material = fillMat;
        fillMat.SetFloat("_Arc1", 0);
    }

    public override void OnFixedUpdate(Entity entity)
    {
        bool ultAccessible = entity.TargetIndex != -1 && entity.UltimateAccumulated >= 1;
        towerFillSr.color = ultAccessible ? accessibleTowerUltimateColor : defaultTowerUltimateColor;
        fillMat.SetFloat("_Arc2", (1 - entity.UltimateAccumulated) * 360);
    }
}
