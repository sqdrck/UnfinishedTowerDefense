using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageCircleUi : MonoBehaviour
{
    public enum StageCircleState
    {
        Completed = 0,
        Current = 1,
        Uncompleted = 2,
    }

    public Color CompletedStageCircleColor;
    public Color CurrentStageCircleColor;
    public Color UncompletedStageCircleColor;

    public GameObject gemsGo;
    public GameObject goldGo;
    public GameObject defaultLootboxGo;
    public GameObject rareLootboxGo;
    public GameObject addRewardGo;

    public Image image;
    public TMPro.TextMeshProUGUI numberText;

    public bool HasAddReward
    {
        set
        {
            addRewardGo.SetActive(value);
        }
    }

    public bool IsGemsActive
    {
        set
        {
            gemsGo.SetActive(value);
        }
    }

    public bool IsGoldActive
    {
        set
        {
            goldGo.SetActive(value);
        }
    }

    public bool IsRareLootboxActive
    {
        set
        {
            rareLootboxGo.SetActive(value);
        }
    }

    public bool IsDefaultLooboxActive
    {
        set
        {
            defaultLootboxGo.SetActive(value);
        }
    }

    public int Number
    {
        set
        {
            numberText.text = value.ToString();
        }
    }

    public StageCircleState State
    {
        set
        {
            image.color = value is StageCircleState.Completed ? CompletedStageCircleColor : value is StageCircleState.Current ? CurrentStageCircleColor : UncompletedStageCircleColor;
            transform.localScale = value is StageCircleState.Current ? Vector3.one * 1.2f : Vector3.one;
        }
    }
}
