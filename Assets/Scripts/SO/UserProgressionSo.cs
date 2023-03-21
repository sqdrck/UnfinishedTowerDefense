using UnityEngine;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public struct UserProgressionItem
{
    public ResourcePack ResourcePack;
    public int RelativeExp;
}
[CreateAssetMenu(fileName = "UserProgression", menuName = "ScriptableObjects/User Progression", order = 1)]
public class UserProgressionSo : SerializedScriptableObject
{
    public Dictionary<int, UserProgressionItem> UserProgressionItems = new();
}
