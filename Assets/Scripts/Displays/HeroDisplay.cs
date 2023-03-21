using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class HeroDisplay : EntityDisplayComponent
{
    private bool shoot;
    private Vector3 bulletPos;
    private Vector3 spriteScale;
    private Vector3 targetPos;
    [SerializeField]
    private float bulletSpeed = 5;
    [SerializeField]
    private float bulletStartT = 0.9f;
    [SerializeField]
    private Transform sprite;
    [SerializeField]
    private Transform bullet;
    [SerializeField]
    private AnimationCurve scaleCurve;
    [SerializeField]
    private float animationDuration;
    private Vector3 initialSpriteScale;


    public override void OnInit(Entity entity)
    {
        initialSpriteScale = sprite.localScale;
        spriteScale = initialSpriteScale;
    }

    private void Update()
    {
        var diff = (spriteScale - sprite.localScale);
        sprite.localScale += diff * Time.deltaTime * EntityDisplay.InterpolationConstant;

        //diff = (bulletPos - bullet.position);
        //bullet.position += diff * Time.deltaTime * EntityDisplay.InterpolationConstant;
        bullet.position = bulletPos;

        Vector3 dir = targetPos - bullet.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);

        bullet.gameObject.SetActive(shoot);
    }


    public override void OnFixedUpdate(Entity entity)
    {
        // TODO(sqdrck): Fix interpolation issue when bullet disappears too early.
        if (entity.CooldownT <= animationDuration)
        {
            float bulletDistance = Vector2.Distance(entity.Pos, entity.TargetEntityPos);
            float bulletTime = bulletDistance / bulletSpeed;
            float addTime = 0;
            if (bulletTime + bulletStartT > 1)
            {
                addTime = bulletStartT + bulletTime - 1;
            }
            float animationT = math.remap(animationDuration + addTime, 0 + addTime, 0, 1, entity.CooldownT);
            if (animationT >= 0 && animationT <= 1)
            {
                float val = scaleCurve.Evaluate(animationT);
                spriteScale = new Vector3(val * initialSpriteScale.x, val * initialSpriteScale.y, 1);
            }

            if (animationT >= bulletStartT)
            {
                float bulletT = math.remap(bulletStartT, bulletStartT + bulletTime, 0, 1, animationT);
                bulletPos = Vector3.Lerp(new Vector3(entity.Pos.x, entity.Pos.y), new Vector3(entity.TargetEntityPos.x, entity.TargetEntityPos.y), bulletT);
                targetPos = new Vector3(entity.TargetEntityPos.x, entity.TargetEntityPos.y);

                if (!shoot)
                {
                    bullet.position = new Vector3(entity.Pos.x, entity.Pos.y);
                }
                shoot = bulletT < 1;
            }
        }
        else
        {
            spriteScale = initialSpriteScale;
            bulletPos = targetPos;
        }
    }
}
