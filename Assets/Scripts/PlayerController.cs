using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using static UnityEngine.Rendering.DebugUI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Projection _projection;
    public Transform reticleTransform;
    public GameObject homeSquare;
    
    public GameObject ballPrefab;
    
    public float moveSpeed = 10f;
    private Rigidbody playerRb;
    
    public GameObject ball;
    private Rigidbody ballRb;
    
    public Camera mainCamera;

    public bool ballServed;
    
    private UnityAction resetEventListener;
    public delegate void HoldingBallChanged(bool isHoldingBall);
    public static event HoldingBallChanged OnHoldingBallChanged;
    private UnityAction<ShotType> shotTypeEventListener;
    private UnityAction<GameObject> shotTimeUpEventListener;
    private PlayerControls playerControls;
    public bool isHoldingBall = false;
    private bool justPickedUp = false;
    private bool justReleased = false;
    private bool quickRelease = false;
    private bool smashInProgress = false;
    private Vector3 smashDir;
    private float smashCurrentPeriod = 0f;
    public float smashMultiplier = 1.2f;
    public float maxThrowForce = 20f;
    private float maxSmashForce = 0;
    public float initialSmashForce = 2f;
    public float smashTransitionPeriod = 0.25f;
    private float chargeAmount = 0f;
    public float chargeRate = 10f;
    public float lobTweak = 1f;
    private ShotType shotType = ShotType.lob_shot;

    void Awake()
    {
        resetEventListener = new UnityAction(ResetStates);
        shotTypeEventListener = new UnityAction<ShotType>(ShotTypeEventHandler);
        shotTimeUpEventListener = new UnityAction<GameObject>(ShotTimeUpEventHandler);
        playerControls = new PlayerControls();
        playerControls.PlayerActions.Movement.Enable();
    }

    void OnEnable()
    {
        EventManager.StartListening<ResetEvent>(resetEventListener);
        EventManager.StartListening<ShotTypeEvent, ShotType>(shotTypeEventListener);
        EventManager.StartListening<ShotTimeUpEvent, GameObject>(shotTimeUpEventListener);
        
    }

    void OnDisable()
    {
        EventManager.StopListening<ResetEvent>(resetEventListener);
        EventManager.StopListening<ShotTypeEvent, ShotType>(shotTypeEventListener);
        EventManager.StopListening<ShotTimeUpEvent, GameObject>(shotTimeUpEventListener);
    }
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();

        ResetStates();
    }

    private void Update()
    {
        if (!ballServed)
        {
            serviceTheBallIfHavent();
        }
        HandleBall();
    }

    void FixedUpdate()
    {
        Vector2 inputVec = playerControls.PlayerActions.Movement.ReadValue<Vector2>();
        Vector3 direction = new Vector3(inputVec.x, 0f, inputVec.y).normalized;

        if (direction.magnitude >= 0.2f)
        {
            // Convert the direction from local to world space based on camera orientation
            Vector3 moveDir = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0) * direction;
            moveDir *= moveSpeed * Time.fixedDeltaTime;
            moveDir.y = 0;
            playerRb.MovePosition(transform.position + moveDir);
        }
        RotatePlayer();
        if (smashInProgress)
        {
          //  SmashProgression();
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

    //public void MovePlayer(InputAction.CallbackContext value)
    //{
    //    float horizontal = Input.GetAxis("Horizontal");
    //    float vertical = Input.GetAxis("Vertical");
    //    Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

    //    if (direction.magnitude >= 0.2f)
    //    {
    //        // Convert the direction from local to world space based on camera orientation
    //        Vector3 moveDir = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0) * direction;
    //        moveDir *= moveSpeed * Time.fixedDeltaTime;
    //        moveDir.y = 0;
    //        playerRb.MovePosition(transform.position + moveDir);
    //    }
    //}

    void HandleBall()
    {
        float distanceToBall = Vector3.Distance(transform.position, ball.transform.position);
        
        // Pick up ball automatically when in range
        if (!isHoldingBall && distanceToBall <= 1.5f && !justReleased)
        {
          
            isHoldingBall = true;
            OnHoldingBallChanged?.Invoke(isHoldingBall);
            EventManager.TriggerEvent<BallCaughtEvent, GameObject, SquareLocation>(gameObject, SquareLocation.square_one);

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
            // Position the ball in front of the player
            ball.transform.position = transform.position + transform.forward;

            // Charge throw while holding the ball
            chargeAmount += Time.deltaTime * chargeRate;

            // Release and throw ball on mouse click, but not if it was just picked up
            if ((Input.GetButton("Fire1") || Input.GetAxis("JoyFire1") > 0.1f || quickRelease) && !justPickedUp)
            {
                justReleased = true;
                if (!ballServed)
                {
                    chargeAmount = maxThrowForce/2;
                    ballServed = true;
                }
                
                if (shotType == ShotType.lob_shot)
                {
                    PlayerLobShot();
                }
                else if (shotType == ShotType.smash_shot)
                {
                    //PlayerSmashShot();
                    PlayerLobShot();
                }
                
                ResetBallHandling();
                EventManager.TriggerEvent<BallHitEvent, SquareLocation, ShotType>(SquareLocation.square_one, shotType);
                quickRelease = false;
            }

            // Reset the justPickedUp flag
            justPickedUp = false;
        }
    }

    void serviceTheBallIfHavent()
    {
        if (ballServed)
        {
            return;
        }
        
        ball.transform.position = transform.position + transform.forward;
       // if (Input.GetButton("Fire1") || Input.GetAxis("JoyFire1") > 0.1f)
      //  {
            //TODO: hardcoded to the first square, but should be the current square
            // EventManager.TriggerEvent<BallHitEvent, SquareLocation, ShotType>(SquareLocation.square_one, ShotType.lob_shot);
            
            // shot the ball
       //     ShotTheBall();
            
          //  ballServed = true;
      //  }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
         //   ShotTheBall();
        }
    }

    private void ShootTheBall(float shootingForce)
    {
        GameObject estimatedTargetSquare = EstimateTarget();
        BallThrowing bt = ball.GetComponent<BallThrowing>();

        float flyingTime = Math.Max(2f - shootingForce/14f * 1.5f, 0.6f);
        
        // Debug.Log("shootingForce: " + shootingForce + "flyingTime:" + flyingTime);
        
        bt.ShootTheBallInDirection(0.5f, homeSquare, estimatedTargetSquare, reticleTransform.position);
    }

    private GameObject EstimateTarget()
    {
        Debug.Log("reticleTransform.position:" + reticleTransform.position.ToString());
        return GameManager.getTargetSquareBasedOnPosition(reticleTransform.position);
    }
    
    void PlayerLobShot()
    {
        float finalThrowForce = Mathf.Clamp(chargeAmount, 0, maxThrowForce);
        ShootTheBall(finalThrowForce);
    }

    void PlayerSmashShot()
    {
        smashInProgress = true;
        maxSmashForce = Mathf.Clamp(chargeAmount, 0, smashMultiplier * maxThrowForce);
        smashDir = (reticleTransform.position - ball.transform.position).normalized;
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
    
    void ShotTimeUpEventHandler(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            quickRelease = true;
        }
    }

    void ResetStates()
    {
        // hold the ball
        ballRb.isKinematic = true;
        ballServed = false;
    }
    
    void ResetBallHandling()
    {
        isHoldingBall = false;
        OnHoldingBallChanged?.Invoke(isHoldingBall);
        chargeAmount = 0f;
        justPickedUp = false;
    }
    
    void ShotTypeEventHandler(ShotType shotTypeSetting)
    {
        shotType = shotTypeSetting;
    }
}
