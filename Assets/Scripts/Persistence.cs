using System.Collections;
using System.Runtime.Serialization;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif

public struct ShallowEntity
{
    public float X;
    public float Y;
    public int EntitySoIndex;
    public EntityType Type;
    public int Level;
}
// NOTE(sqdrck): This structure must be preserved across different version of game.
[Serializable]
public class ShallowSave
{
    public ShallowEntity[] Entities;
    public Tile[,] Tiles;
    public string AppVersion;
    public bool[] CollectedLevelRewards;
    //public bool[] CollectedDailyRewards;
    public int LastCollectedDailyRewardIndex;
    public Dictionary<int, int> EntityLevels;
    public Dictionary<int, MissionProgress> MissionsProgress;
    public Dictionary<int, bool> EntitiesUnlocked;
    public Inventory Inventory;
    public int GoldCount;
    public int GemsCount;
    public int BattleTokensCount;
    public int ExpCount;
    public int CurrentStageIndex;
    public bool IsTutorialCompleted;
    public DateTimeOffset LastTimeCollectedDailyReward;

    public bool SettingsSoundEnabled;
    public bool SettingsMusicEnabled;

}
public static class Persistence
{
    public static string SaveFolderPath => Path.Combine(Application.persistentDataPath, "Saves");
    public static string DeepSavePath => Path.Combine(SaveFolderPath, "deep_save.dat");
    public static string ShallowSavePath => Path.Combine(SaveFolderPath, "shallow_save.json");


    public static bool SerializeShallowSave(GameState gameState)
    {
        bool result = true;
        try
        {
            ShallowSave save = new();
            Span<ShallowEntity> shallowEs = stackalloc ShallowEntity[gameState.Entities.Length];
            int count = 0;
            for (int i = 0; i < gameState.Entities.Length; i++)
            {
                var entity = gameState.Entities[i];
                if (entity.IsActive && entity.IsShallowSerializable)
                {
                    shallowEs[count].X = entity.Pos.x;
                    shallowEs[count].Y = entity.Pos.y;
                    shallowEs[count].EntitySoIndex = entity.EntitySoIndex;
                    shallowEs[count].Type = entity.Type;
                    shallowEs[count].Level = entity.Level;

                    count++;
                }
            }
            save.IsTutorialCompleted = gameState.IsTutorialCompleted;
            save.Entities = shallowEs.Slice(0, count).ToArray();
            save.AppVersion = gameState.AppVersion;
            save.Tiles = gameState.Tiles;
            save.CollectedLevelRewards = gameState.CollectedLevelRewards;
            //save.CollectedDailyRewards = gameState.CollectedDailyRewards;
            save.Inventory = gameState.Inventory;
            save.GoldCount = gameState.GoldCount;
            save.GemsCount = gameState.GemsCount;
            save.BattleTokensCount = gameState.BattleTokensCount;
            save.ExpCount = gameState.ExpCount;
            save.CurrentStageIndex = gameState.CurrentStageIndex;
            save.EntityLevels = gameState.EntityLevels;
            save.MissionsProgress = gameState.MissionsProgress;
            save.EntitiesUnlocked = gameState.EntitiesUnlocked;
            save.LastTimeCollectedDailyReward = gameState.LastTimeCollectedDailyReward;
            save.LastCollectedDailyRewardIndex = gameState.LastCollectedDailyRewardIndex;
            save.SettingsMusicEnabled = gameState.SettingsMusicEnabled;
            save.SettingsSoundEnabled = gameState.SettingsSoundEnabled;

            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(ShallowSavePath))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, save);
                    Debug.Log("ShallowSaved sucessfully");
                }
            }
        }
        catch (System.Exception e)
        {
            result = false;
            Debug.LogWarning("Failed to serialize. Reason: " + e.Message);
        }

        return result;
    }

    public static bool DeserializeShallowSave(out GameState gameState, out ShallowEntity[] shallowEntities)
    {
        gameState = new();
        shallowEntities = null;
        bool result = true;
        try
        {
            JsonSerializer serializer = new JsonSerializer();
            //serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamReader sr = new StreamReader(ShallowSavePath))
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    var save = serializer.Deserialize<ShallowSave>(reader);
                    gameState.Tiles = save.Tiles;
                    gameState.AppVersion = save.AppVersion;
                    gameState.CollectedLevelRewards = save.CollectedLevelRewards;
                    //gameState.CollectedDailyRewards = save.CollectedDailyRewards;
                    gameState.Inventory = save.Inventory;
                    gameState.GoldCount = save.GoldCount;
                    gameState.GemsCount = save.GemsCount;
                    gameState.BattleTokensCount = save.BattleTokensCount;
                    gameState.ExpCount = save.ExpCount;
                    gameState.CurrentStageIndex = save.CurrentStageIndex;
                    gameState.EntityLevels = save.EntityLevels;
                    gameState.MissionsProgress = save.MissionsProgress;
                    gameState.EntitiesUnlocked = save.EntitiesUnlocked;
                    shallowEntities = save.Entities;
                    gameState.IsTutorialCompleted = save.IsTutorialCompleted;
                    gameState.LastTimeCollectedDailyReward = save.LastTimeCollectedDailyReward;
                    gameState.LastCollectedDailyRewardIndex = save.LastCollectedDailyRewardIndex;
                    gameState.SettingsMusicEnabled = save.SettingsMusicEnabled;
                    gameState.SettingsSoundEnabled = save.SettingsSoundEnabled;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Failed to serialize. Reason: " + e.Message);
            result = false;
        }
        return result;
    }

    public static bool SerializeDeepSave(GameState gameState)
    {
        bool result = true;
        Directory.CreateDirectory(Path.GetDirectoryName(DeepSavePath));
        FileStream fs = null;
        try
        {
            fs = new FileStream(DeepSavePath, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fs, gameState);
            Debug.Log("DeepSaved sucessfully");
        }
        catch (Exception e)
        {
            result = false;
            Debug.LogWarning("Failed to serialize. Reason: " + e.Message);
        }
        finally
        {
            fs?.Close();
        }

        return result;
    }

    public static bool DeserializeDeepSave(out GameState gameState)
    {
        gameState = null;
        bool result = true;

        Directory.CreateDirectory(Path.GetDirectoryName(DeepSavePath));
        FileStream fs = null;
        try
        {
            fs = new FileStream(DeepSavePath, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            gameState = (GameState)formatter.Deserialize(fs);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to deserialize. Reason: " + e.Message);
            result = false;
        }
        finally
        {
            fs?.Close();
        }

        return result;
    }

#if UNITY_EDITOR
    [MenuItem("Game/Open saves folder &s")]
    public static void RevealInFinder()
    {
        EditorUtility.RevealInFinder(DeepSavePath);
    }

    [MenuItem("Game/Delete saves &d")]
    public static void DeleteSaves()
    {
        File.Delete(DeepSavePath);
        File.Delete(ShallowSavePath);
    }
#endif
}
