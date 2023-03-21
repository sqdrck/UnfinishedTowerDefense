using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroCardUi : MonoBehaviour
{
    [SerializeField]
    private Color busyColor;

    [SerializeField]
    private Color initialColor;

    [SerializeField]
    private Color pickButtonInitialColor;

    [SerializeField]
    private Color pickButtonBusyColor;

    [SerializeField]
    public Image mainBg;

    [SerializeField]
    public Image avatarImage;

    [SerializeField]
    private TextMeshProUGUI nameText;

    public TextMeshProUGUI LevelText;
    public HeroLevelUpButtonUi LevelUpButton;
    public TextMeshProUGUI HeroFragmentsText;
    public Image HeroFragmentsImage;
    public GameObject HeroFragmentIcon;
    public GameObject BusyOverlay;

    public int Index;

    public void OnInit(GameState gameState, int entitySoIndex)
    {
        EntitySo so = Game.GetEntitySoByIndex(gameState, entitySoIndex);
        OnInit(gameState, so);
    }

    public void OnInit(GameState gameState, EntitySo so)
    {
        avatarImage.color = so.Color;
        if (nameText != null)
        {
            nameText.text = so.Name;
        }
        if (so.UiSprite != null)
        {
            avatarImage.sprite = so.UiSprite;
        }
        else
        {
            avatarImage.sprite = null;
        }
        int currentLevel = gameState.EntityLevels[so.Index];
        if (LevelText != null)
        {
            LevelText.text = currentLevel.ToString();
        }
        if (LevelUpButton != null)
        {
            LevelUpButton.OnInit(gameState, so);
        }

        if (so.LevelDamageMultiplier.Count - 1 == currentLevel || !gameState.EntitiesUnlocked[so.Index])
        {
            HeroFragmentsText.text = "Not invoked";
            HeroFragmentIcon.SetActive(false);
            HeroFragmentsImage.fillAmount = 0;
        }
        else
        {
            HeroFragmentIcon.SetActive(true);
            int fragments = 0;
            gameState.Inventory.HeroesFragments.TryGetValue(so.Index, out fragments);
            if (HeroFragmentsText != null && HeroFragmentsImage != null)
            {
                HeroFragmentsImage.fillAmount = fragments * 1f / so.LevelDamageMultiplier[currentLevel + 1].Price.HeroFragments;
                HeroFragmentsText.text = $"{fragments}/{so.LevelDamageMultiplier[currentLevel + 1].Price.HeroFragments}";
            }
        }
    }

    private bool isBusy;
    public bool IsBusy
    {
        get => isBusy;
        set
        {
            BusyOverlay.SetActive(value);
            isBusy = value;
        }
    }
}
