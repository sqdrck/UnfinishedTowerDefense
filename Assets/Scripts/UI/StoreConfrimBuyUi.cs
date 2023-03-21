using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreConfrimBuyUi : MonoBehaviour
{
    public int CurrentCount = 1;
    public StoreItemUi Item;
    public Button BgButton;
    public Button CloseButton;
    public Button BuyButton;
    public Button Increment1Button;
    public Button Increment10Button;
    public Button Decrement1Button;
    public Button Decrement10Button;
    public TextMeshProUGUI PriceText;
    public TextMeshProUGUI QuantityText;
    public TextMeshProUGUI BuyButtonText;
    public bool NeedToUpdatePrice = false;
    public int InitPrice;
    public ResourcePack InitPricePack;
    public int InitStoreEntryIndex;
    public bool CanBuy;

    public void OnUpdate(GameState gameState)
    {
        if (NeedToUpdatePrice)
        {
            NeedToUpdatePrice = false;
            UpdatePrice(gameState);
        }
    }

    public void OnInit(GameState gameState, int storeEntryIndex)
    {
        StoreEntry entry = gameState.Settings.StoreEntries[storeEntryIndex];
        CanBuy = false;
        Decrement10Button.onClick.RemoveAllListeners();
        Decrement1Button.onClick.RemoveAllListeners();
        Increment10Button.onClick.RemoveAllListeners();
        Increment1Button.onClick.RemoveAllListeners();

        Decrement1Button.onClick.AddListener(delegate
        {
            CurrentCount--;
            if (CurrentCount < 1)
            {
                CurrentCount = 1;
            }
            NeedToUpdatePrice = true;
        });
        Decrement10Button.onClick.AddListener(delegate
        {
            CurrentCount -= 10;
            if (CurrentCount < 1)
            {
                CurrentCount = 1;
            }
            NeedToUpdatePrice = true;
        });
        Increment1Button.onClick.AddListener(delegate
        {
            CurrentCount++;
            NeedToUpdatePrice = true;
        });
        Increment10Button.onClick.AddListener(delegate
        {
            CurrentCount += 10;
            NeedToUpdatePrice = true;
        });

        Item.OnInit(gameState, entry);
        CurrentCount = 1;
        Item.PriceResource.OnInit(gameState, entry.Price.Value);
        Item.ItemResource.OnInit(gameState, entry.Item.Value);
        InitPrice = GetPriceCount(entry);
        InitPricePack = entry.Price.Value;
        InitStoreEntryIndex = storeEntryIndex;

        NeedToUpdatePrice = true;
    }

    public void UpdatePrice(GameState gameState)
    {
        int price = InitPrice * CurrentCount;
        PriceText.text = "x" + price.ToString();
        QuantityText.text = "x" + CurrentCount.ToString();

        CanSpendResourcePackResponse response = Game.CanSpendResourcePack(gameState, InitPricePack * CurrentCount);
        if (response.Result)
        {
            CanBuy = true;
            PriceText.color = Color.white;
            BuyButtonText.text = "Buy";
        }
        else
        {
            CanBuy = false;
            PriceText.color = Color.red;
            BuyButtonText.text = "Not enough";
        }
    }

    public int GetPriceCount(StoreEntry entry)
    {
        if (entry.Price.Value.Gold > 0)
        {
            return entry.Price.Value.Gold;
        }
        if (entry.Price.Value.Gems > 0)
        {
            return entry.Price.Value.Gems;
        }
        if (entry.Price.Value.BattleTokens > 0)
        {
            return entry.Price.Value.BattleTokens;
        }
        if (entry.Price.Value.HeroFragments > 0)
        {
            return entry.Price.Value.HeroFragments;
        }


        return 0;
    }
}
