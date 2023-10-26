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
        if (bt._targetSquare != null && homeSquare.CompareTag(bt._targetSquare.tag))
        {
            Vector3 velocity = ball.GetComponent<Rigidbody>().velocity.normalized;
            velocity.y = 0;
            
            
            agent.SetDestination(bt.targetLocation + velocity * Random.Range(1.5f, 2.5f));    
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
            bt.ShotTheBallToTargetSquare(homeSquare, targetSquare);
        }
    }
}
