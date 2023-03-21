using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffRadiusDisplay : EntityDisplayComponent
{
    [SerializeField]
    private Transform buffRadius;

    public override void OnInit(Entity entity)
    {
        buffRadius.localScale = new Vector3(entity.BuffCastProperties[0].CastRadius * 2, entity.BuffCastProperties[0].CastRadius * 2, buffRadius.localScale.z);
    }

    public override void OnFixedUpdate(Entity entity)
    {
    }
}
