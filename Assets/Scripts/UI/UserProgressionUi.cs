using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UserProgressionUi : ScreenUiElement
{
    public ScrollRect LevelItemsScroll;
    public LevelItemUi LevelItemPrefab;
    public GameObject LevelItemCurrentExpPointerPrefab;
    public GameObject LevelItemCurrentExpPointer;
    public Button CloseButton;
    private List<LevelItemUi> LevelItems = new();
    private int NeedToCollectLevelRewardIndex = -1;

    public override void OnInit(GameState gameState)
    {
        CloseButton.onClick.AddListener(OnCloseButtonPressed);

        for (int i = 0; i < gameState.UserProgressionConfig.UserProgressionItems.Count; i++)
        {
            var levelPack = gameState.UserProgressionConfig.UserProgressionItems[i].ResourcePack;
            LevelItemUi item = Instantiate(LevelItemPrefab, LevelItemsScroll.content);
            item.ResourceUi.OnInit(gameState, levelPack);
            int temp = i;
            item.CollectButton.onClick.AddListener(() => OnCollectButtonPressed(temp));
            LevelItems.Add(item);
        }

    }

    private void OnCollectButtonPressed(int levelIndex)
    {
        NeedToCollectLevelRewardIndex = levelIndex;
    }

    public override void OnFixedUpdate(GameState gameState)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        if (NeedToCollectLevelRewardIndex != -1)
        {
            if (!gameState.CollectedLevelRewards[NeedToCollectLevelRewardIndex])
            {
                if (Game.CollectLevelReward(gameState, NeedToCollectLevelRewardIndex))
                {
                    Root.GetReward.Show(gameState, gameState.UserProgressionConfig.UserProgressionItems[NeedToCollectLevelRewardIndex].ResourcePack);
                    var itemUi = LevelItems[NeedToCollectLevelRewardIndex];
                    itemUi.IsCompleted = true;
                    itemUi.CollectButton.gameObject.SetActive(false);
                }
                NeedToCollectLevelRewardIndex = -1;
            }
        }
    }

    private void OnCloseButtonPressed()
    {
        Root.UserProgression.gameObject.SetActive(false);
    }


    public override void OnOpen(GameState gameState)
    {
        if (LevelItemCurrentExpPointer != null)
        {
            Destroy(LevelItemCurrentExpPointer);
        }
        int currentLevelExp;
        int currentLevel = Game.GetCurrentLevel(gameState, out currentLevelExp);
        int nextLevelRelativeExp;
        float currentLevelProgress;
        if (currentLevel == gameState.UserProgressionConfig.UserProgressionItems.Count - 1)
        {
            currentLevelProgress = 1;
        }
        else
        {
            nextLevelRelativeExp = gameState.UserProgressionConfig.UserProgressionItems[currentLevel + 1].RelativeExp;
            currentLevelProgress = currentLevelExp * 1f / nextLevelRelativeExp;
        }

        int exp = 0;
        for (int i = 0; i < gameState.UserProgressionConfig.UserProgressionItems.Count; i++)
        {
            LevelItemUi item = LevelItems[i];
            int relativeExp = gameState.UserProgressionConfig.UserProgressionItems[i].RelativeExp;
            exp += relativeExp;
            item.Level = i;
            item.LevelExp = exp;
            item.CollectButton.gameObject.SetActive(!gameState.CollectedLevelRewards[i]);
            item.IsCompleted = gameState.CollectedLevelRewards[i];
            if (i < currentLevel)
            {
                item.Progress = 1f;
            }
            else if (i > currentLevel)
            {
                item.Progress = 0;
                item.CollectButton.gameObject.SetActive(false);
            }
            else
            {
                item.Progress = currentLevelProgress;
                LevelItemCurrentExpPointer = Instantiate(LevelItemCurrentExpPointerPrefab, LevelItems[i].transform);
                LevelItemCurrentExpPointer.transform.position = item.ProgressEndpointPosition;
                LevelItemCurrentExpPointer.transform.Translate(-1f, 0, 0);
                LevelItemCurrentExpPointer.GetComponentInChildren<TextMeshProUGUI>().text = gameState.ExpCount.ToString();
            }
        }

        // TODO(sqdrck): Set to current level position.
        LevelItemsScroll.verticalNormalizedPosition = 0;
    }
}
