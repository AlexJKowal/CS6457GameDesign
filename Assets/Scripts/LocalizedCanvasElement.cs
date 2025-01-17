using UnityEngine;
using UnityEngine.Events;
public class LocalizedCanvasElement : MonoBehaviour
{
    public GameObject uiTarget;
    public GameObject subGroup;
    public Canvas canvas;
    public Camera mainCamera;
    public float shotTimer;
    public GameObject shotSlider;

    private RectTransform rectTransform;
    private RectTransform shotRectTransform;


    private Vector2 offsetCanvasSpace;

    private CanvasGroup cg;

    private UnityAction<GameObject, SquareLocation> ballCaughtEventListener;
    private UnityAction<SquareLocation, ShotType> ballHitEventListener;

    private bool startCounter;
    private float countDown;

    private const float MAX_WIDTH = 90f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        shotRectTransform = shotSlider.GetComponent<RectTransform>();

        cg = subGroup.GetComponent<CanvasGroup>();
        cg.alpha = 0;
        countDown = shotTimer;

        // Adjust this as needed
        offsetCanvasSpace = new Vector2(-30, -50);


        UpdatePosition();
    }

    void Awake()
    {
        ballCaughtEventListener = new UnityAction<GameObject, SquareLocation>(BallCaughtEventHandler);
        ballHitEventListener = new UnityAction<SquareLocation, ShotType>(BallHitEventHandler);
    }

    void OnEnable()
    {
        EventManager.StartListening<BallCaughtEvent, GameObject, SquareLocation>(ballCaughtEventListener);
        EventManager.StartListening<BallHitEvent, SquareLocation, ShotType>(ballHitEventListener);
    }

    void OnDisable()
    {
        EventManager.StopListening<BallCaughtEvent, GameObject, SquareLocation>(ballCaughtEventListener);
        EventManager.StopListening<BallHitEvent, SquareLocation, ShotType>(ballHitEventListener);
    }

    void Update()
    {
        if (startCounter)
        {
            UpdatePosition();
            UpdateCountdown();
        }
    }

    void BallCaughtEventHandler(GameObject caughtBy, SquareLocation caughtAt)
    {
        if (caughtBy.CompareTag("Player") && uiTarget.GetComponent<PlayerController>().ballServed)
        {
            cg.alpha = 1;
            startCounter = true;
        }
    }

    void BallHitEventHandler(SquareLocation hitSquare, ShotType shotType)
    {
        cg.alpha = 0;
        startCounter = false;
        countDown = shotTimer;
        

    }

    void UpdatePosition()
    {
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(uiTarget.transform.position);
        Vector2 canvasPosition;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPosition, mainCamera, out canvasPosition))
        {
            rectTransform.localPosition = canvasPosition + offsetCanvasSpace;
        }
    }

    void UpdateCountdown()
    {
        if (countDown <= 0)
        {
            EventManager.TriggerEvent<ShotTimeUpEvent, GameObject>(uiTarget);
            countDown = shotTimer;
        }
        else
        {
            countDown -= Time.deltaTime / shotTimer;
            shotRectTransform.sizeDelta = new Vector2((1 - (countDown / shotTimer)) * MAX_WIDTH, 0);
        }

    }
}
