using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

public class DailyRewardsUi : ScreenUiElement
{
    public Button CloseButton;
    public DailyRewardCardUi CardPrefab;
    public DailyRewardCardUi[] Cards;
    public Transform Content;
    public int NeedToCollectReward = -1;
    public TimeSpan TimeLeft;
    public int TimerCardIndex = -1;
    public CanvasGroup TopShadow;
    public CanvasGroup BottomShadow;
    public ScrollRect scroll;

    public override void OnInit(GameState gameState)
    {
        CloseButton.onClick.AddListener(OnCloseButtonPressed);

        Cards = new DailyRewardCardUi[gameState.Settings.DailyRewards.Length];
        for (int i = 0; i < gameState.Settings.DailyRewards.Length; i++)
        {
            Cards[i] = Instantiate(CardPrefab, Content);
        }

        ReinitCards(gameState);
    }

    public override void OnOpen(GameState gameState)
    {
        ReinitCards(gameState);
    }

    public void ReinitCards(GameState gameState)
    {
        for (int i = 0; i < Cards.Length; i++)
        {
            DailyRewardCardUi.PossibleState state;
            if (gameState.LastCollectedDailyRewardIndex >= i)
            {
                state = DailyRewardCardUi.PossibleState.Collected;
            }
            else if (gameState.LastCollectedDailyRewardIndex + 1 == i)
            {
                if (Game.CanCollectDailyReward(gameState, out TimeLeft))
                {
                    state = DailyRewardCardUi.PossibleState.CanCollect;
                }
                else
                {
                    state = DailyRewardCardUi.PossibleState.OnTimer;
                    TimerCardIndex = i;
                    Cards[i].TimeLeft = TimeLeft;
                }
            }
            else
            {
                state = DailyRewardCardUi.PossibleState.Locked;
            }

            Cards[i].OnInit(gameState.Settings.DailyRewards[i], i + 1, state);
            int temp = i;
            Cards[i].CollectButton.onClick.AddListener(() => NeedToCollectReward = temp);
        }
    }

    public void CollectReward(GameState gameState, int i)
    {
        if (Game.CollectDailyReward(gameState, i))
        {
            Root.GetReward.Show(gameState, gameState.Settings.DailyRewards[i]);
            Root.TopBar.NeedToUpdateTopBarStats = true;
        }
        ReinitCards(gameState);
    }

    public override void OnUpdate(GameState gameState)
    {
        if (!gameObject.activeInHierarchy) return;
        if (TimerCardIndex != -1)
        {
            if (Game.CanCollectDailyReward(gameState, out TimeLeft))
            {
                ReinitCards(gameState);
            }
            else if (Cards[TimerCardIndex].TimeLeft.Seconds != TimeLeft.Seconds)
            {
                Cards[TimerCardIndex].TimeLeft = TimeLeft;
            }

        }
        if (NeedToCollectReward != -1)
        {
            CollectReward(gameState, NeedToCollectReward);
            NeedToCollectReward = -1;
        }

        if (scroll.verticalNormalizedPosition > 0.9f)
        {
            float a = math.remap(0.9f, 1, 1, 0, scroll.verticalNormalizedPosition);
            TopShadow.alpha = a;
        }
        else
        {
            TopShadow.alpha = 1;
        }

        if (scroll.verticalNormalizedPosition < 0.1f)
        {
            float a = math.remap(0, 0.1f, 0, 1, scroll.verticalNormalizedPosition);
            BottomShadow.alpha = a;
        }
        else
        {
            BottomShadow.alpha = 1;
        }

    }

    public void OnCloseButtonPressed()
    {
        Root.SetScreenOpened(Root.Main);
    }
}
