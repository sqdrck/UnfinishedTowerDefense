using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Canvas))]
public class UiHighlighter : MonoBehaviour
{
    public Color DisabledSurroundsColor;
    public Color EnabledSurroundsColor;
    [SerializeField]
    private RectTransform Down, Up, Left, Right, Test, Hand;
    private Canvas Canvas;
    public Camera AvailableCamera;
    private RectTransform CanvasRt;
    public float LeftRightPadding = 0;
    public float SurroundsPadding = 0;
    public float BorderPadding = 0;
    private bool UpdateTargetAlways = false;
    private bool EncapsulateTargetChildrenBounds = false;
    private GameObject Target;

    public bool ShowHand
    {
        set
        {
            Hand.gameObject.SetActive(value);
        }
    }

    public bool ShowSurrounds
    {
        set
        {
            Down.GetComponent<Image>().color = value ? EnabledSurroundsColor : DisabledSurroundsColor;
            Up.GetComponent<Image>().color = value ? EnabledSurroundsColor : DisabledSurroundsColor;
            Left.GetComponent<Image>().color = value ? EnabledSurroundsColor : DisabledSurroundsColor;
            Right.GetComponent<Image>().color = value ? EnabledSurroundsColor : DisabledSurroundsColor;
        }
    }

    public bool ShowBorder
    {
        set
        {
            Test.gameObject.SetActive(value);
        }
    }


    private void OnValidate()
    {
        Canvas = GetComponent<Canvas>();
        CanvasRt = Canvas.GetComponent<RectTransform>();
    }

    [Sirenix.OdinInspector.Button("StartPrompt")]
    public void StartPromptEveryFrame(GameObject go, bool encapsulateChildrenBounds, Camera camera = null)
    {
        Canvas.gameObject.SetActive(true);

        SetupCamera(camera);
        Target = go;
        UpdateTargetAlways = true;
        EncapsulateTargetChildrenBounds = encapsulateChildrenBounds;
    }

    [Sirenix.OdinInspector.Button("StopPrompt")]
    public void StopPromptEveryFrame()
    {
        UpdateTargetAlways = false;
        Target = null;
        Dismiss();
    }

    [Sirenix.OdinInspector.Button("Lerp")]
    public void Lerp(RectTransform fromRt, RectTransform toRt, float duration = 1f)
    {
        Prompt(fromRt);

        Sequence s = DOTween.Sequence();
        s.Append(Test.DOMove(toRt.position, duration)).OnUpdate(() =>
        {
            SetSidesByTest();
            Test.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(fromRt.rect.width + BorderPadding, toRt.rect.width + BorderPadding, s.position));
            Test.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(fromRt.rect.height + BorderPadding, toRt.rect.height + BorderPadding, s.position));
        }).SetEase(Ease.InOutQuad);
    }

    public void Update()
    {
        if (UpdateTargetAlways && Target != null)
        {
            Prompt(Target, EncapsulateTargetChildrenBounds, AvailableCamera);
        }
    }

    private void SetupCamera(Camera camera)
    {
        if (camera == null && AvailableCamera == null)
        {
            camera = Camera.main;
        }
        AvailableCamera = camera;
    }

    [Sirenix.OdinInspector.Button("PromptGo")]
    public void Prompt(GameObject go, bool encapsulateChildrenBounds = false, Camera camera = null)
    {
        Canvas.gameObject.SetActive(true);

        SetupCamera(camera);
        Bounds bounds;
        if (encapsulateChildrenBounds)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; ++i)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }
        else
        {
            bounds = go.GetComponent<Renderer>().bounds;
        }
        float pixelsPerUnit = CanvasRt.rect.height / AvailableCamera.orthographicSize / 2;
        Canvas.gameObject.SetActive(true);

        Test.position = bounds.center;
        Test.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bounds.extents.x * 2 * pixelsPerUnit);
        Test.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bounds.extents.y * 2 * pixelsPerUnit);

        SetSidesByTest();
    }

    [Sirenix.OdinInspector.Button("Prompt")]
    public void Prompt(RectTransform rt)
    {
        Canvas.gameObject.SetActive(true);

        Test.position = rt.position;
        Test.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rt.rect.width + BorderPadding);
        Test.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rt.rect.height + BorderPadding);

        SetSidesByTest();
    }

    private void SetSidesByTest()
    {
        Down.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CanvasRt.rect.width);
        Down.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, Test.offsetMin.y - SurroundsPadding + BorderPadding * 0.5f);

        Up.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CanvasRt.rect.width);
        Up.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, -Test.offsetMax.y - SurroundsPadding + BorderPadding * 0.5f);

        Left.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -Test.offsetMax.y - SurroundsPadding + BorderPadding * 0.5f, Test.rect.height + SurroundsPadding * 2 - BorderPadding);
        Right.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -Test.offsetMax.y - SurroundsPadding + BorderPadding * 0.5f, Test.rect.height + SurroundsPadding * 2 - BorderPadding);

        Left.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, Test.offsetMin.x - SurroundsPadding + BorderPadding * 0.5f - LeftRightPadding);
        Right.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, -Test.offsetMax.x - SurroundsPadding + BorderPadding * 0.5f - LeftRightPadding);
    }

    public void Dismiss()
    {
        Canvas.gameObject.SetActive(false);
    }
}
