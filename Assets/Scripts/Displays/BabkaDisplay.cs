using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Spine.Unity;
using Spine;
using Sirenix.OdinInspector;

public class BabkaDisplay : EntityDisplayComponent
{
    public SpineManualUpdate spineUpdater;
    public bool IsInIdle;
    public GameObject FaceGo;
    public GameObject BackGo;
    public SkeletonAnimation FaceSa;
    public SkeletonAnimation BackSa;
    public Spine.AnimationState FaceAs;
    public Spine.AnimationState BackAs;

    public ParticleSystem MuzzleClouds;
    public ParticleSystem MuzzleFlash;
    public ParticleSystem MuzzleBullets;
    public ParticleSystem BulletsHit;

    public float MuzzleOffsetMult = 1;
    public float BulletsSizePerUnit = 25;
    public float BulletsXRelativeToY = 0.01f;

    [SpineBone(dataField: nameof(FaceSa))]
    public string FaceTargetBoneName;
    [SpineBone(dataField: nameof(BackSa))]
    public string BackTargetBoneName;
    [SpineBone(dataField: nameof(FaceSa))]
    public string FaceMuzzleBoneName;
    [SpineBone(dataField: nameof(BackSa))]
    public string BackMuzzleBoneName;
    [SpineBone(dataField: nameof(FaceSa))]
    public string FaceRootMuzzleBoneName;
    [SpineBone(dataField: nameof(BackSa))]
    public string BackRootMuzzleBoneName;

    public Bone FaceTargetBone;
    public Bone BackTargetBone;
    public Bone FaceRootMuzzleBone;
    public Bone FaceMuzzleBone;
    public Bone BackMuzzleBone;
    public Bone BackRootMuzzleBone;

    [SpineAnimation(dataField: nameof(BackSa))]
    public string BackIdle;
    [SpineAnimation(dataField: nameof(BackSa))]
    public string BackShooting;
    [SpineAnimation(dataField: nameof(FaceSa))]
    public string FaceIdle;
    [SpineAnimation(dataField: nameof(FaceSa))]
    public string FaceShooting;
    [SpineAnimation(dataField: nameof(FaceSa))]
    public string FaceTap;

    public bool PlayTapAnimation;

    public TrackEntry CurrentFaceTrackEntry;
    public TrackEntry CurrentBackTrackEntry;

    public float initialAttackDuration = 2.33333f;
    public float initialAttackDelay = 2;
    public float initialAttackActivePhaseStart = 0.5f;
    public float initialAttackActivePhaseDuration = 0.5f;
    [ShowInInspector, ReadOnly]
    private float attackDelay;
    [ShowInInspector, ReadOnly]
    private float attackActivePhaseStart;
    [ShowInInspector, ReadOnly]
    private float attackActivePhaseDuration;
    [ShowInInspector, ReadOnly]
    private float timeScale;

    public Vector3 targetPos;
    public Vector3 nextTargetPos;
    public float sAngle;
    public float lerpedSAngle;
    public bool HasTarget;
    public bool CurrentShootingState;
    public bool PrevShootingState;

    public bool UpdateParticles = false;

    public void StopParticles()
    {
        MuzzleFlash.Stop();
        MuzzleClouds.Stop();
        MuzzleBullets.Stop();
        BulletsHit.Stop();
    }

    public void StartParticles()
    {
        MuzzleClouds.Play();
        MuzzleFlash.Play();
        MuzzleBullets.Play();
        BulletsHit.Play();
    }

    public override void OnTap(Entity entity)
    {
        PlayTapAnimation = true;
    }

