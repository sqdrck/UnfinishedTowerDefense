using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceUi : MonoBehaviour
{
    public GameObject RareLootboxGo;
    public GameObject DefaultLootboxGo;
    public GameObject GoldGo;
    public GameObject GemsGo;
    public GameObject BattleTokensGo;
    public GameObject ExpGo;
    public GameObject EntityGo;
    public HeroCardUi HeroCard;
    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI BattleTokensText;
    public TextMeshProUGUI ExpText;
    public TextMeshProUGUI GemsText;
    public TextMeshProUGUI DefaultLootboxCountText;
    public TextMeshProUGUI RareLootboxCountText;
    public TextMeshProUGUI HeroCardText;
    public TextMeshProUGUI EntityText;
    public Image EntityImage;

    public void OnInit(GameState gameState, ResourcePack pack)
    {
        IsEntityActive = (pack.Buffers != null && pack.Buffers.Length > 0) ||
            (pack.Boosters != null && pack.Boosters.Length > 0);
        IsGoldActive = pack.Gold > 0;
        IsGemsActive = pack.Gems > 0;
        IsRareLootboxActive = pack.RareLootboxesCount > 0;
        IsDefaultLootboxActive = pack.DefaultLootboxesCount > 0;
        IsBattleTokensActive = pack.BattleTokens > 0;
        IsExpActive = pack.Exp > 0;

        if (EntityGo != null && EntityImage != null)
        {
            if (pack.Buffers != null && pack.Buffers.Length > 0)
            {
                Entities = pack.Buffers[0].Count;
                EntityImage.sprite = Game.GetEntitySoByIndex(gameState, pack.Buffers[0].EntitySoIndex).UiSprite;
            }

            if (pack.Boosters != null && pack.Boosters.Length > 0)
            {
                Entities = pack.Boosters[0].Count;
                EntityImage.sprite = Game.GetEntitySoByIndex(gameState, pack.Boosters[0].EntitySoIndex).UiSprite;
            }
        }

        if (pack.Exp > 0)
            Exp = pack.Exp;
        if (pack.Gold > 0)
            Gold = pack.Gold;
        if (pack.Gems > 0)
            Gems = pack.Gems;
        if (pack.RareLootboxesCount > 0)
            RareLootboxCount = pack.RareLootboxesCount;
        if (pack.DefaultLootboxesCount > 0)
            DefaultLootboxCount = pack.DefaultLootboxesCount;
        if (pack.BattleTokens > 0)
            BattleTokens = pack.BattleTokens;
        if (pack.HeroesFragments != null && pack.HeroesFragments.Length > 0)
        {
            IsHeroActive = true;
            HeroCard.OnInit(gameState, pack.HeroesFragments[0].EntitySoIndex);
            HeroFragments = pack.HeroesFragments[0].Count;
        }
        else
        {
            IsHeroActive = false;
        }
    }

    public bool IsHeroActive
    {
        set
        {
            if (HeroCard != null)
            {
                HeroCard.gameObject.SetActive(value);
            }
        }
    }

    public bool IsAllActive
    {
        set
        {
            IsRareLootboxActive = value;
            IsDefaultLootboxActive = value;
            IsGoldActive = value;
            IsGemsActive = value;
            IsExpActive = value;
            IsBattleTokensActive = value;
            IsHeroActive = value;
        }
    }

    public void SetLootboxType(LootboxType type)
    {
        if (type is LootboxType.Default)
        {
            IsRareLootboxActive = false;
            IsDefaultLootboxActive = true;
        }
        else if (type is LootboxType.Rare)
        {
            IsRareLootboxActive = true;
            IsDefaultLootboxActive = false;
        }

    }

    public bool IsEntityActive
    {
        set
        {
            if (EntityGo != null)
                EntityGo.SetActive(value);
        }
    }

    public bool IsExpActive
    {
        set
        {
            if (ExpGo != null)
                ExpGo.SetActive(value);
        }
    }

    public bool IsBattleTokensActive
    {
        set
        {
            if (BattleTokensGo != null)
                BattleTokensGo.SetActive(value);
        }
    }

    public bool IsRareLootboxActive
    {
        set
        {
            if (RareLootboxGo != null)
                RareLootboxGo.SetActive(value);
        }
    }
    public bool IsDefaultLootboxActive
    {
        set
        {
            if (DefaultLootboxGo != null)
                DefaultLootboxGo.SetActive(value);
        }
    }

    public bool IsGoldActive
    {
        set
        {
            if (GoldGo != null)
                GoldGo.SetActive(value);
        }
    }

    public bool IsGemsActive
    {
        set
        {
            if (GemsGo != null)
                GemsGo.SetActive(value);
        }
    }

    public int RareLootboxCount
    {
        set
        {
            if (RareLootboxCountText != null)
                RareLootboxCountText.text = "x" + value;
        }
    }

    public int DefaultLootboxCount
    {
        set
        {
            if (DefaultLootboxCountText != null)
                DefaultLootboxCountText.text = "x" + value;
        }
    }

    public int Entities
    {
        set
        {
            if (EntityText != null)
            {
                EntityText.text = "x" + value;
            }
        }
    }

    public int Gems
    {
        set
        {
            if (GemsText != null)
                GemsText.text = "x" + value;
        }
    }

    public int Gold
    {
        set
        {
            if (GoldText != null)
                GoldText.text = "x" + value;
        }
    }

    public int Exp
    {
        set
        {
            if (ExpText != null)
                ExpText.text = "x" + value;
        }
    }

    public int HeroFragments
    {
        set
        {
            if (HeroCardText != null)
            {
                HeroCardText.text = "x" + value;
            }
        }
    }

    public int BattleTokens
    {
        set
        {
            if (BattleTokensText != null)
                BattleTokensText.text = "x" + value;
        }
    }
}
