using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using TMPro;

public class HeroProfileUi : ScreenUiElement
{
    public LocalizedString LevelLocString;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI HeroNameText;
    public RectTransform PagingContent;
    public RectTransform SpecContent;
    public Button CloseButton;
    public Button NextHeroButton;
    public Button PrevHeroButton;
    public Button NextSpecPageButton;
    public Button PrevSpecPageButton;
    public HeroLevelUpButtonUi LevelUpButton;
    public Transform SpawnPlace;

    private List<MPUIKIT.MPImageBasic> pagingCircles = new();
    private GameObject createdHeroProfileGo;
    private List<HeroSpecUi> createdSpecUis = new();
    private int specPagesCount = 0;
    private int specsPerPage = 6;
    private int currentSpecPage = 0;
    private int currentHeroIndex;
    public bool NeedNextHero = false;
    public bool NeedPrevHero = false;

    public void Open(GameState gameState, int entitySoIndex)
    {
        Debug.Log("Open hero profile " + entitySoIndex);
        EntitySo so = Game.GetEntitySoByIndex(gameState, entitySoIndex);
        currentHeroIndex = so.Index;
        var entityLevel = gameState.EntityLevels[entitySoIndex];
        Debug.Log("Level " + entityLevel);
        LevelLocString.Arguments = new object[] { entityLevel };
        HeroNameText.text = so.Name;
        Root.HeroProfile.gameObject.SetActive(true);
        if (so.HeroProfilePrefab != null)
        {
            createdHeroProfileGo = Instantiate(so.HeroProfilePrefab, SpawnPlace);
        }

        var spec = Instantiate(Root.HeroSpecUiPrefab, SpecContent);
        var damageMin = so.LevelDamageMultiplier[entityLevel].DamageMin;
        var damageMax = so.LevelDamageMultiplier[entityLevel].DamageMax;
        spec.NameText.text = "Damage:";
        spec.ValueText.text = damageMin.ToString() + "-" + damageMax.ToString();
        createdSpecUis.Add(spec);

        spec.NameText.text = "Damage per second:";
        spec.ValueText.text = damageMin.ToString() + "-" + damageMax.ToString();
        createdSpecUis.Add(spec);

        spec = Instantiate(Root.HeroSpecUiPrefab, SpecContent);
        spec.NameText.text = "Priority:";
        spec.ValueText.text = so.PriorityType.ToString();
        createdSpecUis.Add(spec);

        spec = Instantiate(Root.HeroSpecUiPrefab, SpecContent);
        spec.NameText.text = "Cooldown:";
        spec.ValueText.text = so.CooldownDuration.ToString();
        createdSpecUis.Add(spec);

        spec = Instantiate(Root.HeroSpecUiPrefab, SpecContent);
        spec.NameText.text = "Ultimate damage:";
        spec.ValueText.text = so.UltDamage.ToString();
        createdSpecUis.Add(spec);

        //spec = Instantiate(Root.HeroSpecUiPrefab, SpecContent);
        //spec.NameText.text = "Damage type:";
        //spec.ValueText.text = so.DamageType.ToString();
        //createdSpecUis.Add(spec);

        spec = Instantiate(Root.HeroSpecUiPrefab, SpecContent);
        spec.NameText.text = "Damage radius:";
        spec.ValueText.text = so.DamageRadius.ToString();
        createdSpecUis.Add(spec);
        spec = Instantiate(Root.HeroSpecUiPrefab, SpecContent);
        spec.NameText.text = "Crit chance:";
        spec.ValueText.text = so.CritChance.ToString();
        createdSpecUis.Add(spec);


        specPagesCount = Mathf.CeilToInt(createdSpecUis.Count * 1f / specsPerPage);
        for (int i = 0; i < specPagesCount; i++)
        {
            pagingCircles.Add(Instantiate(Root.PagingCirclePrefab, PagingContent).
                    GetComponent<MPUIKIT.MPImageBasic>());
            pagingCircles[i].color = Root.PagingCircleInactiveColor;
        }
        SelectSpecPage(0);

        LevelUpButton.OnInit(gameState, so);
        LevelLocString.RefreshString();
    }

    public void UpdateLevelText(string text)
    {
        LevelText.text = text;
    }

