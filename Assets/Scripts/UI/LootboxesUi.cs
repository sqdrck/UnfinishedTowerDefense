using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LootboxesUi : ScreenUiElement
{
    public TextMeshProUGUI NoLootboxesText;
    public Transform Content;
    public Button CloseButton;
    public Button BgButton;
    public List<LootboxUi> CreatedLooboxes = new();

    public int NeedToOpenLootboxIndex = -1;
    public int NeedToWatchAdLootboxIndex = -1;
    public int NeedToSkipTimerIndex = -1;


    public override void OnInit(GameState gameState)
    {
        BgButton.onClick.AddListener(Close);
        CloseButton.onClick.AddListener(Close);
    }

    public void Close()
    {
        Root.Lootboxes.gameObject.SetActive(false);
    }

    public override void OnOpen(GameState gameState)
    {
        RecreateLootboxes(gameState);
    }

    public void RecreateLootboxes(GameState gameState)
    {
        Root.TopBar.CurrentDefaultLootboxCount = 0;
        Root.TopBar.CurrentRareLootboxCount = 0;
        for (int i = 0; i < CreatedLooboxes.Count; i++)
        {
            Destroy(CreatedLooboxes[i].gameObject);
        }
        CreatedLooboxes.Clear();

        for (int i = 0; i < gameState.Inventory.Lootboxes.Count; i++)
        {
            LootboxUi lb = Instantiate(Root.LootboxUiPrefab, Content);
            LootboxSo lbSo = Game.GetLootboxSoByIndex(gameState, gameState.Inventory.Lootboxes[i].LootboxSoIndex);

            lb.OnInit(gameState, gameState.Inventory.Lootboxes[i]);
            int temp = i;
            lb.WatchAdButton.onClick.AddListener(() => OnLootboxUiWatchAdButtonPressed(temp));
            lb.SkipTimerButton.onClick.AddListener(() => OnLootboxUiSkipTimerButtonPressed(temp));
            lb.OpenButton.onClick.AddListener(() => OnLootboxUiOpenButtonPressed(temp));
            CreatedLooboxes.Add(lb);
            Root.TopBar.CurrentRareLootboxCount += lbSo.Type is LootboxType.Rare ? 1 : 0;
            Root.TopBar.CurrentDefaultLootboxCount += lbSo.Type is LootboxType.Rare ? 0 : 1;
        }

        NoLootboxesText.enabled = CreatedLooboxes.Count == 0;
    }

    private void OnLootboxUiSkipTimerButtonPressed(int inventoryIndex)
    {
        NeedToSkipTimerIndex = inventoryIndex;
    }

    private void OnLootboxUiWatchAdButtonPressed(int inventoryIndex)
    {
        NeedToWatchAdLootboxIndex = inventoryIndex;
    }

    private void OnLootboxUiOpenButtonPressed(int inventoryIndex)
    {
        NeedToOpenLootboxIndex = inventoryIndex;
    }

    public override void OnFixedUpdate(GameState gameState)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        if (NeedToOpenLootboxIndex != -1)
        {
            var pack = Game.OpenLootbox(gameState, NeedToOpenLootboxIndex);
            Root.GetReward.Show(gameState, pack);

            RecreateLootboxes(gameState);
            NeedToOpenLootboxIndex = -1;
        }
        if (NeedToWatchAdLootboxIndex != -1)
        {
            Debug.Log("Watch ad");
            NeedToWatchAdLootboxIndex = -1;
        }
        if (NeedToSkipTimerIndex != -1)
        {
            Debug.Log("Skip timer");
            NeedToSkipTimerIndex = -1;
        }
    }

    public override void OnUpdate(GameState gameState)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        Debug.Assert(gameState.Inventory.Lootboxes.Count == CreatedLooboxes.Count);
        for (int i = 0; i < CreatedLooboxes.Count; i++)
        {
            var lb = CreatedLooboxes[i];
            var entry = gameState.Inventory.Lootboxes[i];

            lb.OnUpdate(entry);
        }
    }
}
