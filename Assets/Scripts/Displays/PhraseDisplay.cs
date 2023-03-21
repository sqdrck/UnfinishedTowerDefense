using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PhraseDisplay : EntityDisplayComponent
{
    public float PhraseTime;
    public GameObject PhraseGo;
    public override void OnFixedUpdate(Entity entity)
    {
        if (entity.NeedToSayPhrase)
        {
            entity.NeedToSayPhrase = false;
            PhraseGo.SetActive(true);
            Invoke(nameof(Disable), PhraseTime);
        }
    }

    public void Disable()
    {
        PhraseGo.SetActive(false);
    }

    public override void OnInit(Entity entity)
    {

    }
}
