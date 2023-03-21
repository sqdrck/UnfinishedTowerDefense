using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Sirenix.OdinInspector;

public class GameMain : MonoBehaviour
{
    public MainCanvas MainCanvas;
    public TutorialScript Tutorial;

    [SerializeField]
    private CameraDisplay cameraDisplay;

    [SerializeField]
    private BoardSo boardSo;

    [SerializeField]
    private SoDatabase soDatabase;
    [SerializeField]
    private LevelSo config;

    [SerializeField]
    private UserProgressionSo progression;

    [SerializeField]
    private GameSettingsSo gameSettings;

    [Sirenix.OdinInspector.ShowInInspector, System.NonSerialized]
    private GameState gameState = null;

    private Vector2 ddEntityInitialPos;
    private bool isDdEntityFromInventory;
    private Entity ddEntity;

    private bool isTriggeredHeroesOpening;
    private bool isTriggeredHeroDisabling;
    private bool isDdEntityOverCardsUi;

    private TileDisplay[] towersDisplays;

    private bool IsDragging;
    private Vector2 DragBeginPosition;
    private Entity DragBeginEntity;

    public float DragMinimumDistance = 0.1f;

    private void OnApplicationPause()
    {
        if (gameState is not null)
        {
            gameState.AppVersion = Application.version;
            //Persistence.SerializeDeepSave(gameState);
            Persistence.SerializeShallowSave(gameState);
        }
    }

    [Button("PauseGame")]
    public void PauseGame()
    {
        DisableUpdate = !DisableUpdate;
        DisableFixedUpdate = !DisableFixedUpdate;
    }

    private void SetupGameState(GameState gameState)
    {
        gameState.Settings = gameSettings;
        gameState.GemsCount = 100;
        gameState.SoDatabase = soDatabase;
        gameState.BaseSerialNumbers = new int[] { 1, 2, 0, 3 };
        gameState.Config = config;
        // TODO(sqdrck): Move to settings.
        gameState.UserProgressionConfig = progression;
        gameState.CameraDisplay = cameraDisplay;
        gameState.MainCanvas = MainCanvas;
        gameState.HeroesDatabase = gameSettings.HeroesDatabase;
        if (gameState.Inventory.HeroesFragments is null)
        {
            gameState.Inventory.HeroesFragments = new();
        }
        MainCanvas.Canvas.worldCamera = gameState.CameraDisplay.Camera;
    }

    private void LoadSaves()
    {
        ShallowEntity[] shallowEntities;
        if (Persistence.DeserializeDeepSave(out gameState) &&
                Version.Parse(Application.version) == Version.Parse(gameState.AppVersion))
        {
            Debug.Log("Loaded deep save successfully!");
            SetupGameState(gameState);
            Game.OnInitFromDeepSave(gameState);
        }
        else if (Persistence.DeserializeShallowSave(out gameState, out shallowEntities))
        {
            Debug.Log("Loaded shallow save successfully");
            SetupGameState(gameState);
            Game.OnInitFromShallowSave(gameState, shallowEntities);
        }
        else
        {
            Debug.Log("Could not load deep save or shallow save. Creating new GameState...");
            gameState = new GameState();
            gameState.Inventory = new();
            gameState.Inventory.Lootboxes = new();
            gameState.Inventory.Consumables = new();
            gameState.Inventory.HeroesFragments = new();
            for (int i = 0; i < gameSettings.InitialInventory.Lootboxes.Count; i++)
            {
                gameState.Inventory.Lootboxes.Add(gameSettings.InitialInventory.Lootboxes[i]);
            }
            for (int i = 0; i < gameSettings.InitialInventory.Consumables.Count; i++)
            {
                gameState.Inventory.Consumables.Add(gameSettings.InitialInventory.Consumables[i]);
            }
            SetupGameState(gameState);
            Game.OnNewGameStateInit(gameState);
        }

        gameState.CameraDisplay.OnInit(gameState);
        gameState.MainCanvas.OnInit(gameState);
        int2[] towerPositions = Game.GetTilesPositionsByType(gameState.Tiles, TileType.Tower);
        towersDisplays = new TileDisplay[towerPositions.Length];
        for (int i = 0; i < towerPositions.Length; i++)
        {
            towersDisplays[i] = gameState.TileDisplays[towerPositions[i].x, towerPositions[i].y];
        }
    }