    public void OnHeroLevelUpPressed(int index)
    {
        Root.NeedToLevelUpHeroIndex = index;
    }

    public void OnNextHeroPressed()
    {
        Debug.Log("Need Next Hero = true");
        NeedNextHero = true;
    }

    public void OnPrevHeroPressed()
    {
        Debug.Log("Need Prev Hero = true");
        NeedPrevHero = true;
    }


    public void SelectSpecPage(int pageIndex)
    {
        for (int i = 0; i < pagingCircles.Count; i++)
        {
            if (pageIndex == i)
            {
                pagingCircles[i].color = Root.PagingCircleActiveColor;
            }
            else
            {
                pagingCircles[i].color = Root.PagingCircleInactiveColor;
            }
        }

        int activeSpecFirstIndex = specsPerPage * pageIndex;
        int activeSpecLastIndex = activeSpecFirstIndex + specsPerPage - 1;

        for (int i = 0; i < createdSpecUis.Count; i++)
        {
            createdSpecUis[i].gameObject.SetActive(i >= activeSpecFirstIndex && i <= activeSpecLastIndex);
        }
        currentSpecPage = pageIndex;

        PrevSpecPageButton.interactable = pageIndex > 0;
        NextSpecPageButton.interactable = pageIndex < specPagesCount - 1;
    }

    public override void OnInit(GameState gameState)
    {
        LevelLocString.StringChanged += UpdateLevelText;
        CloseButton.onClick.AddListener(Close);
        NextSpecPageButton.onClick.AddListener(OnNextSpecPage);
        PrevSpecPageButton.onClick.AddListener(OnPrevSpecPage);
        LevelUpButton.Button.onClick.AddListener(() => { OnHeroLevelUpPressed(currentHeroIndex); });
        PrevHeroButton.onClick.AddListener(OnPrevHeroPressed);
        NextHeroButton.onClick.AddListener(OnNextHeroPressed);
    }

    public void OnNextSpecPage()
    {
        SelectSpecPage(currentSpecPage + 1);
    }

    public void OnPrevSpecPage()
    {
        SelectSpecPage(currentSpecPage - 1);
    }

    public void Close()
    {
        Root.HeroProfile.gameObject.SetActive(false);
        Root.Heroes.NeedToReinit = true;
        Root.Battlefield.NeedToReinit = true;
        if (createdHeroProfileGo != null)
        {
            Destroy(createdHeroProfileGo);
        }
        int count = createdSpecUis.Count;
        for (int i = 0; i < count; i++)
        {
            Destroy(createdSpecUis[0].gameObject);
            createdSpecUis.RemoveAt(0);
        }
        count = pagingCircles.Count;
        for (int i = 0; i < count; i++)
        {
            Destroy(pagingCircles[0].gameObject);
            pagingCircles.RemoveAt(0);
        }
    }

    public override void OnUpdate(GameState gameState)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (NeedNextHero)
        {
            int currIndex = -1;
            for (int i = 0; i < gameState.HeroesDatabase.Length; i++)
            {
                var entry = gameState.HeroesDatabase[i];
                if (entry.Index == currentHeroIndex)
                {
                    currIndex = i + 1;
                }
            }
            for (int i = currIndex; i < gameState.HeroesDatabase.Length; i++)
            {
                var entry = gameState.HeroesDatabase[i];
                if (gameState.EntitiesUnlocked[entry.Index])
                {
                    Close();
                    Open(gameState, gameState.HeroesDatabase[i].Index);
                    break;
                }
            }
            NeedNextHero = false;
        }
        if (NeedPrevHero)
        {
            int currIndex = -1;
            for (int i = 0; i < gameState.HeroesDatabase.Length; i++)
            {
                var entry = gameState.HeroesDatabase[i];
                if (entry.Index == currentHeroIndex)
                {
                    currIndex = i - 1;
                }
            }

            for (int i = currIndex; i >= 0; i--)
            {
                var entry = gameState.HeroesDatabase[i];
                if (gameState.EntitiesUnlocked[entry.Index])
                {
                    Close();
                    Open(gameState, gameState.HeroesDatabase[i].Index);
                    break;
                }
            }
            NeedPrevHero = false;
        }
    }

}
