using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyControlScript : MonoBehaviour
{

    public GameObject player;
    public GameObject gameBall;
    public GameObject homeSquare;
    public Transform reticleTransform;
    public float animSpeed;
    public float linAccelMax;
    public float linVelMax;
    public float maxThrowForce = 20f;
    public float chargeRate = 2f;
    
    public delegate void HoldingBallChanged(bool isHoldingBall);
    public static event HoldingBallChanged OnHoldingBallChanged;
    
    private float chargeAmount = 0f;
    private bool justPickedUp = false;

    private float distanceToTarget;

    public enum AIState
    {
        GetBallState,
        GoToSmashLocationState,
        GoToLobLocationState,
        ThrowBallState,
        GoHomeState
    }

    public AIState aiState;

    private SquareLocation currentSquare = SquareLocation.square_two; // Temporary

    private Vector2 trackingSpeeds;

    private Animator anim;
    private Rigidbody rbody;
    private Rigidbody ballRbody;
    private Vector3 targetLocation;
    private Quaternion targetRotation;
    private float minimumX;
    private float maximumX;
    private float minimumZ;
    private float maximumZ;
    private float centerX;
    private float centerZ;
    private Collider homeSquareCollider;
    private float linVelCurrent;

    private enum StateEnum
    {
        incomingLob,
        incomingSmash,
        gotBall,
        safelyOutOfRange,
        outgoingHit
    }

    private StateEnum stateEnum = StateEnum.safelyOutOfRange;

    private UnityAction<SquareLocation, ShotType> ballHitEventListener;
    private UnityAction<Vector3, SquareLocation> ballBounceEventListener;

    // Start is called before the first frame update

    void Start()
    {
        anim = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody>();
        ballRbody = gameBall.GetComponent<Rigidbody>();
        homeSquareCollider = homeSquare.GetComponent<Collider>();
        linVelCurrent = 0;
        aiState = AIState.GoHomeState;
        Vector3 homeLocation = new Vector3(centerX, 0f, centerZ);
        DeterminePlayerBounds();
        CalculateTarget(homeLocation);
    }

    void Awake()
    {
        ballHitEventListener = new UnityAction<SquareLocation, ShotType>(BallHitEventHandler);
        ballBounceEventListener = new UnityAction<Vector3, SquareLocation>(BallBounceEventHandler);
    }

    void OnEnable()
    {
        EventManager.StartListening<BallHitEvent, SquareLocation, ShotType>(ballHitEventListener);
        EventManager.StartListening<BallBounceEvent, Vector3, SquareLocation>(ballBounceEventListener);
    }

    void OnDisable()
    {
        EventManager.StopListening<BallHitEvent, SquareLocation, ShotType>(ballHitEventListener);
        EventManager.StopListening<BallBounceEvent, Vector3, SquareLocation>(ballBounceEventListener);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (stateEnum == StateEnum.outgoingHit || stateEnum == StateEnum.safelyOutOfRange)
        {
            aiState = AIState.GoHomeState;
        }
        else if (stateEnum == StateEnum.incomingSmash)
        {
            aiState = AIState.GoToSmashLocationState;
        }
        else if (stateEnum == StateEnum.incomingLob)
        {
            aiState = AIState.GoToLobLocationState;
        }
        else if (stateEnum == StateEnum.gotBall)
        {
            //stateEnum = StateEnum.outgoingHit;

            aiState = AIState.ThrowBallState;
        }

        switch (aiState)
        {
            case AIState.GoToLobLocationState:
                if (!anim.GetBool("IsJumping") && CheckShouldJump())
                {
                    linVelCurrent = 0;
                    anim.SetBool("IsJumping", true);
                    anim.SetBool("IsTraversing", false);
                }
                else
                {
                    CalculateTarget(PredictBall());
                    CheckIfCaught();
                }
                break;

            case AIState.GoToSmashLocationState:
                CalculateTarget(PredictBall());
                CheckIfCaught();
                break;

            case AIState.GoHomeState:
                Vector3 homeLocation = new Vector3(centerX, 0f, centerZ);
                CalculateTarget(homeLocation);
                break;

            case AIState.ThrowBallState:
                CalculateTarget(reticleTransform.position);
                HandleBall();
                break;

        }
    }

    void Landed()
    {
        anim.SetBool("IsJumping", false);
    }

    void FixedUpdate()
    {
        double linearTolerance = 1;
        double angleTolerance = 3.0;
        Quaternion newRotation;
        float newAcceleration = linAccelMax;
        float velx = 0f;
        float vely = 0f;

        if (!justPickedUp && (Mathf.Abs(this.transform.position.x - targetLocation.x) > linearTolerance ||
            Mathf.Abs(this.transform.position.z - targetLocation.z) > linearTolerance))
        {
            anim.SetBool("IsTraversing", true);

            float differenceInAngles = Vector3.Angle(transform.forward, targetLocation - transform.position);
            if (differenceInAngles > linearTolerance)
            {
                newRotation = Quaternion.LerpUnclamped(transform.rotation, targetRotation, 1.0f);
                rbody.MoveRotation(newRotation);
            }

            if (differenceInAngles > 90)
            {
                newAcceleration *= 0.25f;
            }
            else if (differenceInAngles > 45)
            {
                newAcceleration *= 0.5f;
            }
            else if (differenceInAngles > angleTolerance)
            {
                newAcceleration *= 0.75f;
            }

            float newVelocity = Mathf.Clamp(
                linVelCurrent + (newAcceleration * Time.deltaTime),
                -1 * linVelMax,
                linVelMax
                );

            linVelCurrent = newVelocity;
            velx = linVelCurrent / linVelMax * Mathf.Sin(differenceInAngles * Mathf.Deg2Rad);
            vely = linVelCurrent / linVelMax * Mathf.Cos(differenceInAngles * Mathf.Deg2Rad);

        }
        else
        {
            anim.SetBool("IsTraversing", false);
            newRotation = Quaternion.LerpUnclamped(transform.rotation, targetRotation, 1.0f);
            rbody.MoveRotation(newRotation);
            
        }

        anim.SetFloat("velx", velx);
        anim.SetFloat("vely", vely);

    }

    void CheckIfCaught()
    {
        // Pick up ball automatically when in range
        if (distanceToTarget < 1.5f)
        {
            OnHoldingBallChanged?.Invoke(true);
            EventManager.TriggerEvent<BallCaughtEvent, GameObject>(gameObject);

            justPickedUp = true;
            stateEnum = StateEnum.gotBall;
        }
    }

    void OnAnimatorMove()
    {
        Vector3 newRootPosition;
        Quaternion newRootRotation;

        newRootPosition = anim.rootPosition;
        newRootPosition = Vector3.LerpUnclamped(this.transform.position, newRootPosition, animSpeed);
        newRootRotation = anim.rootRotation;


        rbody.MovePosition(newRootPosition);
        rbody.MoveRotation(newRootRotation);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            anim.SetBool("TargetPlayer", false);
            anim.SetBool("TargetBall", true);
        }
        else if (other.CompareTag("Player") && !anim.GetBool("TargetBall"))
        {
            anim.SetBool("TargetPlayer", true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball") && BallNotInQuadrant())
        {
            anim.SetBool("TargetPlayer", true);
            anim.SetBool("TargetBall", false);
        }
        else if (other.CompareTag("Player"))
        {
            anim.SetBool("TargetPlayer", false);
        }
    }

    void BallHitEventHandler(SquareLocation squareNum, ShotType shot)
    {
        if (squareNum != currentSquare)
        {
            if (shot == ShotType.lob_shot)
            {
                stateEnum = StateEnum.incomingLob;
            }
            else if (shot == ShotType.smash_shot)
            {
                stateEnum = StateEnum.incomingSmash;
            }
        }
    }

    void BallBounceEventHandler(Vector3 location, SquareLocation squareNum)
    {
        if (squareNum != currentSquare)
        {
            stateEnum = StateEnum.safelyOutOfRange;
        }
        else
        {
            stateEnum = StateEnum.incomingLob;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            EventManager.TriggerEvent<BallHitEvent, SquareLocation, ShotType>(currentSquare, ShotType.lob_shot);
        }
    }

    private bool BallNotInQuadrant()
    {
        return true; //placeholder
    }

    private Vector3 PredictBall()
    {
        Vector3 distanceFromTarget = gameBall.transform.position - rbody.transform.position;
        float magFromTarget = Mathf.Sqrt(Mathf.Pow(distanceFromTarget.z, 2.0f) + Mathf.Pow(distanceFromTarget.x, 2.0f));
        float lookAheadTime = Mathf.Clamp(magFromTarget / linVelMax, 0f, 0.1f);
        Vector3 ballPos = gameBall.transform.position;
        Vector3 ballVel = ballRbody.velocity;
        float futureX = ballPos.x + ballVel.x * lookAheadTime;
        float futureY = ballPos.y + ballVel.y * lookAheadTime + 0.5f * Physics.gravity.y * Mathf.Pow(lookAheadTime, 2.0f);
        float futureZ = ballPos.z + ballVel.z * lookAheadTime;

        return new Vector3(futureX, futureY, futureZ);
    }

    private void CalculateTarget(Vector3 target)
    {
        Vector3.Distance(transform.position, targetLocation);
        targetLocation.x = Mathf.Clamp(target.x, minimumX, maximumX);
        targetLocation.z = Mathf.Clamp(target.z, minimumZ, maximumZ);
        targetLocation.y = transform.position.y;
        targetRotation = Quaternion.LookRotation(targetLocation - transform.position);
        distanceToTarget = Vector3.Distance(transform.position, targetLocation);
    }

    private void DeterminePlayerBounds()
    {
        centerX = homeSquareCollider.bounds.center.x;
        centerZ = homeSquareCollider.bounds.center.z;

        if (homeSquare.CompareTag("Square1"))
        {
            minimumX = (-1) * Mathf.Infinity;
            minimumZ = (-1) * Mathf.Infinity;
            maximumX = homeSquareCollider.bounds.max.x;
            maximumZ = homeSquareCollider.bounds.max.z;

        }
        else if (homeSquare.CompareTag("Square2"))
        {
            minimumX = (-1) * Mathf.Infinity;
            minimumZ = homeSquareCollider.bounds.min.z;
            maximumX = homeSquareCollider.bounds.max.x;
            maximumZ = Mathf.Infinity;

        }
        else if (homeSquare.CompareTag("Square3"))
        {
            minimumX = homeSquareCollider.bounds.min.x;
            minimumZ = homeSquareCollider.bounds.min.z;
            maximumX = Mathf.Infinity;
            maximumZ = Mathf.Infinity;
        }
        else if (homeSquare.CompareTag("Square4"))
        {
            minimumX = homeSquareCollider.bounds.min.x;
            minimumZ = (-1) * Mathf.Infinity;
            maximumX = Mathf.Infinity;
            maximumZ = homeSquareCollider.bounds.max.z;
        }
    }

    private bool CheckShouldJump()
    {
        bool shouldJump = false;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out hit) && hit.transform.CompareTag("Ball"))
        {
            shouldJump = true;
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.up) * hit.distance, Color.red);
        }

        return shouldJump;
    }
    
     void HandleBall()
    {
        
        // Reset flag so that player can register a pick up again
      
        
        gameBall.transform.position = transform.position + transform.forward;

        // Charge throw while holding the ball
        chargeAmount += Time.deltaTime * chargeRate;

        bool npcFireDecision = (chargeAmount / maxThrowForce > 0.5);
        

        // Release and throw ball on mouse click, but not if it was just picked up
        if (npcFireDecision)
        {
                float finalThrowForce = Mathf.Clamp(chargeAmount, 0, maxThrowForce);
                Vector3 throwDir = (reticleTransform.position - gameBall.transform.position).normalized;
                ballRbody.velocity = throwDir * finalThrowForce;
                ResetBallHandling();
                EventManager.TriggerEvent<BallHitEvent, SquareLocation, ShotType>(currentSquare, ShotType.lob_shot);
                stateEnum = StateEnum.outgoingHit;
                
                justPickedUp = false;
       
        }
        
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        float lineLength = 2.0f;
        Vector3 forwardPosition = transform.position + transform.forward * lineLength;

        Gizmos.DrawLine(transform.position, forwardPosition);

        float arrowHeadAngle = 25.0f;
        float arrowHeadLength = 0.5f;

        Vector3 rightArrow = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 leftArrow = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);

        Gizmos.DrawRay(forwardPosition, rightArrow * arrowHeadLength);
        Gizmos.DrawRay(forwardPosition, leftArrow * arrowHeadLength);
        if (GetComponent<CapsuleCollider>() != null)
        {
            Vector3 size = new Vector3(1f, 1f, 1f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(targetLocation + GetComponent<CapsuleCollider>().center, size);
        }

    }
    
    void ResetBallHandling()
    {
        OnHoldingBallChanged?.Invoke(false);
        chargeAmount = 0f;
        justPickedUp = false;
    }


}