    private void Start()
    {
        Application.targetFrameRate = 300;
        Input.simulateMouseWithTouches = true;
        LoadSaves();
        GC.Collect();

        if (!gameSettings.SkipTutorial && !gameState.IsTutorialCompleted)
        {
            Tutorial.Main = this;
            Tutorial.StartTutorial();
        }
    }

    private void HighlightAllTowers(bool highlight)
    {
        for (int i = 0; i < towersDisplays.Length; i++)
        {
            if (highlight)
            {
                towersDisplays[i].HighlightColor(new Color(0.2f, 0.2f, 0.2f, 0.5f));
            }
            else
            {
                towersDisplays[i].HighlightDefault(TileType.Tower, 0);
            }
        }
    }



    private void EndLevel()
    {
        Game.OnEnd(gameState);
        gameState = null;
    }

    private void OnMouseButtonDown(GameState gameState, MouseInfo mouse)
    {
        if (mouse.IsWithinTiles && MainCanvas.Battlefield.SelectedConsumableIndex != -1)
        {
            var selectedEntityConfig = Game.GetEntitySoByIndex(gameState, gameState.Inventory.Consumables[MainCanvas.Battlefield.SelectedConsumableIndex].EntitySoIndex);
            if (Game.IsEntityCanBePlaced(gameState, mouse.WorldIntPos.x, mouse.WorldIntPos.y, selectedEntityConfig))
            {
                bool placed = true;
                if (selectedEntityConfig.Type == EntityType.Barrier)
                {
                    Game.SpawnBarrier(gameState, mouse.WorldRoundedPos);
                }
                else if (selectedEntityConfig.Type == EntityType.Mine)
                {
                    Game.SpawnMineEntity(gameState, mouse.WorldRoundedPos);
                }
                else if (selectedEntityConfig.Type == EntityType.Travolator)
                {
                    Game.SpawnTravolator(gameState, mouse.WorldRoundedPos);
                }
                else if (selectedEntityConfig.Type == EntityType.Buffer && gameState.CurrentPhase is GamePhase.Preparation)
                {
                    Game.SpawnBuffer(gameState, mouse.WorldRoundedPos, selectedEntityConfig);
                    gameState.Tiles[mouse.WorldIntPos.x, mouse.WorldIntPos.y].Type = TileType.ActiveBuffer;
                }
                else
                {
                    placed = false;
                }

                if (placed)
                {
                    gameState.Inventory.Consumables[MainCanvas.Battlefield.SelectedConsumableIndex].Count--;
                    MainCanvas.Battlefield.ConsumableUis[MainCanvas.Battlefield.SelectedConsumableIndex].Count--;
                    MainCanvas.Battlefield.ConsumableUis[MainCanvas.Battlefield.SelectedConsumableIndex].IsSelected = false;
                    MainCanvas.Battlefield.SelectedConsumableIndex = -1;
                }
            }
        }

        var entitiesUnderCursor = Game.GetEntitiesOnPosition(gameState, mouse.WorldIntPos);
        Entity heroUnderCursor = null;
        for (int i = 0; i < entitiesUnderCursor.Length; i++)
        {
            var e = gameState.Entities[entitiesUnderCursor[i]];
            if (e.Type is EntityType.Hero)
            {
                heroUnderCursor = e;
                break;
            }
        }

        if (heroUnderCursor is not null)
        {
            if (gameState.IsBattlePhase)
            {
                if (heroUnderCursor.UltimateAccumulated >= 1)
                {
                    Game.UseUltimate(gameState, heroUnderCursor);
                    Vibration.MediumImpact();
                }
            }
            else if (gameState.IsPreparationPhase)
            {
                DragBeginEntity = heroUnderCursor;
                DragBeginPosition = mouse.WorldPos;
                IsDragging = true;
            }
        }
        else if (!Extensions.IsPointerOverGameObject())
        {
            MainCanvas.Battlefield.CloseHeroes();
        }
        Debug.Log("GetMouseButtonDown");
    }

