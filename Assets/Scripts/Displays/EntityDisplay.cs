using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;

public class EntityDisplay : MonoBehaviour
{
    public bool Interpolate;
    public const float InterpolationConstant = 16;
    private bool disable;
    private bool disableWithDelay;
    private float t = 0;
    private Vector3 nextPos;
    [SerializeField, ReadOnly]
    private EntityDisplayComponent[] components;

#if UNITY_EDITOR
    public Entity EntityPtr;
    public void OnValidate()
    {
        components = GetComponents<EntityDisplayComponent>();
    }
#endif
    public void OnInit(Entity entity)
    {
#if UNITY_EDITOR
        EntityPtr = entity;
#endif
        Interpolate = entity.InterpolateDisplay;
        foreach (var c in components)
        {
            c.OnInit(entity);
        }
        transform.position = new Vector3(entity.Pos.x, entity.Pos.y);
        nextPos = new Vector3(entity.Pos.x, entity.Pos.y);
    }

#if UNITY_EDITOR
    private void OnDestroy()
    {
        EntityPtr = null;
    }
#endif

    public void OnTap(Entity entity)
    {
        foreach (var c in components)
        {
            c.OnTap(entity);
        }
    }

    public void OnUpdate(Entity entity)
    {
        if (Interpolate)
        {
            var diff = (nextPos - transform.position);
            transform.position += diff * Time.deltaTime * InterpolationConstant;
        }
        else
        {
            transform.position = nextPos;
        }
        if (disable)
        {
            if (disableWithDelay)
            {
                t += Time.deltaTime;
                if (t > 0.1f)
                {
                    gameObject.SetActive(false);
                    t = 0;
                }
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            gameObject.SetActive(true);
        }

        foreach (var c in components)
        {
            c.OnUpdate(entity);
        }
    }

    public void OnFixedUpdate(Entity entity)
    {
        Interpolate = entity.InterpolateDisplay;
        foreach (var c in components)
        {
            c.OnFixedUpdate(entity);
        }
        nextPos = new Vector3(entity.Pos.x, entity.Pos.y, 0);
        if (entity.IsActive)
        {
            disable = false;
            gameObject.SetActive(true);
        }
        else
        {
            disable = true;
            if (entity.ContainsComponent(ComponentType.Expiring))
            {
                disableWithDelay = true;
            }
        }
    }
}
