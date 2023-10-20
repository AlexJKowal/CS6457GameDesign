using System.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Projection _projection;
    public GameObject ballPrefab;
    
    public float moveSpeed = 10f;
    public Transform ballTransform;
    public Transform reticleTransform;
    public Camera mainCamera;
    public float maxThrowForce = 20f;
    public float chargeRate = 10f;

    public float smashMultiplier = 1.2f;

    public float lobTweak = 1f;

    public delegate void HoldingBallChanged(bool isHoldingBall);
    public static event HoldingBallChanged OnHoldingBallChanged;

    private Rigidbody playerRb;
    private Rigidbody ballRb;
    private bool isHoldingBall = false;
    private float chargeAmount = 0f;
    private bool justPickedUp = false;
    private bool justReleased = false;
    private bool quickRelease = false;
    
    private bool smashInProgress = false;
    private float maxSmashForce = 0;

    public float initialSmashForce = 2f;
    private Vector3 smashDir;
    public float smashTransitionPeriod = 0.25f;

    private float smashCurrentPeriod = 0f;
    private UnityAction<GameObject> shotTimeUpEventListener;
    private UnityAction<ShotType> shotTypeEventListener;
    private UnityAction<Vector3, SquareLocation> ballBounceEventListener;
    private UnityAction<GameObject, BoostType, float, float> statBoostEventListener;

    private SquareLocation currentSquare = SquareLocation.square_one; // Temporary

    private ShotType shotType = ShotType.lob_shot;
    
    private Stats playerStats;


    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        ballRb = ballTransform.GetComponent<Rigidbody>();
        playerStats = new Stats { speed = 1, power = 1 };
    }

    void Awake()
    {
        ballBounceEventListener = new UnityAction<Vector3, SquareLocation>(BallBounceEventHandler);
        shotTimeUpEventListener = new UnityAction<GameObject>(ShotTimeUpEventHandler);
        shotTypeEventListener = new UnityAction<ShotType>(ShotTypeEventHandler);
        statBoostEventListener = new UnityAction<GameObject, BoostType, float, float>(StatBoostEventHandler);
    }

    void OnEnable()
    {
        EventManager.StartListening<BallBounceEvent, Vector3, SquareLocation>(ballBounceEventListener);
        EventManager.StartListening<ShotTimeUpEvent, GameObject>(shotTimeUpEventListener);
        EventManager.StartListening<ShotTypeEvent, ShotType>(shotTypeEventListener);
        EventManager.StartListening<StatBoostEvent, GameObject, BoostType, float, float>(statBoostEventListener);
    }

    void OnDisable()
    {
        EventManager.StopListening<BallBounceEvent, Vector3, SquareLocation>(ballBounceEventListener);
        EventManager.StopListening<ShotTimeUpEvent, GameObject>(shotTimeUpEventListener);
        EventManager.StopListening<ShotTypeEvent, ShotType>(shotTypeEventListener);
        EventManager.StopListening<StatBoostEvent, GameObject, BoostType, float, float>(statBoostEventListener);
    }

    void FixedUpdate()
    {
        MovePlayer();
        if (!isHoldingBall)
        {
            _projection.SimulateTrajectory(ballPrefab, ballTransform.position, ballRb.velocity);    
        }

        if (smashInProgress)
        {
            SmashProgression();
        }
    }

    void Update()
    {
        UpdateStats();
        RotatePlayer();
        HandleBall();
    }

    void UpdateStats()
    {
        if (playerStats.boostTime <= 0)
        {
            playerStats.power = 1;
            playerStats.speed = 1;
            playerStats.boostTime = 0;
        }
    }

    void SmashProgression()
    {
        
        if (smashCurrentPeriod == 0)
        {
            ballRb.useGravity = false;
            ballRb.velocity = smashDir * initialSmashForce;
        }
        else if (smashCurrentPeriod >= smashTransitionPeriod)
        {
            ballRb.velocity = smashDir * maxSmashForce * smashMultiplier;
        }
        
        smashCurrentPeriod += Time.fixedDeltaTime;
    }
    
    void BallBounceEventHandler(Vector3 location, SquareLocation square)
    {
        if (smashInProgress) // A smash ends when it hits any surface and bounces
        {
            smashCurrentPeriod = 0f;
            ballRb.useGravity = true;
            maxSmashForce = 0;
            smashInProgress = false;

        }
    }

    void StatBoostEventHandler(GameObject target, BoostType type, float amount, float duration)
    {
        if (type == BoostType.resetAll)
        {
            playerStats.power = 1;
            playerStats.speed = 1;
            playerStats.boostTime = 0;
        }
        else if (target.CompareTag("Player"))
        {
            if (type == BoostType.power)
            {
                playerStats.power *= amount;
                playerStats.boostTime = duration;
            }
            else if (type == BoostType.speed)
            {
                playerStats.speed *= amount;
                playerStats.boostTime = duration;
            }
        }
        
    }
    

    void RotatePlayer()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPos = mainCamera.WorldToScreenPoint(transform.position);

        Vector3 aimDirection = mousePos - playerScreenPos;
        float angle = Mathf.Atan2(aimDirection.x, aimDirection.y) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, angle, 0);
    }

    void MovePlayer()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Convert the direction from local to world space based on camera orientation
            Vector3 moveDir = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0) * direction;
            moveDir *= moveSpeed * playerStats.speed * Time.fixedDeltaTime;
            playerRb.MovePosition(transform.position + moveDir);
        }
    }


    void HandleBall()
    {
        float distanceToBall = Vector3.Distance(transform.position, ballTransform.position);

        // Debug message for proximity to ball
        // Debug.Log("Distance to Ball: " + distanceToBall);

        // Pick up ball automatically when in range
        if (!isHoldingBall && distanceToBall <= 1.5f && !justReleased)
        {
          
            isHoldingBall = true;
            OnHoldingBallChanged?.Invoke(isHoldingBall);
            EventManager.TriggerEvent<BallCaughtEvent, GameObject, SquareLocation>(gameObject, currentSquare);

            justPickedUp = true;
        }

        // Reset flag so that player can register a pick up again
        if (justReleased && distanceToBall > 1.5f)
        {
            justReleased = false;
        }

        // If holding the ball
        if (isHoldingBall)
        {
            //   Debug.Log("Holding ball");  // Debug

            // Position the ball in front of the player
            ballTransform.position = transform.position + transform.forward;

            // Charge throw while holding the ball
            chargeAmount += Time.deltaTime * chargeRate * playerStats.power;

            // Debug message for charge amount
            // Debug.Log("Charge Amount: " + chargeAmount);

            // Release and throw ball on mouse click, but not if it was just picked up
            if ((Input.GetButton("Fire1") || Input.GetAxis("JoyFire1") > 0.1f || quickRelease) && !justPickedUp)
            {
                justReleased = true;

                if (shotType == ShotType.lob_shot)
                {
                    PlayerLobShot();
                }
                else if (shotType == ShotType.smash_shot)
                {
                    PlayerSmashShot();
                }
                
                ResetBallHandling();
                EventManager.TriggerEvent<BallHitEvent, SquareLocation, ShotType>(currentSquare, shotType);
                quickRelease = false;
            }

            // Reset the justPickedUp flag
            justPickedUp = false;
        }
    }

    void PlayerLobShot()
    {
        float finalThrowForce = Mathf.Clamp(chargeAmount, 0, maxThrowForce * playerStats.power);
        Vector3 throwDir = (reticleTransform.position - ballTransform.position).normalized;
        throwDir.y = finalThrowForce * lobTweak;
        ballRb.velocity = throwDir * finalThrowForce;
    }

    void PlayerSmashShot()
    {
        smashInProgress = true;
        maxSmashForce = Mathf.Clamp(chargeAmount, 0, smashMultiplier * maxThrowForce * playerStats.power);
        smashDir = (reticleTransform.position - ballTransform.position).normalized;
    }

    void ShotTimeUpEventHandler(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            quickRelease = true;
            playerStats.boostTime -= 1;
        }
    }

    void ShotTypeEventHandler(ShotType shotTypeSetting)
    {
        shotType = shotTypeSetting;
    }


    void ResetBallHandling()
    {
        isHoldingBall = false;
        OnHoldingBallChanged?.Invoke(isHoldingBall);
        chargeAmount = 0f;
        justPickedUp = false;
    }

}
