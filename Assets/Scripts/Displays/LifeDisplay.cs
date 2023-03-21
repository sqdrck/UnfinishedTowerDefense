using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class LifeDisplay : EntityDisplayComponent
{
    [SerializeField]
    private Transform lifeFg;

    public override void OnInit(Entity entity)
    {

    }

    public override void OnFixedUpdate(Entity entity)
    {
        if (entity.LifeDuration == 0) return;
        var mappedX = math.remap(0, entity.LifeDuration, -0.5f, 0, entity.LifeT);
        var mappedXScale = math.remap(0, entity.LifeDuration, 0, 1, entity.LifeT);
        lifeFg.localScale = new Vector3(mappedXScale, lifeFg.localScale.y, lifeFg.localScale.z);
        lifeFg.localPosition = new Vector3(mappedX, lifeFg.localPosition.y, lifeFg.localPosition.z);
    }
}
