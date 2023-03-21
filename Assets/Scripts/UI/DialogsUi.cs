using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;
using DG.Tweening;

public class DialogsUi : ScreenUiElement
{
    [Serializable]
    public struct Dialog
    {
        public bool IsSkippable;
        public DialogEntry[] Entries;
        public Sprite Sprite1;
        public Sprite Sprite2;
        public LocalizedString Name1;
        public LocalizedString Name2;
    }

    [Serializable]
    public struct DialogEntry
    {
        public LocalizedString LocString;
        public int DurationMs;
        public WhoTalks WhoTalks;
        public bool HideNextButton;
        [HideInInspector]
        public string Text;
        [HideInInspector]
        public bool HasNextEntry;
    }

    [Sirenix.OdinInspector.EnumToggleButtons]
    public enum WhoTalks
    {
        Left,
        Right,
    }

    private CancellationTokenSource dialogEntryCancelSource;
    private CancellationTokenSource dialogCancelSource;

    public TextMeshProUGUI Text;

    public TextMeshProUGUI Name1Text;
    public TextMeshProUGUI Name2Text;
    public GameObject Name1Go;
    public GameObject Name2Go;

    public Image Image1;
    public Image Image2;

    public Image NextButtonFillImage;

    public Button SkipButton;
    public Button NextButton;

    private bool dialogSkipped;

    public void NextDialogEntry()
    {
        dialogEntryCancelSource.Cancel();
    }

    public void SkipDialogEntry()
    {
        if (dialogEntryCancelSource != null && !dialogEntryCancelSource.IsCancellationRequested)
        {
            dialogSkipped = true;
            dialogEntryCancelSource.Cancel();
        }
    }

    public async UniTask PlayDialog(Dialog dialog)
    {
        NextButton.onClick.AddListener(NextDialogEntry);
        SkipButton.gameObject.SetActive(dialog.IsSkippable);
        SkipButton.onClick.AddListener(SkipDialogEntry);
        if (!dialog.Name1.IsEmpty)
        {
            Name1Text.text = await dialog.Name1.GetLocalizedStringAsync().Task;
            Image1.sprite = dialog.Sprite1;
        }
        else
        {
            Name1Go.SetActive(false);
            Image1.gameObject.SetActive(false);
        }
        if (!dialog.Name2.IsEmpty)
        {
            Name2Text.text = await dialog.Name2.GetLocalizedStringAsync().Task;
            Image2.sprite = dialog.Sprite2;
        }
        else
        {
            Name2Go.SetActive(false);
            Image2.gameObject.SetActive(false);
        }
        Tween fillNextButtonTween = null;
        for (int i = 0; i < dialog.Entries.Length; i++)
        {
            dialog.Entries[i].Text = await dialog.Entries[i].LocString.GetLocalizedStringAsync().Task;
            dialog.Entries[i].HasNextEntry = true;
            dialogEntryCancelSource = new CancellationTokenSource();
            NextButtonFillImage.fillAmount = 0;
            Show(dialog.Entries[i]);
            try
            {
                NextButton.gameObject.SetActive(!dialog.Entries[i].HideNextButton);
                fillNextButtonTween =
                    NextButtonFillImage.DOFillAmount(1, dialog.Entries[i].DurationMs * 0.001f).SetEase(Ease.Linear);
                await UniTask.Delay(dialog.Entries[i].DurationMs, cancellationToken: dialogEntryCancelSource.Token);
                fillNextButtonTween.Kill();
                NextButtonFillImage.fillAmount = 0;

            }
            catch (OperationCanceledException)
            {
                dialogEntryCancelSource.Dispose();
                fillNextButtonTween.Kill();
                NextButtonFillImage.fillAmount = 0;
                if (dialogSkipped)
                {
                    break;
                }
            }
        }

        Hide();
        dialogSkipped = false;
    }

    private AsyncOperationHandle<string> GetLocalizedText(string key, string table = "tutorial")
    {
        var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, key);
        return handle;
    }


    public void Show(DialogEntry entry)
    {
        // TODO(sqdrck): Animations.
        gameObject.SetActive(true);

        Image1.transform.DOScale(entry.WhoTalks is WhoTalks.Left ? Vector3.one * 1.25f : Vector3.one, 0.2f).SetEase(Ease.OutCubic);
        Image2.transform.DOScale(entry.WhoTalks is WhoTalks.Right ? Vector3.one * 1.25f : Vector3.one, 0.2f).SetEase(Ease.OutCubic);
        Text.text = entry.Text;
        NextButton.gameObject.SetActive(entry.HasNextEntry);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

}
