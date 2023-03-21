using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenUiElement : MonoBehaviour
{
    [HideInInspector]
    public MainCanvas Root;

    public virtual void OnInit(GameState gameState) { }
    public virtual void OnUpdate(GameState gameState) { }
    public virtual void OnFixedUpdate(GameState gameState) { }
    public virtual void OnOpen(GameState gameState) { }
    public virtual void OnClose(GameState gameState) { }

}
