using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class BattlefieldUi : ScreenUiElement, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public LocalizedString StartLevelLocalizedString;
    public TextMeshProUGUI StartLevelText;
    [Header("BattlefieldUI")]
    public Button BackToMainUiButton;
    public int SelectedConsumableIndex = -1;
    public int NeedToOpenHeroProfileIndex = -1;
    public bool NeedToReinit = false;

    public bool StartStagePressed = false;
    public bool SpeedUpPressed = false;
    public bool SpeedUpMorePressed = false;
    public bool StartNextWave = false;
    public bool IsPointerOverCards;

    public Button CloseHeroesButton;
    public Button WithdrawAllHeroesButton;
    public float HeroesT = 0;
    public HeroCardUi CardDd;
    public GameObject DropCardOverlay;
    public WinLosePopup LosePopup;
    public WinLosePopup WinPopup;
    public GameObject PlaceAtLeastOneHeroPopup;
    public Image[] BaseHeartImages;
    public TextMeshProUGUI WaveNumberText;
    public Button SpeedUpButton;
    public Button SpeedUpMoreButton;
    public Slider WaveProgressSlider;
    public List<MPUIKIT.MPImageBasic> PagingCircles = new();
    public int CurrentHeroesPage;
    public int HeroesPagesCount;
    public const int CardsPerPage = 8;
    public Button LeftPageButton;
    public Button RightPageButton;
    public Transform PagingContent;
    public GameObject PreparationUi;
    public GameObject BattleUi;
    public Button ArsenalButton;
    public Button HeroesButton;
    public RectTransform CardsRt;
    public ScrollRect CardsScroll;
    public LayoutGroup CardsLayout;
    public MPUIKIT.MPImageBasic HeroesMpImage;
    public RectTransform HeroesRt;
    public RectTransform HeroesOpenTargetRt;
    public RectTransform HeroesClosedTargetRt;
    public CanvasGroup HeroesCardsGroup;

    public Button StartStageButton;
    public Button NextWaveButton;

    public ScrollRect ArsenalBustersScroll;
    public ScrollRect ArsenalBuffersScroll;
    public CanvasGroup ArsenalBuffersGroup;
    public CanvasGroup ArsenalBustersGroup;
    public bool NeedToWithdrawAllHeroes;

    public bool IsBoosterPressPointsToStore;

    public List<ConsumableUi> ConsumableUis = new();
    public List<HeroCardUi> HeroCardUis = new();

    public override void OnInit(GameState gameState)
    {
        BackToMainUiButton.onClick.AddListener(ReturnToMain);

        ArsenalButton.onClick.AddListener(OnArsenalPressed);
        NextWaveButton.gameObject.SetActive(false);
        StartStageButton.onClick.AddListener(OnStartStageButtonPressed);
        NextWaveButton.onClick.AddListener(OnNextWaveButtonPressed);
        SpeedUpButton.onClick.AddListener(OnSpeedUpButtonPressed);
        SpeedUpMoreButton.onClick.AddListener(OnSpeedUpMoreButtonPressed);
        CloseHeroesButton.onClick.AddListener(() => CloseHeroes());
        WithdrawAllHeroesButton.onClick.AddListener(() => { NeedToWithdrawAllHeroes = true; });

        Debug.Assert(BaseHeartImages.Length == gameState.BaseMaxHp);

        DOTween.Init();

        HeroesMpImage = HeroesButton.GetComponent<MPUIKIT.MPImageBasic>();
        HeroesRt = HeroesButton.transform as RectTransform;
        CardsRt = HeroesCardsGroup.transform as RectTransform;
        CardsScroll = CardsRt.GetComponentInChildren<ScrollRect>();
        CardsLayout = CardsRt.GetComponentInChildren<LayoutGroup>();
        HeroesButton.onClick.AddListener(OnHeroesPressed);

        bustersInitialRect = ArsenalBuffersScroll.viewport.rect;
        buffersInitialRect = ArsenalBuffersScroll.viewport.rect;
        ArsenalBuffersScroll.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
        ArsenalBustersScroll.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        ArsenalBustersGroup = ArsenalBustersScroll.GetComponent<CanvasGroup>();
        ArsenalBuffersGroup = ArsenalBuffersScroll.GetComponent<CanvasGroup>();

        LeftPageButton.onClick.AddListener(OnLeftPageButton);
        RightPageButton.onClick.AddListener(OnRightPageButton);

    }

    public override void OnOpen(GameState gameState)
    {
        int count = ConsumableUis.Count;
        for (int i = 0; i < count; i++)
        {
            Destroy(ConsumableUis[0].gameObject);
            ConsumableUis.RemoveAt(0);
        }
        count = PagingCircles.Count;
        for (int i = 0; i < count; i++)
        {
            Destroy(PagingCircles[0].gameObject);
            PagingCircles.RemoveAt(0);
        }
        count = HeroCardUis.Count;
        for (int i = 0; i < count; i++)
        {
            Destroy(HeroCardUis[0].gameObject);
            HeroCardUis.RemoveAt(0);
        }
        Debug.Log("OnOpen");
        for (int i = 0; i < gameState.Inventory.Consumables.Count; i++)
        {
            var entry = gameState.Inventory.Consumables[i];
            var entitySo = Game.GetEntitySoByIndex(gameState, entry.EntitySoIndex);
            ConsumableUi bb;
            // TODO(sqdrck): Thing about adding ConsumableType to EntitySo;
            if (entitySo.Type is EntityType.Buffer)
            {
                bb = Instantiate(Root.BusterBufferPrefab, ArsenalBuffersScroll.content);
                bb.Type = ConsumableType.Buffer;
            }
            else
            {
                bb = Instantiate(Root.BusterBufferPrefab, ArsenalBustersScroll.content);
                bb.Type = ConsumableType.Booster;
            }
            bb.OnInit(entitySo.UiSprite, entry.Count);
            int index = i;
            bb.Button.onClick.AddListener(() => OnConsumablePressed(index));
            ConsumableUis.Add(bb);
        }

        int openedHeroesCount = 0;
        for (int i = 0; i < gameState.HeroesDatabase.Length; i++)
        {
            var entry = gameState.HeroesDatabase[i];
            var entitySo = Game.GetEntitySoByIndex(gameState, entry.Index);
            if (!gameState.EntitiesUnlocked[entitySo.Index]) continue;
            openedHeroesCount++;
            var heroCard = Instantiate(Root.HeroCardPrefab, CardsScroll.content);
            heroCard.OnInit(gameState, entitySo);
            heroCard.LevelUpButton.Button.onClick.AddListener(() => Root.NeedToLevelUpHeroIndex = entitySo.Index);
            heroCard.GetComponent<Button>().onClick.AddListener(() => OnCardPressed(entitySo.Index));
            heroCard.Index = entitySo.Index;
            HeroCardUis.Add(heroCard);
        }

        HeroesPagesCount = Mathf.CeilToInt(openedHeroesCount * 1f / CardsPerPage);
        for (int i = 0; i < HeroesPagesCount; i++)
        {
            PagingCircles.Add(Instantiate(Root.PagingCirclePrefab, PagingContent).GetComponent<MPUIKIT.MPImageBasic>());
            PagingCircles[i].color = Root.PagingCircleInactiveColor;
        }
        SelectHeroesPage(0);

        for (int i = 0; i < gameState.Entities.Length; i++)
        {
            var entity = gameState.Entities[i];
            if (entity.IsActive)
            {
                var index = HeroCardUis.FindIndex(c => c.Index == entity.EntitySoIndex);
                if (index != -1)
                {
                    HeroCardUis[index].IsBusy = true;
                }
            }
        }
    }

    public void OnCardPressed(int entitySoIndex)
    {
        NeedToOpenHeroProfileIndex = entitySoIndex;
    }

    public void OnLeftPageButton()
    {
        SelectHeroesPage(CurrentHeroesPage - 1);
    }

    public void OnRightPageButton()
    {
        SelectHeroesPage(CurrentHeroesPage + 1);
    }

    public void SelectHeroesPage(int pageIndex)
    {
        for (int i = 0; i < PagingCircles.Count; i++)
        {
            if (pageIndex == i)
            {
                PagingCircles[i].color = Root.PagingCircleActiveColor;
            }
            else
            {
                PagingCircles[i].color = Root.PagingCircleInactiveColor;
            }
        }

        int activeCardFirstIndex = CardsPerPage * pageIndex;
        int activeCardLastIndex = activeCardFirstIndex + CardsPerPage - 1;

        for (int i = 0; i < HeroCardUis.Count; i++)
        {
            HeroCardUis[i].gameObject.SetActive(i >= activeCardFirstIndex && i <= activeCardLastIndex);
        }

        CurrentHeroesPage = pageIndex;

        LeftPageButton.gameObject.SetActive(pageIndex > 0);
        RightPageButton.gameObject.SetActive(pageIndex < HeroesPagesCount - 1);

        //LeftPageButton.interactable = pageIndex > 0;
        //RightPageButton.interactable = pageIndex < HeroesPagesCount - 1;
    }

    public override void OnClose(GameState gameState)
    {
        CloseArsenal();
    }

    public void ReturnToMain()
    {
        Root.SetScreenOpened(Root.Main);
    }

    public void DeselectConsumableIfAny()
    {
        if (SelectedConsumableIndex != -1)
        {
            ConsumableUis[SelectedConsumableIndex].IsSelected = false;
            SelectedConsumableIndex = -1;
        }
    }

    public void OnConsumablePressed(int index)
    {
        DeselectConsumableIfAny();
        if (ConsumableUis[index].Count <= 0 ||
                (IsBoosterPressPointsToStore && ConsumableUis[index].Type is ConsumableType.Booster))
        {
            Root.SetScreenOpened(Root.Store);
        }
        else
        {
            ConsumableUis[index].IsSelected = true;
            SelectedConsumableIndex = index;
        }
    }

    public void EnablePlaceOneHeroPopup()
    {
        PlaceAtLeastOneHeroPopup.SetActive(true);
    }

    public void OnStartStageButtonPressed()
    {
        StartStagePressed = true;
        WaveProgressSlider.value = 0;
    }

    public void OnNextWaveButtonPressed()
    {
        Debug.Log("OnNextWaveButtonPressed");
        StartNextWave = true;
    }

    public void EnableWinLosePopup(GameState gameState, WinLosePopup popup, ResourcePack pack)
    {
        popup.gameObject.SetActive(true);
        popup.ResourceUi.OnInit(gameState, pack);

        popup.CollectButton.onClick.AddListener(OnWinLosePopupCollectPressed);
        popup.CollectAndLeaveButton.onClick.AddListener(OnWinLosePopupCollectAndLeavePressed);
    }

    public void EnableWinPopup(GameState gameState, ResourcePack pack)
    {
        EnableWinLosePopup(gameState, WinPopup, pack);
    }

    public void EnableLosePopup(GameState gameState, ResourcePack pack)
    {
        EnableWinLosePopup(gameState, LosePopup, pack);
    }

    public void OnWinLosePopupCollectAndLeavePressed()
    {
        OnWinLosePopupCollectPressed();
        ReturnToMain();
    }

    public void OnWinLosePopupCollectPressed()
    {
        WinPopup.CollectButton.onClick.RemoveAllListeners();
        WinPopup.CollectAndLeaveButton.onClick.RemoveAllListeners();
        LosePopup.CollectButton.onClick.RemoveAllListeners();
        LosePopup.CollectAndLeaveButton.onClick.RemoveAllListeners();
        WinPopup.gameObject.SetActive(false);
        LosePopup.gameObject.SetActive(false);
        Root.TopBar.gameObject.SetActive(true);
        Root.TopBar.NeedToUpdateTopBarStats = true;
        Root.Main.NeedToUpdateStats = true;
        Root.Heroes.NeedToReinit = true;
    }

    public override void OnFixedUpdate(GameState gameState)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        if (gameState.CurrentStageIndex > -1 && gameState.CurrentPhase is GamePhase.Battle)
        {
            WaveNumberText.text = $"{(gameState.LastSummonedWaveIndex + 1)}/{gameState.Config.Stages[gameState.CurrentStageIndex].Waves.Length}";
        }

        StartStageButton.gameObject.SetActive(gameState.IsPreparationPhase && !gameState.IsGameWon);
        BattleUi.SetActive(gameState.IsBattlePhase);
        PreparationUi.SetActive(gameState.IsPreparationPhase);

        for (int i = 0; i < gameState.BaseMaxHp; i++)
        {
            BaseHeartImages[i].color = gameState.BaseHp > i ? Color.white : Color.black;
        }


        StartLevelLocalizedString.Arguments = new object[] { gameState.CurrentStageIndex + 1 };
        StartLevelLocalizedString.StringChanged += UpdateStartLevelText;

        IsBoosterPressPointsToStore = gameState.CurrentPhase is GamePhase.Preparation;
    }

    public void UpdateStartLevelText(string val)
    {
        StartLevelLocalizedString.StringChanged -= UpdateStartLevelText;
        StartLevelText.text = val;
    }

    public void OnMouseButton(GameState gameState)
    {
        if (CardDd is not null)
        {
            Vector3 globalMousePos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(CardsRt,
                        Input.mousePosition,
                        gameState.CameraDisplay.Camera,
                        out globalMousePos);
            CardDd.transform.position = globalMousePos;
            var b = RectTransformUtility.RectangleContainsScreenPoint(CardsRt, Input.mousePosition, gameState.CameraDisplay.Camera);
            CardDd.gameObject.SetActive(b);
        }
    }

    public void OnMouseButtonDown(GameState gameState)
    {
    }

    public void OnMouseButtonUp(GameState gameState)
    {
        if (CardDd is not null)
        {
            if (IsPointerOverCards)
            {
                SetHeroCardBusy(CardDd.Index, false);
            }
            GameObject.Destroy(CardDd?.gameObject);
            CardDd = null;
        }
        EnableDropCardOverlay(false);
    }

    public void OnDrag(PointerEventData data)
    {

    }

    public void OnEndDrag(PointerEventData data)
    {
    }

    public HeroCardUi GetCardByIndex(int index)
    {
        return HeroCardUis.Find(c => c.Index == index);
    }

    public void SetHeroCardBusy(int index, bool busy)
    {
        HeroCardUis.Find(c => c.Index == index).IsBusy = busy;
    }

    public void EnableDropCardOverlay(bool enable)
    {
        DropCardOverlay.SetActive(enable);
    }

    public HeroCardUi CloneCard(HeroCardUi heroCardUi)
    {
        Debug.Log("CloneCard");
        var clone = Instantiate(heroCardUi, CardsScroll.content);
        clone.transform.position = heroCardUi.transform.position;
        clone.gameObject.name = "Clone";
        clone.GetComponent<LayoutElement>().ignoreLayout = true;
        var size = heroCardUi.GetComponent<RectTransform>().sizeDelta;
        var cloneRt = clone.GetComponent<RectTransform>();
        cloneRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        cloneRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        clone.transform.SetAsLastSibling();
        return clone;
    }

    public void OnBeginDrag(PointerEventData data)
    {
        if (data.selectedObject != null)
        {
            var heroCardUi = data.selectedObject.GetComponent<HeroCardUi>();
            if (heroCardUi != null && !heroCardUi.IsBusy)
            {
                CardDd = CloneCard(heroCardUi);
                SetHeroCardBusy(CardDd.Index, true);
            }

        }
    }

    public override void OnUpdate(GameState gameState)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        if (NeedToReinit)
        {
            NeedToReinit = false;
            OnOpen(gameState);
        }
        if (NeedToOpenHeroProfileIndex != -1)
        {
            Root.HeroProfile.Open(gameState, NeedToOpenHeroProfileIndex);
            NeedToOpenHeroProfileIndex = -1;
        }
        IsPointerOverCards = RectTransformUtility.RectangleContainsScreenPoint(CardsRt, Input.mousePosition, gameState.CameraDisplay.Camera);
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseButtonDown(gameState);
        }
        if (Input.GetMouseButton(0))
        {
            OnMouseButton(gameState);
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnMouseButtonUp(gameState);
        }


        NextWaveButton.gameObject.SetActive(!gameState.IsLastWave);
        if (gameState.IsBattlePhase)
        {
            StageSo stage = Game.GetCurrentStage(gameState);
            float waveProgress = gameState.LastSummonedWaveIndex * 1f / (stage.Waves.Length - 1);

            var diff = (waveProgress - WaveProgressSlider.value);
            float interpolationConstant = 0.1f;
            WaveProgressSlider.value += diff * Time.deltaTime * interpolationConstant;
            NextWaveButton.gameObject.SetActive(waveProgress > 0.8f && !gameState.IsLastWave);
        }

        ArsenalBustersScroll.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left,
                0,
                ArsenalT * bustersInitialRect.height);
        if (gameState.CurrentPhase is GamePhase.Battle)
        {
            ArsenalBuffersGroup.alpha = 0;
            ArsenalBuffersScroll.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom,
                    0,
                    0);
        }
        else
        {
            ArsenalBuffersGroup.alpha = ArsenalT;
            ArsenalBuffersScroll.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom,
                    0,
                    ArsenalT * buffersInitialRect.height);
        }
        ArsenalBustersGroup.alpha = ArsenalT;

        HeroesMpImage.RectangleCornerRadius = Vector4.Lerp(heroesInitialRectRadius, Vector4.zero, HeroesT);
        HeroesRt.sizeDelta = Vector2.Lerp(HeroesClosedTargetRt.sizeDelta, HeroesOpenTargetRt.sizeDelta, HeroesT);
        HeroesRt.localPosition = Vector3.Lerp(HeroesClosedTargetRt.localPosition, HeroesOpenTargetRt.localPosition, HeroesT);
        // NOTE(sqdrck): Use last 0.3 seconds draw cards.
        HeroesCardsGroup.alpha = (HeroesT - 0.7f) * 5;
        HeroesButton.interactable = HeroesT == 0;
        HeroesCardsGroup.blocksRaycasts = HeroesT != 0;
    }

    public void OnSpeedUpButtonPressed()
    {
        SpeedUpPressed = true;
    }

    public void OnSpeedUpMoreButtonPressed()
    {
        SpeedUpMorePressed = true;
    }

    float ArsenalT = 0;
    Tween arsenalTween;
    Tween heroesTween;
    Rect bustersInitialRect;
    Rect buffersInitialRect;

    [SerializeField]
    public Vector4 heroesInitialRectRadius;

    public bool isClosedArsenalWhenOpeningHeroes;

    public void CloseHeroes()
    {
        Debug.Log("CloseHeroes");
        if (HeroesT != 0)
        {
            OnHeroesPressed();
            if (isClosedArsenalWhenOpeningHeroes)
            {
                OnArsenalPressed(true);
                isClosedArsenalWhenOpeningHeroes = false;
            }
        }
    }

    public void OpenHeroes()
    {
        if (HeroesT != 1)
        {
            OnHeroesPressed();
        }
    }

    public void CloseArsenal()
    {
        if (ArsenalT != 0)
        {
            ArsenalT = 1;
            OnArsenalPressed();
        }

    }

    public void CloseAllUis()
    {
        CloseHeroes();
        CloseArsenal();
    }

    public void OnHeroesPressed()
    {
        Debug.Log("OnHeroesPressed");
        if (HeroesT > 0 && HeroesT != 1)
        {
            heroesTween.PlayBackwards();
        }
        else if (HeroesT == 1)
        {
            heroesTween.Kill();
            heroesTween = DOTween.To(() => HeroesT, x => HeroesT = x, 0, 0.35f).SetEase(Ease.InOutCirc).SetAutoKill(false);
        }
        else
        {
            heroesTween.Kill();
            heroesTween = DOTween.To(() => HeroesT, x => HeroesT = x, 1, 0.35f).SetEase(Ease.OutCirc).SetAutoKill(false);
        }

        if (ArsenalT != 0)
        {
            CloseArsenal();
            isClosedArsenalWhenOpeningHeroes = true;
        }
    }

    public void OnArsenalPressed()
    {
        OnArsenalPressed(false);
    }

    public void OnArsenalPressed(bool force = false)
    {
        if (HeroesT > 0 && !force) return;

        if (ArsenalT > 0 && ArsenalT != 1)
        {
            arsenalTween.PlayBackwards();
        }
        else if (ArsenalT == 1)
        {
            arsenalTween.Kill();
            arsenalTween = DOTween.To(() => ArsenalT, x => ArsenalT = x, 0, 0.35f).SetEase(Ease.OutQuart).SetAutoKill(false);
        }
        else
        {
            arsenalTween = DOTween.To(() => ArsenalT, x => ArsenalT = x, 1, 0.35f).SetEase(Ease.OutQuart).SetAutoKill(false);
        }
    }
}
