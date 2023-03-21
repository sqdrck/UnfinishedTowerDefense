using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using TMPro;

public class MissionItemUi : MonoBehaviour
{
    public Image GlowImage;
    public ParticleSystem GlowPs;
    public Image BgImage;
    public Image ResourceUiBgImage;
    public Sprite DefaultBgSprite;
    public Sprite CollectableBgSprite;
    public GameObject CollectedOverlay;
    public Button CollectButton;
    public Slider Slider;
    public Image MissionImage;
    public ResourceUi ResourceUi;
    public int Max;
    public int Current;
    public LocalizedString LocString;
    public TextMeshProUGUI MissionText;
    public TextMeshProUGUI SliderText;
    public TextMeshProUGUI SliderCollectText;
    public int MissionIndex;

    public void OnInit(GameState gameState, MissionSo missionSo, MissionProgress progress)
    {
        LocString = missionSo.LocString;
        LocString.StringChanged += OnStringChanged;
        ResourceUi.OnInit(gameState, missionSo.Pack.Value);
        Max = missionSo.Max;
        MissionIndex = missionSo.Index;
        MissionImage.sprite = missionSo.MissionSprite;
        OnUpdate(progress);
    }

    public void OnUpdate(MissionProgress progress)
    {
        bool isCollected = progress.IsCollected;
        bool isCompleted = progress.Progress >= Max;
        GlowImage.gameObject.SetActive(isCompleted && !isCollected);
        CollectButton.gameObject.SetActive(isCompleted && !isCollected);
        GlowPs.gameObject.SetActive(isCompleted && !isCollected);
        BgImage.sprite = isCompleted ? CollectableBgSprite : DefaultBgSprite;
        CollectedOverlay.SetActive(isCollected);
        Current = progress.Progress;
        ResourceUiBgImage.enabled = !(isCompleted && !isCollected);
        SliderText.text = $"{Current}/{Max}";
        SliderCollectText.gameObject.SetActive(isCompleted);
        SliderText.gameObject.SetActive(!isCompleted);

        Slider.value = Current * 1f / Max;
    }

    public void OnStringChanged(string newValue)
    {
        MissionText.text = newValue;
    }
}
