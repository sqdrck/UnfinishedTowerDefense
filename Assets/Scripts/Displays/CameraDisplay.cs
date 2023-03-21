using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Camera))]
public class CameraDisplay : MonoBehaviour
{
    public Camera Camera;
    [SerializeField, FoldoutGroup("Shake config")]
    private bool enableShake;
    [Min(0), SerializeField, ShowIf(nameof(enableShake)), FoldoutGroup("Shake config")]
    private float shakeMagnitude;
    [Min(0), SerializeField, ShowIf(nameof(enableShake)), FoldoutGroup("Shake config")]
    private float shakeRoughness;
    [Min(0), SerializeField, ShowIf(nameof(enableShake)), FoldoutGroup("Shake config")]
    private float shakeFadeInTime;
    [Min(0), SerializeField, ShowIf(nameof(enableShake)), FoldoutGroup("Shake config")]
    private float shakeFadeOutTime;

    private Vector3 restPositionOffset = new Vector3(0, 0, 0);
    private float magnitude;
    private float roughness;
    private float fadeOutDuration, fadeInDuration;
    private bool sustain;
    private float currentFadeTime;
    private float tick = 0;
    private Vector3 amt;
    private Vector3 posAddShake, rotAddShake;

    public void Shake(float magnitude, float roughness, float fadeInTime, float fadeOutTime)
    {
        if (!enableShake) return;
        this.magnitude = magnitude;
        this.roughness = roughness;
        fadeInDuration = fadeInTime;
        fadeOutDuration = fadeOutTime;
        if (fadeInTime > 0)
        {
            sustain = true;
            currentFadeTime = 0;
        }
        else
        {
            sustain = false;
            currentFadeTime = 1;
        }

        tick = Random.Range(-100, 100);
    }
    public void Shake()
    {
        Shake(shakeMagnitude, shakeRoughness, shakeFadeInTime, shakeFadeOutTime);
    }

    public void OnInit(GameState gameState)
    {
        restPositionOffset = transform.position;
        Camera = GetComponent<Camera>();
    }

    public void OnFixedUpdate(GameState gameState)
    {

    }

    private void Update()
    {
        if (enableShake)
        {
            posAddShake = Vector3.zero;
            if (IsShakeActive)
            {
                posAddShake += MultiplyVectors(UpdateShake(), new Vector3(0.15f, 0.15f, 0.15f));
            }
            transform.localPosition = posAddShake + restPositionOffset;
        }
    }
    private static Vector3 SmoothDampEuler(Vector3 current, Vector3 target, ref Vector3 velocity, float smoothTime)
    {
        Vector3 v;

        v.x = Mathf.SmoothDampAngle(current.x, target.x, ref velocity.x, smoothTime);
        v.y = Mathf.SmoothDampAngle(current.y, target.y, ref velocity.y, smoothTime);
        v.z = Mathf.SmoothDampAngle(current.z, target.z, ref velocity.z, smoothTime);

        return v;
    }

    private Vector3 UpdateShake()
    {
        amt.x = Mathf.PerlinNoise(tick, 0) - 0.5f;
        amt.y = Mathf.PerlinNoise(0, tick) - 0.5f;
        amt.z = Mathf.PerlinNoise(tick, tick) - 0.5f;

        if (fadeInDuration > 0 && sustain)
        {
            if (currentFadeTime < 1)
                currentFadeTime += Time.deltaTime / fadeInDuration;
            else if (fadeOutDuration > 0)
                sustain = false;
        }

        if (!sustain)
            currentFadeTime -= Time.deltaTime / fadeOutDuration;

        if (sustain)
            tick += Time.deltaTime * roughness;
        else
            tick += Time.deltaTime * roughness * currentFadeTime;

        return amt * magnitude * currentFadeTime;
    }
    private void StartFadeOut(float fadeOutTime)
    {
        if (fadeOutTime == 0)
            currentFadeTime = 0;

        fadeOutDuration = fadeOutTime;
        fadeInDuration = 0;
        sustain = false;
    }
    private void StartFadeIn(float fadeInTime)
    {
        if (fadeInTime == 0)
            currentFadeTime = 1;

        fadeInDuration = fadeInTime;
        fadeOutDuration = 0;
        sustain = true;
    }
    private static Vector3 MultiplyVectors(Vector3 v, Vector3 w)
    {
        v.x *= w.x;
        v.y *= w.y;
        v.z *= w.z;

        return v;
    }

    public float NormalizedFadeTime
    { get { return currentFadeTime; } }

    public bool IsShaking
    { get { return currentFadeTime > 0 || sustain; } }

    public bool IsFadingOut
    { get { return !sustain && currentFadeTime > 0; } }

    public bool IsFadingIn
    { get { return currentFadeTime < 1 && sustain && fadeInDuration > 0; } }

    public bool IsShakeActive => IsFadingIn || IsFadingOut || IsShaking;
}
