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
            
            float distance = Vector3.Distance(ball.transform.position, bt.targetLocation);
            
            Vector3 extraPosition;
            if (distance > 9f)
            {
                // when ball is far away, AI player would walk through the radius of target location
                extraPosition = Quaternion.Euler(0, UnityEngine.Random.Range(-180.0f, 180.0f), 0)
                                * new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            }
            else
            {
                NormalDistribution nd = new NormalDistribution(3.5f, 1f);
                extraPosition = velocity * (float)nd.Sample(new System.Random());
            }
            
            agent.SetDestination(bt.targetLocation + extraPosition);    
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

            float flyingTime = GetFlyingTimeBasedOnGameLevel();
            bt.ShotTheBallToTargetSquare(homeSquare, targetSquare, flyingTime);
        }
    }

    private float GetFlyingTimeBasedOnGameLevel()
    {
        int level = GameManager.Instance.currentLevel;

        float flyingTime = Random.Range(1.2f, 2f);

        // Each level will reduce the flying time to its 80%
        return flyingTime * (float)Math.Pow(0.8f, level - 1);
    }
}
