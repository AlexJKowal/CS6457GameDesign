using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
public class AIPlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    
    public AIPlayerState aiState = AIPlayerState.CatchBall;
    public GameObject homeSquare;
    public GameObject ball;
    
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
    private SquareLocation homeSquareEnum;
    private float linVelCurrent;
    
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.Log("NavMeshAgent could not be found");
        DeterminePlayerBounds();
    }

    // Update is called once per frame
    void Update()
    {
        BallThrowing bt = ball.GetComponent<BallThrowing>();
        Vector3 targetLocation;

        // If ball is moving with player-applied force -- this will 
        // be pertinent when we add ball prediction back, for now behaviour is identical in each
        // case.
        if (bt._freeTargeting)
        {
            targetLocation = CalculateTargetInBounds(bt.targetLocation);
        }
        else
        {
            targetLocation = bt.targetLocation;
        }
        
        // ball is hitting to our location
        if (bt._targetSquare != null && homeSquare.CompareTag(bt._targetSquare.tag))
        {
            Vector3 velocity = ball.GetComponent<Rigidbody>().velocity.normalized;
            velocity.y = 0;
            
            
            agent.SetDestination(targetLocation + velocity * Random.Range(1.5f, 2.5f));    
        }
        else
        {
            agent.SetDestination(homeSquare.transform.position);  
        }
    }
    
    private Vector3 CalculateTargetInBounds(Vector3 target)
    {
        Vector3 clampedTarget;
        clampedTarget.x = Mathf.Clamp(target.x, minimumX, maximumX);
        clampedTarget.z = Mathf.Clamp(target.z, minimumZ, maximumZ);
        clampedTarget.y = target.y;

        return clampedTarget;
    }
    
    private void DeterminePlayerBounds()
    {
        
        Collider homeSquareCollider = homeSquare.GetComponent<Collider>();
        centerX = homeSquareCollider.bounds.center.x;
        centerZ = homeSquareCollider.bounds.center.z;

        if (homeSquare.CompareTag("square_one"))
        {
            minimumX = (-1) * Mathf.Infinity;
            minimumZ = (-1) * Mathf.Infinity;
            maximumX = homeSquareCollider.bounds.max.x;
            maximumZ = homeSquareCollider.bounds.max.z;

        }
        else if (homeSquare.CompareTag("square_two"))
        {
            minimumX = (-1) * Mathf.Infinity;
            minimumZ = homeSquareCollider.bounds.min.z;
            maximumX = homeSquareCollider.bounds.max.x;
            maximumZ = Mathf.Infinity;

        }
        else if (homeSquare.CompareTag("square_three"))
        {
            minimumX = homeSquareCollider.bounds.min.x;
            minimumZ = homeSquareCollider.bounds.min.z;
            maximumX = Mathf.Infinity;
            maximumZ = Mathf.Infinity;
        }
        else if (homeSquare.CompareTag("square_four"))
        {
            minimumX = homeSquareCollider.bounds.min.x;
            minimumZ = (-1) * Mathf.Infinity;
            maximumX = Mathf.Infinity;
            maximumZ = homeSquareCollider.bounds.max.z;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            BallThrowing bt = ball.GetComponent<BallThrowing>();
            GameObject targetSquare = bt.GetRandomTargetSquare(homeSquare.tag);
            bt.ShotTheBallToTargetSquare(homeSquare, targetSquare);
        }
    }
}