    private void OnMouseButton(GameState gameState, MouseInfo mouse)
    {
        if (gameState.IsPreparationPhase &&
                ddEntity is null &&
                IsDragging &&
                math.distance(mouse.WorldPos, DragBeginPosition) >= DragMinimumDistance)
        {
            Debug.Log("Dragged entity");
            ddEntity = DragBeginEntity;
            Vibration.Selection();
            ddEntityInitialPos = ddEntity.Pos;
            IsDragging = false;
            DragBeginEntity = null;
        }
        if (ddEntity is not null && gameState.EntityDisplays[ddEntity.Index] != null)
        {
            ddEntity.Pos = mouse.LiftedPos;
            gameState.EntityDisplays[ddEntity.Index].transform.position = new Vector3(mouse.LiftedPos.x, mouse.LiftedPos.y);
            ddEntity.IsActive = !isDdEntityOverCardsUi;
            if (isTriggeredHeroDisabling)
            {
                gameState.EntityDisplays[ddEntity.Index].transform.position = new Vector3(ddEntity.Pos.x, ddEntity.Pos.y);
                isTriggeredHeroDisabling = false;
            }
            if (MainCanvas.Battlefield.HeroesT == 0 && mouse.WorldPos.y < 3)
            {
                Debug.Log(mouse.WorldPos);
                Debug.Log(MainCanvas.Battlefield.HeroesT);
                MainCanvas.Battlefield.OpenHeroes();
                isTriggeredHeroesOpening = true;
            }
            else if (mouse.WorldPos.y >= 3 && isTriggeredHeroesOpening)
            {
                MainCanvas.Battlefield.CloseHeroes();
                isTriggeredHeroesOpening = false;
            }

            if (mouse.IsWithinTiles)
            {
                Tile tileUnderCursor = gameState.Tiles[mouse.LiftedIntPos.x, mouse.LiftedIntPos.y];
                if (tileUnderCursor.Type is TileType.Tower)
                {
                    TileDisplay tileDisplayUnderCursor = gameState.TileDisplays[mouse.LiftedIntPos.x, mouse.LiftedIntPos.y];
                    tileDisplayUnderCursor.HighlightColor(Color.yellow);
                }
            }
        }
        else if (MainCanvas.Battlefield.CardDd is not null && !MainCanvas.Battlefield.IsPointerOverCards)
        {
            var entitySo = Game.GetEntitySoByIndex(gameState, MainCanvas.Battlefield.CardDd.Index);
            var entity = Game.SpawnHeroEntity(gameState,
                    mouse.WorldPos,
                    entitySo);
            isDdEntityFromInventory = true;
            ddEntity = entity;
            gameState.EntityDisplays[ddEntity.Index].transform.position = new Vector3(ddEntity.Pos.x, ddEntity.Pos.y);
        }
    }

    private void OnMouseButtonUp(GameState gameState, MouseInfo mouse)
    {
        Debug.Log("GetMouseButtonUp");
        if (gameState.CurrentPhase is GamePhase.Preparation)
        {
            if (IsDragging && math.distance(mouse.WorldPos, DragBeginPosition) < DragMinimumDistance)
            {
                Debug.Log("Clicked on entity");
                gameState.EntityDisplays[DragBeginEntity.Index].OnTap(DragBeginEntity);
                IsDragging = false;
                DragBeginEntity.NeedToSayPhrase = true;
                DragBeginEntity = null;
            }
            if (ddEntity is not null)
            {
                if (mouse.IsWithinTiles && Game.IsEntityCanBePlaced(gameState, mouse.LiftedIntPos.x, mouse.LiftedIntPos.y, ddEntity))
                {
                    var entitiesUnderCursor = Game.GetEntitiesOnPosition(gameState, mouse.LiftedIntPos);
                    Entity entityUnderCursor = null;
                    for (int i = 0; i < entitiesUnderCursor.Length; i++)
                    {
                        var e = gameState.Entities[entitiesUnderCursor[i]];
                        if (e.Index != ddEntity.Index && e.Type is EntityType.Hero)
                        {
                            entityUnderCursor = e;
                            break;
                        }
                    }
                    if (entityUnderCursor is not null)
                    {
                        if (isDdEntityFromInventory)
                        {
                            MainCanvas.Battlefield.SetHeroCardBusy(entityUnderCursor.EntitySoIndex, false);
                            entityUnderCursor.IsActive = false;
                        }
                        else
                        {
                            entityUnderCursor.Pos = ddEntityInitialPos;
                        }
                    }

                    ddEntity.Pos = mouse.LiftedRoundedPos;
                }
                else
                {
                    if (isDdEntityFromInventory)
                    {
                        MainCanvas.Battlefield.SetHeroCardBusy(ddEntity.EntitySoIndex, false);
                        ddEntity.IsActive = false;
                    }
                    ddEntity.Pos = ddEntityInitialPos;
                }

                if (isTriggeredHeroesOpening)
                {
                    isTriggeredHeroesOpening = false;
                    MainCanvas.Battlefield.CloseHeroes();
                }

                ddEntity = null;
                Vibration.Selection();
                isDdEntityFromInventory = false;
            }
        }
    }

