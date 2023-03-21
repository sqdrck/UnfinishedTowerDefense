using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuButtonUi : MonoBehaviour
{
    public Button Button;
    public TextMeshProUGUI CounterText;
    public GameObject CounterGo;

    public int Counter
    {
        set
        {
            CounterGo.SetActive(value != 0);
            CounterText.text = value.ToString();
        }
    }
}
