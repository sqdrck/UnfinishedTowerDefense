using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;
using Sirenix.OdinInspector;

// TODO(sqdrck): Cleanup.
public class CatDisplay : EntityDisplayComponent
{
    public SpineManualUpdate spineUpdater;
    public bool IsInIdle;
    public bool PlayTapAnimation;
    public GameObject CatFaceGo;
    public GameObject CatBackGo;
    public SkeletonAnimation CatFaceSa;
    public SkeletonAnimation CatBackSa;

    public ParticleSystem lPs;
    public ParticleSystem lPsR;
    public ParticleSystem lBlobPs;

    public Spine.AnimationState CatFaceAs;
    public Spine.AnimationState CatBackAs;

    [SpineBone(dataField: nameof(CatFaceSa))]
    public string CatFaceRightHandBoneName;
    [SpineBone(dataField: nameof(CatFaceSa))]
    public string CatFaceLeftHandBoneName;
    [SpineBone(dataField: nameof(CatBackSa))]
    public string CatBackRightHandBoneName;
    [SpineBone(dataField: nameof(CatBackSa))]
    public string CatBackLeftHandBoneName;

    [SpineAnimation(dataField: nameof(CatBackSa))]
    public string CatBackIdle;
    [SpineAnimation(dataField: nameof(CatBackSa))]
    public string CatBackShoot;
    [SpineAnimation(dataField: nameof(CatFaceSa))]
    public string CatFaceIdle;
    [SpineAnimation(dataField: nameof(CatFaceSa))]
    public string CatFaceShootMiddleUp;
    [SpineAnimation(dataField: nameof(CatFaceSa))]
    public string CatFaceShootMiddleDown;
    [SpineAnimation(dataField: nameof(CatFaceSa))]
    public string CatFaceShootDown;
    [SpineAnimation(dataField: nameof(CatFaceSa))]
    public string FaceTap;

    public TrackEntry CurrentFaceTrackEntry;
    public TrackEntry CurrentBackTrackEntry;

    public Bone faceLeftHandBone;
    public Bone faceRightHandBone;
    public Bone backLeftHandBone;
    public Bone backRightHandBone;
    public float initialAttackDuration = 2.33333f;
    public float initialAttackDelay = 2;
    public float initialAttackActivePhaseStart = 0.5f;
    public float initialAttackActivePhaseDuration = 0.5f;
    public float distMult = 0.2f;
    [ShowInInspector, ReadOnly]
    private float attackDelay;
    [ShowInInspector, ReadOnly]
    private float attackActivePhaseStart;
    [ShowInInspector, ReadOnly]
    private float attackActivePhaseDuration;
    [ShowInInspector, ReadOnly]
    private float timeScale;

    public override void OnInit(Entity entity)
    {
        faceLeftHandBone = CatFaceSa.Skeleton.FindBone(CatFaceLeftHandBoneName);
        faceRightHandBone = CatFaceSa.Skeleton.FindBone(CatFaceRightHandBoneName);
        backLeftHandBone = CatBackSa.Skeleton.FindBone(CatBackLeftHandBoneName);
        backRightHandBone = CatBackSa.Skeleton.FindBone(CatBackRightHandBoneName);
        CatBackAs = CatBackSa.AnimationState;
        CatFaceAs = CatFaceSa.AnimationState;

        timeScale = initialAttackDuration / entity.CooldownDuration;
        attackDelay = initialAttackDelay;
        attackActivePhaseStart = initialAttackActivePhaseStart;
        attackActivePhaseDuration = initialAttackActivePhaseDuration;

        attackDelay /= timeScale;
        if (timeScale > 1)
        {
            CatBackSa.timeScale = timeScale;
            CatFaceSa.timeScale = timeScale;
            attackActivePhaseStart /= timeScale;
            attackActivePhaseDuration /= timeScale;
        }
        else
        {
            timeScale = 1;
        }
    }

    public override void OnTap(Entity entity)
    {
        PlayTapAnimation = true;
    }

    public override void OnUpdate(Entity entity)
    {
        spineUpdater.OnUpdate();
        lPs.Simulate(Time.deltaTime, true, false, false);
        lBlobPs.Simulate(Time.deltaTime, true, false, false);
        if (PlayTapAnimation)
        {
            PlayTapAnimation = false;
            CatFaceAs.SetAnimation(0, FaceTap, false);
            CatFaceAs.AddAnimation(0, CatFaceIdle, true, 0);
        }
    }

    public void StartParticles()
    {
        lPs.Play();
        lBlobPs.Play();
    }

    public void StopParticles()
    {
        lPs.Stop();
        lBlobPs.Stop();
    }

