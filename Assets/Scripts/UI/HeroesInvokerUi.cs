using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using DG.Tweening;

public class HeroesInvokerUi : ScreenUiElement
{
    public Sprite OpenedBg;
    public Sprite ClosedBg;
    public Button InvokeButton;
    public Button WelcomeButton;
    public Image BgImage;
    public TextMeshProUGUI InvokeButtonText;

    public ParticleSystem InvokePs;
    public ParticleSystem CollectPs;
    public ParticleSystem FlyPs;
    public ParticleSystem FlyCompletePs;

    private bool hasMoney = false;
    private EntitySo accessableHero;
    private bool needToUnlockHero = false;
    private bool needToGetFragments = false;
    public ResourcePack defaultPriceResourcePack;
    public ResourcePack defaultRewardResourcePack;
    private ResourcePack resourcePack;
    public Transform HeroUiPrefabSpawnPlace;
    private GameObject spawnedHero;
    private Tween flyTween;

    public override void OnOpen(GameState gameState)
    {
        EntitySo firstLockedHero = null;
        Debug.Log("Looping through heroes database");
        for (int i = 0; i < gameState.HeroesDatabase.Length; i++)
        {
            var hero = gameState.HeroesDatabase[i];
            Debug.Log(hero.Name + " unlocked: " + gameState.EntitiesUnlocked[hero.Index]);
            if (!gameState.EntitiesUnlocked[hero.Index])
            {
                firstLockedHero = hero;
                break;
            }
        }

        if (firstLockedHero != null)
        {
            accessableHero = firstLockedHero;
            resourcePack = firstLockedHero.UnlockPrice;
            Debug.Log("First locked hero: " + firstLockedHero.Name);
        }
        else
        {
            resourcePack = defaultPriceResourcePack;
            Debug.Log("Using default resource pack");
        }

        var response = Game.CanSpendResourcePack(gameState, resourcePack);
        hasMoney = response.Result;

        InvokeButton.image.color = hasMoney ? Root.ActiveButtonColor : Root.InactiveButtonColor;
        InvokeButtonText.text = resourcePack.Gems.ToString() + " Invoke";
        BgImage.sprite = hasMoney ? OpenedBg : ClosedBg;
    }

    public override void OnInit(GameState gameState)
    {
        InvokeButton.onClick.AddListener(OnInvokeButtonPressed);
        WelcomeButton.onClick.AddListener(OnWelcomeButtonPressed);
        WelcomeButton.gameObject.SetActive(false);
        InvokeButton.gameObject.SetActive(true);
    }

    public void OnInvokeButtonPressed()
    {
        if (accessableHero)
        {
            needToUnlockHero = true;
        }
        else
        {
            needToGetFragments = true;
        }
    }

    public void OnWelcomeButtonPressed()
    {
        if (flyTween != null)
        {
            flyTween.Kill();
        }
        WelcomeButton.gameObject.SetActive(false);
        InvokeButton.gameObject.SetActive(true);
        FlyPs.transform.position = CollectPs.transform.position;
        CollectPs.Play();
        FlyPs.Play();

        //spawnedHero.transform.DOScale(Vector3.zero, 1f);
        Destroy(spawnedHero);
        flyTween = FlyPs.transform.DOMove(Root.HeroesButtonText.transform.position, 1f)
                //.OnUpdate(() =>
                //spawnedHero.transform.Rotate(0, 0, 500 * Time.deltaTime);
                //spawnedHero.transform.position = FlyPs.transform.position;
                //})
                .OnComplete(() =>
                        {
                            FlyPs.Stop();
                            FlyCompletePs.transform.position = FlyPs.transform.position;
                            FlyCompletePs.Play();
                            Root.HighlightHeroesButton();
                        });
    }

    public override void OnFixedUpdate(GameState gameState)
    {

    }

    public override void OnUpdate(GameState gameState)
    {
        if (needToUnlockHero || needToGetFragments)
        {
            bool success = false;
            CanSpendResourcePackResponse notEnough = new();
            if (needToUnlockHero)
            {
                needToUnlockHero = false;
                success = Game.UnlockHero(gameState, accessableHero, out notEnough);
                if (success)
                {
                    Debug.Log("Spawning hero prefab");
                    spawnedHero = Instantiate(accessableHero.HeroProfilePrefab, HeroUiPrefabSpawnPlace);
                    WelcomeButton.gameObject.SetActive(true);
                    InvokeButton.gameObject.SetActive(false);
                    accessableHero = null;
                    InvokePs.Play();
                    CollectPs.Play();
                }
            }
            else if (needToGetFragments)
            {
                needToGetFragments = false;
                success = Game.SpendResourcePack(gameState, defaultPriceResourcePack, out notEnough);
                if (success)
                {
                    Game.ApplyResourcePack(gameState, defaultRewardResourcePack);
                    Root.GetReward.Show(gameState, defaultRewardResourcePack);
                }
            }

            if (success)
            {
                Root.TopBar.NeedToUpdateTopBarStats = true;
            }
            else
            {
                Root.NotEnoughResourcesPopup.gameObject.SetActive(true);
                Root.NotEnoughResourcesPopup.OnInit(gameState, notEnough, null);
            }
            OnOpen(gameState);
        }
    }

}
