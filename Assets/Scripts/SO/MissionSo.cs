using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Mission", menuName = "ScriptableObjects/Mission", order = 1)]
public class MissionSo : SerializedScriptableObject
{
    public Sprite MissionSprite;
    public ResourcePackSo Pack;
    public int Max;
    public LocalizedString LocString;
    public int Index = -1;
    public GameEventType Event;

    public void OnValidate()
    {
        if (Index == -1)
        {
            // TODO(sqdrck): Collision checker.
            Index = GetHashCode();
        }
    }
}
