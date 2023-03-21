using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Canvas))]
public class MainCanvas : MonoBehaviour
{
    public UiHighlighter UiHighlighter;
    [Header("MainCanvas")]
    public Color ActiveTabViewButtonColor;
    public Color InactiveTabViewButtonColor;
    public Color ActiveButtonColor;
    public Color InactiveButtonColor;
    public GameObject TabView;
    public Canvas Canvas;
    public GameObject PagingCirclePrefab;

    public GameObject CoinPrefab;
    public GameObject GemPrefab;
    public GameObject HeroFragmentPrefab;
    public GameObject BattleTokenPrefab;
    public GameObject DefaultLootboxPrefab;
    public GameObject RareLootboxPrefab;
    public GameObject ExpPrefab;
    public ConsumableUi BusterBufferPrefab;
    public HeroCardUi HeroCardPrefab;
    public HeroSpecUi HeroSpecUiPrefab;
    public StageCircleUi StageCirclePrefab;

    public LootboxUi LootboxUiPrefab;
    public Color PagingCircleActiveColor = Color.white;
    public Color PagingCircleInactiveColor = Color.gray;

    // NOTE(sqdrck): ScreenUiElements(begin) ===============
    public List<ScreenUiElement> Elements = new(); // Populated by reflection.
    public TopBarUi TopBar;
    public HeroesUi Heroes;
    public HeroesInvokerUi HeroesInvoker;
    public StoreUi Store;
    public MainUi Main;
    public BattlefieldUi Battlefield;
    public UserProgressionUi UserProgression;
    public LootboxesUi Lootboxes;
    public GetRewardUi GetReward;
    public HeroProfileUi HeroProfile;
    public NotEnoughResourcesPopup NotEnoughResourcesPopup;
    public DialogsUi Dialogs;
    public MenuUi Menu;
    public MissionsUi Missions;
    public SettingsUi Settings;
    public DailyRewardsUi DailyRewards;
    // NOTE(sqdrck): ScreenUiElements(end) ===============

    public Button StoreButton;
    public Button MainButton;
    public Button HeroesButton;
    public Button InvokeHeroButton;
    public TextMeshProUGUI HeroesButtonText;

    public int NeedToLevelUpHeroIndex = -1;
    public bool NeedToUpdateStats = false;

    private ScreenUiElement NeedToOpenScreen = null;

    public void HighlightHeroesButton()
    {
        Vector3 localScale = HeroesButtonText.transform.localScale;
        HeroesButtonText.transform.DOScale(localScale * 1.5f, 0.2f)
            .OnComplete(() =>
                    {
                        HeroesButtonText.transform.DOScale(localScale, 0.2f);
                    });
    }

    public void OnValidate()
    {
        Canvas = GetComponent<Canvas>();
    }

    public void OnInit(GameState gameState)
    {
        var fields = typeof(MainCanvas).GetFields();
        Elements.Capacity = 32;
        for (int i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            if (field.GetValue(this).GetType().IsSubclassOf(typeof(ScreenUiElement)))
            {
                Elements.Add((ScreenUiElement)field.GetValue(this));
            }
        }

        for (int i = 0; i < Elements.Count; i++)
        {
            Elements[i].Root = this;
            Elements[i].OnInit(gameState);
        }

        OnTabViewInit(gameState);
        TopBar.SetTopBarStats(gameState, false);
        Main.SetStats(gameState, false);

        NotEnoughResourcesPopup.CloseButton.onClick.AddListener(() => NotEnoughResourcesPopup.gameObject.SetActive(false));
        NotEnoughResourcesPopup.BgButton.onClick.AddListener(() => NotEnoughResourcesPopup.gameObject.SetActive(false));
        NotEnoughResourcesPopup.ShopButton.onClick.AddListener(() =>
        {
            NotEnoughResourcesPopup.gameObject.SetActive(false);
            SetScreenOpened(Store);
        });

        SetScreenOpened(Main);


    }
    public bool DisableUpdate = false;
    public void OnUpdate(GameState gameState)
    {
        if (DisableUpdate) return;
        if (NeedToUpdateStats)
        {
            NeedToUpdateStats = false;
            TopBar.NeedToUpdateTopBarStats = true;
            Main.NeedToUpdateStats = true;
        }

        for (int i = 0; i < Elements.Count; i++)
        {
            Elements[i].OnUpdate(gameState);
        }

        if (NeedToOpenScreen != null)
        {
            OpenScreen(gameState, NeedToOpenScreen);
            NeedToOpenScreen = null;
        }

        if (NeedToLevelUpHeroIndex != -1)
        {
            CanSpendResourcePackResponse notEnough;
            var success = Game.LevelUpHero(gameState, NeedToLevelUpHeroIndex, out notEnough);
            if (success)
            {
                if (HeroProfile.gameObject.activeInHierarchy)
                {
                    HeroProfile.Close();
                    HeroProfile.Open(gameState, NeedToLevelUpHeroIndex);
                }
                Battlefield.NeedToReinit = true;
                Heroes.NeedToReinit = true;
                TopBar.NeedToUpdateTopBarStats = true;
            }
            else
            {
                NotEnoughResourcesPopup.gameObject.SetActive(true);
                NotEnoughResourcesPopup.OnInit(gameState, notEnough, Game.GetEntitySoByIndex(gameState, NeedToLevelUpHeroIndex));

            }
            NeedToLevelUpHeroIndex = -1;
        }
    }
    public bool DisableFixedUpdate = false;
    public void OnFixedUpdate(GameState gameState)
    {
        if (DisableFixedUpdate) return;
        for (int i = 0; i < Elements.Count; i++)
        {
            Elements[i].OnFixedUpdate(gameState);
        }
    }

