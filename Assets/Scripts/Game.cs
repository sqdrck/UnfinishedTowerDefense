using System.Collections;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum GameEventType
{
    WatchedAd,
    WonStage,
    LevelUppedHero,
    UnlockedHero,
}

[System.Serializable]
[Flags]
public enum TileType
{
    None = 0,
    Base = 1,
    Tower = 2,
    InactiveBuffer = 4,
    EnemySpawner = 8,
    Road = 16,
    ActiveBuffer = 32,
    BoosterUnplaceableRoad = 64,
}

[System.Serializable]
public enum PriorityType
{
    Closest,
    HighestHp,
    LowestHp,
    ClosestToBase,
    AllInRange,
}

[System.Serializable]
public enum DamageType
{
    TargetOnly,
    AoeOnTarget,
    AoeOnSelf,
    AoeOnRandomTile,
    AoeInFrontOfTarget,
}

[System.Serializable]
public enum BuffStrengthType
{
    Absolute,
    Relative,
}

public static class TdUtils
{
    public static Dictionary<TileType, Color> TileColorTable = new()
    {
        {TileType.None, new Color(0.1f, 0.1f, 0.1f)},
        {TileType.Base, new Color(0.2f, 1, 0.3f)},
        {TileType.Tower, new Color(0.2f, 0.2f, 0.2f)},
        {TileType.InactiveBuffer,new Color(0.2f, 0.4f, 1)},
        {TileType.EnemySpawner,new Color(1, 0.7f, 0)},
        {TileType.Road, new Color(1, 1, 1)},
        {TileType.ActiveBuffer, new Color(0.1f, 0.1f, 0.3f)},
        {TileType.BoosterUnplaceableRoad, new Color(0.8f, 0.8f, 0.8f)},
    };

    public static Color GetTileColor(TileType value)
    {
        return TileColorTable[value];
    }
}

[System.Serializable]
public class Tile
{
    public TileType Type;
    public int SerialNumber;
}

// TODO(sqdrck): Initializer for every component.
[Flags]
public enum ComponentType : Int32
{
    None = 0,
    Walking = 1 << 0,
    TargetDamager = 1 << 1,
    BufferBreaker = 1 << 2,
    Mortal = 1 << 3,
    Buffer = 1 << 4,
    Cooldown = 1 << 5,
    Expiring = 1 << 6,
    EntitySpawner = 1 << 7,
    BaseDamager = 1 << 8,
    TeamTargeter = 1 << 9,
    DisableOnEmptyAmmo = 1 << 10,
    BuffCaster = 1 << 11,
    Ultimate = 1 << 12,
    CooldownIfHasTarget = 1 << 13, // Deprecated
    CooldownWithActivePhase = 1 << 14,
    SpawnWithDelay = 1 << 15,
}

public enum TeamType : Int32
{
    None,
    Good,
    Evil,
}

[Flags]
public enum BuffType : Int32
{
    None = 0,
    AffectSpeed = 1,
    DamageOverTime = 2,
}
[Flags]
public enum BuffApplyType
{
    None = 0,
    ByRadius = 1,
    ByDealingDamage = 2,
}

[System.Serializable]
public struct ResourcePack
{
    // NOTE(sqdrck): Used for both apply and spend:
    public int Gold;
    public int Gems;
    public int BattleTokens;

    // NOTE(sqdrck): For spending resource pack:
    public int HeroFragments;

    // NOTE(sqdrck): For applying resource pack:
    public InventoryEntry[] Boosters;
    public InventoryEntry[] Buffers;
    public InventoryEntry[] HeroesFragments;
    public LootboxSo[] Lootboxes;
    public int Exp;

    // NOTE(sqdrck): Helpers:
    public int RareLootboxesCount => (Lootboxes?.Count(lb => lb.Type is LootboxType.Rare)).GetValueOrDefault(0);
    public int DefaultLootboxesCount => (Lootboxes?.Count(lb => lb.Type is LootboxType.Default)).GetValueOrDefault(0);

    public static ResourcePack operator *(ResourcePack pack, int count)
    {
        ResourcePack result = pack;
        result.Gems *= count;
        result.BattleTokens *= count;
        result.Gold *= count;
        result.HeroFragments *= count;
        result.Exp *= count;

        // TODO(sqdrck): Need to test this.
        for (int i = 0; i < result.Boosters.Length; i++)
        {
            result.Boosters[i].Count *= count;
        }

        for (int i = 0; i < result.Buffers.Length; i++)
        {
            result.Buffers[i].Count *= count;
        }

        return result;
    }
}

[System.Serializable]
public struct BuffProperties
{
    // TODO(sqdrck): Buff color for displaying.
    [HideInInspector]
    public float LifeT;
    [HideInInspector]
    public int CasterIndex;
    [HideInInspector]
    public int Index;
    [HideInInspector]
    public float CooldownT;

    public bool IsApplyingColor;
    [ShowIf(nameof(IsApplyingColor))]
    public Color DisplayColor;
    [ShowIf(nameof(IsApplyingColor))]
    public int DisplayColorPriority;

    public BuffType Type;
    public BuffApplyType ApplyType;
    public BuffStrengthType StrengthType;
    public float Strength;
    public bool ApplyImmediately;
    public bool CancelIfLeftDamageRadius;
    public bool CancelIfLeftCastRadius;
    public float CooldownDuration;
    public float LifeDuration;
    public bool UseDamagePropertiesFromCaster;
    [HideIf(nameof(UseDamagePropertiesFromCaster))]
    public int DamageMin;
    [HideIf(nameof(UseDamagePropertiesFromCaster))]
    public int DamageMax;
    [HideIf(nameof(UseDamagePropertiesFromCaster))]
    public float CritChance;
    public float CastRadius;
    public TeamType CastTeam;

    public override string ToString()
    {
        return $"{Type.ToString()} {StrengthType.ToString()} {Strength} {CastRadius} {CastTeam.ToString()}";
    }
    public int RandomDamage => UnityEngine.Random.Range(DamageMin, DamageMax);
}

[System.Serializable]
public enum EntityType
{
    None,
    Barrier,
    Buffer,
    BufferBreaker,
    Creep,
    Mine,
    Travolator,
    Psycho,
    Hero,
}

[System.Serializable]
public class InventoryEntry
{
#if UNITY_EDITOR
    [ReadOnly, ValidateInput(nameof(ValidateSoIndex))]
#endif
    public int EntitySoIndex;
#if UNITY_EDITOR
    [ReadOnly, ValidateInput(nameof(ValidateSoIndex))]
#endif
    public int LootboxSoIndex;
    public DateTimeOffset UnlockTime;
    public int Count;

#if UNITY_EDITOR
    public bool ValidateSoIndex(int i = 0)
    {
        if (EntitySoIndex == 0 && LootboxSoIndex == 0)
        {
            return true;
        }
        else
        {
            if (LootboxSoIndex != 0)
            {
                LootboxSo = SoDatabase.Instance.LootboxSoDatabase[LootboxSoIndex];
            }
            if (EntitySoIndex != 0)
            {
                EntitySo = SoDatabase.Instance.EntitySoDatabase[EntitySoIndex];
            }
            return true;
        }
    }
    [ShowInInspector, OnValueChanged(nameof(SetIndex)), NonSerialized]
    private EntitySo EntitySo;
    [ShowInInspector, OnValueChanged(nameof(SetIndex)), NonSerialized]
    private LootboxSo LootboxSo;


    public void SetIndex()
    {
        if (EntitySo is not null)
        {
            EntitySoIndex = EntitySo.Index;
        }
        if (LootboxSo is not null)
        {
            LootboxSoIndex = LootboxSo.Index;
        }
    }
#endif
}

[System.Serializable]
public struct Inventory
{
    public List<InventoryEntry> Consumables;
    public List<InventoryEntry> Lootboxes;
    public Dictionary<int, int> HeroesFragments;
}

public enum ConsumableType
{
    Booster,
    Buffer,
}

[System.Serializable]
//[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public class Entity
{
    [NonSerialized]
    public EntitySo AoeDamageConfig;
    [NonSerialized]
    public GameObject DisplayPrefab;

    public float2 AoePos;
    public int AoeInFrontOfTargetPreemptivePathSegments;
    public bool UpdateCooldownOnlyIfHasTarget;
    public bool ForceKeepTarget;
    public bool IsShallowSerializable;
    public float4 Color;
    public WaveInfo SpawnerWaveInfo;
    public int SpawnerWaveEntryIndex;

    public int Index;
    public bool InterpolateDisplay = true;
    public bool NeedToSayPhrase;
    public float SpawnDelay;
    public float SpawnDelayT;

    public int Power;
    public TileType PlaceableTiles;
    public EntityType Type;

    public ComponentType Components;

    public TeamType Team;
    public TeamType TargetTeam;

    public BuffProperties[] BuffCastProperties;

    // TODO(sqdrck): Remove list?
    public Dictionary<int, BuffProperties> AppliedBuffsProperties = new();

    public int WaveIndex = -1;
    public int SerialNumber;
    public float LifeDuration;
    public float LifeT;

    public float TimeLived;
    public int DamageDone;
    public int CalculatedDps;

    public int UltimateDamage;
    public float UltimateAccumulated;
    public float UltimateAccumulationSpeed;
    [NonSerialized]
    public AnimationCurve UltimateMultiplierCurve;

    public int BaseDamage;

    public float Armor;

    public bool IsActive;
    public bool IsReadyToRecycle;

    public int2 IntPos => new int2((int)(Pos.x), (int)(Pos.y));
    public float2 Pos;

    public int Hp;
    public int MaxHp;
    public int AmmoLeft;

    public float AoeDamageRadius;
    public float DamageRadius;
    public int DamageMin;
    public int DamageMax;
    public float CooldownDuration;
    public float Cooldown1Duration;
    public float Cooldown2Duration;
    public float AoeCooldownDuration;
    public float CooldownScaler;
    public int Level = 1;
    public float CooldownT;
    public float Cooldown1T;
    public float Cooldown2T;

    public int[] TargetIndices;
    public int TargetIndex = -1;
    public float2 TargetEntityPos;
    public EntityType TargetType;

    public bool IsWalking;
    public float2[] Path;
    public int CurrentPathSegment = 0;
    public float SegmentT;
    public float InitialSpeed;
    public float Speed;

    public int BuffersDestroyedCount;
    public bool WalkingTowardsBase;

    public float CritChance;
    public PriorityType PriorityType;
    public DamageType DamageType;
    public int EntitySoIndex;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsComponent(ComponentType type) => (Components & type) != 0;
    public int RandomDamage => UnityEngine.Random.Range(DamageMin, DamageMax);

    public Entity Clone()
    {
        return (Entity)this.MemberwiseClone();
    }
}

[Serializable]
public class Wave
{
    public int Index;
    public bool IsActive;
    public float T;
    public int CurrentEntryIndex;
}

public enum GamePhase
{
    Preparation,
    Battle,
}

[System.Serializable]
public class GameState
{
    public string AppVersion;
    public bool IsInitialized;
    public bool IsTutorialCompleted;
    public GameSettingsSo Settings;


    public bool SettingsSoundEnabled;
    public bool SettingsMusicEnabled;


    // NOTE(sqdrck): NonSerialized field are populated by GameMain when 
    // GameState gets loaded or created.
    [NonSerialized]
    public SoDatabase SoDatabase;
    [NonSerialized]
    public LevelSo Config;
    [NonSerialized]
    public UserProgressionSo UserProgressionConfig;
    [NonSerialized]
    public TileDisplay[,] TileDisplays;
    [NonSerialized]
    public CameraDisplay CameraDisplay;
    [NonSerialized]
    public MainCanvas MainCanvas;
    [NonSerialized]
    public Transform FloatingDamageDisplayRoot;
    [NonSerialized]
    public Transform DeathAnimationsDisplayRoot;
    [NonSerialized]
    public Transform TilesRoot;
    [NonSerialized]
    public Transform EntitiesRoot;
    [NonSerialized]
    public FloatingDamageDisplay[] FloatingDamageDisplays;
    [NonSerialized]
    public DeathAnimationDisplay[] DeathAnimationDisplays;
    [NonSerialized]
    public EntityDisplay[] EntityDisplays;

