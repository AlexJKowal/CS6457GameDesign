using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Projection _projection;

    public GameObject homeSquare;
    
    public GameObject ballPrefab;
    
    public float moveSpeed = 10f;
    private Rigidbody playerRb;
    
    public GameObject ball;
    private Rigidbody ballRb;
    
    public Camera mainCamera;

    private bool ballServed;
    
    private UnityAction resetEventListener;

    void Awake()
    {
        resetEventListener = new UnityAction(ResetStates);
    }

    void OnEnable()
    {
        EventManager.StartListening<ResetEvent>(resetEventListener);
    }

    void OnDisable()
    {
        EventManager.StopListening<ResetEvent>(resetEventListener);
    }
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();

        ResetStates();
    }

    private void Update()
    {
        serviceTheBallIfHavent();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
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

        if (direction.magnitude >= 0.2f)
        {
            // Convert the direction from local to world space based on camera orientation
            Vector3 moveDir = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0) * direction;
            moveDir *= moveSpeed * Time.fixedDeltaTime;
            moveDir.y = 0;
            playerRb.MovePosition(transform.position + moveDir);
        }
    }

    void serviceTheBallIfHavent()
    {
        if (ballServed)
        {
            return;
        }
        
        ball.transform.position = transform.position + transform.forward;
        if (Input.GetButton("Fire1") || Input.GetAxis("JoyFire1") > 0.1f)
        {
            //TODO: hardcoded to the first square, but should be the current square
            // EventManager.TriggerEvent<BallHitEvent, SquareLocation, ShotType>(SquareLocation.square_one, ShotType.lob_shot);
            
            // shot the ball
            ShotTheBall();
            
            ballServed = true;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            ShotTheBall();
        }
    }

    private void ShotTheBall()
    {
        BallThrowing bt = ball.GetComponent<BallThrowing>();
        GameObject targetSquare = bt.GetRandomTargetSquare(homeSquare.tag);
        bt.ShotTheBallToTargetSquare(homeSquare, targetSquare);
    }

    void ResetStates()
    {
        // hold the ball
        ballRb.isKinematic = true;
        ballServed = false;
    }
}
