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

public enum LootboxType
{
    Default,
    Rare,
    Lose,
}

[CreateAssetMenu(fileName = "Lootbox Data", menuName = "ScriptableObjects/Lootbox", order = 1)]
public class LootboxSo : SerializedScriptableObject
{
    [ReadOnly]
    public int Index;
    public LootboxType Type;
    public ResourcePack Pack;
    // TODO(sqdrck): Hero shards.

    private void OnValidate()
    {
        while (Index == 0)
        {
            Index = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }
    }
}
