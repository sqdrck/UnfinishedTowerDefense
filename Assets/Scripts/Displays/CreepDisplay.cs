using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreepDisplay : EntityDisplayComponent
{
    public Animator animator;
    public float animationSpeedRatio = 1;
    public string attackBoolName;
    public string verticalWalkBoolName;

    public override void OnInit(Entity entity)
    {

    }

    public override void OnFixedUpdate(Entity entity)
    {
        var s = entity.Speed / animationSpeedRatio;
        animator.speed = s;

        if (entity.Pos.x - transform.position.x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        if (Mathf.Abs(entity.Pos.y - transform.position.y) > 0.001f)
        {
            animator.SetBool(verticalWalkBoolName, true);
        }
        else
        {
            animator.SetBool(verticalWalkBoolName, false);
        }
    }
}
