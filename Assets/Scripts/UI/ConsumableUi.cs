using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConsumableUi : MonoBehaviour
{
    public Button Button;
    [SerializeField]
    private TextMeshProUGUI counter;
    [SerializeField]
    private Image image;
    [SerializeField]
    private GameObject selection;

    public ConsumableType Type;

    private int count;
    public int Count
    {
        get => count;
        set
        {
            count = value;
            if (value == 0)
            {
                counter.text = "+";
            }
            else
            {
                counter.text = value.ToString();
            }
        }
    }

    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            selection.SetActive(value);
        }
    }

    public void OnInit(Sprite sprite, int count)
    {
        IsSelected = false;
        image.sprite = sprite;
        Count = count;
    }
}