    // TODO(sqdrck): Save only last index like DailyRewards.
    public bool[] CollectedLevelRewards;
    //public bool[] CollectedDailyRewards;
    public int LastCollectedDailyRewardIndex;
    public DateTimeOffset LastTimeCollectedDailyReward;
    public Dictionary<int, MissionProgress> MissionsProgress;
    public Dictionary<int, int> EntityLevels;
    public Dictionary<int, bool> EntitiesUnlocked;

    public bool IsStageLost;
    public bool IsStageWon;
    public bool IsGameWon;
    public EntitySo[] HeroesDatabase;
    public Inventory Inventory;
    public GamePhase CurrentPhase;
    public Entity[] Entities;

    public int FloatingDamageDisplayPoolIndex;
    public int DeathAnimationDisplayPoolIndex;

    public Tile[,] Tiles;

    // NOTE(sqdrck): Array index basically stays for tile index in array.
    public int[] BaseSerialNumbers;

    public int BaseMaxHp;
    public int BaseHp;

    public int CurrentStageIndex;
    public int LastSummonedWaveIndex;
    public int[] ActiveWaveIndices;

    public float WaveCooldownT;
    public float WaveCooldownDuration;
    public float WaveDuration;
    public float WaveT;

    public bool IsLastWave;
    public bool IsLastStage;
    public bool IsWavesEnded;
    public bool IsStageEnded;

    // TODO(sqdrck): Persistence.
    public int GoldCount;
    public int GemsCount;
    public int BattleTokensCount;
    public int ExpCount;

    public bool IsBattlePhase => CurrentPhase is GamePhase.Battle;
    public bool IsPreparationPhase => CurrentPhase is GamePhase.Preparation;
}

public struct MissionProgress
{
    public bool IsCollected;
    public int Progress;
}

public struct CanSpendResourcePackResponse
{
    public bool Result;
    public bool IsHeroClosed;
    public bool IsHeroLastLevel;
    public bool HasGold;
    public bool HasGems;
    public bool HasBattleTokens;
    public bool HasHeroFragments;

    public int GoldPrice;
    public int HeroFragmentsPrice;
    public int BattleTokensPrice;
    public int GemsPrice;

    public int GoldNeed;
    public int BattleTokensNeed;
    public int HeroFragmentsNeed;
    public int GemsNeed;
}

public static class Game
{
    public static void OnGameEvent(GameState gameState, GameEventType type)
    {
        // NOTE(sqdrck): Process missions:
        // PERFORMANCE(sqdrck): This might get tricky when missions count grow up.
        for (int i = 0; i < gameState.Settings.Missions.Length; i++)
        {
            MissionSo missionSo = gameState.Settings.Missions[i];
            if (missionSo.Event == type)
            {
                var save = gameState.MissionsProgress[missionSo.Index];
                save.Progress++;
                gameState.MissionsProgress[missionSo.Index] = save;
            }
        }

    }

    public static bool CollectDailyReward(GameState gameState, int index)
    {
        if (gameState.LastCollectedDailyRewardIndex < index)
        {
            gameState.LastCollectedDailyRewardIndex = index;
            gameState.LastTimeCollectedDailyReward = DateTimeOffset.Now;
            ApplyResourcePack(gameState, gameState.Settings.DailyRewards[index]);
            if (gameState.LastCollectedDailyRewardIndex == 27)
            {
                gameState.LastCollectedDailyRewardIndex = -1;
            }
            return true;
        }

        return false;
    }

    //public static bool CanCollectMissionReward(GameState gameState, )

    public static bool CanCollectDailyReward(GameState gameState, out TimeSpan timeLeft)
    {
        timeLeft = DateTimeOffset.Now.AddDays(1).Date - DateTimeOffset.Now;
        return DateTimeOffset.Now.Day > gameState.LastTimeCollectedDailyReward.Day;
    }

    public static bool LevelUpHero(GameState gameState, int entitySoIndex, out CanSpendResourcePackResponse response)
    {
        EntitySo so = Game.GetEntitySoByIndex(gameState, entitySoIndex);

        int currentLevel = gameState.EntityLevels[entitySoIndex];
        int nextLevelPrice = so.LevelDamageMultiplier[currentLevel + 1].Price.Gold;
        response = CanLevelUpHero(gameState, so);
        if (response.Result)
        {
            gameState.GoldCount -= response.GoldPrice;
            gameState.BattleTokensCount -= response.BattleTokensPrice;
            if (gameState.Inventory.HeroesFragments.ContainsKey(entitySoIndex))
            {
                gameState.Inventory.HeroesFragments[entitySoIndex] -= response.HeroFragmentsPrice;
            }
            currentLevel++;
            Debug.Log("Setting level: " + currentLevel);
            gameState.EntityLevels[entitySoIndex] = currentLevel;
            OnGameEvent(gameState, GameEventType.LevelUppedHero);
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool UnlockHero(GameState gameState, EntitySo so, out CanSpendResourcePackResponse response)
    {
        response = Game.CanSpendResourcePack(gameState, so.UnlockPrice);

        if (response.Result)
        {
            gameState.EntitiesUnlocked[so.Index] = true;
            gameState.GemsCount -= response.GemsPrice;
            OnGameEvent(gameState, GameEventType.UnlockedHero);
            return true;
        }

        else
        {
            return false;
        }
    }

    public static bool SpendResourcePack(GameState gameState, ResourcePack resourcePack, out CanSpendResourcePackResponse response)
    {
        response = Game.CanSpendResourcePack(gameState, resourcePack);

        if (response.Result)
        {
            gameState.GemsCount -= response.GemsPrice;
            gameState.GoldCount -= response.GoldPrice;
            gameState.BattleTokensCount -= response.BattleTokensPrice;
            return true;
        }

        else
        {
            return false;
        }
    }

    public static EntitySo GetInventoryHero(GameState gameState, int entitySoIndex)
    {
        for (int i = 0; i < gameState.HeroesDatabase.Length; i++)
        {
            if (gameState.HeroesDatabase[i].Index == entitySoIndex)
            {
                return gameState.HeroesDatabase[i];
            }
        }

        return null;
    }

    public static CanSpendResourcePackResponse CanSpendResourcePack(GameState gameState, ResourcePack resourcePack)
    {
        CanSpendResourcePackResponse result = new();
        result.HasGold = true;
        result.HasGems = true;
        result.IsHeroLastLevel = false;
        result.HasHeroFragments = true;
        result.HasBattleTokens = true;
        result.IsHeroClosed = false;
        result.Result = true;
        result.GoldPrice = resourcePack.Gold;
        result.GemsPrice = resourcePack.Gems;
        result.BattleTokensPrice = resourcePack.BattleTokens;

        if (result.GoldPrice > gameState.GoldCount)
        {
            result.HasGold = false;
            result.Result = false;
        }
        if (result.BattleTokensPrice > gameState.BattleTokensCount)
        {
            result.HasBattleTokens = false;
            result.Result = false;
        }
        if (result.GemsPrice > gameState.GemsCount)
        {
            result.HasGems = false;
            result.Result = false;
        }

        result.GoldNeed = result.GoldPrice - gameState.GoldCount;
        result.BattleTokensNeed = result.BattleTokensPrice - gameState.BattleTokensCount;
        result.GemsNeed = result.GemsPrice - gameState.GemsCount;

        return result;
    }

    public static CanSpendResourcePackResponse CanLevelUpHero(GameState gameState, EntitySo so)
    {
        int currentLevel = gameState.EntityLevels[so.Index];
        CanSpendResourcePackResponse result = CanSpendResourcePack(gameState, so.LevelDamageMultiplier[currentLevel + 1].Price);
        result.HeroFragmentsPrice = so.LevelDamageMultiplier[currentLevel + 1].Price.HeroFragments;
        if (!gameState.EntitiesUnlocked[so.Index])
        {
            result.IsHeroClosed = true;
            result.Result = false;
        }

        if (so.LevelDamageMultiplier.Count - 1 == currentLevel)
        {
            result.IsHeroLastLevel = true;
            result.Result = false;
        }

        int fragments = 0;
        gameState.Inventory.HeroesFragments.TryGetValue(so.Index, out fragments);

        if (result.HeroFragmentsPrice > 0 &&
                so.LevelDamageMultiplier[currentLevel + 1].Price.HeroFragments > fragments)
        {
            result.HasHeroFragments = false;
            result.Result = false;
        }

        result.HeroFragmentsNeed = result.HeroFragmentsPrice - fragments;

        return result;
    }

    public static void ZeroEntity(Entity entity)
    {
        entity.IsActive = false;
        entity.IsReadyToRecycle = false;
        entity.WaveIndex = -1;
        entity.WalkingTowardsBase = false;
        entity.BuffersDestroyedCount = 0;
        entity.CurrentPathSegment = 0;
        entity.SegmentT = 0;
        entity.Path = null;
        entity.TargetIndex = -1;
        entity.TargetIndices = null;
        entity.SpawnerWaveEntryIndex = 0;
        entity.AppliedBuffsProperties = new();
        entity.SerialNumber = 0;
        entity.TimeLived = 0;
        entity.UltimateAccumulated = 0;
        entity.AmmoLeft = 0;
        entity.Level = 1;
        entity.IsWalking = false;

    }

    public static void SetupEntityFromConfig(Entity entity, EntitySo config, bool initEntity, bool zeroEntity = false)
    {
        if (zeroEntity)
        {
            ZeroEntity(entity);
        }
        entity.AoeInFrontOfTargetPreemptivePathSegments = config.AoeInFrontOfTargetPreemptivePathSegments;
        entity.SpawnDelay = config.SpawnDelay;
        entity.SpawnDelayT = entity.SpawnDelay;
        entity.UpdateCooldownOnlyIfHasTarget = config.UpdateCooldownOnlyIfHasTarget;
        entity.Cooldown1Duration = config.Cooldown1Duration;
        entity.Cooldown2Duration = config.Cooldown2Duration;
        entity.ForceKeepTarget = config.ForceKeepTarget;
        entity.IsShallowSerializable = config.IsShallowSerializable;
        entity.EntitySoIndex = config.Index;
        entity.Color = new float4(config.Color.r, config.Color.g, config.Color.b, config.Color.a);
        entity.Power = config.Power;
        entity.DisplayPrefab = config.Prefab;
        entity.PlaceableTiles = config.PlaceableTiles;
        entity.Type = config.Type;

        //entity.Team = config.Team;
        //entity.TargetTeam = config.TargetTeam;
        entity.Components = config.Components;

        if (config.BuffsToCast?.Length > 0)
        {
            entity.BuffCastProperties = new BuffProperties[config.BuffsToCast.Length];
            for (int i = 0; i < config.BuffsToCast?.Length; i++)
            {
                var properties = config.BuffsToCast[i];
                properties.CastRadius += 0.5f;
                // TODO(sqdrck): Think about setting unique buff indices.
                properties.Index = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                properties.CasterIndex = entity.Index;
                entity.BuffCastProperties[i] = properties;
            }
        }

        entity.UltimateDamage = config.UltDamage;
        entity.UltimateMultiplierCurve = config.UltMultiplier;
        entity.UltimateAccumulationSpeed = config.UltAccumulationSpeed;
        entity.Armor = config.Armor;
        entity.AoeDamageConfig = config.AoeDamageConfig;
        entity.BaseDamage = config.BaseDamage;
        entity.DamageMin = config.DamageMin;
        entity.DamageMax = config.DamageMax;
        entity.DamageRadius = config.DamageRadius + 0.5f;
        entity.MaxHp = config.Hp;
        entity.LifeDuration = config.LifeDuration;

        entity.InitialSpeed = config.Speed;
        entity.CritChance = config.CritChance;
        entity.PriorityType = config.PriorityType;
        entity.DamageType = config.DamageType;
        entity.CooldownDuration = config.CooldownDuration;

        if (initEntity)
        {
            entity.Hp = entity.MaxHp;
            entity.LifeT = config.LifeDuration;
            entity.Speed = config.Speed;
        }
    }

    public static bool CollectLevelReward(GameState gameState, int levelIndex)
    {
        if (!gameState.CollectedLevelRewards[levelIndex])
        {
            gameState.CollectedLevelRewards[levelIndex] = true;
            var pack = gameState.UserProgressionConfig.UserProgressionItems[levelIndex].ResourcePack;
            Game.ApplyResourcePack(gameState, pack);
            return true;
        }

        return false;
    }

    public static bool CollectMissionReward(GameState gameState, int missionIndex)
    {
        var mission = gameState.Settings.Missions[missionIndex];
        var progress = gameState.MissionsProgress[mission.Index];
        if (!progress.IsCollected)
        {
            progress.IsCollected = true;
            gameState.MissionsProgress[mission.Index] = progress;
            ApplyResourcePack(gameState, mission.Pack.Value);
            return true;
        }

        return false;
    }

    public static EntitySo GetEntitySoByIndex(GameState gameState, int index)
    {
        return gameState.SoDatabase.EntitySoDatabase[index];
    }

    public static LootboxSo GetLootboxSoByIndex(GameState gameState, int index)
    {
        return gameState.SoDatabase.LootboxSoDatabase[index];
    }

    public static bool IsEntityCanBePlaced(GameState gameState, int x, int y, Entity entity)
    {
        return (gameState.Tiles[x, y].Type & entity.PlaceableTiles) != TileType.None;
    }

    public static bool IsEntityCanBePlaced(GameState gameState, int x, int y, EntitySo entity)
    {
        return (gameState.Tiles[x, y].Type & entity.PlaceableTiles) != TileType.None;
    }

    public static int[] GetEntitiesOfType(GameState gameState, EntityType type)
    {
        Span<int> entities = stackalloc int[gameState.Entities.Length];
        int count = 0;

        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            if (gameState.Entities[i].Type == type && gameState.Entities[i].IsActive)
            {
                entities[count++] = i;
            }
        }

        return entities.Slice(0, count).ToArray();
    }

    public static TileDisplay CreateTileDisplay(GameState gameState, int x, int y, Tile tile)
    {
        TileDisplay prefab;
        if (tile.Type is TileType.Tower)
        {
            prefab = gameState.Config.TowerTileDisplayPrefab;
        }
        else
        {
            prefab = gameState.Config.DefaultTileDisplayPrefab;
        }
        TileDisplay display = GameObject.Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
        display.OnInit(tile);
        display.transform.SetParent(gameState.TilesRoot);
        gameState.TileDisplays[x, y] = display;

        return display;
    }

    public static Tile CreateTile(GameState gameState, int x, int y, TileType type)
    {
        Tile tile = new Tile();
        tile.Type = type;
        gameState.Tiles[x, y] = tile;

        return tile;
    }

    public static void UpdateTile(GameState gameState, int x, int y, TileType type)
    {
        gameState.Tiles[x, y].Type = type;
        gameState.TileDisplays[x, y].OnUpdate(gameState.Tiles[x, y]);
    }

    public static Entity SpawnEntity(GameState gameState, float2 pos, EntitySo config)
    {
        Debug.Log("SpawnEntity " + config.Type.ToString());
        int firstInactiveEntityIndex = -1;
        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            if (gameState.Entities[i].IsReadyToRecycle)
            {
                firstInactiveEntityIndex = i;
                break;
            }
        }

        if (firstInactiveEntityIndex != -1 && gameState.EntityDisplays[firstInactiveEntityIndex] != null)
        {
            GameObject.Destroy(gameState.EntityDisplays[firstInactiveEntityIndex].gameObject);
            gameState.EntityDisplays[firstInactiveEntityIndex] = null;
        }
        Debug.Assert(firstInactiveEntityIndex != -1, "You spawned too many entities. Increase entity buffer!");
        Entity entity = gameState.Entities[firstInactiveEntityIndex];
        SetupEntityFromConfig(entity, config, true, true);
        entity.Pos = pos;

        entity.IsActive = !entity.ContainsComponent(ComponentType.SpawnWithDelay) || entity.SpawnDelay <= 0;

        return entity;
    }

