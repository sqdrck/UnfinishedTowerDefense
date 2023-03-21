using UnityEngine;
using System.Collections.Generic;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "GameSettingsData", menuName = "ScriptableObjects/GameSettings", order = 1)]
public class GameSettingsSo : SerializedScriptableObject
{
    public bool SkipTutorial;
    // TODO(sqdrck): Merge with SoDatabase eventually.
    // NOTE(sqdrck): Placed in invoke order.
    public EntitySo[] HeroesDatabase;
    public Inventory InitialInventory;

    public StoreEntry[] StoreEntries;
    public MissionSo[] Missions;

    public ResourcePack[] DailyRewardCycle;
    [ReadOnly]
    public ResourcePack[] DailyRewards;

    [Button("Generate Daily Rewards")]
    public void GenerateDailyRewards()
    {
        if (DailyRewardCycle != null && DailyRewardCycle.Length < 1)
        {
            return;
        }
        DailyRewards = new ResourcePack[28];
        for (int i = 0; i < 28; i++)
        {
            DailyRewards[i] = DailyRewardCycle[i % DailyRewardCycle.Length];
        }
    }
}

[System.Serializable]
public struct StoreEntry
{
    public UnityEngine.Localization.LocalizedString LocString;
    public StoreCategory Category;
    public ResourcePackSo Price;
    public ResourcePackSo Item;
}
