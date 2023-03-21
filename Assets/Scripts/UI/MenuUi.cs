using UnityEngine;
using UnityEngine.UI;

public class MenuUi : ScreenUiElement
{
    public Button CloseButton;
    public Button BgButton;
    public MenuButtonUi MissionsButtonUi;
    public MenuButtonUi DailyRewardsButtonUi;
    public MenuButtonUi SettingsButtonUi;

    public override void OnInit(GameState gameState)
    {
        CloseButton.onClick.AddListener(OnCloseButtonPressed);
        BgButton.onClick.AddListener(OnCloseButtonPressed);
        DailyRewardsButtonUi.Button.onClick.AddListener(OnDailyRewardsButtonPressed);
        SettingsButtonUi.Button.onClick.AddListener(OnSettingsButtonPressed);
        MissionsButtonUi.Button.onClick.AddListener(OnMissionsButtonPressed);
    }

    public override void OnOpen(GameState gameState)
    {
        Vibration.Selection();
        DailyRewardsButtonUi.Counter = Game.CanCollectDailyReward(gameState, out _) ? 1 : 0;
        SettingsButtonUi.Counter = 0;
        MissionsButtonUi.Counter = 0;
    }

    public void OnDailyRewardsButtonPressed()
    {
        Debug.Log("OnDailyRewardsPressed");
        OnCloseButtonPressed();
        Root.SetScreenOpened(Root.DailyRewards);
    }

    public void OnMissionsButtonPressed()
    {
        Debug.Log("OnMissions");
        OnCloseButtonPressed();
        Root.SetScreenOpened(Root.Missions);
    }

    public void OnSettingsButtonPressed()
    {
        Debug.Log("OnSettings");
        Root.SetScreenOpened(Root.Settings);
        OnCloseButtonPressed();
    }

    public void OnCloseButtonPressed()
    {
        gameObject.SetActive(false);
    }
}