    public static EntityDisplay SpawnEntityDisplay(GameState gameState, Entity entity, GameObject displayPrefab)
    {
        var go = GameObject.Instantiate(displayPrefab, new Vector3(entity.Pos.x, entity.Pos.y, 0), Quaternion.identity);
        go.SetActive(false);
        var display = go.GetComponent<EntityDisplay>();

        gameState.EntityDisplays[entity.Index] = display;
        display.transform.SetParent(gameState.EntitiesRoot);
        return display;
    }

    public static void SetBufferBreakerPath(GameState gameState, Entity entity)
    {
        int2 endPos = int2.zero;
        bool rollTheDice = entity.BuffersDestroyedCount > 0;
        entity.WalkingTowardsBase = false;
        if (rollTheDice)
        {
            var dice = UnityEngine.Random.Range(1, 2);
            if (dice == 0)
            {
                entity.WalkingTowardsBase = true;
            }
        }

        if (!entity.WalkingTowardsBase)
        {
            var dice = UnityEngine.Random.Range(0, 2);
            if (dice == 0)
            {
                var barrierIndex = Game.GetClosestActiveEntityByRadius(gameState, entity.Pos, float.MaxValue, EntityType.Barrier);

                if (barrierIndex == -1)
                {
                    entity.WalkingTowardsBase = true;
                }
                else
                {
                    endPos = gameState.Entities[barrierIndex].IntPos;
                    entity.TargetType = EntityType.Barrier;
                    entity.TargetIndex = barrierIndex;
                }

            }
            else
            {
                var bufferIndex = Game.GetClosestActiveEntityByRadius(gameState, entity.Pos, float.MaxValue, EntityType.Buffer);

                if (bufferIndex == -1)
                {
                    entity.WalkingTowardsBase = true;
                }
                else
                {
                    endPos = gameState.Entities[bufferIndex].IntPos;
                    entity.TargetType = EntityType.Buffer;
                    entity.TargetIndex = bufferIndex;
                }
            }
        }

        if (entity.WalkingTowardsBase)
        {
            var bases = Game.GetTilesPositionsByType(gameState.Tiles, TileType.Base);
            endPos = bases[UnityEngine.Random.Range(0, bases.Length)];
            Debug.Log("Walking towards base");
        }

        var startPos = Game.GetEntityStartPositionForPathfinder(gameState, entity);
        var path = Pathfinding.CalculatePath(gameState.Tiles,
            TileType.Road | TileType.InactiveBuffer | TileType.BoosterUnplaceableRoad,
            startPos,
            endPos, entity.WalkingTowardsBase);
        entity.CurrentPathSegment = 0;
        entity.Path = path;
        entity.IsWalking = true;
    }

    public static Entity SpawnPsycho(GameState gameState, float2 pos, EntitySo config)
    {
        var entity = SpawnCreepEntity(gameState, pos, config);
        entity.Components = entity.Components | ComponentType.BuffCaster;
        return entity;
    }

    public static Entity SpawnBufferBreaker(GameState gameState, float2 pos, EntitySo config)
    {
        var entity = SpawnEntity(gameState, pos, config);
        entity.CooldownScaler = 1;
        entity.TargetIndex = -1;
        entity.TargetTeam = TeamType.Good;
        entity.Team = TeamType.Evil;

        SetBufferBreakerPath(gameState, entity);

        entity.Components = ComponentType.Walking |
            ComponentType.BufferBreaker |
            ComponentType.Mortal |
            ComponentType.Cooldown |
            ComponentType.BaseDamager |
            ComponentType.TeamTargeter;

        SpawnEntityDisplay(gameState, entity, entity.DisplayPrefab).OnInit(entity);
        return entity;
    }

    public static Entity SpawnMineEntity(GameState gameState, float2 pos)
    {
        var entity = SpawnEntity(gameState, pos, gameState.Config.MineConfig);
        entity.Components = ComponentType.TeamTargeter | ComponentType.TargetDamager | ComponentType.DisableOnEmptyAmmo;
        entity.AmmoLeft = 1;
        entity.TargetTeam = TeamType.Evil;
        entity.Team = TeamType.Good;


        SpawnEntityDisplay(gameState, entity, entity.DisplayPrefab).OnInit(entity);
        return entity;
    }

    public static Entity SpawnHeroEntity(GameState gameState, float2 pos, EntitySo config)
    {
        var entity = SpawnEntity(gameState, pos, config);
        entity.TargetTeam = TeamType.Evil;
        entity.Team = TeamType.Good;

        if (config.Components == ComponentType.None)
        {
            entity.Components = ComponentType.TargetDamager |
                ComponentType.Cooldown | ComponentType.TeamTargeter |
                ComponentType.Ultimate | ComponentType.BuffCaster;
        }

        SpawnEntityDisplay(gameState, entity, entity.DisplayPrefab).OnInit(entity);
        return entity;
    }

    public static int2 GetEntityStartPositionForPathfinder(GameState gameState, Entity entity)
    {
        int2 startPos;
        // NOTE(sqdrck): From top clockwise.
        if (entity.Path is not null && entity.Path.Length > 0)
        {
            startPos = new int2((int)entity.Path[entity.CurrentPathSegment].x, (int)entity.Path[entity.CurrentPathSegment].y);
        }
        else
        {
            startPos = entity.IntPos;
        }
        return startPos;
    }

    public static void SetCreepPath(GameState gameState, Entity entity)
    {
        var baseIndex = Array.IndexOf(gameState.BaseSerialNumbers, entity.SerialNumber);
        var basePositions = Game.GetTilesPositionsByType(gameState.Tiles, TileType.Base);
        var endPos = basePositions[baseIndex % basePositions.Length];

        var startPos = Game.GetEntityStartPositionForPathfinder(gameState, entity);
        var path = Pathfinding.CalculatePath(gameState.Tiles,
                TileType.Road | TileType.InactiveBuffer | TileType.BoosterUnplaceableRoad,
                startPos,
                endPos);
        entity.CurrentPathSegment = 0;
        entity.Path = path;
    }

    public static Entity SpawnCreepEntity(GameState gameState, float2 pos, EntitySo config, int spawnerSerialNumber = 0)
    {
        var entity = SpawnEntity(gameState, pos, config);
        entity.SerialNumber = spawnerSerialNumber;
        SetCreepPath(gameState, entity);

        entity.IsWalking = true;
        entity.WalkingTowardsBase = true;
        entity.TargetTeam = TeamType.Good;
        entity.Team = TeamType.Evil;

        entity.Components = ComponentType.Walking | ComponentType.Mortal | ComponentType.BaseDamager;

        SpawnEntityDisplay(gameState, entity, entity.DisplayPrefab).OnInit(entity);
        return entity;
    }

    public static Entity SpawnBuffer(GameState gameState, float2 pos, EntitySo config)
    {
        var entity = SpawnEntity(gameState, pos, config);

        entity.TargetTeam = TeamType.Evil;
        entity.Team = TeamType.Good;

        entity.Components = ComponentType.Buffer |
            ComponentType.Mortal |
            ComponentType.TargetDamager |
            ComponentType.Cooldown |
            ComponentType.TeamTargeter;

        SpawnEntityDisplay(gameState, entity, entity.DisplayPrefab).OnInit(entity);
        UpdateTile(gameState, entity.IntPos.x, entity.IntPos.y, TileType.ActiveBuffer);
        return entity;
    }

    public static Entity SpawnEntitySpawner(GameState gameState, float2 pos, int serialNumber)
    {
        var entity = SpawnEntity(gameState, pos, gameState.Config.EntitySpawnerConfig);

        // TODO(sqdrck): Config these.
        entity.SerialNumber = serialNumber;

        entity.Components = ComponentType.EntitySpawner | ComponentType.Cooldown;
        SpawnEntityDisplay(gameState, entity, entity.DisplayPrefab).OnInit(entity); ;
        return entity;
    }

