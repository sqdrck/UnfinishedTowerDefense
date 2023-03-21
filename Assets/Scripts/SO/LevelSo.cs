using UnityEngine;
using Newtonsoft.Json;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/Level", order = 1)]
public class LevelSo : SerializedScriptableObject
{
    public StageSo[] Stages;
    public BoardSo Board;
    public EntitySo EntitySpawnerConfig;

    public CameraDisplay CameraDisplayPrefab;
    public MainCanvas MainCanvasPrefab;

    public TileDisplay DefaultTileDisplayPrefab;
    public TileDisplay TowerTileDisplayPrefab;
    public FloatingDamageDisplay FloatingDamageDisplayPrefab;
    public DeathAnimationDisplay DeathAnimationDisplayPrefab;

    public EntitySo DefaultHeroConfig;
    public EntitySo WoodFenceConfig;
    public EntitySo HedgehogFenceConfig;
    public EntitySo ElectroFenceConfig;
    public EntitySo MineConfig;
    public EntitySo AoeDamagerConfig;
    public EntitySo TravolatorConfig;
    public EntitySo BarrierConfig;
    public int DefaultLootboxTimerSeconds;
    public int RareLootboxTimerSeconds;

#if UNITY_EDITOR

    [Button("Backup Stages")]
    private void SerializeStages()
    {
        var settings = new JsonSerializerSettings();
        settings.NullValueHandling = NullValueHandling.Ignore;
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        var serializedStages = JsonConvert.SerializeObject(Stages, settings);
        var path = Path.Combine(Application.dataPath, "Editor", "StagesBackups");
        Directory.CreateDirectory(path);
        var name = Directory.GetFiles(path).Length / 2 + ".json";
        System.IO.File.WriteAllText(Path.Combine(Application.dataPath, "Editor", "StagesBackups", name.ToString()), serializedStages);
        AssetDatabase.Refresh();
        Debug.Log($"Successfully saved backup [{name}].");
    }
#endif
}
