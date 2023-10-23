using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIPlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    
    public AIPlayerState aiState = AIPlayerState.CatchBall;
    public GameObject homeSquare;
    public GameObject ball;
    
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.Log("NavMeshAgent could not be found");
    }

    // Update is called once per frame
    void Update()
    {
        BallThrowing bt = ball.GetComponent<BallThrowing>();
        
        // ball is hitting to our location
        if (bt.targetSquareTag!= null && homeSquare.CompareTag(bt.targetSquareTag))
        {
            agent.SetDestination(bt.targetLocation);    
        }
        else
        {
            agent.SetDestination(homeSquare.transform.position);  
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            BallThrowing bt = ball.GetComponent<BallThrowing>();
            GameObject targetSquare = bt.GetRandomTargetSquare(homeSquare.tag);
            bt.ShotTheBallToTargetSquare(targetSquare);
        }
    }
}
