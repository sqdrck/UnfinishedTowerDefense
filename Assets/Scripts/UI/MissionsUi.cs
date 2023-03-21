using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

public class MissionsUi : ScreenUiElement
{
    public MissionItemUi MissionPrefab;
    public Transform MissionsContent;
    public Button CloseButton;
    public List<MissionItemUi> Items = new();
    public int NeedToCollectRewardIndex = -1;
    public CanvasGroup TopShadow;
    public CanvasGroup BottomShadow;
    public ScrollRect scroll;

    public override void OnInit(GameState gameState)
    {
        CloseButton.onClick.AddListener(OnCloseButtonPressed);

        InitMissions(gameState);
    }

    public void InitMissions(GameState gameState)
    {
        for (int i = 0; i < gameState.Settings.Missions.Length; i++)
        {
            MissionItemUi item = Instantiate(MissionPrefab, MissionsContent);
            Items.Add(item);
            MissionSo mission = gameState.Settings.Missions[i];
            var progress = gameState.MissionsProgress[mission.Index];

            item.OnInit(gameState, mission, progress);
            int temp = i;
            item.CollectButton.onClick.AddListener(() =>
            {
                OnCollectButtonPressed(temp);
            });
        }
    }

    public override void OnUpdate(GameState gameState)
    {
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

        if (NeedToCollectRewardIndex != -1)
        {
            if (Game.CollectMissionReward(gameState, NeedToCollectRewardIndex))
            {
                Root.GetReward.Show(gameState, gameState.Settings.Missions[NeedToCollectRewardIndex].Pack.Value);
                Root.NeedToUpdateStats = true;
            }
            else
            {
                Debug.Log("Can't collect?");
            }
            NeedToCollectRewardIndex = -1;
            UpdateMissions(gameState);
        }

    }

    public void OnCollectButtonPressed(int itemIndex)
    {
        NeedToCollectRewardIndex = itemIndex;
    }

    public void UpdateMissions(GameState gameState)
    {
        for (int i = 0; i < gameState.Settings.Missions.Length; i++)
        {
            Items[i].OnUpdate(gameState.MissionsProgress[Items[i].MissionIndex]);
        }
    }

    public override void OnOpen(GameState gameState)
    {
        UpdateMissions(gameState);
    }

    public void OnCloseButtonPressed()
    {
        Root.SetScreenOpened(Root.Main);
    }
}