    // TODO(sqdrck): Animation track time should be mapped directly to CooldownT?
    // TODO(sqdrck): Move to update.
    public override void OnFixedUpdate(Entity entity)
    {
        if (entity.TargetIndex != -1)
        {
            CatFaceSa.timeScale = timeScale;
            CatBackSa.timeScale = timeScale;
            CatBackGo.SetActive(true);
            IsInIdle = false;
            var dir = entity.TargetEntityPos - entity.Pos;
            var lPsDir = new Vector3(entity.TargetEntityPos.x, entity.TargetEntityPos.y) - lPs.transform.position;
            var lPsRdir = new Vector3(entity.TargetEntityPos.x, entity.TargetEntityPos.y) - lPsR.transform.position;
            var sAngle = Vector2.SignedAngle(new Vector2(dir.x, dir.y), Vector2.up);
            var sAngleLps = Vector2.SignedAngle(new Vector2(lPsDir.x, lPsDir.y), Vector2.up);
            var sAngleLpsR = Vector2.SignedAngle(new Vector2(lPsRdir.x, lPsRdir.y), Vector2.up);
            var lRotZ = -sAngleLps - 180 - 90 - 180;
            var lrRotZ = -sAngleLpsR - 180 - 90 - 180;
            lPs.transform.eulerAngles = new Vector3(0, 0, lRotZ);
            lPsR.transform.eulerAngles = new Vector3(0, 0, lrRotZ);

            var main = lPs.main;
            var rMain = lPsR.main;
            var dist = entity.Pos - entity.TargetEntityPos;
            main.startSizeXMultiplier = Mathf.Sqrt(dist.x * dist.x + dist.y * dist.y);
            rMain.startSizeXMultiplier = main.startSizeXMultiplier;

            CatFaceSa.Skeleton.ScaleX = sAngle > 0 ? 1f : -1f;
            CatBackSa.Skeleton.ScaleX = sAngle > 0 ? 1f : -1f;
            var angle = Unity.Mathematics.math.abs(sAngle);
            if (angle < 90)
            {
                var leftHandPos = backLeftHandBone.GetWorldPosition(CatBackGo.transform);
                var rightHandPos = backRightHandBone.GetWorldPosition(CatBackGo.transform);
                lPs.transform.position = leftHandPos;
                lPsR.transform.position = rightHandPos;
                if (CurrentBackTrackEntry == null || CurrentBackTrackEntry.IsComplete)
                {
                    CurrentBackTrackEntry = CatBackAs.SetAnimation(0, CatBackShoot, false);
                    if (CurrentFaceTrackEntry != null)
                    {
                        Debug.Log("Setting track time to " + CurrentFaceTrackEntry.AnimationTime);
                        CurrentBackTrackEntry.TrackTime = CurrentFaceTrackEntry.AnimationTime;
                        CurrentFaceTrackEntry = null;
                    }
                    else
                    {
                        CurrentBackTrackEntry.TrackTime = entity.CooldownDuration - entity.CooldownT - attackDelay;
                    }
                    CurrentBackTrackEntry.MixDuration = 0;
                }
                CatFaceGo.SetActive(false);
            }
            else
            {
                CatFaceGo.SetActive(true);
                var leftHandPos = faceLeftHandBone.GetWorldPosition(CatFaceGo.transform);
                var rightHandPos = faceRightHandBone.GetWorldPosition(CatFaceGo.transform);
                lPs.transform.position = leftHandPos;
                lPsR.transform.position = rightHandPos;
                if (CurrentFaceTrackEntry == null || CurrentFaceTrackEntry.IsComplete)
                {
                    if (angle > 140)
                    {
                        CurrentFaceTrackEntry = CatFaceAs.SetAnimation(0, CatFaceShootDown, false);
                    }
                    else if (angle > 95)
                    {
                        CurrentFaceTrackEntry = CatFaceAs.SetAnimation(0, CatFaceShootMiddleDown, false);
                    }
                    else
                    {
                        CurrentFaceTrackEntry = CatFaceAs.SetAnimation(0, CatFaceShootMiddleUp, false);
                    }
                    if (CurrentBackTrackEntry != null)
                    {
                        CurrentFaceTrackEntry.TrackTime = CurrentBackTrackEntry.AnimationTime;
                        CurrentBackTrackEntry = null;
                    }
                    else
                    {
                        CurrentFaceTrackEntry.TrackTime = entity.CooldownDuration - entity.CooldownT - attackDelay;
                    }
                    CurrentFaceTrackEntry.MixDuration = 0;
                }
                CatBackGo.SetActive(false);
            }

            if (CatBackGo.activeInHierarchy)
            {
                if (CurrentBackTrackEntry.AnimationTime > attackActivePhaseStart && CurrentBackTrackEntry.AnimationTime < attackActivePhaseStart + attackActivePhaseDuration)
                {
                    if (!lPs.isPlaying)
                    {
                        StartParticles();
                    }
                }
                else
                {
                    StopParticles();
                }
                //lPs.gameObject.SetActive(CurrentBackTrackEntry.AnimationTime > attackActivePhaseStart && CurrentBackTrackEntry.AnimationTime < attackActivePhaseStart + attackActivePhaseDuration);
            }
            else if (CatFaceGo.activeInHierarchy)
            {
                if (CurrentFaceTrackEntry.AnimationTime > attackActivePhaseStart && CurrentFaceTrackEntry.AnimationTime < attackActivePhaseStart + attackActivePhaseDuration)
                {
                    if (!lPs.isPlaying)
                    {
                        StartParticles();
                    }
                }
                else
                {
                    StopParticles();
                }
                //lPs.gameObject.SetActive(CurrentFaceTrackEntry.AnimationTime > attackActivePhaseStart && CurrentFaceTrackEntry.AnimationTime < attackActivePhaseStart + attackActivePhaseDuration);
            }
            lBlobPs.transform.position = new Vector3(entity.TargetEntityPos.x, entity.TargetEntityPos.y);
        }
        else
        {
            StopParticles();
            CatFaceGo.SetActive(true);
            CatBackGo.SetActive(false);
            CurrentBackTrackEntry = null;
            CurrentFaceTrackEntry = null;
            if (!IsInIdle)
            {
                var entry = CatFaceAs.SetAnimation(0, CatFaceIdle, true);
                CatFaceSa.timeScale = 1;
                CatBackSa.timeScale = 1;
                entry.MixDuration = 0.5f;
                IsInIdle = true;
            }
        }
    }
}
