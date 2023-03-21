using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class HealthDisplay : EntityDisplayComponent
{
    private int lastHp;
    [SerializeField]
    private TMPro.TextMeshPro hpText;
    [SerializeField]
    private bool showHpIfFull;
    [SerializeField]
    private Transform healthFg;
    [SerializeField]
    private Transform healthBg;

    public override void OnInit(Entity entity)
    {
        if (healthFg != null)
        {
            healthFg.gameObject.SetActive(showHpIfFull);
        }
    }

    public override void OnFixedUpdate(Entity entity)
    {
        if (healthFg != null)
        {
            healthFg.gameObject.SetActive(showHpIfFull || entity.Hp != entity.MaxHp);
            healthBg.gameObject.SetActive(showHpIfFull || entity.Hp != entity.MaxHp);
            var mappedX = math.remap(0, entity.MaxHp, -0.5f, 0, entity.Hp);
            var mappedXScale = math.remap(0, entity.MaxHp, 0, 1, entity.Hp);
            healthFg.localScale = new Vector3(mappedXScale, healthFg.localScale.y, healthFg.localScale.z);
            healthFg.localPosition = new Vector3(mappedX, healthFg.localPosition.y, healthFg.localPosition.z);
        }
        if (entity.Hp != lastHp)
        {
            lastHp = entity.Hp;
            hpText.text = Utils.GetHpText(lastHp);
        }
    }
}
