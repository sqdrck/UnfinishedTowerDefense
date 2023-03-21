using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityDisplayComponent : MonoBehaviour
{
    public abstract void OnInit(Entity entity);
    public virtual void OnFixedUpdate(Entity entity) { }
    public virtual void OnUpdate(Entity entity) { }
    public virtual void OnTap(Entity entity) { }
}
