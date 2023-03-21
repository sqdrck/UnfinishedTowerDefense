using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using DG.Tweening;

public class DeathAnimationDisplay : MonoBehaviour
{
    public ParticleSystem Ps;
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
    public float2 Pos
    {
        set
        {
            transform.position = new Vector3(value.x, value.y);
        }
    }
    public void OnInit(float2 pos)
    {
        Pos = pos;
        Ps.Play();
        float t = 0;
        DOTween.To(x => t = x, 0, 1, Ps.main.startLifetime.constant).OnComplete(() => { IsActive = false; });
    }
}
