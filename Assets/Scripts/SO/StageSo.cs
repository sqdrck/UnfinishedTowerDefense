using UnityEngine;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct WaveInfo
{
    [ReadOnly, LabelText("P.O.W.E.R:")]
    public int Power;

    public float Cooldown;
    public WaveEntry[] Entries;
    [ReadOnly]
    public float Duration;
}

[Serializable]
public struct WaveEntry
{
    // TODO(sqdrck): Create based on board through reflection.
    [Flags]
    public enum Spawner
    {
        A = 1,
        B = 2,
        C = 4,
        D = 8,
    }

    [LabelText("Cooldown"), Min(0)]
    public float SpawnCooldown;
#if UNITY_EDITOR
    [ReadOnly, ValidateInput(nameof(ValidateInput))]
#endif
    public int EntitySoIndex;
    [EnumToggleButtons, HideLabel]
    public Spawner Spawners;
    [Range(1, 4), HideInInspector]
    public int SpawnCount;


#if UNITY_EDITOR
    [ShowInInspector, OnValueChanged(nameof(SetIndex)), NonSerialized]
    public EntitySo EntityToSpawn;

    private bool ValidateInput(int index)
    {
        if (EntitySoIndex != 0)
        {
            EntityToSpawn = SoDatabase.Instance.EntitySoDatabase[EntitySoIndex];
        }
        return true;
    }

    private void SetIndex()
    {
        if (EntityToSpawn is not null)
        {
            EntitySoIndex = EntityToSpawn.Index;
        }
    }

#endif
}

//[Serializable]
//public struct ResourcePackItem
//{
//#if UNITY_EDITOR
//[ValidateInput(nameof(ValidateSoIndex)), ReadOnly]
//#endif
//public int EntitySoIndex;
//public int Count;

//#if UNITY_EDITOR
//public bool ValidateSoIndex(int i = 0)
//{
//if (EntitySoIndex == 0)
//{
//return true;
//}
//else
//{
//if (EntitySoIndex != 0)
//{
//EntitySo = SoDatabase.Instance.EntitySoDatabase[EntitySoIndex];
//}
//return true;
//}
//}
//[ShowInInspector, OnValueChanged(nameof(SetIndex)), NonSerialized]
//private EntitySo EntitySo;

//public void SetIndex()
//{
//if (EntitySo is not null)
//{
//EntitySoIndex = EntitySo.Index;
//}
//}
//#endif
//}

public enum StoreCategory
{
    None,
    Buffer,
    Booster,
    Gold,
    Gems,
}

[CreateAssetMenu(fileName = "StageData", menuName = "ScriptableObjects/Stage", order = 1)]
public class StageSo : SerializedScriptableObject
{
    [ReadOnly, LabelText("P.O.W.E.R:")]
    public int Power;
    [ReadOnly]
    public float Duration;
    public WaveInfo[] Waves;

    public ResourcePack LoseResourcePack;
    public ResourcePack WinResourcePack;

    [Tooltip("Shows Gold on main menu stage circle")]
    public bool HasAdditionalGold;

#if UNITY_EDITOR
    private void OnValidate()
    {
        Duration = 0;
        for (int i = 0; i < Waves.Length; i++)
        {
            var w = Waves[i];

            Duration += w.Cooldown;
            w.Duration = 0;
            for (int j = 0; j < w.Entries.Length; j++)
            {
                w.Duration += w.Entries[j].SpawnCooldown;
                Duration += w.Duration;
            }
            Waves[i] = w;
        }


        Undo.RecordObject(this, "StagesOnValidate");
    }
#endif
}
