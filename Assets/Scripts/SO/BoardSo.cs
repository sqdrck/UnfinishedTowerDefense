using UnityEngine;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(fileName = "BoardData", menuName = "ScriptableObjects/Board", order = 1)]
public class BoardSo : SerializedScriptableObject
{
    [Header("Colors")]
    [SerializeField, ReadOnly]
    private Color noCell = TdUtils.TileColorTable[TileType.None];
    [SerializeField, ReadOnly]
    private Color playerBase = TdUtils.TileColorTable[TileType.Base];
    [SerializeField, ReadOnly]
    private Color tower = TdUtils.TileColorTable[TileType.Tower];
    [SerializeField, ReadOnly]
    private Color buffer = TdUtils.TileColorTable[TileType.InactiveBuffer];
    [SerializeField, ReadOnly]
    private Color enemySpawner = TdUtils.TileColorTable[TileType.EnemySpawner];
    [SerializeField, ReadOnly]
    private Color road = TdUtils.TileColorTable[TileType.Road];
    [SerializeField, ReadOnly]
    private Color activeBuffer = TdUtils.TileColorTable[TileType.ActiveBuffer];
    [SerializeField, ReadOnly]
    private Color BoosterUnplaceableRoad = TdUtils.TileColorTable[TileType.BoosterUnplaceableRoad];
#if UNITY_EDITOR
    [TableMatrix(DrawElementMethod = nameof(DrawCell), SquareCells = true)]
#endif
    public TileType[,] Tiles = new TileType[8, 13];

#if UNITY_EDITOR
    [Button("Fill")]
    private void Fill(TileType type)
    {
        for (int x = 0; x < Tiles.GetLength(0); x++)
        {
            for (int y = 0; y < Tiles.GetLength(1); y++)
            {
                Tiles[x, y] = type;
            }
        }
    }
    private static TileType DrawCell(Rect rect, TileType value)
    {
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            if (Event.current.button == 0)
            {
                value = value.Next();
            }
            else if (Event.current.button == 1)
            {
                value = value.Previous();
            }
            GUI.changed = true;
            Event.current.Use();
        }

        EditorGUI.DrawRect(rect.Padding(1), TdUtils.GetTileColor(value));

        return value;
    }
#endif
}
