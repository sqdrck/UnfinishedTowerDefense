using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using TMPro;

public class SettingsUi : ScreenUiElement
{
    public GameObject DevelopersWindow;
    public Button DevelopersCloseButton;
    public Button DevelopersBgButton;

    public GameObject LanguageSelectorWindow;
    public GameObject LanguageSelectorButtonPrefab;
    public Transform LanguageSelelctorContent;
    public Button[] LanguageSelectorButtons;
    public Button LanguageSelectorCloseButton;
    public Button LanguageSelectorBgButton;

    public Button CloseButton;
    public Button BgButton;
    public Button DevelopersButton;
    public Button LanguageButton;
    public TextMeshProUGUI LanguageButtonText;
    public Toggle SoundToggle;
    public Toggle MusicToggle;
    public int NeedToSwitchMusicToogle = -1;
    public int NeedToSwitchSoundToogle = -1;

    public int CurrentLocaleIndex;

    public void OnLocaleSelected(int index)
    {
        string name = LocalizationSettings.AvailableLocales.Locales[index].name;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        //PlayerPrefs.SetString("selected-locale", name);
        CurrentLocaleIndex = index;
        LanguageButtonText.text = LocalizationSettings.AvailableLocales.Locales[index].name;
        OnLanguageSelectorCloseButtonPressed();
    }

    public void OnSoundToggle(bool val)
    {
        NeedToSwitchSoundToogle = val ? 1 : 0;
    }

    public void OnMusicToggle(bool val)
    {
        NeedToSwitchMusicToogle = val ? 1 : 0;
    }

    public override void OnUpdate(GameState gameState)
    {
        if (NeedToSwitchMusicToogle != -1)
        {
            gameState.SettingsMusicEnabled = NeedToSwitchMusicToogle == 0 ? false : true;
            Debug.Log("Switching to " + gameState.SettingsMusicEnabled.ToString());
            NeedToSwitchMusicToogle = -1;
        }

        if (NeedToSwitchSoundToogle != -1)
        {
            gameState.SettingsSoundEnabled = NeedToSwitchSoundToogle == 0 ? false : true;
            Debug.Log("Switching to " + gameState.SettingsSoundEnabled.ToString());
            NeedToSwitchSoundToogle = -1;
        }
    }

    public void OnDevelopersButtonPressed()
    {
        DevelopersWindow.SetActive(true);
    }

    public void OnDevelopersCloseButtonPressed()
    {
        DevelopersWindow.SetActive(false);
    }

    public override void OnInit(GameState gameState)
    {
        CloseButton.onClick.AddListener(OnCloseButtonPressed);
        BgButton.onClick.AddListener(OnCloseButtonPressed);
        LanguageButton.onClick.AddListener(OnLanguageButtonPressed);
        LanguageSelectorCloseButton.onClick.AddListener(OnLanguageSelectorCloseButtonPressed);
        LanguageSelectorBgButton.onClick.AddListener(OnLanguageSelectorCloseButtonPressed);
        DevelopersCloseButton.onClick.AddListener(OnDevelopersCloseButtonPressed);
        DevelopersBgButton.onClick.AddListener(OnDevelopersCloseButtonPressed);
        DevelopersButton.onClick.AddListener(OnDevelopersButtonPressed);

        int localesCount = LocalizationSettings.AvailableLocales.Locales.Count;
        LanguageSelectorButtons = new Button[localesCount];
        var options = new List<string>();
        CurrentLocaleIndex = 0;
        Debug.Log("CurrentLocaleIndex " + CurrentLocaleIndex);
        for (int i = 0; i < localesCount; ++i)
        {
            var locale = LocalizationSettings.AvailableLocales.Locales[i];
            if (LocalizationSettings.SelectedLocale == locale)
            {
                CurrentLocaleIndex = i;
            }
            options.Add(locale.name);
            LanguageSelectorButtons[i] = Instantiate(LanguageSelectorButtonPrefab, LanguageSelelctorContent).GetComponent<Button>();
            LanguageSelectorButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = locale.name;
            int temp = i;
            LanguageSelectorButtons[i].onClick.AddListener(() => OnLocaleSelected(temp));
        }
        LanguageButtonText.text = options[CurrentLocaleIndex];

        SoundToggle.isOn = gameState.SettingsSoundEnabled;
        MusicToggle.isOn = gameState.SettingsMusicEnabled;
        MusicToggle.onValueChanged.AddListener(OnMusicToggle);
        SoundToggle.onValueChanged.AddListener(OnSoundToggle);

        //OnLocaleSelected(CurrentLocaleIndex);
    }

    public void OnLanguageSelectorCloseButtonPressed()
    {
        LanguageSelectorWindow.SetActive(false);
    }

    public void OnLanguageButtonPressed()
    {
        LanguageSelectorWindow.SetActive(true);
    }

    public void OnCloseButtonPressed()
    {
        gameObject.SetActive(false);
    }
}
