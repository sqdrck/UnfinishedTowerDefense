using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(fileName = "SoDatabase", menuName = "ScriptableObjects/So Database", order = 1)]
public class SoDatabase : SerializedScriptableObject
{
    [FolderPath]
    public string ScriptableObjectsPath;
    [ReadOnly]
    public Dictionary<int, EntitySo> EntitySoDatabase;
    [ReadOnly]
    public Dictionary<int, LootboxSo> LootboxSoDatabase;

#if UNITY_EDITOR
    public static SoDatabase Instance;
    [Button("Validate")]
    public void OnValidate()
    {
        Instance = this;
        if (EntitySoDatabase?.Count > 0)
        {
            EntitySoDatabase.Clear();
        }
        if (LootboxSoDatabase?.Count > 0)
        {
            LootboxSoDatabase.Clear();
        }
        EntitySoDatabase = new();
        LootboxSoDatabase = new();

        var path = ScriptableObjectsPath;//= Path.Combine("Assets", "Data", "ScriptableObjects");
        var assets = AssetDatabase.FindAssets($"t:{nameof(EntitySo)}", new string[] { path });
        for (int i = 0; i < assets.Length; i++)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(assets[i]);
            EntitySo so = AssetDatabase.LoadAssetAtPath<EntitySo>(assetPath);
            while (so.Index == 0)
            {
                so.Index = UnityEngine.Random.Range(-int.MaxValue, int.MaxValue);
                EditorUtility.SetDirty(so);
            }
            EntitySoDatabase.Add(so.Index, so);
        }
        assets = AssetDatabase.FindAssets($"t:{nameof(LootboxSo)}", new string[] { path });
        for (int i = 0; i < assets.Length; i++)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(assets[i]);
            LootboxSo so = AssetDatabase.LoadAssetAtPath<LootboxSo>(assetPath);
            while (so.Index == 0 || so.Index == -1)
            {
                so.Index = UnityEngine.Random.Range(-int.MaxValue, int.MaxValue);
                EditorUtility.SetDirty(so);
            }
            LootboxSoDatabase.Add(so.Index, so);
        }
    }
#endif
}
