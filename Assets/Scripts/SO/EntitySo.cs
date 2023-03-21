using UnityEngine;
using System.Linq;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "EntityData", menuName = "ScriptableObjects/Entity", order = 1)]
public class EntitySo : SerializedScriptableObject
{
#if UNITY_EDITOR
    [Button("Regenerate index"), GUIColor(0.9f, 0.2f, 0.2f)]
    private void RegenerateIndex()
    {
        Index = 0;
        SoDatabase.Instance.OnValidate();
        EditorUtility.SetDirty(this);
    }
#endif
    [ReadOnly, InfoBox("Index is set up by SoDatabase")]
    public int Index;
    public bool UnlockedByDefault;
    public bool IsShallowSerializable;
    public ResourcePack UnlockPrice;

    private void OnValidate()
    {
        if (useRandomColor && randomColor == Color.black)
        {
            randomColor = UnityEngine.Random.ColorHSV();
        }
    }
    [PreviewField(50, ObjectFieldAlignment.Left)]
    public GameObject Prefab;
    [PreviewField(50, ObjectFieldAlignment.Left)]
    public Sprite UiSprite;
    [PreviewField(50, ObjectFieldAlignment.Left)]
    public GameObject HeroProfilePrefab;
    [Space(10)]
    // NOTE(sqdrck): Used only for behaviour differentiation and targeting.
    public EntityType Type;
    public string Name;
    [SerializeField]
    private bool useRandomColor;
    [ShowIf(nameof(useRandomColor)), ReadOnly]
    [SerializeField]
    private Color randomColor = Color.black;
    [HideIf(nameof(useRandomColor))]
    [SerializeField]
    private Color userColor;
    public Color Color
    {
        get
        {
            if (useRandomColor)
            {
                return randomColor;
            }
            else
            {
                return userColor;
            }
        }
    }

    [Space(10)]

    [ReadOnly, LabelText("P.O.W.E.R:")]
    public int Power;
    [ReadOnly, LabelText("DPS:")]
    public int Dps;

    [Space(10)]

    public TileType PlaceableTiles;
    public ComponentType Components = ComponentType.None;
    //public TeamType Team;
    //public TeamType TargetTeam;

    [Space(10)]

    [Min(0)]
    public int UltDamage;

    [Min(0), Tooltip("Per second")]
    public float UltAccumulationSpeed;

    [Tooltip("Ultimate accumulation multiplier vs. calculated dps")]
    public AnimationCurve UltMultiplier;

    [Space(10)]

    public BuffProperties[] BuffsToCast;

    [Space(10)]

    public DamageType DamageType;

    [ShowIf(nameof(ShowAoeParameters))]
    public EntitySo AoeDamageConfig;
    [ShowIf(nameof(ShowAoeParameters))]
    public int AoeInFrontOfTargetPreemptivePathSegments = 1;

    [Space(10)]

    public PriorityType PriorityType;
    [Tooltip("Forces entity to keep target even if there is more suitable entity in raidus")]
    public bool ForceKeepTarget;

    [Min(0)]
    public float DamageRadius = 1f;

    [Range(0, 1), OnValueChanged(nameof(OnDamageMinChanged))]
    public float Armor;

    [Min(0), OnValueChanged(nameof(OnDamageMinChanged))]
    public float Speed = 1;

    [Min(0), OnValueChanged(nameof(OnDamageMinChanged))]
    public int Hp = 100;

    [Min(0)]
    public float SpawnDelay = 0;

    [Min(0)]
    public int LifeDuration = 0;

    [Min(0)]
    public int BaseDamage = 1;

    [Range(0, 1)]
    public float CritChance = 0.1f;

    [OnValueChanged(nameof(OnDamageMinChanged)), Min(0)]
    public int DamageMin = 0;
    [MinValue(nameof(DamageMin))]
    public int DamageMax;
    public bool UpdateCooldownOnlyIfHasTarget;

    [Min(0), OnValueChanged(nameof(CalculateDps))]
    public float CooldownDuration = 1;
    [Min(0)]
    public float Cooldown1Duration = 0;
    [Min(0)]
    public float Cooldown2Duration = 0;