    public struct MouseInfo
    {
        public bool IsWithinTiles;
        public float2 WorldPos;
        public int2 WorldIntPos;
        public float2 WorldRoundedPos;
        public float2 LiftedPos;
        public int2 LiftedIntPos;
        public float2 LiftedRoundedPos;
    }

    public bool DisableUpdate;
    public bool DisableFixedUpdate;
    private void Update()
    {
        if (DisableUpdate) return;
        HighlightAllTowers(MainCanvas.Battlefield.HeroesT != 0);

        MouseInfo mouse;
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.WorldPos = new float2(mousePos.x, mousePos.y);
        mouse.WorldIntPos = new int2((int)math.round(mouse.WorldPos.x), (int)math.round(mouse.WorldPos.y));
        mouse.WorldRoundedPos = new float2(mouse.WorldIntPos.x, mouse.WorldIntPos.y);
        mouse.LiftedPos = new float2(mouse.WorldPos.x, mouse.WorldPos.y + 0.8f);
        mouse.LiftedIntPos = new int2((int)math.round(mouse.LiftedPos.x), (int)math.round(mouse.LiftedPos.y));
        mouse.LiftedRoundedPos = new float2(mouse.LiftedIntPos.x, mouse.LiftedIntPos.y);

        if (!MainCanvas.Battlefield.IsPointerOverCards)
        {
            isDdEntityOverCardsUi = false;
            MainCanvas.Battlefield.EnableDropCardOverlay(false);
        }

        if (MainCanvas.Battlefield.NeedToWithdrawAllHeroes)
        {
            MainCanvas.Battlefield.NeedToWithdrawAllHeroes = false;
            for (int i = 0; i < gameState.Entities.Length; i++)
            {
                var entity = gameState.Entities[i];
                if (entity.IsActive && entity.Type is EntityType.Hero)
                {
                    entity.IsActive = false;
                    MainCanvas.Battlefield.SetHeroCardBusy(entity.EntitySoIndex, false);
                }
            }
        }

        if (ddEntity is not null)
        {
            if (MainCanvas.Battlefield.IsPointerOverCards)
            {
                MainCanvas.Battlefield.SetHeroCardBusy(ddEntity.EntitySoIndex, false);
                isDdEntityOverCardsUi = true;
                isTriggeredHeroDisabling = true;
                MainCanvas.Battlefield.EnableDropCardOverlay(true);
                if (MainCanvas.Battlefield.CardDd is null)
                {
                    MainCanvas.Battlefield.CardDd = MainCanvas.Battlefield.CloneCard(MainCanvas.Battlefield.GetCardByIndex(ddEntity.EntitySoIndex));
                }
            }
            else
            {
                MainCanvas.Battlefield.SetHeroCardBusy(ddEntity.EntitySoIndex, true);
            }
        }

        if (gameState?.TileDisplays is not null)
        {
            mouse.IsWithinTiles = mouse.WorldIntPos.x < gameState.TileDisplays.GetLength(0)
                    && mouse.WorldIntPos.y < gameState.TileDisplays.GetLength(1)
                    && mouse.WorldIntPos.x >= 0
                    && mouse.WorldIntPos.y >= 0;

            if (Input.GetMouseButtonDown(0))
            {
                OnMouseButtonDown(gameState, mouse);
            }
            else if (Input.GetMouseButton(0))
            {
                OnMouseButton(gameState, mouse);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnMouseButtonUp(gameState, mouse);
            }
        }


        if (MainCanvas.Battlefield.StartStagePressed && gameState.IsStageEnded)
        {
            Debug.Log("Start next stage pressed");
            MainCanvas.Battlefield.StartStagePressed = false;
            if (Game.GetEntitiesOfType(gameState, EntityType.Hero).Length == 0)
            {
                MainCanvas.Battlefield.EnablePlaceOneHeroPopup();
            }
            else
            {
                MainCanvas.TopBar.gameObject.SetActive(false);
                MainCanvas.Battlefield.CloseHeroes();
                Game.StartCurrentStage(gameState);
            }
        }

        if (MainCanvas.Battlefield.StartNextWave)
        {
            Debug.Log("Start next wave pressed");
            MainCanvas.Battlefield.StartNextWave = false;
            Game.StartNextWave(gameState);
        }

        if (MainCanvas.Battlefield.SpeedUpPressed)
        {
            MainCanvas.Battlefield.SpeedUpPressed = false;
            if (Time.timeScale == 1)
            {
                Time.timeScale = 2;
            }
            else
            {
                Time.timeScale = 1;
            }

            Debug.Log("Timescale set to " + Time.timeScale);
        }
        else if (MainCanvas.Battlefield.SpeedUpMorePressed)
        {
            MainCanvas.Battlefield.SpeedUpMorePressed = false;
            if (Time.timeScale == 1)
            {
                Time.timeScale = 5;
            }
            else
            {
                Time.timeScale = 1;
            }

            Debug.Log("Timescale set to " + Time.timeScale);
        }
        else
        {
            if (gameState.IsStageEnded)
            {
                Time.timeScale = 1;
            }
        }

        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            Debug.Log("Unloading level");
            EndLevel();
        }

