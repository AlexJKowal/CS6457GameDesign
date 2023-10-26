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
    private bool coroutineStarted = false;

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

    void Update()
    {
        BallThrowing bt = ball.GetComponent<BallThrowing>();

        if (bt.targetSquareTag != null && homeSquare.CompareTag(bt.targetSquareTag) && !coroutineStarted)
        {
            float delay = Random.Range(0.1f, 0.5f);  // delay between 0.1 to 0.5 seconds
            Debug.Log("Starting Coroutine with delay: " + delay);
            StartCoroutine(SetDestinationAfterDelay(bt.targetLocation, delay));
            coroutineStarted = true;  
        }
        else if (bt.targetSquareTag == null || !homeSquare.CompareTag(bt.targetSquareTag))
        {
            agent.SetDestination(homeSquare.transform.position);
            coroutineStarted = false; 
        }
    }

    private IEnumerator SetDestinationAfterDelay(Vector3 targetLocation, float delay)
    {
        Debug.Log("Coroutine initiated, waiting for " + delay + " seconds");
        yield return new WaitForSeconds(delay);
        Debug.Log("Coroutine ended, setting destination");
        agent.SetDestination(targetLocation);
        coroutineStarted = false;  
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
