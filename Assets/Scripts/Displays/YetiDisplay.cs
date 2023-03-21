using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Spine.Unity;
using Spine;
using Sirenix.OdinInspector;
using DG.Tweening;

public class YetiDisplay : EntityDisplayComponent
{
    public SpineManualUpdate spineUpdater;
    public GameObject Snowball;
    public Transform SnowballOrigin;
    public bool PlayAttackAnimation;
    public SkeletonAnimation FaceSa;
    public float SnowBallScale = 0.09f;
    public Spine.AnimationState FaceAs;
    public TrackEntry CurrentFaceTrackEntry;
    private float2 AoePos;
    public float SnowballFlyTime = 1;
    [SpineAnimation(dataField: nameof(FaceSa))]
    public string FaceIdle;
    [SpineAnimation(dataField: nameof(FaceSa))]
    public string FaceAttack;
    [SpineAnimation(dataField: nameof(FaceSa))]
    public string FaceTap;
    public float SnowballDelay = 0.2f;
    public bool PlayTapAnimation;

    public Bone FaceHandBone;
    [SpineBone(dataField: nameof(FaceSa))]
    public string FaceHandBoneName;

    public override void OnInit(Entity entity)
    {
        FaceAs = FaceSa.AnimationState;
        FaceHandBone = FaceSa.Skeleton.FindBone(FaceHandBoneName);
    }

    public override void OnUpdate(Entity entity)
    {
        if (SnowballSeq != null)
        {
            SnowballSeq.ManualUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }
        spineUpdater.OnUpdate();
        if (PlayAttackAnimation)
        {
            FlySnowballToPos(new Vector3(AoePos.x, AoePos.y, 0));
            FaceAs.SetAnimation(0, FaceAttack, false);
            FaceAs.AddAnimation(0, FaceIdle, true, 0);
            PlayAttackAnimation = false;
        }
        else if (PlayTapAnimation)
        {
            FaceAs.SetAnimation(0, FaceTap, false);
            FaceAs.AddAnimation(0, FaceIdle, true, 0);
            PlayTapAnimation = false;
        }
    }

    public DG.Tweening.Sequence SnowballSeq;
    public void FlySnowballToPos(Vector3 pos)
    {
        Snowball.transform.position = FaceHandBone.GetWorldPosition(FaceSa.transform);
        Snowball.SetActive(true);
        Snowball.transform.localScale = Vector3.zero;
        Snowball.transform.DOScale(new Vector3(SnowBallScale, SnowBallScale, SnowBallScale), SnowballDelay);
        Ease xEase = Ease.OutQuad;
        Ease yEase = Ease.InQuad;
        float flyTime = SnowballFlyTime - SnowballDelay;
        if (pos.y > FaceSa.transform.position.y)
        {
            xEase = Ease.InQuad;
            yEase = Ease.OutQuad;
        }
        Vector3 flyScale = Vector3.one * (SnowBallScale * 1.7f);
        SnowballSeq = DOTween.Sequence().SetUpdate(UpdateType.Manual);
        SnowballSeq.Append(Snowball.transform.DOScale(flyScale, flyTime * 0.5f).SetEase(Ease.OutQuad));
        SnowballSeq.Append(Snowball.transform.DOScale(SnowBallScale, flyTime * 0.5f).SetEase(Ease.InQuad));
        SnowballSeq.Insert(0, Snowball.transform.DOMoveX(pos.x, flyTime)
            .SetDelay(SnowballDelay).SetEase(xEase));
        SnowballSeq.Insert(0, Snowball.transform.DOMoveY(pos.y, flyTime)
            .SetDelay(SnowballDelay).SetEase(yEase));
        SnowballSeq.OnComplete(() =>
        {
            Snowball.SetActive(false);
        });
    }

    public override void OnTap(Entity entity)
    {
        PlayTapAnimation = true;
    }

    public override void OnFixedUpdate(Entity entity)
    {
        bool udpatedAoePos = !entity.AoePos.Equals(AoePos);
        if (udpatedAoePos)
        {
            PlayAttackAnimation = true;
        }
        AoePos = entity.AoePos;
    }
}
