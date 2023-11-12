using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

// check below youtube videos
// 1) How to calculate the time for a free fall with initial velocity
// https://www.youtube.com/watch?v=Wd36d2C-iaA
// 2) How to solve a quadratic formula
// https://www.khanacademy.org/math/algebra/x2f8bb11595b61c86:quadratic-functions-equations/x2f8bb11595b61c86:quadratic-formula-a1/v/using-the-quadratic-formula#:~:text=The%20quadratic%20formula%20helps%20us,))%2F(2a)%20.
public class BallThrowing : MonoBehaviour
{
    private float a = 9.81f / 2;
    
    public GameObject ballPrefab;
    
    private Rigidbody ballRb;
    private Transform ballTransform;
    private SphereCollider _collider;

    public List<GameObject> squares;

    // this is the target indicator where the ball will hit the ground
    public GameObject target;

    public GameObject _fromSquare;
    public GameObject _targetSquare;

    public Vector3 targetLocation;
    
    private int _bounced;
    public bool shouldCheckWinOrLose = false;
    
    // Ignore duplicated collisions
    private long _lastCollisionTime;
    private GameObject _lastCollisionObject;
    
    // Based on formula delta_Y = Vi * t + 1/2 * g * t^2
    private float GetFreeFallTime(float initialVelocity, float height)
    {
        float b = -initialVelocity;
        float c = -height;

        return (float)(-b + Math.Sqrt(b*b - 4 * a * c)) / (2 * a);
    }
    
    private void UpdateHittingGround()
    {
        Vector3 position = ballTransform.position;
        Vector3 velocity= ballRb.velocity;

        float t = GetFreeFallTime(velocity.y, position.y - _collider.radius);
        
        if (t > 0.01f)
        {
            double x = position.x + t * velocity.x;
            double z = position.z + t * velocity.z;

            target.transform.position = new Vector3((float)x, 0.1f, (float)z);
        }
    }

    // Pure function to calculate the velocity required to let the ball hit the target ground with given vertical velocity
    public Vector3 GetVelocityToHitTargetGroundBasedOnInitialVerticalVelocity(float initialVerticalVelocity, Vector3 initialPosition, Vector3 targetPosition)
    {
        float height = initialPosition.y - _collider.radius;
        float t = GetFreeFallTime(initialVerticalVelocity, height);

        float x = (targetPosition.x - initialPosition.x) / t;
        float z = (targetPosition.z - initialPosition.z) / t;

        return new Vector3(x, initialVerticalVelocity, z);
    }
    
    // Pure function to calculate the velocity required with expected time given
    public Vector3 GetVelocityToHitTargetGroundBasedOnExpectedTime(Vector3 initialPosition, Vector3 targetPosition, float expectedTime)
    {
        // delta_Y = Vi * t + 1/2 * g * t^2
        float height = initialPosition.y - _collider.radius;
        float y = -(height - a * expectedTime * expectedTime) / expectedTime;
        float x = (targetPosition.x - initialPosition.x) / expectedTime;
        float z = (targetPosition.z - initialPosition.z) / expectedTime;
        
        return new Vector3(x, y, z);
    }

    public GameObject GetRandomTargetSquare(String excludedSquare)
    {
        // form a new list
        List<GameObject> targetSquares = squares.Where(s => !s.CompareTag(excludedSquare)).ToList(); 
        
        // randomly pick one of three squares
        return targetSquares[UnityEngine.Random.Range(0, targetSquares.Count)];
    }
    
    public Vector3 GetRandomTargetPosition(GameObject square)
    {
        // randomly pick the location of position within chosen square
        Vector3 size = square.GetComponent<MeshRenderer>().bounds.size;

        float width = size.x;
        float length = size.z;

        NormalDistribution nd = new NormalDistribution(width/2, width / 4);
        float widthDelta = Math.Max(Math.Min((float)nd.Sample(new System.Random()), width), 0.1f) - width/2;
        float lengthDelta =Math.Max(Math.Min((float)nd.Sample(new System.Random()), width), 0.1f) - width/2;

        Vector3 position = square.transform.position;
        
        return new Vector3(position.x + widthDelta, 0.01f, position.z + lengthDelta);
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        if (squares.Count == 0)
        {
            throw new ArgumentException("Need to set Squares");
        }

        ballTransform = this.gameObject.transform;
        _collider = GetComponent<SphereCollider>();
        ballRb = GetComponent<Rigidbody>();
        target.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHittingGround();
    }