    public static int2[] GetTilesPositionsWithTypes(Tile[,] tiles, TileType type)
    {
        Span<int2> result = stackalloc int2[tiles.GetLength(0) * tiles.GetLength(1)];
        int count = 0;
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                if ((tiles[x, y].Type & type) != 0)
                {
                    result[count++] = new int2(x, y);
                }
            }
        }

        return result.Slice(0, count).ToArray();
    }

    public static int2[] GetTilesPositionsByType(Tile[,] tiles, TileType type)
    {
        Span<int2> result = stackalloc int2[tiles.GetLength(0) * tiles.GetLength(1)];
        int count = 0;
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                if (tiles[x, y].Type == type)
                {
                    result[count++] = new int2(x, y);
                }
            }
        }

        return result.Slice(0, count).ToArray();
    }

    public static bool DealDamageToBarrier(GameState gameState, int index, float lifePart)
    {
        // TODO(sqdrck): Calculate DamageDone.
        var barrier = gameState.Entities[index];
        barrier.LifeT -= barrier.LifeDuration * 0.2f;
        return barrier.LifeT <= 0;
    }

    public static bool DealDamage(GameState gameState,
            int index,
            int damage,
            int dealerIndex,
            float critChance = 0,
            bool ultimate = false)
    {
        if (damage == 0) return false;
        var entity = gameState.Entities[index];
        if (entity.Hp <= 0) return false;
        var dealer = gameState.Entities[dealerIndex];
        var crit = UnityEngine.Random.Range(0f, 1f) < critChance;
        if (crit)
        {
            damage *= 2;
        }
        if (gameState.Entities[index].Hp > 0)
        {
            damage = Mathf.RoundToInt(damage * (1 - entity.Armor));
            var realDamage = entity.Hp < damage ? entity.Hp : damage;
            entity.Hp -= realDamage;
            dealer.DamageDone += realDamage;
            for (int i = 0; i < dealer.BuffCastProperties?.Length; i++)
            {
                if (dealer.BuffCastProperties[i].ApplyType is BuffApplyType.ByDealingDamage)
                {
                    ApplyBuff(gameState, entity, dealer.BuffCastProperties[i]);
                }
            }
            SpawnFloatingDamageDisplay(gameState, entity.Pos, realDamage, crit, ultimate);
        }
        if (entity.Hp <= 0)
        {
            SpawnDeathAnimationDisplay(gameState, entity.Pos);
        }
        return entity.Hp <= 0;
    }

    public static ResourcePack OpenLootbox(GameState gameState, int lootboxIndex)
    {
        var entry = gameState.Inventory.Lootboxes[lootboxIndex];
        var lbSo = Game.GetLootboxSoByIndex(gameState, entry.LootboxSoIndex);
        var pack = lbSo.Pack;

        Game.ApplyResourcePack(gameState, lbSo.Pack);

        gameState.Inventory.Lootboxes.RemoveAt(lootboxIndex);
        return pack;
    }

    public static int[] GetEntitiesWithComponents(GameState gameState, ComponentType mask)
    {
        Span<int> entities = stackalloc int[gameState.Entities.Length];
        int count = 0;
        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            var e = gameState.Entities[i];
            if ((e.Components & mask) == ComponentType.None || !e.IsActive) continue;
            entities[count++] = i;
        }

        var result = entities.Slice(0, count).ToArray();

        return result;
    }

    public static int[] GetEntitiesInRadius(GameState gameState, float2 center, float radius, EntityType mask, ComponentType componentMask)
    {
        Span<int> entities = stackalloc int[gameState.Entities.Length];
        int count = 0;
        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            var e = gameState.Entities[i];
            if (componentMask != ComponentType.None)
            {
                if (!e.ContainsComponent(componentMask) || e.Type != mask || !e.IsActive) continue;
            }
            else
            {

                if (e.Type != mask || !e.IsActive) continue;
            }

            var dist = center - e.Pos;
            float sqrMagnitude = dist.x * dist.x + dist.y * dist.y;
            if (sqrMagnitude <= radius * radius)
            {
                entities[count++] = i;
            }
        }

        var result = entities.Slice(0, count).ToArray();

        return result;
    }

    public static int[] GetEntitiesInRadius(GameState gameState, float2 center, float radius, TeamType mask,
            ComponentType componentMask = ComponentType.None, bool considerImmortalityTiles = true)
    {
        Span<int> entities = stackalloc int[gameState.Entities.Length];
        int count = 0;
        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            var e = gameState.Entities[i];
            if (componentMask != ComponentType.None)
            {

                if (!e.ContainsComponent(componentMask) || e.Team != mask || !e.IsActive) continue;
            }
            else
            {
                if (e.Team != mask || !e.IsActive) continue;
            }
            var dist = e.Pos - center;
            float sqrMagnitude = dist.x * dist.x + dist.y * dist.y;
            if (sqrMagnitude <= radius * radius)
            {
                if (!considerImmortalityTiles || !IsImmortalOnItsPosition(gameState, i))
                {
                    entities[count++] = i;
                }
            }
        }

        var result = entities.Slice(0, count).ToArray();

        return result;
    }

    public static bool IsImmortalOnItsPosition(GameState gameState, int entityIndex)
    {
        // NOTE(sqdrck): For now it's booster unplacable road.
        var currentType = gameState.Tiles[gameState.Entities[entityIndex].IntPos.x, gameState.Entities[entityIndex].IntPos.y].Type;
        return currentType is TileType.BoosterUnplaceableRoad || currentType is TileType.EnemySpawner;
    }

    public static int GetLowestHpInRadius(GameState gameState, float2 center, float radius, TeamType mask,
            ComponentType componentMask = ComponentType.None)
    {
        var entitiesInRadius = GetEntitiesInRadius(gameState, center, radius, mask, componentMask);

        int lowestHpIndex = -1;
        int lowestHp = int.MaxValue;
        for (int i = 0; i < entitiesInRadius.Length; i++)
        {
            var hp = gameState.Entities[entitiesInRadius[i]].Hp;
            if (hp < lowestHp)
            {
                lowestHpIndex = entitiesInRadius[i];
                lowestHp = hp;
            }
        }

        return lowestHpIndex;
    }

    public static int GetHighestHpInRadius(GameState gameState, float2 center, float radius, TeamType mask,
            ComponentType componentMask = ComponentType.None)
    {
        var entitiesInRadius = GetEntitiesInRadius(gameState, center, radius, mask, componentMask);

        int highestHpIndex = -1;
        int highestHp = int.MinValue;
        for (int i = 0; i < entitiesInRadius.Length; i++)
        {
            var hp = gameState.Entities[entitiesInRadius[i]].Hp;
            if (hp > highestHp)
            {
                highestHpIndex = entitiesInRadius[i];
                highestHp = hp;
            }
        }

        return highestHpIndex;
    }

    public static int GetClosestToBaseInRadius(GameState gameState, float2 center, float radius, TeamType mask,
            ComponentType componentMask = ComponentType.None)
    {
        var entitiesInRadius = GetEntitiesInRadius(gameState, center, radius, mask, componentMask);

        int closestIndex = -1;
        int shortestPath = int.MaxValue;
        for (int i = 0; i < entitiesInRadius.Length; i++)
        {
            var e = gameState.Entities[entitiesInRadius[i]];
            if (!e.WalkingTowardsBase || e.Path == null) continue;
            if (e.Path.Length < shortestPath)
            {
                shortestPath = e.Path.Length;
                closestIndex = entitiesInRadius[i];
            }
        }

        return closestIndex;
    }

    public static int GetClosestActiveEntityByRadius(GameState gameState, float2 center, float radius, EntityType mask,
            ComponentType componentMask = ComponentType.None)
    {
        var entitiesInRadius = GetEntitiesInRadius(gameState, center, radius, mask, componentMask);

        int closestIndex = -1;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < entitiesInRadius.Length; i++)
        {
            var dist = (center - gameState.Entities[entitiesInRadius[i]].Pos);
            float sqrMagnitude = dist.x * dist.x + dist.y * dist.y;
            if (sqrMagnitude < closestDistance)
            {
                closestIndex = entitiesInRadius[i];
                closestDistance = sqrMagnitude;
            }
        }

        return closestIndex;
    }

    public static int GetClosestActiveEntityByRadius(GameState gameState, float2 center, float radius, TeamType mask, ComponentType componentMask = ComponentType.None)
    {
        var entitiesInRadius = GetEntitiesInRadius(gameState, center, radius, mask, componentMask);

        int closestIndex = -1;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < entitiesInRadius.Length; i++)
        {
            var dist = (center - gameState.Entities[entitiesInRadius[i]].Pos);
            float sqrMagnitude = dist.x * dist.x + dist.y * dist.y;
            if (sqrMagnitude < closestDistance)
            {
                closestIndex = entitiesInRadius[i];
                closestDistance = sqrMagnitude;
            }
        }

        return closestIndex;
    }

    public static void UpdateBufferBreaker(GameState gameState, Entity entity)
    {
        if (entity.IsWalking)
        {
            // NOTE(sqdrck): Coming to path end point and got stopped by barrier.
            if (entity.Speed == 0)
            {
                if (entity.CooldownT <= 0)
                {
                    var barrierIndex = Game.GetClosestActiveEntityByRadius(gameState,
                            entity.Pos,
                            entity.DamageRadius,
                            EntityType.Barrier);

                    if (barrierIndex != -1)
                    {
                        Game.DealDamageToBarrier(gameState, barrierIndex, 0.2f);
                        entity.CooldownT = entity.CooldownDuration;
                    }

                }
            }
        }
        if (!entity.IsWalking && !entity.WalkingTowardsBase)
        {
            if (entity.TargetIndex == -1 || !gameState.Entities[entity.TargetIndex].IsActive)
            {
                SetBufferBreakerPath(gameState, entity);
            }
            else if (entity.CooldownT <= 0)
            {
                entity.CooldownT = entity.CooldownDuration;
                if (entity.TargetType is EntityType.Buffer)
                {
                    if (Game.DealDamage(gameState, entity.TargetIndex, entity.RandomDamage, entity.Index, entity.CritChance))
                    {
                        DestroyBuffer(gameState, gameState.Entities[entity.TargetIndex]);
                        entity.TargetIndex = -1;
                        entity.BuffersDestroyedCount++;
                        SetBufferBreakerPath(gameState, entity);
                    }
                }
                else if (entity.TargetType is EntityType.Barrier)
                {
                    if (Game.DealDamageToBarrier(gameState, entity.TargetIndex, 0.2f))
                    {
                        entity.TargetIndex = -1;
                        entity.BuffersDestroyedCount++;
                        SetBufferBreakerPath(gameState, entity);
                    }
                }
            }
        }
    }

    public static void UpdateTargetDamager(GameState gameState, Entity entity)
    {
        bool heroHasTwoPhaseDamage = entity.ContainsComponent(ComponentType.CooldownWithActivePhase);
        bool isInActivePhase = heroHasTwoPhaseDamage &&
            entity.Cooldown1T > 0 && // active phase is continuing 
            entity.Cooldown2T <= 0; // active phass shot cooldown is done
        if ((heroHasTwoPhaseDamage && isInActivePhase) || (!heroHasTwoPhaseDamage && entity.CooldownT <= 0))
        {
            if (entity.TargetIndex != -1)
            {
                if (entity.DamageType is DamageType.TargetOnly)
                {
                    entity.AmmoLeft--;
                    Game.DealDamage(gameState, entity.TargetIndex, entity.RandomDamage, entity.Index, entity.CritChance);
                }
                else if (entity.DamageType is DamageType.AoeOnTarget ||
                        entity.DamageType is DamageType.AoeOnSelf ||
                        entity.DamageType is DamageType.AoeOnRandomTile ||
                        entity.DamageType is DamageType.AoeInFrontOfTarget)
                {
                    entity.AmmoLeft--;
                    Game.SpawnAoeDamage(gameState, entity);
                }
                else
                {
                    Debug.LogError("Not implemented damage type");
                }
            }
            else if (entity.TargetIndices?.Length > 0)
            {
                entity.AmmoLeft--;
                for (int i = 0; i < entity.TargetIndices.Length; i++)
                {
                    entity.CooldownT = entity.CooldownDuration;
                    var damage = entity.RandomDamage;
                    Game.DealDamage(gameState,
                            entity.TargetIndices[i],
                            damage,
                            entity.Index,
                            entity.CritChance);
                }
            }
        }
    }

    public static void UpdateTeamTargeter(GameState gameState, Entity entity)
    {
        if (entity.PriorityType is PriorityType.AllInRange)
        {
            entity.TargetIndices = Game.GetEntitiesInRadius(gameState, entity.Pos, entity.DamageRadius, entity.TargetTeam);
        }
        else
        {
            if (entity.TargetIndex != -1)
            {
                var dist = entity.Pos - gameState.Entities[entity.TargetIndex].Pos;
                float sqrMagnitude = dist.x * dist.x + dist.y * dist.y;
                if (!gameState.Entities[entity.TargetIndex].IsActive ||
                        gameState.Entities[entity.TargetIndex].Hp <= 0 ||
                        (sqrMagnitude > entity.DamageRadius * entity.DamageRadius))
                {
                    entity.TargetIndex = -1;
                }
            }

            if (!entity.ForceKeepTarget || entity.TargetIndex == -1)
            {
                int targetIndex = -1;
                if (entity.PriorityType is PriorityType.Closest)
                {
                    targetIndex = Game.GetClosestActiveEntityByRadius(gameState,
                            entity.Pos,
                            entity.DamageRadius,
                            entity.TargetTeam,
                            ComponentType.Mortal);
                }
                else if (entity.PriorityType is PriorityType.LowestHp)
                {
                    // TODO(sqdrck): Keep target if found one is same hp?
                    targetIndex = Game.GetLowestHpInRadius(gameState,
                            entity.Pos,
                            entity.DamageRadius,
                            entity.TargetTeam,
                            ComponentType.Mortal);
                }
                else if (entity.PriorityType is PriorityType.HighestHp)
                {
                    targetIndex = Game.GetHighestHpInRadius(gameState,
                            entity.Pos,
                            entity.DamageRadius,
                            entity.TargetTeam,
                            ComponentType.Mortal);
                }
                else if (entity.PriorityType is PriorityType.ClosestToBase)
                {
                    targetIndex = Game.GetClosestToBaseInRadius(gameState,
                            entity.Pos,
                            entity.DamageRadius,
                            entity.TargetTeam,
                            ComponentType.Mortal);
                }
                else
                {
                    targetIndex = -1;
                }

                if (targetIndex != -1)
                {
                    entity.TargetIndex = targetIndex;
                }

            }

            if (entity.TargetIndex != -1)
            {
                entity.TargetEntityPos = gameState.Entities[entity.TargetIndex].Pos;
            }
        }
    }

    public static void UpdateCooldownWithActivePhase(GameState gameState, Entity entity)
    {
        if (entity.CooldownT <= 0)
        {
            if (entity.Cooldown1T <= 0)
            {
                entity.CooldownT = entity.CooldownDuration;
                entity.Cooldown1T = entity.Cooldown1Duration;
                entity.Cooldown2T = entity.Cooldown2Duration;
            }
            else
            {
                entity.Cooldown1T -= Time.fixedDeltaTime;
                if (entity.Cooldown2T <= 0)
                {
                    entity.Cooldown2T = entity.Cooldown2Duration;
                }
                else
                {
                    entity.Cooldown2T -= Time.fixedDeltaTime;
                }
            }
        }
        else if (!entity.UpdateCooldownOnlyIfHasTarget || entity.TargetIndex != -1)
        {
            entity.CooldownT -= Time.fixedDeltaTime;
        }
    }

    public static void UpdateCooldown(GameState gameState, Entity entity)
    {
        if (entity.CooldownT <= 0 || (entity.UpdateCooldownOnlyIfHasTarget && entity.TargetIndex == -1))
        {
            entity.CooldownT = entity.CooldownDuration;
        }
        else if (entity.CooldownT > 0 && !entity.UpdateCooldownOnlyIfHasTarget || entity.TargetIndex != -1)
        {
            entity.CooldownT -= Time.fixedDeltaTime;
        }
    }

    public static void UpdateExpiring(GameState gameState, Entity entity)
    {
        if (entity.LifeT > 0)
        {
            entity.LifeT -= Time.fixedDeltaTime;
        }
        else
        {
            entity.IsActive = false;
            entity.IsReadyToRecycle = true;
        }
    }

    public static void UpdateWalking(GameState gameState, Entity entity)
    {
        if (!entity.IsWalking) return;
        entity.SegmentT += Time.fixedDeltaTime * entity.Speed;
        if (entity.SegmentT > 1)
        {
            entity.CurrentPathSegment++;
            entity.SegmentT -= 1;
        }
        float t = Mathf.Lerp(0, 1, entity.SegmentT);
        if (entity.CurrentPathSegment < entity.Path.Length - 1)
        {
            var newPos = math.lerp(entity.Path[entity.CurrentPathSegment],
                    entity.Path[entity.CurrentPathSegment + 1],
                    entity.SegmentT);

            entity.Pos = newPos;
        }
        else
        {
            // NOTE(sqdrck): Reached destination.
            entity.Pos = entity.Path[entity.Path.Length - 1];
            entity.IsWalking = false;
        }
    }

    public static void DestroyBuffer(GameState gameState, Entity entity)
    {
        UpdateTile(gameState, entity.IntPos.x, entity.IntPos.y, TileType.InactiveBuffer);
        // TODO(sqdrck): Zero entity.
        entity.IsActive = false;
        entity.IsReadyToRecycle = true;
    }

    public static int[] GetUniqueWaves(GameState gameState)
    {
        var len = GetCurrentStage(gameState).Waves.Length;
        Span<int> waveNumbers = stackalloc int[len];
        for (int i = 0; i < waveNumbers.Length; i++)
        {
            waveNumbers[i] = -1;
        }
        int count = 0;
        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            var e = gameState.Entities[i];
            if (e.WaveIndex != -1 && e.IsActive && waveNumbers.IndexOf(e.WaveIndex) == -1)
            {
                waveNumbers[count++] = e.WaveIndex;
            }
        }
        var result = waveNumbers.Slice(0, count).ToArray();

        return result;
    }

    public static void UpdateEntitySpawner(GameState gameState, Entity entity)
    {
        if (entity.CooldownT <= 0)
        {
            var soIndex = entity.SpawnerWaveInfo.Entries[entity.SpawnerWaveEntryIndex].EntitySoIndex;

            var entityToSpawn = Game.GetEntitySoByIndex(gameState, soIndex);
            Entity spawnedEntity = null;
            if (entityToSpawn.Type is EntityType.Creep)
            {
                spawnedEntity = Game.SpawnCreepEntity(gameState, entity.Pos, entityToSpawn, entity.SerialNumber);
            }
            else if (entityToSpawn.Type is EntityType.BufferBreaker)
            {
                spawnedEntity = Game.SpawnBufferBreaker(gameState, entity.Pos, entityToSpawn);
            }
            else if (entityToSpawn.Type is EntityType.Psycho)
            {
                spawnedEntity = Game.SpawnPsycho(gameState, entity.Pos, entityToSpawn);
            }
            else
            {
                throw new NotImplementedException();
            }

            spawnedEntity.WaveIndex = entity.WaveIndex;

            if (!Game.SetNextEntryForEntitySpawner(gameState, entity))
            {
                entity.IsActive = false;
                entity.IsReadyToRecycle = true;
            }
        }
    }

    public static void UpdateMortal(GameState gameState, Entity entity)
    {
        if (entity.Hp <= 0)
        {
            entity.IsActive = false;
            entity.IsReadyToRecycle = true;
        }
    }

    public static Entity SpawnAoeDamage(GameState gameState, Entity dealer)
    {
        Debug.Log("SpawnAoeDamage");
        EntitySo config = dealer.AoeDamageConfig;
        float2 pos;
        if (dealer.DamageType is DamageType.AoeOnSelf)
        {
            pos = dealer.Pos;
        }
        else if (dealer.DamageType is DamageType.AoeOnTarget)
        {
            pos = gameState.Entities[dealer.TargetIndex].Pos;
        }
        else if (dealer.DamageType is DamageType.AoeOnRandomTile)
        {
            var tiles = Game.GetTilesPositionsWithTypes(gameState.Tiles, config.PlaceableTiles);
            Debug.Log(" " + config.PlaceableTiles.ToString());
            Debug.Log("Tiles len " + tiles.Length);
            int randIndex = UnityEngine.Random.Range(0, tiles.Length);
            var intPos = tiles[randIndex];
            pos = new float2(intPos.x, intPos.y);
        }
        else if (dealer.DamageType is DamageType.AoeInFrontOfTarget)
        {
            Entity target = gameState.Entities[dealer.TargetIndex];
            if (target.CurrentPathSegment + dealer.AoeInFrontOfTargetPreemptivePathSegments < target.Path.Length)
            {
                pos = target.Path[target.CurrentPathSegment + dealer.AoeInFrontOfTargetPreemptivePathSegments];
            }
            else
            {
                pos = target.Path[target.Path.Length - 1];
            }
        }
        else
        {
            throw new();
        }
        var entity = SpawnEntity(gameState, pos, config);
        // TODO(sqdrck): Make this work without cloning dealer.
        entity.LifeT = entity.LifeDuration;
        entity.CooldownT = entity.CooldownDuration;
        entity.TargetIndex = -1;

        dealer.AoePos = entity.Pos;

        SpawnEntityDisplay(gameState, entity, config.Prefab).OnInit(entity);

        return entity;
    }

    public static void DealBaseDamage(GameState gameState, int damage)
    {
        gameState.BaseHp -= damage;
        Vibration.Warning();
        gameState.CameraDisplay.Shake();
        if (gameState.BaseHp <= 0)
        {
            OnLost(gameState);
        }
    }

    public static void UpdateBaseDamager(GameState gameState, Entity entity)
    {
        if (entity.IsWalking) return;

        if (gameState.Tiles[entity.IntPos.x, entity.IntPos.y].Type == TileType.Base)
        {
            DealBaseDamage(gameState, entity.BaseDamage);
            entity.IsActive = false;
            entity.IsReadyToRecycle = true;
        }
    }

    public static int GetTileIndex(Tile[,] tiles, int x, int y)
    {
        return x * tiles.GetLength(0) + y;
    }

    public static int2 GetTilePosition(Tile[,] tiles, int index)
    {
        int l = tiles.GetLength(0);
        int x = index / l;
        int y = index % l;

        return new int2(x, y);
    }

