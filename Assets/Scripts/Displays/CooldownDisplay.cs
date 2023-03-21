using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class CooldownDisplay : EntityDisplayComponent
{
    [SerializeField]
    private Transform cooldownFg;

    public override void OnInit(Entity entity)
    {

    }

    public override void OnFixedUpdate(Entity entity)
    {
        if (entity.CooldownDuration == 0) return;
        var mappedX = math.remap(0, entity.CooldownDuration, 0, -0.5f, entity.CooldownT);
        var mappedXScale = math.remap(0, entity.CooldownDuration, 1, 0, entity.CooldownT);
        cooldownFg.localScale = new Vector3(mappedXScale, cooldownFg.localScale.y, cooldownFg.localScale.z);
        cooldownFg.localPosition = new Vector3(mappedX, cooldownFg.localPosition.y, cooldownFg.localPosition.z);
    }
}