    public override void OnInit(Entity entity)
    {
        BackAs = BackSa.AnimationState;
        FaceAs = FaceSa.AnimationState;
        FaceTargetBone = FaceSa.Skeleton.FindBone(FaceTargetBoneName);
        BackTargetBone = BackSa.Skeleton.FindBone(BackTargetBoneName);
        FaceMuzzleBone = FaceSa.Skeleton.FindBone(FaceMuzzleBoneName);
        BackMuzzleBone = BackSa.Skeleton.FindBone(BackMuzzleBoneName);
        BackRootMuzzleBone = BackSa.Skeleton.FindBone(BackRootMuzzleBoneName);
        FaceRootMuzzleBone = FaceSa.Skeleton.FindBone(FaceRootMuzzleBoneName);

        timeScale = initialAttackDuration / entity.CooldownDuration;
        attackDelay = initialAttackDelay;
        attackActivePhaseStart = initialAttackActivePhaseStart;
        attackActivePhaseDuration = initialAttackActivePhaseDuration;

        attackDelay /= timeScale;
        if (timeScale > 1)
        {
            BackSa.timeScale = timeScale;
            FaceSa.timeScale = timeScale;
            attackActivePhaseStart /= timeScale;
            attackActivePhaseDuration /= timeScale;
        }
        else
        {
            timeScale = 1;
        }
        timeScale = 1;
    }

    public override void OnUpdate(Entity entity)
    {
        spineUpdater.OnUpdate();
        MuzzleClouds.Simulate(Time.deltaTime, true, false, false);
        MuzzleFlash.Simulate(Time.deltaTime, true, false, false);
        MuzzleBullets.Simulate(Time.deltaTime, true, false, false);
        BulletsHit.Simulate(Time.deltaTime, true, false, false);

        if (PlayTapAnimation)
        {
            PlayTapAnimation = false;
            FaceAs.SetAnimation(0, FaceTap, false);
            FaceAs.AddAnimation(0, FaceIdle, true, 0);
        }
        if (HasTarget)
        {
            var angleDiff = (sAngle - lerpedSAngle);
            lerpedSAngle += angleDiff * Time.deltaTime * EntityDisplay.InterpolationConstant * 2;
            FaceSa.timeScale = timeScale;
            BackSa.timeScale = timeScale;
            IsInIdle = false;

            var targetDiff = nextTargetPos - targetPos;
            targetPos += targetDiff * Time.deltaTime * EntityDisplay.InterpolationConstant * 2;
            var skeletonSpacePoint = FaceGo.transform.InverseTransformPoint(targetPos);
            skeletonSpacePoint.x *= FaceSa.Skeleton.ScaleX;
            skeletonSpacePoint.y *= FaceSa.Skeleton.ScaleY;

            FaceSa.Skeleton.ScaleX = lerpedSAngle > 0 ? 1f : -1f;
            BackSa.Skeleton.ScaleX = lerpedSAngle > 0 ? 1f : -1f;
            var angle = math.abs(lerpedSAngle);
            bool reaplyAnimations = CurrentShootingState != PrevShootingState;

            if (angle < 90)
            {
                BackGo.SetActive(true);
                var pos = BackRootMuzzleBone.GetWorldPosition(BackSa.transform) + (targetPos - BackRootMuzzleBone.GetWorldPosition(BackSa.transform)).normalized * MuzzleOffsetMult;
                MuzzleFlash.transform.position = pos;
                MuzzleClouds.transform.position = pos;
                MuzzleBullets.transform.position = pos;
                BackTargetBone.SetLocalPosition(skeletonSpacePoint);

                if (CurrentBackTrackEntry == null || reaplyAnimations)
                {
                    if (CurrentShootingState)
                    {
                        CurrentBackTrackEntry = BackAs.SetAnimation(0, BackShooting, true);
                        StartParticles();
                    }
                    else
                    {
                        CurrentBackTrackEntry = BackAs.SetAnimation(0, BackIdle, true);
                        StopParticles();
                    }
                    if (CurrentFaceTrackEntry != null)
                    {
                        CurrentBackTrackEntry.TrackTime = CurrentFaceTrackEntry.AnimationTime;
                        CurrentFaceTrackEntry = null;
                    }
                }

                FaceGo.SetActive(false);
            }
            else
            {
                FaceGo.SetActive(true);
                var pos = FaceRootMuzzleBone.GetWorldPosition(FaceSa.transform) + (targetPos - FaceRootMuzzleBone.GetWorldPosition(FaceSa.transform)).normalized * MuzzleOffsetMult;
                MuzzleFlash.transform.position = pos;
                MuzzleClouds.transform.position = pos;
                MuzzleBullets.transform.position = pos;
                FaceTargetBone.SetLocalPosition(skeletonSpacePoint);

                if (CurrentFaceTrackEntry == null || reaplyAnimations)
                {
                    if (CurrentShootingState)
                    {
                        CurrentFaceTrackEntry = FaceAs.SetAnimation(0, FaceShooting, true);
                        StartParticles();
                    }
                    else
                    {
                        CurrentFaceTrackEntry = FaceAs.SetAnimation(0, FaceIdle, true);
                        StopParticles();
                    }
                    if (CurrentBackTrackEntry != null)
                    {
                        CurrentFaceTrackEntry.TrackTime = CurrentBackTrackEntry.AnimationTime;
                        CurrentBackTrackEntry = null;
                    }
                }

                BackGo.SetActive(false);
            }
            PrevShootingState = CurrentShootingState;
            var babkaPos = angle >= 90 ? FaceRootMuzzleBone.GetWorldPosition(FaceSa.transform) : BackRootMuzzleBone.GetWorldPosition(BackSa.transform);
            var dir = targetPos - babkaPos;
            var distance = targetPos - MuzzleBullets.transform.position;
            var main = MuzzleBullets.main;
            main.startSizeYMultiplier = distance.magnitude * BulletsSizePerUnit;
            main.startSizeXMultiplier = main.startSizeYMultiplier * BulletsXRelativeToY;

            var bulletsAngle = Vector2.SignedAngle(new Vector2(dir.x, dir.y), Vector2.down);
            //bulletsAngle *= FaceSa.Skeleton.ScaleX;
            MuzzleBullets.transform.eulerAngles = new Vector3(0, 0, -bulletsAngle);
            BulletsHit.transform.position = targetPos;
        }
        else
        {
            FaceGo.SetActive(true);
            BackGo.SetActive(false);
            CurrentBackTrackEntry = null;
            CurrentFaceTrackEntry = null;
            if (!IsInIdle)
            {
                var entry = FaceAs.SetAnimation(0, FaceIdle, true);
                FaceAs.SetEmptyAnimation(1, 0);
                BackAs.SetEmptyAnimation(1, 0);
                FaceSa.timeScale = 1;
                BackSa.timeScale = 1;
                entry.MixDuration = 0.5f;
                IsInIdle = true;
                if (!MuzzleClouds.isStopped || !MuzzleFlash.isStopped)
                {
                    StopParticles();
                }
            }
        }

    }

