using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageRadiusDisplay : EntityDisplayComponent
{
    [SerializeField]
    private Transform attackRange;

    public override void OnInit(Entity entity)
    {
        attackRange.localScale = new Vector3(entity.DamageRadius * 2, entity.DamageRadius * 2, attackRange.localScale.z);
    }

    public override void OnFixedUpdate(Entity entity)
    {

    }
}
