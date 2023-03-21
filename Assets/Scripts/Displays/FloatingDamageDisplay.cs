using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingDamageDisplay : MonoBehaviour
{
    private bool isActive;
    public bool IsActive
    {
        get => isActive;
        set
        {
            gameObject.SetActive(value);
            isActive = value;
        }
    }
    [SerializeField]
    private Color defaultColor;
    [SerializeField]
    private Color critColor;
    [SerializeField]
    private Color ultimateColor;
    [SerializeField]
    private TextMeshPro text;
    [Range(0, 1)]
    public float OpacityPhaseStart = 0.1f;
    [Range(0, 1)]
    public float ScalePhaseStart = 0.3f;
    public float Lifetime = 0.7f;
    public float T;
    public Vector2 TargetPos;
    public Vector2 Pos;
    public Vector3 initialScale;
    public Vector2 spawnOffset;

    public void OnInit(Vector2 pos, int damage, bool crit, bool ultimate)
    {
        Pos = pos + spawnOffset;
        transform.position = Pos;
        TargetPos = Pos + Vector2.up * 0.3f;
        text.text = Utils.GetHpText(damage);
        text.alpha = 1;
        transform.localScale = crit ? Vector3.one * 1.5f : ultimate ? Vector3.one * 2f : Vector3.one;
        initialScale = transform.localScale;
        T = 0;

        text.color = crit ? critColor : ultimate ? ultimateColor : defaultColor;
    }

    private void Update()
    {
        var diff = ((Vector3)Pos - transform.position);
        transform.position += diff * Time.deltaTime * EntityDisplay.InterpolationConstant;
    }

    public void OnFixedUpdate()
    {
        T += Time.fixedDeltaTime * (1 / Lifetime);
        if (T > 1)
        {
            T = 1;
            IsActive = false;
        }
        float scaleT = (T - ScalePhaseStart) * (1f / (1 - ScalePhaseStart));
        float opacityT = (T - OpacityPhaseStart) * (1f / (1 - OpacityPhaseStart));
        var easedScaleT = EasingFunctions.OutCirc(1 - scaleT) * initialScale.x;
        var easedOpacityT = EasingFunctions.Linear(1 - opacityT);
        if (T > 0 && T <= 1)
        {
            Pos = Vector3.Lerp(Pos, TargetPos, EasingFunctions.OutCirc(T));
            if (T > ScalePhaseStart)
            {
                transform.localScale = new Vector3(easedScaleT, easedScaleT, easedScaleT);
            }
            if (T > OpacityPhaseStart)
            {
                text.alpha = easedOpacityT;
            }
        }
    }
}