#if UNITY_EDITOR
    //void OnGUI()
    //{
    //GUILayout.Label("Base health: " + gameState.BaseHp);
    //}

    public static void DrawGizmos(GameState gameState)
    {
        if (!gameState.IsInitialized) return;
        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            Entity entity = gameState.Entities[i];
            Handles.Label(new Vector3(entity.Pos.x, entity.Pos.y), i.ToString());
            if (!entity.IsActive) continue;

            if (entity.ContainsComponent(ComponentType.Walking))
            {
                for (int j = 1; j < entity.Path.Length; j++)
                {
                    var pathSegment = entity.Path[j];
                    var prevPathSegment = entity.Path[j - 1];
                    Handles.color = Color.red;
                    Handles.DrawDottedLine(new Vector3(prevPathSegment.x, prevPathSegment.y, 0),
                            new Vector3(pathSegment.x, pathSegment.y, 0), 10);
                }
            }

            if (entity.ContainsComponent(ComponentType.TeamTargeter))
            {
                if (entity.TargetIndex != -1)
                {
                    var p1 = new Vector3(entity.Pos.x, entity.Pos.y);
                    var p2 = new Vector3(gameState.Entities[entity.TargetIndex].Pos.x,
                            gameState.Entities[entity.TargetIndex].Pos.y);
                    var thickness = 6;
                    Handles.DrawBezier(p1, p2, p1, p2, Color.red, null, thickness);
                }
            }
        }

        for (int x = 0; x < gameState.Tiles.GetLength(0); x++)
        {
            for (int y = 0; y < gameState.Tiles.GetLength(1); y++)
            {
                if (gameState.Tiles[x, y].Type == TileType.Base)
                {
                    Handles.Label(new Vector3(x, y, 0), gameState.Tiles[x, y].SerialNumber.ToString());
                }
            }
        }
    }

