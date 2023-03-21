using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using DG.Tweening;
using Unity.Mathematics;

public class TopBarUi : ScreenUiElement
{
    [Header("TopBarUi")]
    public Transform TopBarContent;
    public TextMeshProUGUI TopBarGoldText;
    public TextMeshProUGUI TopBarGemsText;
    public TextMeshProUGUI TopBarBattleTokensText;
    public Button TopBarGoldButton;
    public Button TopBarGemsButton;
    public Button TopBarBattleTokensButton;

    public bool NeedToUpdateTopBarStats;

    public int CurrentGoldCount;
    public int CurrentGemsCount;
    public int CurrentBattleTokensCount;
    public int CurrentDefaultLootboxCount;
    public int CurrentRareLootboxCount;

    public override void OnInit(GameState gameState)
    {
        TopBarGemsButton.onClick.AddListener(() =>
        {
            Root.SetScreenOpened(Root.Store);
        });
        TopBarBattleTokensButton.onClick.AddListener(() =>
        {
            Root.SetScreenOpened(Root.Store);
        });
        TopBarGoldButton.onClick.AddListener(() =>
        {
            Root.SetScreenOpened(Root.Store);
        });
    }

    public override void OnClose(GameState gameState)
    {
        Debug.Log("Closed");
    }

    public override void OnFixedUpdate(GameState gameState)
    {
        if (NeedToUpdateTopBarStats)
        {
            NeedToUpdateTopBarStats = false;
            SetTopBarStats(gameState);
        }
    }

    public void SetTopBarStats(GameState gameState, bool animate = true)
    {
        if (animate)
        {
            TopBarGoldText.text = CurrentGoldCount.ToString();
            TopBarBattleTokensText.text = CurrentBattleTokensCount.ToString();
            TopBarGemsText.text = CurrentGemsCount.ToString();
            if (CurrentGoldCount < gameState.GoldCount)
            {
                AnimateTextIncreaseWithFlyingPrefabs(Root.CoinPrefab, TopBarGoldText, TopBarGoldText.transform.position,
                        CurrentGoldCount,
                        gameState.GoldCount, math.min(gameState.GoldCount - CurrentGoldCount, 12));
            }
            else
            {
                AnimateTextIncrease(TopBarGoldText, CurrentGoldCount, gameState.GoldCount);
            }
            if (CurrentGemsCount < gameState.GemsCount)
            {
                AnimateTextIncreaseWithFlyingPrefabs(Root.GemPrefab, TopBarGemsText, TopBarGemsText.transform.position,
                        CurrentGemsCount,
                        gameState.GemsCount, math.min(gameState.GemsCount - CurrentGemsCount, 12));
            }
            else
            {
                AnimateTextIncrease(TopBarGemsText, CurrentGemsCount, gameState.GemsCount);
            }
            if (CurrentBattleTokensCount < gameState.BattleTokensCount)
            {
                AnimateTextIncreaseWithFlyingPrefabs(Root.BattleTokenPrefab, TopBarBattleTokensText, TopBarBattleTokensText.transform.position,
                        CurrentBattleTokensCount,
                        gameState.BattleTokensCount, math.min(gameState.BattleTokensCount - CurrentBattleTokensCount, 12));
            }
            else
            {
                AnimateTextIncrease(TopBarBattleTokensText, CurrentBattleTokensCount, gameState.BattleTokensCount);
            }
        }
        else
        {
            TopBarGoldText.text = gameState.GoldCount.ToString();
            TopBarBattleTokensText.text = gameState.BattleTokensCount.ToString();
            TopBarGemsText.text = gameState.GemsCount.ToString();
        }
        CurrentGemsCount = gameState.GemsCount;
        CurrentGoldCount = gameState.GoldCount;
        CurrentBattleTokensCount = gameState.BattleTokensCount;
    }

    public void AnimateFlyingPrefabs(GameObject prefab, Vector3 pos, int maxVisualPrefabs = 12)
    {
        for (int i = 0; i < maxVisualPrefabs; i++)
        {
            var go = GameObject.Instantiate(prefab, TopBarContent);
            go.transform.localScale = Vector3.zero;
            var goPos = go.transform.position;
            goPos = new Vector3(goPos.x, goPos.y, 0);

            var randScale = UnityEngine.Random.Range(1, 1.5f);
            var randDelay = UnityEngine.Random.Range(0, 0.3f);

            Sequence seq = DOTween.Sequence();
            seq.Append(go.transform.DOScale(Vector3.one * randScale, 0.3f).SetEase(Ease.OutCubic));
            seq.Join(go.transform.DOMove(goPos + (Vector3)UnityEngine.Random.insideUnitCircle * 2, 0.3f).SetEase(Ease.OutCubic));
            seq.Append(go.transform.DOScale(Vector3.zero, 0.7f).SetEase(Ease.InCubic));
            seq.Join(go.transform.DOMove(pos, 0.7f).SetEase(Ease.InCubic));
            seq.SetDelay(randDelay);

            seq.OnComplete(() =>
            {
                Destroy(go);
            });
        }
    }

    public void AnimateTextIncrease(TextMeshProUGUI text, int from, int to, float delay = 0)
    {
        float t = 0;
        DOTween.To(() => t, x => t = x, 1, 0.7f)
            .SetDelay(delay)
            .OnUpdate(() =>
            {
                int animatedCount = Mathf.CeilToInt(math.remap(0, 1, from, to, t));

                text.text = animatedCount.ToString();
            });
    }

    public void AnimateTextIncreaseWithFlyingPrefabs(GameObject prefab, TextMeshProUGUI text, Vector3 pos, int from, int to, int maxVisualPrefabs = 12)
    {
        AnimateFlyingPrefabs(prefab, pos, maxVisualPrefabs);
        AnimateTextIncrease(text, from, to, 0.7f);
    }
}
