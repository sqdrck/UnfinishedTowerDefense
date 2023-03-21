using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreUi : ScreenUiElement
{
    public StoreItemUi StoreItemPrefab;

    public StoreConfrimBuyUi ConfirmPopup;

    public Transform BuffersContent;
    public Transform BustersContent;

    public int NeedToConfirmEntry = -1;
    public int NeedToBuyEntry = -1;

    public override void OnInit(GameState gameState)
    {
        ConfirmPopup.CloseButton.onClick.AddListener(CloseConfirmButton);
        ConfirmPopup.BgButton.onClick.AddListener(CloseConfirmButton);
        ConfirmPopup.BuyButton.onClick.AddListener(OnConfirmPopupBuyPressed);

        for (int i = 0; i < gameState.Settings.StoreEntries?.Length; i++)
        {
            Transform content =
                gameState.Settings.StoreEntries[i].Category == StoreCategory.Booster ? BustersContent : BuffersContent;
            StoreItemUi item = Instantiate(StoreItemPrefab, content);
            item.OnInit(gameState, gameState.Settings.StoreEntries[i]);
            int temp = i;
            item.GetComponent<Button>().onClick.AddListener(() => OnEntryPressed(temp));
        }
    }

    public void OnConfirmPopupBuyPressed()
    {
        NeedToBuyEntry = ConfirmPopup.InitStoreEntryIndex;
    }

    public override void OnUpdate(GameState gameState)
    {
        if (NeedToConfirmEntry != -1)
        {
            ConfirmPopup.OnInit(gameState, NeedToConfirmEntry);
            ConfirmPopup.gameObject.SetActive(true);
            NeedToConfirmEntry = -1;
        }

        if (ConfirmPopup.gameObject.activeInHierarchy)
        {
            ConfirmPopup.OnUpdate(gameState);
        }

        if (NeedToBuyEntry != -1)
        {
            CloseConfirmButton();
            if (ConfirmPopup.CanBuy)
            {
                for (int i = 0; i < ConfirmPopup.CurrentCount; i++)
                {
                    Game.SpendResourcePack(gameState, gameState.Settings.StoreEntries[NeedToBuyEntry].Price.Value, out _);
                    Game.ApplyResourcePack(gameState, gameState.Settings.StoreEntries[NeedToBuyEntry].Item.Value);
                }
            }

            Root.TopBar.NeedToUpdateTopBarStats = true;
            NeedToBuyEntry = -1;
        }
    }

    public void CloseConfirmButton()
    {
        ConfirmPopup.gameObject.SetActive(false);
    }

    public void OnEntryPressed(int index)
    {
        NeedToConfirmEntry = index;
    }
}