    [InlineButton(nameof(AddLevelDamageMultiplier), "Add")]
    [OnCollectionChanged(nameof(AfterLevelDamageMultiplierCollectionChanged))]
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.OneLine, KeyLabel = "Level", ValueLabel = "Multiplier")]
    public Dictionary<int, DamageMultiplierInfo> LevelDamageMultiplier = new();

    private void AddLevelDamageMultiplier()
    {
        DamageMultiplierInfo info;
        if (LevelDamageMultiplier.Count > 0)
        {
            info = LevelDamageMultiplier.ElementAt(LevelDamageMultiplier.Count - 1).Value;
        }
        else
        {
            info = new DamageMultiplierInfo();
        }

        LevelDamageMultiplier[LevelDamageMultiplier.Count + 1] = info;

        AfterLevelDamageMultiplierCollectionChanged();
    }

    private void OnDamageMinChanged()
    {
        //DamageMax = 5 * DamageMin;
        CalculateDps();
        AfterLevelDamageMultiplierCollectionChanged();
    }

    private bool ShowAoeParameters()
    {
        return DamageType is DamageType.AoeOnSelf || DamageType is DamageType.AoeOnTarget || DamageType is DamageType.AoeOnRandomTile || DamageType is DamageType.AoeInFrontOfTarget;
    }

    private void CalculateDps()
    {
        // TODO(sqdrck): Caclulate AOE
        var averageDamage = (DamageMin + DamageMax) * 0.5f;
        if (CooldownDuration < Time.fixedDeltaTime)
        {
            CooldownDuration = Time.fixedDeltaTime;
        }
        float dps = averageDamage / CooldownDuration;
        Dps = Mathf.CeilToInt(dps);
        float hp = Hp * (1f + Armor);
        Power = Mathf.CeilToInt(Speed * hp * (dps == 0 ? 1f : dps));
    }

    private void OnEnable()
    {
        CalculateDps();
    }

    [Serializable]
    public enum MultiplierType
    {
        Absolute,
        RelativeToPrevious,
    }

    // TODO(sqdrck): Rename it.
    [Serializable]
    public struct DamageMultiplierInfo
    {
        [HideInInspector]
        public int BaseDamageMin;

        //[VerticalGroup("Multiplier")]
        //[TableColumnWidth(60)]
        //public MultiplierType Type;
        //[VerticalGroup("Multiplier")]
        //[SerializeField]
        //public float Multiplier;

        [TableColumnWidth(100)]
        [VerticalGroup("Calculations")]
        public int DamageMin;

        [VerticalGroup("Calculations"), MinValue(nameof(DamageMin))]
        public int DamageMax;

        [VerticalGroup("Calculations")]
        [ReadOnly]
        public int DamageAverage;

        public ResourcePack Price;
    }

    public void AfterLevelDamageMultiplierCollectionChanged()
    {
        if (LevelDamageMultiplier is null) return;
        try
        {
            for (int i = 0; i < LevelDamageMultiplier.Count; i++)
            {
                var pair = LevelDamageMultiplier.ElementAt(i);
                var val = pair.Value;
                var key = pair.Key;
                //val.BaseDamageMin = DamageMin;
                //if (pair.Key == 1 && val.Type is MultiplierType.RelativeToPrevious)
                //{
                //val.Type = MultiplierType.Absolute;
                //Debug.LogWarning("No previous level. Use Absolute multiplier");
                //}
                //if (val.Type is MultiplierType.Absolute)
                //{
                //val.DamageMin = Mathf.RoundToInt(val.BaseDamageMin * val.Multiplier);
                //}
                //else if (val.Type is MultiplierType.RelativeToPrevious)
                //{
                //var prevDamageMin = LevelDamageMultiplier[pair.Key - 1].DamageMin;
                //val.DamageMin = Mathf.RoundToInt(prevDamageMin * val.Multiplier);
                //}
                //val.DamageMax = val.DamageMin * 5;
                val.DamageAverage = (val.DamageMin + val.DamageMax) / 2;

                LevelDamageMultiplier[pair.Key] = val;
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
}
