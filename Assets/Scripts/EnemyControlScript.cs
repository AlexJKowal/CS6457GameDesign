using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControlScript : MonoBehaviour
{

    public GameObject player;
    public GameObject gameBall;
    public GameObject homeSquare;
    public float animSpeed;
    public float linAccelMax;
    public float linVelMax;

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

    // Start is called before the first frame update

    void Start()
    {
        anim = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody>();
        ballRbody = gameBall.GetComponent<Rigidbody>();
        homeSquareCollider = homeSquare.GetComponent<Collider>();
        linVelCurrent = 0;
        CalculateTarget();
        DeterminePlayerBounds();
    }

    // Update is called once per frame
    void Update()
    {
        CalculateTarget();
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

        if (!anim.GetBool("IsJumping") && CheckShouldJump())
        {
            linVelCurrent = 0;
            anim.SetBool("IsJumping", true);
            anim.SetBool("IsTraversing", false);
        }
        else if (Mathf.Abs(this.transform.position.x - targetLocation.x) > linearTolerance ||
            Mathf.Abs(this.transform.position.z - targetLocation.z) > linearTolerance)
        {
            anim.SetBool("IsTraversing", true);

            float differenceInAngles = Vector3.Angle(this.transform.forward, targetLocation - this.transform.position);
            if (differenceInAngles > linearTolerance || 1 == 1)
            {
                newRotation = Quaternion.LerpUnclamped(this.transform.rotation, targetRotation, 1.0f);
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

            Vector3 toTarget = (targetLocation - this.transform.position).normalized;
            float turnAngle = Vector3.SignedAngle(this.transform.forward, toTarget, Vector3.up);
            if (Mathf.Abs(turnAngle) > angleTolerance)
            {
                velx = Mathf.Sign(turnAngle) * 1.0f;
            }
            else
            {
                velx = 0;
            }
        }

        anim.SetFloat("velx", velx);
        anim.SetFloat("vely", vely);

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

    private bool BallNotInQuadrant()
    {
        return true; //placeholder
    }

    private Vector3 PredictBall()
    {
        Vector3 distanceFromTarget = gameBall.transform.position - rbody.transform.position;
        float magFromTarget = Mathf.Sqrt(Mathf.Pow(distanceFromTarget.z, 2.0f) + Mathf.Pow(distanceFromTarget.x, 2.0f));
        float lookAheadTime = magFromTarget / linVelMax;
        Vector3 ballPos = gameBall.transform.position;
        Vector3 ballVel = ballRbody.velocity;
        float futureX = ballPos.x + ballVel.x * lookAheadTime;
        float futureY = ballPos.y + ballVel.y * lookAheadTime + 0.5f * Physics.gravity.y * Mathf.Pow(lookAheadTime, 2.0f);
        float futureZ = ballPos.z + ballVel.z * lookAheadTime;

        return new Vector3(futureX, futureY, futureZ);
    }

    private void CalculateTarget()
    {

        if (anim.GetBool("TargetBall"))
        {
            targetLocation = PredictBall();
            targetLocation.x = Mathf.Clamp(targetLocation.x, minimumX, maximumX);
            targetLocation.z = Mathf.Clamp(targetLocation.z, minimumZ, maximumZ);
        }
        else if (anim.GetBool("TargetPlayer"))
        {
            targetLocation.x = Mathf.Clamp(player.transform.position.x, minimumX,
                maximumX);
            targetLocation.z = Mathf.Clamp(player.transform.position.z, minimumZ,
                maximumZ);
        }
        else
        {
            targetLocation.x = centerX;
            targetLocation.z = centerZ;
        }
        targetLocation.y = this.transform.position.y;
        targetRotation = Quaternion.LookRotation(targetLocation - this.transform.position);

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


}