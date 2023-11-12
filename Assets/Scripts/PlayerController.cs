using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DefaultNamespace;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using static UnityEngine.Rendering.DebugUI;

public class PlayerController : MonoBehaviour
{
    public PlayerState playerState { get; set; } = PlayerState.Playing;
    
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
    private UnityAction<ShotType> shotTypeEventListener;
    private UnityAction<GameObject> shotTimeUpEventListener;
    private PlayerControls playerControls;
    private Animator anim;
    public bool isHoldingBall = false;
    private bool justPickedUp = false;
    private bool justReleased = false;
    private bool quickRelease = false;
    public float maxThrowForce = 20f;
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
        
        playerRb = GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        ResetStates();
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

    void FixedUpdate()
    {
        if (!ballServed)
        {
            // ball moves with player
            ballRb.isKinematic = true;
            ball.transform.position = transform.position + transform.forward;
        }

        switch (playerState)
        {
            case PlayerState.Playing:
                // ready to pick up the ball again
                HandleBall();
                HandleMovePlayer();
                HandleRotatePlayer();
                break;
            default:
                transform.position = homeSquare.transform.position;
                break;
        }
    }

    void HandleRotatePlayer()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPos = mainCamera.WorldToScreenPoint(transform.position);

        Vector3 aimDirection = mousePos - playerScreenPos;
        float angle = Mathf.Atan2(aimDirection.x, aimDirection.y) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, angle, 0);
    }

    void HandleMovePlayer()
    {
        Vector2 inputVec = playerControls.PlayerActions.Movement.ReadValue<Vector2>();
        Vector3 direction = new Vector3(inputVec.x, 0f, inputVec.y);

        if (direction.magnitude >= 0.2f)
        {
            // Convert the direction from local to world space based on camera orientation
            Vector3 moveDir = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0) * direction;
            moveDir *= moveSpeed * Time.fixedDeltaTime;
            moveDir.y = 0;

            transform.position += moveDir;
            anim.SetFloat("vely", moveDir.z * 5);
            anim.SetFloat("velx", moveDir.x * 5);
            playerRb.angularVelocity = Vector3.zero;
        }
        else
        {
            anim.SetFloat("vely", 0);
            anim.SetFloat("velx", 0);
        }
    }

    void HandleBall()
    {
        Vector3 ballPosition = ball.transform.position;
        Vector3 position = playerRb.ClosestPointOnBounds(ballPosition);
        
        // The distance to ball is now optimized to use a closest point from player collider boundary to the ball
        // So it can handle better if ball is hitting from player's head
        float distanceToBall = Vector3.Distance(position, ballPosition);
        
        // Pick up ball automatically when in range
        if (!isHoldingBall && distanceToBall <= 1.5f && !justReleased)
        {
          
            isHoldingBall = true;
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
            ball.transform.position = transform.position + transform.forward + new Vector3(0f, 1.5f, 0f);
 
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
                
                ResetBallHandling();
                EventManager.TriggerEvent<BallHitEvent, SquareLocation, ShotType>(SquareLocation.square_one, shotType);
                quickRelease = false;
            }

            // Reset the justPickedUp flag
            justPickedUp = false;
        }
    }

    private void ShootTheBall(float shootingForce)
    {
        GameObject estimatedTargetSquare = GameManager.getTargetSquareBasedOnPosition(reticleTransform.position);
        BallThrowing bt = ball.GetComponent<BallThrowing>();

        float flyingTime = Math.Max(2f - shootingForce/14f * 1.5f, 0.6f);
        
        bt.ShotTheBallToTargetSquare( homeSquare, estimatedTargetSquare, 0.5f, reticleTransform.position);
    }

    void PlayerLobShot()
    {
        float finalThrowForce = Mathf.Clamp(chargeAmount, 0, maxThrowForce);
        ShootTheBall(finalThrowForce);
    }
    
    void ShotTimeUpEventHandler(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            quickRelease = true;
        }
    }

    public void ResetStates()
    {
        // hold the ball
        ballRb.isKinematic = true;
        ballServed = false;
        isHoldingBall = true;
    }
    
    void ResetBallHandling()
    {
        isHoldingBall = false;
        chargeAmount = 0f;
        justPickedUp = false;
    }
    
    void ShotTypeEventHandler(ShotType shotTypeSetting)
    {
        shotType = shotTypeSetting;
    }
}
