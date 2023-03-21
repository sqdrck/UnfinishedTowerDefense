using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroLevelUpButtonUi : MonoBehaviour
{
    public Color ActiveButtonColor;
    public Color InactiveButtonColor;
    public Button Button;
    public TextMeshProUGUI Text;
    public GameObject CoinGo;

    public void OnInit(GameState gameState, EntitySo so)
    {
        var result = Game.CanLevelUpHero(gameState, so);
        int currentLevel = gameState.EntityLevels[so.Index];

        Button.image.color = InactiveButtonColor;
        Button.interactable = true;
        CoinGo.SetActive(false);

        // TODO(sqdrck): Use .Result?
        if (result.HasGold && result.HasBattleTokens && result.HasHeroFragments && !result.IsHeroLastLevel && !result.IsHeroClosed)
        {
            Button.image.color = ActiveButtonColor;
            CoinGo.SetActive(true);
        }

        if (!result.IsHeroLastLevel)
        {
            Text.text = $"Level up\n{so.LevelDamageMultiplier[currentLevel + 1].Price.Gold}";
        }
        else
        {
            Text.text = "Last level";
        }

        if (result.IsHeroClosed)
        {
            Text.text = "Closed";
        }
    }
}
