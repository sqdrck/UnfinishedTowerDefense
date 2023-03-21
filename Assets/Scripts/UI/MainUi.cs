using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Unity.Mathematics;

public class MainUi : ScreenUiElement
{
    [Header("MainUI")]
    public Button StartBattleButton;
    public Slider ExpProgressBar;
    public Button ExpProgressBarButton;
    public Button LootboxesButton;
    public Button MenuButton;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI ExpProgressText;
    public ResourceUi ExpProgressRewardRecourceUi;
    public ResourceUi LootboxResourceUi;
    public TextMeshProUGUI LootboxTimerText;
    public TextMeshProUGUI LootboxTimerReadyText;
    public GameObject ShinePsGo;

    public bool NeedToUpdateStats;

    public LayoutGroup StagesCirclesGroup;
    public List<StageCircleUi> StageCircles = new();

    public int CurrentLevel;
    public int PreviousStageIndex;
    public Tween ExpProgressBarTween;

    public int CurrentExpCount;
    public int CurrentDefaultLootboxesCount;
    public int CurrentRareLootboxesCount;

    public override void OnInit(GameState gameState)
    {
        StartBattleButton.onClick.AddListener(OnStartBattlePressed);

        ExpProgressBarButton.onClick.AddListener(OnExpProgressBarPressed);
        LootboxesButton.onClick.AddListener(OnLootboxesButtonPressed);
        MenuButton.onClick.AddListener(OnMenuButtonPressed);

        //Root.UiHighlighter.ShowSurrounds = false;
        //Root.UiHighlighter.Lerp(StartBattleButton.transform as RectTransform, LootboxesButton.transform as RectTransform);
    }

    public void OnMenuButtonPressed()
    {
        Root.SetScreenOpened(Root.Menu);
    }

    public void OnLootboxesButtonPressed()
    {
        Root.SetScreenOpened(Root.Lootboxes);
    }

    public override void OnOpen(GameState gameState)
    {
        InitStageCircles(gameState);
        int currLevel = Game.GetCurrentLevel(gameState, out _);
        if (Game.IsUserHasLastProgressionLevel(gameState))
        {
            ExpProgressRewardRecourceUi.IsAllActive = false;
        }
        else
        {
            ExpProgressRewardRecourceUi.OnInit(gameState, Game.GetLevelProgressionItem(gameState, currLevel + 1).ResourcePack);
        }
    }

    public void OnExpProgressBarPressed()
    {
        Root.SetScreenOpened(Root.UserProgression);
    }

    public void InitStageCircles(GameState gameState)
    {
        if (StageCircles.Count > 0)
        {
            for (int i = 0; i < StageCircles.Count; i++)
            {
                GameObject.Destroy(StageCircles[i].gameObject);
            }
            StageCircles.Clear();
        }
        var currentTenStages = gameState.CurrentStageIndex / 10;
        for (int i = currentTenStages * 10; i < MathF.Min(currentTenStages * 10 + 10, gameState.Config.Stages.Length); i++)
        {
            bool isStageCurrent = i == gameState.CurrentStageIndex;
            bool isStageCompleted = i < gameState.CurrentStageIndex;
            StageCircleUi circle = Instantiate(Root.StageCirclePrefab, StagesCirclesGroup.transform);
            circle.Number = i + 1;
            circle.State =
                 isStageCurrent ? StageCircleUi.StageCircleState.Current
                : isStageCompleted ? StageCircleUi.StageCircleState.Completed
                : StageCircleUi.StageCircleState.Uncompleted;
            var stage = gameState.Config.Stages[i];
            bool hasAddReward = !isStageCompleted && (stage.HasAdditionalGold ||
                stage.WinResourcePack.Gems > 0 ||
                stage.WinResourcePack.Lootboxes?.Length > 0);
            circle.HasAddReward = hasAddReward;
            circle.IsGoldActive = stage.HasAdditionalGold;
            circle.IsGemsActive = stage.WinResourcePack.Gems > 0;
            circle.IsRareLootboxActive = stage.WinResourcePack.RareLootboxesCount > 0;
            circle.IsDefaultLooboxActive = stage.WinResourcePack.DefaultLootboxesCount > 0;
            StageCircles.Add(circle);
        }

        PreviousStageIndex = gameState.CurrentStageIndex;
    }