#endif

    public static StageSo GetCurrentStage(GameState gameState)
    {
        return gameState.Config.Stages[gameState.CurrentStageIndex];
    }

    public static WaveInfo GetLastSummonedWave(GameState gameState)
    {
        return GetCurrentStage(gameState).Waves[gameState.LastSummonedWaveIndex];
    }

    public static DeathAnimationDisplay CreateDeathAnimationDisplay(GameState gameState, float2 pos)
    {
        DeathAnimationDisplay display = FloatingDamageDisplay.Instantiate(gameState.Config.DeathAnimationDisplayPrefab,
                new Vector3(pos.x, pos.y),
                Quaternion.identity);
        display.Pos = pos;
        display.transform.SetParent(gameState.DeathAnimationsDisplayRoot);

        return display;
    }

    public static FloatingDamageDisplay CreateFloatingDamageDisplay(GameState gameState, float2 pos)
    {
        FloatingDamageDisplay display = FloatingDamageDisplay.Instantiate(gameState.Config.FloatingDamageDisplayPrefab,
                new Vector3(pos.x, pos.y),
                Quaternion.identity);
        display.Pos = pos;
        display.transform.SetParent(gameState.FloatingDamageDisplayRoot);

        return display;
    }

    public static void SpawnDeathAnimationDisplay(GameState gameState, float2 pos)
    {
        gameState.DeathAnimationDisplayPoolIndex++;
        if (gameState.DeathAnimationDisplayPoolIndex >= gameState.DeathAnimationDisplays.Length)
        {
            gameState.DeathAnimationDisplayPoolIndex = 0;
        }

        DeathAnimationDisplay display = gameState.DeathAnimationDisplays[gameState.DeathAnimationDisplayPoolIndex];
        display.OnInit(pos);
    }

    public static void SpawnFloatingDamageDisplay(GameState gameState, float2 pos, int damage, bool crit, bool ultimate)
    {
        gameState.FloatingDamageDisplayPoolIndex++;
        if (gameState.FloatingDamageDisplayPoolIndex >= gameState.FloatingDamageDisplays.Length)
        {
            gameState.FloatingDamageDisplayPoolIndex = 0;
        }

        FloatingDamageDisplay display = gameState.FloatingDamageDisplays[gameState.FloatingDamageDisplayPoolIndex];
        display.OnInit(pos, damage, crit, ultimate);
        display.IsActive = true;
    }

    public static void UpdateFloatingDamageDisplay(FloatingDamageDisplay display)
    {
        display.OnFixedUpdate();
        if (display.T > 1)
        {
            display.IsActive = false;
        }
    }

    public static void OnInitFromDeepSave(GameState gameState)
    {
        OnPostInit(gameState, false);
    }

    public static void OnPostInit(GameState gameState, bool reinitAllEntities)
    {
        // NOTE(sqdrck): Create entities pool.
        // TODO(sqdrck): Profile and find optimal count.
        int maxEntitiesCount = 1000;
        for (int i = 0; i < gameState.Config.Stages.Length; i++)
        {
            for (int j = 0; j < gameState.Config.Stages[i].Waves.Length - 1; j++)
            {
                WaveInfo wi = gameState.Config.Stages[i].Waves[j];
                WaveInfo nextWi = gameState.Config.Stages[i].Waves[j + 1];
                // NOTE(sqdrck): We consider that user can summon next wave before previous ends.
                int twoWavesEntitiesCount = wi.Entries.Length + nextWi.Entries.Length;

                if (maxEntitiesCount < twoWavesEntitiesCount)
                {
                    maxEntitiesCount = twoWavesEntitiesCount;
                }
            }
            if (gameState.Config.Stages[i].Waves.Length == 1)
            {
                if (maxEntitiesCount < gameState.Config.Stages[i].Waves[0].Entries.Length)
                {
                    maxEntitiesCount = gameState.Config.Stages[i].Waves[0].Entries.Length;
                }
            }
        }
        Debug.Log("Max entities count is: " + maxEntitiesCount);
        gameState.Entities = new Entity[maxEntitiesCount];
        gameState.EntityDisplays = new EntityDisplay[maxEntitiesCount];
        for (int i = 0; i < maxEntitiesCount; i++)
        {
            gameState.Entities[i] = new Entity();
            gameState.Entities[i].Index = i;
            gameState.Entities[i].IsActive = false;
            gameState.Entities[i].IsReadyToRecycle = true;
        }

        var go = new GameObject("FloatingDamageRoot");
        gameState.FloatingDamageDisplayRoot = go.transform;
        go = new GameObject("TilesRoot");
        gameState.TilesRoot = go.transform;
        go = new GameObject("EntitiesRoot");
        gameState.EntitiesRoot = go.transform;
        gameState.FloatingDamageDisplays = new FloatingDamageDisplay[50];
        for (int i = 0; i < gameState.FloatingDamageDisplays.Length; i++)
        {
            gameState.FloatingDamageDisplays[i] = Game.CreateFloatingDamageDisplay(gameState, new Vector2(-10, -10));
            gameState.FloatingDamageDisplays[i].IsActive = false;
            gameState.FloatingDamageDisplayPoolIndex = 0;
        }

        go = new GameObject("DeathAnimationsRoot");
        gameState.DeathAnimationsDisplayRoot = go.transform;
        gameState.DeathAnimationDisplays = new DeathAnimationDisplay[50];
        for (int i = 0; i < gameState.DeathAnimationDisplays.Length; i++)
        {
            gameState.DeathAnimationDisplays[i] = Game.CreateDeathAnimationDisplay(gameState, new Vector2(-10, -10));
            gameState.DeathAnimationDisplays[i].IsActive = false;
            gameState.DeathAnimationDisplayPoolIndex = 0;
        }

        gameState.TileDisplays = new TileDisplay[gameState.Tiles.GetLength(0), gameState.Tiles.GetLength(1)];
        for (int x = 0; x < gameState.Tiles.GetLength(0); x++)
        {
            for (int y = 0; y < gameState.Tiles.GetLength(1); y++)
            {
                var tile = gameState.Tiles[x, y];
                CreateTileDisplay(gameState, x, y, tile);
            }
        }

        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            var entity = gameState.Entities[i];
            if (!entity.IsActive) continue;
            var entitySo = GetEntitySoByIndex(gameState, entity.EntitySoIndex);
            SetupEntityFromConfig(entity, entitySo, reinitAllEntities);
            var display = Game.SpawnEntityDisplay(gameState, entity, entity.DisplayPrefab);
            display.OnInit(entity);
        }

        // NOTE(sqdrck): Enlarge user levels if added new.
        if (gameState.CollectedLevelRewards.Length != gameState.UserProgressionConfig.UserProgressionItems.Count)
        {
            Array.Resize(ref gameState.CollectedLevelRewards, gameState.UserProgressionConfig.UserProgressionItems.Count);
        }

        if (gameState.EntityLevels.Count != gameState.SoDatabase.EntitySoDatabase.Count)
        {
            foreach (var pair in gameState.SoDatabase.EntitySoDatabase)
            {
                if (!gameState.EntityLevels.ContainsKey(pair.Key))
                {
                    Debug.Log("Added new entity. Setting level to 1");
                    gameState.EntityLevels[pair.Key] = 1;
                }
            }
        }

        if (gameState.MissionsProgress.Count != gameState.Settings.Missions.Length)
        {
            foreach (var mission in gameState.Settings.Missions)
            {
                if (!gameState.MissionsProgress.ContainsKey(mission.Index))
                {
                    Debug.Log("Added new mission. Setting progress to 0.");
                    MissionProgress save = new MissionProgress();
                    gameState.MissionsProgress[mission.Index] = save;
                }
            }
        }

        if (gameState.EntitiesUnlocked.Count != gameState.SoDatabase.EntitySoDatabase.Count)
        {
            foreach (var pair in gameState.SoDatabase.EntitySoDatabase)
            {
                if (!gameState.EntitiesUnlocked.ContainsKey(pair.Key))
                {
                    Debug.Log("Added new entity. Setting level to 1");
                    gameState.EntitiesUnlocked[pair.Key] = pair.Value.UnlockedByDefault;
                }
            }
        }
    }

    public static void OnInitFromShallowSave(GameState gameState, ShallowEntity[] shallowEntities)
    {
        OnPostInit(gameState, false);

        for (int i = 0; i < shallowEntities.Length; i++)
        {
            ShallowEntity se = shallowEntities[i];
            EntitySo config = GetEntitySoByIndex(gameState, se.EntitySoIndex);
            float2 pos = new float2(se.X, se.Y);
            Entity e;
            if (se.Type is EntityType.Hero)
            {
                e = SpawnHeroEntity(gameState, pos, config);
            }
            else if (se.Type is EntityType.Buffer)
            {
                e = SpawnBuffer(gameState, pos, config);
            }
            else
            {
                throw new NotImplementedException("This type of entity cannot be deserialized");
            }

            e.Level = se.Level;
        }
        gameState.CurrentPhase = GamePhase.Preparation;
        gameState.IsStageLost = false;
        gameState.IsStageEnded = true;
        gameState.BaseMaxHp = 5;
        gameState.BaseHp = gameState.BaseMaxHp;
        gameState.IsInitialized = true;
    }

    public static void OnNewGameStateInit(GameState gameState)
    {
        int boardX = gameState.Config.Board.Tiles.GetLength(0);
        int boardY = gameState.Config.Board.Tiles.GetLength(1);
        gameState.Tiles = new Tile[boardX, boardY];
        for (int x = 0; x < boardX; x++)
        {
            for (int y = 0; y < boardY; y++)
            {
                var tile = CreateTile(gameState, x, boardY - y - 1, gameState.Config.Board.Tiles[x, y]);
                gameState.Tiles[x, boardY - y - 1] = tile;
            }
        }
        gameState.CurrentPhase = GamePhase.Preparation;
        gameState.IsStageLost = false;
        gameState.IsStageEnded = true;
        gameState.BaseMaxHp = 5;
        gameState.BaseHp = gameState.BaseMaxHp;
        gameState.CollectedLevelRewards = new bool[gameState.UserProgressionConfig.UserProgressionItems.Count];
        gameState.LastCollectedDailyRewardIndex = -1;
        //gameState.CollectedDailyRewards = new bool[gameState.Settings.DailyRewards.Length];
        gameState.LastTimeCollectedDailyReward = DateTimeOffset.MinValue;
        gameState.EntitiesUnlocked = new();
        foreach (var pair in gameState.SoDatabase.EntitySoDatabase)
        {
            gameState.EntitiesUnlocked[pair.Key] = pair.Value.UnlockedByDefault;
        }

        gameState.MissionsProgress = new();
        foreach (var missionSo in gameState.Settings.Missions)
        {
            MissionProgress save = new MissionProgress();
            gameState.MissionsProgress.Add(missionSo.Index, save);
        }

        gameState.EntityLevels = new();
        foreach (var pair in gameState.SoDatabase.EntitySoDatabase)
        {
            gameState.EntityLevels[pair.Key] = 1;
        }
        gameState.IsLastStage = false;
        gameState.CurrentStageIndex = 0;

        OnPostInit(gameState, true);

        var bufferPositions = Game.GetTilesPositionsByType(gameState.Tiles, TileType.InactiveBuffer);
        for (int i = 0; i < bufferPositions.Length; i++)
        {
            Game.SpawnBuffer(gameState, bufferPositions[i], gameState.Config.WoodFenceConfig);
        }

        var towerPositions = Game.GetTilesPositionsByType(gameState.Tiles, TileType.Tower);

        for (int i = 0; i < towerPositions.Length; i++)
        {
            //Game.SpawnHeroEntity(gameState, towerPositions[i], gameState.Config.DefaultHeroConfig);
            //break;
        }

        gameState.IsInitialized = true;
    }

    public static Entity SpawnBarrier(GameState gameState, float2 pos)
    {
        var entity = SpawnEntity(gameState, pos, gameState.Config.BarrierConfig);
        entity.Components = ComponentType.Expiring | ComponentType.BuffCaster;

        SpawnEntityDisplay(gameState, entity, entity.DisplayPrefab).OnInit(entity);
        return entity;
    }

    public static Entity SpawnTravolator(GameState gameState, float2 pos)
    {
        var entity = SpawnEntity(gameState, pos, gameState.Config.TravolatorConfig);
        entity.Components = ComponentType.BuffCaster | ComponentType.Expiring;

        SpawnEntityDisplay(gameState, entity, entity.DisplayPrefab).OnInit(entity);
        return entity;
    }

    public static void UpdateDisableOnEmptyAmmo(GameState gameState, Entity entity)
    {
        if (entity.AmmoLeft <= 0)
        {
            entity.IsActive = false;
            entity.IsReadyToRecycle = true;
        }

    }

    public static InventoryEntry GetClosestLootboxFromInventory(GameState gameState)
    {
        InventoryEntry closestEntry = null;
        if (gameState.Inventory.Lootboxes.Count > 0)
        {
            DateTimeOffset closestUnlockTime = DateTimeOffset.MaxValue;
            for (int i = 0; i < gameState.Inventory.Lootboxes.Count; i++)
            {
                var val = gameState.Inventory.Lootboxes[i];
                if (val.UnlockTime < closestUnlockTime)
                {
                    closestUnlockTime = val.UnlockTime;
                    closestEntry = val;
                }
            }
        }

        return closestEntry;
    }

    public static void ApplyBuff(GameState gameState, Entity entity, BuffProperties buff)
    {
        if (!entity.AppliedBuffsProperties.ContainsKey(buff.Index))
        {
            if (buff.ApplyImmediately)
            {
                buff.CooldownT = 0;
            }
            else
            {
                buff.CooldownT = buff.CooldownDuration;
            }
        }
        else
        {
            buff.CooldownT = entity.AppliedBuffsProperties[buff.Index].CooldownT;
        }
        buff.LifeT = buff.LifeDuration;
        entity.AppliedBuffsProperties[buff.Index] = (buff);
    }

    public static void UpdateBuffCaster(GameState gameState, Entity entity)
    {
        for (int i = 0; i < entity.BuffCastProperties?.Length; i++)
        {
            var buff = entity.BuffCastProperties[i];
            if (buff.ApplyType is BuffApplyType.ByRadius)
            {
                var buffTargetsIndices = Game.GetEntitiesInRadius(gameState,
                        entity.Pos,
                        entity.BuffCastProperties[i].CastRadius,
                        entity.BuffCastProperties[i].CastTeam);

                for (int j = 0; j < buffTargetsIndices?.Length; j++)
                {
                    var e = gameState.Entities[buffTargetsIndices[j]];
                    if (e.Index != entity.Index)
                    {
                        ApplyBuff(gameState, e, buff);
                    }
                }
            }
        }
    }

    public static void UpdateAppliedBuffs(GameState gameState, Entity entity)
    {
        if (entity.AppliedBuffsProperties.Count == 0)
        {
            return;
        }

        for (int i = 0; i < entity.AppliedBuffsProperties.Count; i++)
        {
            var buff = entity.AppliedBuffsProperties.ElementAt(i).Value;
            if (buff.Type is BuffType.AffectSpeed)
            {
                buff = UpdateBuffAffectSpeed(gameState, entity, buff);
            }
            if (buff.Type is BuffType.DamageOverTime)
            {
                buff = UpdateBuffDamageOverTime(gameState, entity, buff);
            }
            entity.AppliedBuffsProperties[buff.Index] = buff;
        }
    }

    public static BuffProperties UpdateBuffDamageOverTime(GameState gameState, Entity entity, BuffProperties buff)
    {
        if (buff.CooldownT <= 0)
        {
            if (buff.UseDamagePropertiesFromCaster)
            {
                var caster = gameState.Entities[buff.CasterIndex];
                DealDamage(gameState, entity.Index, caster.RandomDamage, buff.CasterIndex, caster.CritChance);
            }
            else
            {
                DealDamage(gameState, entity.Index, buff.RandomDamage, buff.CasterIndex, buff.CritChance);
            }
            buff.CooldownT = buff.CooldownDuration;
        }
        else
        {
            buff.CooldownT -= Time.fixedDeltaTime;
        }

        return buff;
    }

    public static void ZeroAppliedBuffs(GameState gameState, Entity entity)
    {
        if (entity.AppliedBuffsProperties.Count == 0)
        {
            return;
        }
        Span<int> indicesToRemove = stackalloc int[entity.AppliedBuffsProperties.Count];
        int count = 0;
        for (int i = 0; i < entity.AppliedBuffsProperties.Count; i++)
        {
            var buff = entity.AppliedBuffsProperties.ElementAt(i).Value;
            bool cancel = false;

            // TODO(sqdrck): Caster might be recycled at this moment.
            // Need to store some unique GUID in every entity to resolve issue
            // when Entity points at another entity.
            Entity caster = gameState.Entities[buff.CasterIndex];
            var distToCaster = math.distancesq(entity.Pos, caster.Pos);
            bool leftDamageRadius = (buff.CancelIfLeftDamageRadius &&
                    (distToCaster > caster.DamageRadius * caster.DamageRadius));
            bool leftCastRadius = (buff.CancelIfLeftCastRadius &&
                    (distToCaster > buff.CastRadius * buff.CastRadius));

            cancel = buff.LifeT <= 0 || leftDamageRadius || leftCastRadius;

            if (cancel)
            {
                // NOTE(sqdrck): Zero out changed properties.
                if (buff.Type is BuffType.AffectSpeed)
                {
                    entity.Speed = entity.InitialSpeed;
                }

                indicesToRemove[count++] = buff.Index;
            }
            else
            {
                buff.LifeT -= Time.fixedDeltaTime;
                entity.AppliedBuffsProperties[buff.Index] = buff;
            }
        }

        for (int i = 0; i < count; i++)
        {
            entity.AppliedBuffsProperties.Remove(indicesToRemove[i]);
        }
    }

    public static BuffProperties UpdateBuffAffectSpeed(GameState gameState, Entity entity, BuffProperties buff)
    {
        if (buff.CooldownT > 0)
        {
            buff.CooldownT -= Time.fixedDeltaTime;
        }
        else
        {
            if (buff.StrengthType is BuffStrengthType.Absolute)
            {
                entity.Speed += buff.Strength;
                if (entity.Speed < 0)
                {
                    entity.Speed = 0;
                }
            }
            else if (buff.StrengthType is BuffStrengthType.Relative)
            {
                entity.Speed *= buff.Strength;
            }

            buff.CooldownT = buff.CooldownDuration;
        }

        return buff;
    }

    public static void UpdateUltimate(GameState gameState, Entity entity)
    {
        if (gameState.CurrentPhase is GamePhase.Preparation)
        {
            entity.UltimateAccumulated = 0;
        }
        else
        {
            entity.TimeLived += Time.fixedDeltaTime;
            entity.CalculatedDps = Mathf.CeilToInt(entity.DamageDone / entity.TimeLived);
            if (entity.UltimateAccumulated < 1f)
            {
                var multiplier = entity.UltimateMultiplierCurve.Evaluate(entity.CalculatedDps);
                entity.UltimateAccumulated += entity.UltimateAccumulationSpeed * Time.deltaTime * multiplier;
            }
        }
    }

    public static void UseUltimate(GameState gameState, Entity entity)
    {
        if (entity.UltimateAccumulated >= 1)
        {
            if (entity.TargetIndex != -1)
            {
                Game.DealDamage(gameState, entity.TargetIndex, entity.UltimateDamage, entity.Index, 0, true);
                entity.UltimateAccumulated = 0;
            }
        }
    }

    public static int2 GetInventoryLootboxCount(GameState gameState)
    {
        int def = 0;
        int rare = 0;
        for (int i = 0; i < gameState.Inventory.Lootboxes.Count; i++)
        {
            if (Game.GetLootboxSoByIndex(gameState, gameState.Inventory.Lootboxes[i].LootboxSoIndex).Type is LootboxType.Rare)
            {
                rare++;
            }
            else
            {
                def++;
            }
        }
        return new int2(def, rare);
    }

    public static int[] GetEntitiesOnPosition(GameState gameState, int2 pos)
    {
        Span<int> entities = stackalloc int[gameState.Entities.Length];
        int count = 0;
        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            var b = gameState.Entities[i].IntPos == pos;
            if (b.x && b.y && gameState.Entities[i].IsActive)
            {
                entities[count++] = i;
            }
        }

        return entities.Slice(0, count).ToArray();
    }

    public static void OnUpdate(GameState gameState)
    {
        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            if (gameState.EntityDisplays[i] != null)
            {
                gameState.EntityDisplays[i].OnUpdate(gameState.Entities[i]);
            }
        }
    }

    public static void UpdateSpawnWithDelay(GameState gameState, Entity entity)
    {
        Debug.Log("Update spawnwithdelay " + entity.SpawnDelayT);
        if (entity.SpawnDelayT > 0)
        {
            entity.SpawnDelayT -= Time.deltaTime;
        }
        else
        {
            Debug.Log("before : " + entity.Components.ToString());
            entity.Components = entity.Components & ~ComponentType.SpawnWithDelay;
            entity.IsActive = true;
            Debug.Log(entity.Components.ToString());
        }
    }

    public static void OnFixedUpdate(GameState gameState)
    {
        if (!gameState.IsInitialized) return;
        System.Diagnostics.Stopwatch sw = new();
        sw.Restart();
        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            var entity = gameState.Entities[i];
            if (entity.IsActive && entity.ContainsComponent(ComponentType.BuffCaster))
            {
                UpdateBuffCaster(gameState, entity);
            }
        }
        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            var entity = gameState.Entities[i];
            if (!entity.IsActive)
            {
                if (entity.ContainsComponent(ComponentType.SpawnWithDelay))
                {
                    UpdateSpawnWithDelay(gameState, entity);
                }
            }
            if (entity.IsActive)
            {
                if (entity.AppliedBuffsProperties?.Count > 0)
                {
                    UpdateAppliedBuffs(gameState, entity);
                }

                if (entity.ContainsComponent(ComponentType.Ultimate))
                {
                    UpdateUltimate(gameState, entity);
                }

                if (entity.ContainsComponent(ComponentType.Walking))
                {
                    UpdateWalking(gameState, entity);
                }

                if (entity.ContainsComponent(ComponentType.TeamTargeter))
                {
                    UpdateTeamTargeter(gameState, entity);
                }

                if (entity.ContainsComponent(ComponentType.TargetDamager))
                {
                    UpdateTargetDamager(gameState, entity);
                }

                if (entity.ContainsComponent(ComponentType.Mortal))
                {
                    UpdateMortal(gameState, entity);
                }

                if (entity.ContainsComponent(ComponentType.BufferBreaker))
                {
                    UpdateBufferBreaker(gameState, entity);
                }

                if (entity.ContainsComponent(ComponentType.EntitySpawner))
                {
                    UpdateEntitySpawner(gameState, entity);
                }

                if (entity.ContainsComponent(ComponentType.BaseDamager))
                {
                    UpdateBaseDamager(gameState, entity);
                }

                if (entity.ContainsComponent(ComponentType.Cooldown))
                {
                    UpdateCooldown(gameState, entity);
                }

                if (entity.ContainsComponent(ComponentType.CooldownWithActivePhase))
                {
                    UpdateCooldownWithActivePhase(gameState, entity);
                }

                if (entity.ContainsComponent(ComponentType.DisableOnEmptyAmmo))
                {
                    UpdateDisableOnEmptyAmmo(gameState, entity);
                }

                if (entity.ContainsComponent(ComponentType.Expiring))
                {
                    UpdateExpiring(gameState, entity);
                }

            }

            // TODO(sqdrck): Display pool.
            if (gameState.EntityDisplays[i] != null)
            {
                gameState.EntityDisplays[i].OnFixedUpdate(entity);
            }
        }

        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            var entity = gameState.Entities[i];
            if (entity.IsActive)
            {
                ZeroAppliedBuffs(gameState, entity);
            }
        }

        for (int i = 0; i < gameState.FloatingDamageDisplays.Length; i++)
        {
            if (gameState.FloatingDamageDisplays[i].IsActive)
            {
                UpdateFloatingDamageDisplay(gameState.FloatingDamageDisplays[i]);
            }
        }

        sw.Stop();
        if (!gameState.IsStageEnded)
        {
            CheckWaves(gameState);
        }
    }

    public static bool IsLastWave(GameState gameState)
    {
        return (gameState.LastSummonedWaveIndex == GetCurrentStage(gameState).Waves.Length - 1);
    }

    public static bool IsLastStage(GameState gameState)
    {
        return (gameState.Config.Stages.Length - 1 == gameState.CurrentStageIndex);
    }

    public static UserProgressionItem GetLevelProgressionItem(GameState gameState, int level)
    {
        return gameState.UserProgressionConfig.UserProgressionItems[level];
    }

    public static UserProgressionItem GetCurrentLevelProgressionItem(GameState gameState)
    {
        return Game.GetLevelProgressionItem(gameState, Game.GetCurrentLevel(gameState, out _));
    }

    public static bool IsUserHasLastProgressionLevel(GameState gameState)
    {
        return Game.GetCurrentLevel(gameState, out _) == gameState.UserProgressionConfig.UserProgressionItems.Count - 1;
    }

    public static int GetCurrentLevel(GameState gameState, out int currentLevelExp)
    {
        int exp = 0;
        currentLevelExp = 0;
        for (int i = 0; i < gameState.UserProgressionConfig.UserProgressionItems.Count; i++)
        {
            int relativeExp = gameState.UserProgressionConfig.UserProgressionItems[i].RelativeExp;
            currentLevelExp = gameState.ExpCount - exp;
            exp += relativeExp;
            if (gameState.ExpCount < exp)
            {
                return i - 1;
            }
        }
        return gameState.UserProgressionConfig.UserProgressionItems.Count - 1;
    }

    public static void SetStage(GameState gameState, int stageIndex)
    {
        Debug.Log("SetStage " + stageIndex);
        gameState.CurrentStageIndex = stageIndex;
        gameState.IsGameWon = false;
        gameState.IsStageWon = false;
        gameState.IsStageLost = false;
        gameState.BaseHp = gameState.BaseMaxHp;
        gameState.IsLastStage = false;
        gameState.CurrentPhase = GamePhase.Preparation;
        gameState.LastSummonedWaveIndex = -1;
        gameState.IsStageEnded = true;

        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            if (gameState.Entities[i].IsActive && !gameState.Entities[i].IsShallowSerializable)
            {
                gameState.Entities[i].IsActive = false;
                gameState.Entities[i].IsReadyToRecycle = true;
            }
        }
    }

    public static void ResetCurrentStage(GameState gameState)
    {
        SetStage(gameState, gameState.CurrentStageIndex);
    }

    public static void ApplyResourcePack(GameState gameState, ResourcePack pack)
    {
        gameState.GemsCount += pack.Gems;
        gameState.GoldCount += pack.Gold;
        gameState.ExpCount += pack.Exp;
        gameState.BattleTokensCount += pack.BattleTokens;
        if (pack.Lootboxes is not null)
        {
            for (int i = 0; i < pack.Lootboxes.Length; i++)
            {
                InventoryEntry entry = new();
                if (pack.Lootboxes[i].Type is LootboxType.Rare)
                {
                    entry.UnlockTime = System.DateTimeOffset.UtcNow +
                        TimeSpan.FromSeconds(gameState.Config.RareLootboxTimerSeconds);
                }
                else
                {
                    entry.UnlockTime = System.DateTimeOffset.UtcNow +
                        TimeSpan.FromSeconds(gameState.Config.DefaultLootboxTimerSeconds);
                }
                entry.LootboxSoIndex = pack.Lootboxes[i].Index;
                gameState.Inventory.Lootboxes.Add(entry);
            }
        }
        if (pack.Buffers is not null)
        {
            for (int i = 0; i < pack.Buffers.Length; i++)
            {
                var existing = gameState.Inventory.Consumables.FirstOrDefault(
                        c => c.EntitySoIndex == pack.Buffers[i].EntitySoIndex);
                if (existing is null)
                {
                    InventoryEntry entry = new();
                    entry.EntitySoIndex = pack.Buffers[i].EntitySoIndex;
                    entry.Count = pack.Buffers[i].Count;
                    gameState.Inventory.Consumables.Add(entry);
                }
                else
                {
                    existing.Count += pack.Buffers[i].Count;
                }
            }
        }
        if (pack.Boosters is not null)
        {
            for (int i = 0; i < pack.Boosters.Length; i++)
            {
                var existing = gameState.Inventory.Consumables.FirstOrDefault(
                        c => c.EntitySoIndex == pack.Boosters[i].EntitySoIndex);
                if (existing is null)
                {
                    InventoryEntry entry = new();
                    entry.EntitySoIndex = pack.Boosters[i].EntitySoIndex;
                    entry.Count = pack.Boosters[i].Count;
                    gameState.Inventory.Consumables.Add(entry);
                }
                else
                {
                    existing.Count += pack.Boosters[i].Count;
                }
            }
        }
        if (pack.HeroesFragments is not null)
        {
            for (int i = 0; i < pack.HeroesFragments.Length; i++)
            {
                if (gameState.Inventory.HeroesFragments.ContainsKey(pack.HeroesFragments[i].EntitySoIndex))
                {
                    gameState.Inventory.HeroesFragments[pack.HeroesFragments[i].EntitySoIndex] += pack.HeroesFragments[i].Count;
                }
                else
                {
                    gameState.Inventory.HeroesFragments[pack.HeroesFragments[i].EntitySoIndex] = pack.HeroesFragments[i].Count;
                }
            }
        }

        Debug.Log("User hero framents:");
        for (int i = 0; i < gameState.Inventory.HeroesFragments.Count; i++)
        {
            Debug.Log(gameState.Inventory.HeroesFragments.ElementAt(i).Key +
                    ": " +
                    gameState.Inventory.HeroesFragments.ElementAt(i).Value);
        }


    }

    public static void OnStageWon(GameState gameState)
    {
        Debug.Log("OnStageWon " + gameState.BaseHp);
        gameState.IsStageWon = true;
        gameState.IsStageLost = false;

        OnGameEvent(gameState, GameEventType.WonStage);
        ApplyResourcePack(gameState, GetCurrentStage(gameState).WinResourcePack);
    }

    public static void OnGameWon(GameState gameState)
    {
        Debug.Log("OnGameWon");
        gameState.IsGameWon = true;
    }

    public static void OnLost(GameState gameState)
    {
        Debug.Log("OnLost " + gameState.BaseHp);
        Debug.Log("stage: " + gameState.CurrentStageIndex);
        gameState.IsStageWon = false;
        gameState.IsStageLost = true;
        ApplyResourcePack(gameState, GetCurrentStage(gameState).LoseResourcePack);
    }

    public static void CheckWaves(GameState gameState)
    {
        if (gameState.WaveT > 0)
        {
            gameState.WaveT -= Time.fixedDeltaTime;
        }

        if (!gameState.IsStageEnded)
        {
            int[] uniqueWaves = Game.GetUniqueWaves(gameState);
            if (uniqueWaves.Length == 0)
            {
                gameState.IsWavesEnded = true;
                if (Game.UpdateWaveCooldown(gameState))
                {
                    if (Game.IsLastWave(gameState))
                    {
                        gameState.IsLastWave = true;
                        gameState.IsStageEnded = true;
                        gameState.CurrentPhase = GamePhase.Preparation;

                        if (gameState.BaseHp > 0)
                        {
                            OnStageWon(gameState);
                            if (IsLastStage(gameState))
                            {
                                gameState.IsLastStage = true;
                                OnGameWon(gameState);
                            }
                        }
                    }
                    else
                    {
                        StartNextWave(gameState);
                    }
                }
            }
            else
            {
                gameState.IsWavesEnded = false;
            }
        }
    }

    public static void StartNextWave(GameState gameState)
    {
        Debug.Log("Start next wave");
        gameState.LastSummonedWaveIndex++;

        var basePositions = GetTilesPositionsByType(gameState.Tiles, TileType.Base);
        var spawnerPositions = GetTilesPositionsByType(gameState.Tiles, TileType.EnemySpawner);
        for (int i = 0; i < spawnerPositions.Length; i++)
        {
            Debug.Log("Spawner position: " + spawnerPositions[i]);
        }

        Debug.Assert(basePositions.Length <= spawnerPositions.Length, "Missmatched number of bases and enemy spawners");

        List<int> waveSpawnerIndices = new();
        var waveEntries = Game.GetLastSummonedWave(gameState).Entries;
        var spawnerIndices = new bool[4];
        for (int i = 0; i < waveEntries.Length; i++)
        {
            var entrySpawnerIndices = GetSpawnerIndices(waveEntries[i]);
            for (int j = 0; j < entrySpawnerIndices.Length; j++)
            {
                if (entrySpawnerIndices[j])
                {
                    spawnerIndices[j] = true;
                }
            }
        }

        gameState.WaveDuration = Game.GetLastSummonedWave(gameState).Duration;
        gameState.WaveT = gameState.WaveDuration;

        for (int i = 0; i < spawnerIndices.Length; i++)
        {
            if (spawnerIndices[i])
            {
                var e = SpawnEntitySpawner(gameState, spawnerPositions[i], i);
                gameState.Tiles[basePositions[i % basePositions.Length].x, basePositions[i % basePositions.Length].y].SerialNumber = gameState.BaseSerialNumbers[i];
                e.SpawnerWaveInfo = GetLastSummonedWave(gameState);
                e.WaveIndex = gameState.LastSummonedWaveIndex;
                e.SpawnerWaveEntryIndex = -1;
                Game.SetNextEntryForEntitySpawner(gameState, e);
            }
        }

        if (IsLastWave(gameState))
        {
            gameState.IsLastWave = true;
            gameState.WaveCooldownT = 0;
            //gameState.WaveDuration = 0;
        }
        else
        {
            gameState.WaveCooldownDuration = GetCurrentStage(gameState).Waves[gameState.LastSummonedWaveIndex + 1].Cooldown;
            gameState.WaveCooldownT = gameState.WaveCooldownDuration;

        }
    }

    public static bool SetNextEntryForEntitySpawner(GameState gameState, Entity e)
    {
        var currIndex = e.SpawnerWaveEntryIndex;
        e.CooldownDuration = 0;
        for (int i = e.SpawnerWaveEntryIndex + 1; i < e.SpawnerWaveInfo.Entries.Length; i++)
        {
            var entry = e.SpawnerWaveInfo.Entries[i];
            e.CooldownDuration += entry.SpawnCooldown;
            var spawnerIndices = GetSpawnerIndices(entry);
            if (spawnerIndices[e.SerialNumber])
            {
                e.SpawnerWaveEntryIndex = i;
                break;
            }
        }
        e.CooldownT = e.CooldownDuration;

        return currIndex != e.SpawnerWaveEntryIndex;
    }

    public static bool[] GetSpawnerIndices(WaveEntry entry)
    {
        var spawners = entry.Spawners;
        var len = 4;
        var spawnerIndices = new bool[len];
        if (spawners != 0)
        {
            int spawnerIndex = 0;
            for (int j = 0; j < len; j++)
            {
                int powed = (int)math.pow(2, j);
                spawnerIndices[j] = spawners.HasFlag((WaveEntry.Spawner)powed);
            }
        }
        return spawnerIndices;
    }

    public static void StartCurrentStage(GameState gameState)
    {
        gameState.CurrentPhase = GamePhase.Battle;
        gameState.IsLastWave = false;
        gameState.IsStageEnded = false;
        if (IsLastStage(gameState))
        {
            gameState.IsLastStage = true;
            Debug.Log("IsLastStage");
        }
        Debug.Log("Start current stage");
        gameState.LastSummonedWaveIndex = -1;
        gameState.WaveCooldownDuration = GetCurrentStage(gameState).Waves[gameState.LastSummonedWaveIndex + 1].Cooldown;
        gameState.WaveCooldownT = gameState.WaveCooldownDuration;

        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            var entity = gameState.Entities[i];
            if (!entity.IsActive) continue;
            var config = Game.GetEntitySoByIndex(gameState, entity.EntitySoIndex);
            if (config.LevelDamageMultiplier is not null)
            {
                entity.Level = gameState.EntityLevels[entity.EntitySoIndex];
                entity.DamageMin = config.LevelDamageMultiplier[entity.Level].DamageMin;
                entity.DamageMax = config.LevelDamageMultiplier[entity.Level].DamageMax;
            }
        }
    }

    public static bool UpdateWaveCooldown(GameState gameState)
    {
        if (gameState.WaveCooldownT > 0)
        {
            gameState.WaveCooldownT -= Time.fixedDeltaTime;
        }
        return gameState.WaveCooldownT <= 0;
    }

    public static void OnEnd(GameState gameState)
    {
        foreach (var display in gameState.EntityDisplays)
        {
            GameObject.Destroy(display.gameObject);
        }
        foreach (var display in gameState.TileDisplays)
        {
            GameObject.Destroy(display.gameObject);
        }

        GameObject.Destroy(gameState.CameraDisplay.gameObject);
    }
}