        if (gameState is not null)
        {
            Game.OnUpdate(gameState);
        }

        if (MainCanvas is not null)
        {
            MainCanvas.OnUpdate(gameState);
        }
    }

    private bool reinitBattlefieldUiHandled = false;
    private void FixedUpdate()
    {
        if (DisableFixedUpdate) return;
        if (gameState is not null)
        {
            Game.OnFixedUpdate(gameState);

            // TODO(sqdrck): Handle level win.
            if (gameState.IsStageWon || gameState.IsStageLost)
            {
                StageSo stage = Game.GetCurrentStage(gameState);
                ResourcePack pack = gameState.IsStageWon ? stage.WinResourcePack : stage.LoseResourcePack;
                MainCanvas.Battlefield.DeselectConsumableIfAny();
                // NOTE(sqdrck): Reinit battlefield ui.
                if (!reinitBattlefieldUiHandled)
                {
                    reinitBattlefieldUiHandled = true;
                    MainCanvas.Battlefield.OnOpen(gameState);
                }

                if (gameState.IsStageWon)
                {
                    MainCanvas.Battlefield.EnableWinPopup(gameState, pack);

                    if (!gameState.IsGameWon)
                    {
                        Game.SetStage(gameState, gameState.CurrentStageIndex + 1);
                    }
                    Vibration.Success();
                }
                else if (gameState.IsStageLost)
                {
                    MainCanvas.Battlefield.EnableLosePopup(gameState, pack);

                    Vibration.Failure();
                    Game.ResetCurrentStage(gameState);
                }
            }
            else
            {
                reinitBattlefieldUiHandled = false;
            }
        }

        if (cameraDisplay is not null)
        {
            cameraDisplay.OnFixedUpdate(gameState);
        }
        if (MainCanvas is not null)
        {
            MainCanvas.OnFixedUpdate(gameState);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (gameState != null)
        {
            Game.DrawGizmos(gameState);
        }
    }
#endif
}
