using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LootboxUi : MonoBehaviour
{
    public TextMeshProUGUI Timer;
    public ResourceUi ResourceUi;
    public Button WatchAdButton;
    public Button SkipTimerButton;
    public Button OpenButton;

    public void OnInit(GameState gameState, InventoryEntry entry)
    {
        LootboxSo lb = Game.GetLootboxSoByIndex(gameState, entry.LootboxSoIndex);
        ResourceUi.SetLootboxType(lb.Type);

        Timer.text = Utils.FormatTime(entry.UnlockTime - DateTimeOffset.UtcNow);
    }

    public void OnUpdate(InventoryEntry entry)
    {
        var timeLeft = entry.UnlockTime - DateTimeOffset.UtcNow;
        if (timeLeft < TimeSpan.Zero)
        {
            Timer.enabled = false;
            OpenButton.gameObject.SetActive(true);
            SkipTimerButton.gameObject.SetActive(false);
            WatchAdButton.gameObject.SetActive(false); 
        }
        else
        {
            Timer.text = Utils.FormatTime(timeLeft);
            OpenButton.gameObject.SetActive(false);
            SkipTimerButton.gameObject.SetActive(true);
            WatchAdButton.gameObject.SetActive(true); 
        }
    }
}