    public void SetStats(GameState gameState, bool animate = true)
    {
        if (gameState.CurrentStageIndex != PreviousStageIndex)
        {
            InitStageCircles(gameState);
        }

        int2 lbCount = Game.GetInventoryLootboxCount(gameState);
        SetExpProgressBar(gameState, animate);
        if (animate)
        {
            if (CurrentExpCount < gameState.ExpCount)
            {
                int diff = gameState.ExpCount - CurrentExpCount;
                Root.TopBar.AnimateFlyingPrefabs(Root.ExpPrefab, ExpProgressText.transform.position, Unity.Mathematics.math.min(diff, 12));
            }
            if (lbCount.x > CurrentDefaultLootboxesCount)
            {
                int diff = lbCount.x - CurrentDefaultLootboxesCount;
                Root.TopBar.AnimateFlyingPrefabs(Root.DefaultLootboxPrefab, LootboxesButton.transform.position, Unity.Mathematics.math.min(diff, 12));
            }
            if (lbCount.y > CurrentRareLootboxesCount)
            {
                int diff = lbCount.y - CurrentRareLootboxesCount;
                Root.TopBar.AnimateFlyingPrefabs(Root.RareLootboxPrefab, LootboxesButton.transform.position, Unity.Mathematics.math.min(diff, 12));
            }
        }

        CurrentExpCount = gameState.ExpCount;
        CurrentDefaultLootboxesCount = lbCount.x;
        CurrentRareLootboxesCount = lbCount.y;
    }

    public override void OnFixedUpdate(GameState gameState)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
    }


    public override void OnUpdate(GameState gameState)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (NeedToUpdateStats)
        {
            NeedToUpdateStats = false;
            SetStats(gameState);
        }

        var lbEntry = Game.GetClosestLootboxFromInventory(gameState);
        LootboxesButton.image.color = Color.white;
        if (lbEntry is not null)
        {
            LootboxSo lbSo = Game.GetLootboxSoByIndex(gameState, lbEntry.LootboxSoIndex);
            TimeSpan timeLeft = lbEntry.UnlockTime - DateTimeOffset.Now;
            LootboxTimerText.enabled = true;
            if (timeLeft < TimeSpan.Zero)
            {
                LootboxTimerReadyText.enabled = true;
                LootboxTimerText.enabled = false;
                LootboxesButton.image.color = Color.yellow;
                ShinePsGo.SetActive(true);
            }
            else
            {
                LootboxTimerText.enabled = true;
                LootboxTimerText.text = Utils.FormatTime(timeLeft);
                LootboxTimerReadyText.enabled = false;
            }
            LootboxResourceUi.IsRareLootboxActive = lbSo.Type is LootboxType.Rare;
            LootboxResourceUi.IsDefaultLootboxActive = lbSo.Type is LootboxType.Default;
        }
        else
        {
            LootboxResourceUi.IsAllActive = false;
            LootboxTimerText.enabled = false;
            LootboxTimerReadyText.enabled = false;
            ShinePsGo.SetActive(false);
        }
    }

    public void SetExpProgressBar(GameState gameState, bool animate = true)
    {
        int currentLevelExp;
        int currentLevel = Game.GetCurrentLevel(gameState, out currentLevelExp);
        LevelText.text = currentLevel.ToString();
        Debug.Log("Current level is " + currentLevel);
        float value;
        LevelText.text = currentLevel.ToString();
        if (currentLevel == gameState.UserProgressionConfig.UserProgressionItems.Count - 1)
        {
            ExpProgressText.text = "You have last level!";
            value = 1;
        }
        else
        {
            ExpProgressText.text = $"{currentLevelExp}/{gameState.UserProgressionConfig.UserProgressionItems[currentLevel + 1].RelativeExp}";
            value = currentLevelExp * 1f / gameState.UserProgressionConfig.UserProgressionItems[currentLevel + 1].RelativeExp;
        }
        if (currentLevel > CurrentLevel)
        {
            ExpProgressBar.value = 0;
        }

        if (animate)
        {

            ExpProgressBarTween = DOTween.To(() => ExpProgressBar.value, x => ExpProgressBar.value = x, value, 1)
                .SetEase(Ease.InOutCubic);
        }
        else
        {
            ExpProgressBar.value = value;
        }

        CurrentLevel = currentLevel;

    }

    public void OnStartBattlePressed()
    {
        Root.SetScreenOpened(Root.Battlefield);
    }
}
