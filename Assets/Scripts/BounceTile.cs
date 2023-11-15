using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BounceTile : MonoBehaviour
{
    
    public float maxAnimateHeight = 0.3f;
    public float animateStep = 0.1f;
    private Vector3 originalPosition;
    private bool animateState = false;
    private Rigidbody playerRb;
    private Animator playerAnimator;
    private NavMeshAgent navMesh;
    private GameObject player;
    

    private bool animateDir = true; // true = up, false = down;
    // Start is called before the first frame update
    void Start()
    {
        originalPosition = gameObject.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (animateState)
        {
            Vector3 currentPosition = transform.position;
            Vector3 newPos = originalPosition;
            if (animateDir && currentPosition.y < maxAnimateHeight)
            {
                newPos.y = Math.Clamp(gameObject.transform.position.y + animateStep, originalPosition.y, maxAnimateHeight);
                GetComponent<Rigidbody>().transform.position = newPos;
            }
            else if (!animateDir && currentPosition.y > originalPosition.y)
            {
                newPos.y = Math.Clamp(gameObject.transform.position.y - animateStep, originalPosition.y, maxAnimateHeight);
                GetComponent<Rigidbody>().transform.position = newPos;
            }
            else
            {
                if (!animateDir)
                {
                    if (navMesh != null)
                    {
                        navMesh.enabled = true;
                    }
                    player.transform.parent = null;
                    animateState = false; // Returned home, done animating
                }
                
                animateDir = !animateDir;
            }
        }
    }

    private void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("Player"))
        {
            player = c.gameObject;
            playerAnimator = player.GetComponent<Animator>();
            playerRb = player.GetComponent<Rigidbody>();
            player.transform.parent = gameObject.transform;
            animateState = true;
            navMesh = player.GetComponent<NavMeshAgent>();
            if (navMesh != null)
            {
                navMesh.enabled = false;
            }

        }
    }
    
    private void OnCollisionExit(Collision c)
    {
        navMesh = null;
    }
}
