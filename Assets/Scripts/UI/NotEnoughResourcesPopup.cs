using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotEnoughResourcesPopup : MonoBehaviour
{
    public Color DefaultTextColor;
    public Color NotEnoughTextColor;
    public HeroCardUi HeroCard;
    public Button OkButton;
    public Button ShopButton;
    public Button CloseButton;
    public Button BgButton;

    public GameObject GoldGo;
    public GameObject GemsGo;
    public GameObject BattleTokensGo;
    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI BattleTokensText;
    public TextMeshProUGUI GemsText;
    public TextMeshProUGUI HeroCardText;

    public void OnInit(GameState gameState, CanSpendResourcePackResponse response, EntitySo entitySo = null)
    {
        if (response.GoldPrice > 0)
        {
            GoldGo.SetActive(true);
            GoldText.text = $"{gameState.GoldCount}/{response.GoldPrice}";
            GoldText.color = response.GoldNeed > 0 ? NotEnoughTextColor : DefaultTextColor;
        }
        else
        {
            GoldGo.SetActive(false);
        }
        if (response.GemsPrice > 0)
        {
            GemsGo.SetActive(true);
            GemsText.text = $"{gameState.GemsCount}/{response.GemsPrice}";
            GemsText.color = response.GemsNeed > 0 ? NotEnoughTextColor : DefaultTextColor;
        }
        else
        {
            GemsGo.SetActive(false);

        }
        if (response.BattleTokensPrice > 0)
        {
            BattleTokensGo.SetActive(true);
            BattleTokensText.text = $"{gameState.BattleTokensCount}/{response.BattleTokensPrice}";
            BattleTokensText.color = response.BattleTokensNeed > 0 ? NotEnoughTextColor : DefaultTextColor;
        }
        else
        {
            BattleTokensGo.SetActive(false);
        }
        if (response.HeroFragmentsPrice > 0)
        {
            if (entitySo != null)
            {
                HeroCard.gameObject.SetActive(true);
                HeroCard.OnInit(gameState, entitySo.Index);
                HeroCardText.text = $"{response.HeroFragmentsPrice - response.HeroFragmentsNeed}/{response.HeroFragmentsPrice}";
                HeroCardText.color = response.HeroFragmentsNeed > 0 ? NotEnoughTextColor : DefaultTextColor;
            }
        }
        else
        {
            HeroCard.gameObject.SetActive(false);
        }
    }
}
