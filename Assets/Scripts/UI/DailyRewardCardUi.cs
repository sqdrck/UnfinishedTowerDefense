using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyRewardCardUi : MonoBehaviour
{
    public enum PossibleState
    {
        Locked,
        OnTimer,
        CanCollect,
        Collected,
    }
    public ResourceUi resourceUi;
    public Button CollectButton;
    public TextMeshProUGUI NumberText;
    public GameObject CheckGo;
    public GameObject TimerGo;
    public TextMeshProUGUI TimerText;
    private TimeSpan timeLeft;
    public TimeSpan TimeLeft
    {
        get => timeLeft;
        set
        {
            TimerText.text = Utils.FormatTime(value);
            timeLeft = value;
        }
    }

    public void OnInit(ResourcePack pack, int number, PossibleState state)
    {
        resourceUi.OnInit(null, pack);
        NumberText.text = number.ToString();

        CollectButton.gameObject.SetActive(state is PossibleState.CanCollect);
        CheckGo.SetActive(state is PossibleState.Collected);
        TimerGo.SetActive(state is PossibleState.OnTimer);
    }
}