    public void OnTabViewInit(GameState gameState)
    {
        StoreButton.onClick.AddListener(() => SetScreenOpened(Store));
        MainButton.onClick.AddListener(() => SetScreenOpened(Main));
        HeroesButton.onClick.AddListener(() => SetScreenOpened(Heroes));
        InvokeHeroButton.onClick.AddListener(() => SetScreenOpened(HeroesInvoker));
    }

    public void SetScreenOpened(ScreenUiElement element)
    {
        NeedToOpenScreen = element;
    }

    public void OpenScreen(GameState gameState, ScreenUiElement element)
    {
        Debug.Log("OpenScreen " + element.name);
        StoreButton.GetComponent<Image>().color = InactiveTabViewButtonColor;
        MainButton.GetComponent<Image>().color = InactiveTabViewButtonColor;
        HeroesButton.GetComponent<Image>().color = InactiveTabViewButtonColor;
        InvokeHeroButton.GetComponent<Image>().color = InactiveTabViewButtonColor;

        TabView.gameObject.SetActive(element is not BattlefieldUi);

        bool defaultBeh = false;

        if (element is MainUi)
        {
            defaultBeh = true;
            MainButton.GetComponent<Image>().color = ActiveButtonColor;
        }
        else if (element is HeroesUi)
        {
            defaultBeh = true;
            HeroesButton.GetComponent<Image>().color = ActiveButtonColor;
        }
        else if (element is StoreUi)
        {
            defaultBeh = true;
            StoreButton.GetComponent<Image>().color = ActiveButtonColor;
        }
        else if (element is HeroesInvokerUi)
        {
            defaultBeh = true;
            InvokeHeroButton.GetComponent<Image>().color = ActiveButtonColor;
        }
        else if (element is MenuUi)
        {
            Menu.gameObject.SetActive(true);
            MainButton.GetComponent<Image>().color = ActiveTabViewButtonColor;
        }
        else if (element is UserProgressionUi)
        {
            UserProgression.gameObject.SetActive(true);
            MainButton.GetComponent<Image>().color = ActiveTabViewButtonColor;
        }
        else if (element is LootboxesUi)
        {
            Lootboxes.gameObject.SetActive(true);
        }
        else if (element is GetRewardUi)
        {
            GetReward.gameObject.SetActive(true);
        }
        else if (element is SettingsUi)
        {
            Settings.gameObject.SetActive(true);
        }
        else
        {
            defaultBeh = true;
        }

        if (defaultBeh)
        {
            element.gameObject.SetActive(true);

            for (int i = 0; i < Elements.Count; i++)
            {
                if (Elements[i] != element && Elements[i] != TopBar)
                {
                    Elements[i].gameObject.SetActive(false);
                    Elements[i].OnClose(gameState);
                }
            }

        }

        element.OnOpen(gameState);
    }

}