    public override void OnFixedUpdate(Entity entity)
    {
        CurrentShootingState = entity.TargetIndex != -1 && entity.Cooldown1T > 0 && entity.CooldownT <= 0;
        if (entity.TargetIndex != -1)
        {
            var dir = entity.TargetEntityPos - entity.Pos;
            var newSAngle = Vector2.SignedAngle(new Vector2(dir.x, dir.y), Vector2.up);
            nextTargetPos = new Vector3(entity.TargetEntityPos.x, entity.TargetEntityPos.y);
            if (!HasTarget)
            {
                targetPos = nextTargetPos;
            }
            if ((newSAngle > 0 && sAngle <= 0) || (newSAngle <= 0 && sAngle > 0) ||
                    (math.abs(newSAngle) >= 90 && math.abs(sAngle) < 90) || (math.abs(newSAngle) < 90 && math.abs(sAngle) >= 90))
            {
                Debug.Log("SetLerped");
                lerpedSAngle = newSAngle;
            }
            sAngle = newSAngle;
            HasTarget = true;
        }
        else
        {
            HasTarget = false;
        }
    }

    public void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if (BackRootMuzzleBone != null && math.abs(lerpedSAngle) < 90)
            {
                var pos = BackRootMuzzleBone.GetWorldPosition(BackSa.transform);
                Gizmos.DrawSphere(pos, 0.1f);
                Gizmos.DrawLine(pos, targetPos);
            }
            if (FaceRootMuzzleBone != null && math.abs(lerpedSAngle) >= 90)
            {
                var pos = FaceRootMuzzleBone.GetWorldPosition(FaceSa.transform);
                Gizmos.DrawSphere(pos, 0.1f);
                Gizmos.DrawLine(pos, targetPos);
            }
        }
    }
}
