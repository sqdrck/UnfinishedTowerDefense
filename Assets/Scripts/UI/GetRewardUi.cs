using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetRewardUi : ScreenUiElement
{
    [Header("GetRewardUI")]
    public ResourceUi ResourceUi;
    public Button ConfirmButton;

    public override void OnInit(GameState gameState)
    {
        ConfirmButton.onClick.AddListener(OnConfirmButtonPressed);
    }

    private void OnConfirmButtonPressed()
    {
        Root.GetReward.gameObject.SetActive(false);
        Root.TopBar.NeedToUpdateTopBarStats = true;
        Root.Main.NeedToUpdateStats = true;
        Root.Heroes.NeedToReinit = true;
        Root.Battlefield.NeedToReinit = true;
    }

    public void Show(GameState gameState, ResourcePack pack)
    {
        Root.GetReward.gameObject.SetActive(true);
        ResourceUi.OnInit(gameState, pack);
    }

}
