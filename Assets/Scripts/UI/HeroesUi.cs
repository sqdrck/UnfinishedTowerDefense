using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroesUi : ScreenUiElement
{
    [Header("HeroesUi")]
    public Transform HeroesUiContent;
    public Transform HeroesUiOpenedCardsContent;
    public Transform HeroesUiClosedCardsContent;
    public int NeedOpenHeroProfileIndex = -1;
    public bool NeedToReinit = false;

    private List<HeroCardUi> HeroesUiOpenedCards = new();
    private List<HeroCardUi> HeroesUiClosedCards = new();

    public override void OnOpen(GameState gameState)
    {
        int count = HeroesUiClosedCards.Count;
        for (int i = 0; i < count; i++)
        {
            Destroy(HeroesUiClosedCards[0].gameObject);
            HeroesUiClosedCards.RemoveAt(0);
        }
        HeroesUiClosedCards.Clear();
        count = HeroesUiOpenedCards.Count;
        for (int i = 0; i < count; i++)
        {
            Destroy(HeroesUiOpenedCards[0].gameObject);
            HeroesUiOpenedCards.RemoveAt(0);
        }
        HeroesUiOpenedCards.Clear();

        for (int i = 0; i < gameState.HeroesDatabase.Length; i++)
        {
            var entry = gameState.HeroesDatabase[i];
            var entitySo = Game.GetEntitySoByIndex(gameState, entry.Index);
            HeroCardUi heroCard;
            if (gameState.EntitiesUnlocked[entry.Index])
            {
                heroCard = Instantiate(Root.HeroCardPrefab, HeroesUiOpenedCardsContent);
                HeroesUiOpenedCards.Add(heroCard);
            }
            else
            {
                heroCard = Instantiate(Root.HeroCardPrefab, HeroesUiClosedCardsContent);
                HeroesUiClosedCards.Add(heroCard);
            }
            heroCard.OnInit(gameState,
                    entitySo);
            int temp = entitySo.Index;
            heroCard.Index = temp;
            heroCard.GetComponent<Button>().onClick.AddListener(() => OnCardPressed(temp));
            heroCard.LevelUpButton.Button.onClick.AddListener(() => OnCardLevelUpPressed(temp));
        }
    }

    public override void OnUpdate(GameState gameState)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        if (NeedToReinit)
        {
            NeedToReinit = false;
            OnOpen(gameState);
        }
        if (NeedOpenHeroProfileIndex != -1)
        {
            Debug.Log("Need to open hero profile");
            Root.HeroProfile.Open(gameState, NeedOpenHeroProfileIndex);
            NeedOpenHeroProfileIndex = -1;
        }
    }

    public void OnCardLevelUpPressed(int entitySoIndex)
    {
        Root.NeedToLevelUpHeroIndex = entitySoIndex;
    }

    public void OnCardPressed(int entitySoIndex)
    {
        NeedOpenHeroProfileIndex = entitySoIndex;
    }

}