    private void OnCollisionExit(Collision other)
    {
        // when player holds the ball, it triggers false collision events
        if (!shouldCheckWinOrLose)
        {
            return;
        }

        if (ballRb.isKinematic)
        {
            return;
        }
        
        // For players, the hit sound is controlled with shooting logic itself
        if(!other.gameObject.CompareTag("Player"))
        {
            EventManager.TriggerEvent<BallBounceEvent, Vector3, SquareLocation>(Vector3.zero,
                SquareLocation.square_one);
        }

        // Ignore false collision signals if they are too closed
        if (IsDuplicatedCollision(other))
        {
            return;
        }
        
        CheckIfGameIsFinished(other);
        
        // Call previous win lose logic
        // PreviousWinLoseLogic(other);
    }

    private bool IsDuplicatedCollision(Collision other)
    {
        long collisionTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        // ignore if collision time is less than 0.3 seconds
        if (collisionTime - _lastCollisionTime < 300 && GameObject.ReferenceEquals(other.gameObject, _lastCollisionObject))
        {
            _lastCollisionTime = collisionTime;
            _lastCollisionObject = other.gameObject;
            return true;
        }

        _lastCollisionTime = collisionTime;
        _lastCollisionObject = other.gameObject;

        return false;
    }

    private void CheckIfGameIsFinished(Collision other)
    {
        
        // If detect any win/lose situation, call GameManager API to inform which player is lost the game
        // Otherwise, continue playing
        Debug.Log(_bounced + ", Ball has colliding with something " + other.gameObject.name);  
        
        if (_bounced == 0)
        {
            // ball should not hit the from square
            if (other.gameObject.CompareTag(_fromSquare.tag) || !other.gameObject.tag.Contains("Square"))
            {
                GameObject player = GameManager.getPlayerOnSquare(_fromSquare);
                GameManager.UpdateWinLose(player);
                return;
            }

        } else if (_bounced == 1)
        {
            // the collision has to be the player on target court, otherwise, someone has lost the game
            if (_targetSquare)
            {
                GameObject targetPlayer = GameManager.getPlayerOnSquare(_targetSquare);
                // if player is expected, then it is good
                if (!GameObject.ReferenceEquals(targetPlayer, other.gameObject))
                {
                    // if other players took the ball mistakenly, consider they are failed
                    if (other.gameObject.CompareTag("Player"))
                    {
                        // Not the right player who touched the ball, consider their fault
                        GameManager.UpdateWinLose(other.gameObject);
                        return;
                    }
                    else // if hit anything else, targetPlayer is failed to picking the ball up
                    {
                        // the player missed the ball, so lose the game
                        GameManager.UpdateWinLose(targetPlayer);
                        return;
                    }
                }
            }
        }
        
        // should hit the other 3 squares
        _bounced = 1;    
    }

    public void ShotTheBallToTargetSquare(GameObject fromSquare, GameObject targetSquare, float flyingTime, Vector3? location)
    {
        Debug.Log(fromSquare.name + " shot the ball to " + targetSquare.name);
        
        shouldCheckWinOrLose = false;
        Vector3 ballPosition = ballTransform.position;
        
        ballRb.isKinematic = true;
        _fromSquare = fromSquare; 
        _targetSquare = targetSquare;

        targetLocation = location ?? GetRandomTargetPosition(targetSquare);

        Vector3 velocity = GetVelocityToHitTargetGroundBasedOnExpectedTime(ballPosition, targetLocation, flyingTime);

        // we need to set bounced to zero with slightly delay because collision event triggering order issue
        StartCoroutine(StartCheckingBounce());
        
        EventManager.TriggerEvent<BallBounceEvent, Vector3, SquareLocation>(ballPosition,
            SquareLocation.square_one);

        ShowInstruction();
        
        ballRb.velocity = velocity;
        ballRb.isKinematic = false;
    }

    IEnumerator StartCheckingBounce()
    {
        yield return new WaitForSeconds(0.2f);
        shouldCheckWinOrLose = true;
        _bounced = 0;
    }

    private void ShowInstruction()
    {
        // if level 1, show target
        if (GameManager.Instance.currentLevel == 1)
        {
            target.SetActive(true);
        }
    }
}
