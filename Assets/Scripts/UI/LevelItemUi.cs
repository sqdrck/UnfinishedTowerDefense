using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelItemUi : MonoBehaviour
{
    public RectTransform ProgressRt;
    public RectTransform ProgressParentRt;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI LevelExpText;
    public ResourceUi ResourceUi;
    public GameObject CompletedMarkGo;
    public Button CollectButton;

    public bool IsCompleted
    {
        set
        {
            CompletedMarkGo.SetActive(value);
        }
    }

    public Vector3 ProgressEndpointPosition
    {
        get
        {
            Vector3[] corners = new Vector3[4];
            ProgressRt.GetWorldCorners(corners);
            return corners[1];
        }
    }

    public int LevelExp
    {
        set
        {
            LevelExpText.text = value.ToString();
        }
    }

    public int Level
    {
        set
        {
            LevelText.text = value.ToString();
        }
    }

    public float Progress
    {
        set
        {
            ProgressRt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, ProgressParentRt.rect.height * value);
        }
    }

}
