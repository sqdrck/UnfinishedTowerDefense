using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class SelfDestroyingPopup : MonoBehaviour
{
    private CanvasGroup group;
    private Tween tween;
    private void OnEnable()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 1;
        tween.Kill();
        tween = group.DOFade(0, 0.5f).SetDelay(1).OnComplete(() => gameObject.SetActive(false));
    }

}
