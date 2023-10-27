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
    private float[] EASY = {0.5f, 0.7f, 1.2f, 2f};
    
    private float a = 9.81f / 2;

    private Rigidbody ballRb;
    private Transform ballTransform;
    private SphereCollider _collider;

    public List<GameObject> squares;

    // this is the target indicator where the ball will hit the ground
    public GameObject target;

    public GameObject humanPlayer;

    public GameObject _fromSquare;
    public GameObject _targetSquare;
    
    private GameObject _currentSquare;
    private GameObject _lastTouched;

    public bool _freeTargeting;
    public Vector3 targetLocation;
    
    public int bounced;

    private PlayerController pc;

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
    
    private Vector3 GetRandomTargetPosition(GameObject square)
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

        pc = humanPlayer.GetComponent<PlayerController>();
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

    private void OnCollisionEnter(Collision other)
    {
        if (pc.isHoldingBall)
        {
            return;
        }
        if (other.gameObject.tag.Contains("Square"))
        {
            
            EventManager.TriggerEvent<BallBounceEvent, Vector3, SquareLocation>(other.contacts[0].point,
                SquareLocation.square_one);
        }
        else
        {
            EventManager.TriggerEvent<BallBounceEvent, Vector3, SquareLocation>(other.contacts[0].point,
                SquareLocation.square_one);
        }

        if (other.gameObject.CompareTag("Player"))
        {
            EventManager.TriggerEvent<BallHitEvent, SquareLocation, ShotType>(
                SquareLocation.square_one, ShotType.lob_shot);
        }
    }

    public void OnCollisionExit(Collision other)
    {
        if (pc.isHoldingBall)
        {
            return;
        }
        if (other.gameObject.CompareTag("Player"))
        {
            _currentSquare = null;
            _lastTouched = other.gameObject;
            bounced = 0;
        }
        else if (other.gameObject.tag.Contains("Square"))
        {
            if (_currentSquare == null)
            {
                _currentSquare = other.gameObject;
            }
            else if (_currentSquare != other.gameObject)
            {
                bounced = 0;
                _currentSquare = other.gameObject;
            }
        }
        else
        {
            if (_currentSquare != null)
            {
                GameManager.updateScoreResult(_currentSquare, _lastTouched);
                _lastTouched = null;
                _currentSquare = null;
                bounced = -1;
            }
        }
        
        if (bounced == 1 && _currentSquare != null)
        {
            GameManager.updateScoreResult(_currentSquare, _lastTouched);
            _lastTouched = other.gameObject;
            _currentSquare = null;
            bounced = -1;
        }

        bounced++;
    }

    public void ShootTheBallInDirection(Vector3 velocity, GameObject fromSquare, GameObject estimateSquare, Vector3 location)
    {
        ballRb.isKinematic = true;
        _fromSquare = fromSquare;
        _targetSquare = estimateSquare;
        GameManager.updateGameStatus("Ball is from " + fromSquare.tag + " and heading to ???");

        targetLocation = location;
        
        ballRb.velocity = velocity;
        ballRb.isKinematic = false;
        
        StartCoroutine(ResetBounced());
        
        target.SetActive(true);
    }

    public void ShotTheBallToTargetSquare(GameObject fromSquare, GameObject targetSquare)
    {
        ballRb.isKinematic = true;
        _fromSquare = fromSquare; 
        _targetSquare = targetSquare;
        _freeTargeting = true;
        GameManager.updateGameStatus("Ball is from " + fromSquare.tag + " and heading to " + targetSquare.tag);
        
        targetLocation = GetRandomTargetPosition(targetSquare);

        Vector3 velocity = GetVelocityToHitTargetGroundBasedOnExpectedTime(ballTransform.position, targetLocation, Random.Range(EASY[2], EASY[3]));

        ballRb.velocity = velocity;
        ballRb.isKinematic = false;

        // we need to set bounced to zero with slightly delay because collision event triggering order issue
        StartCoroutine(ResetBounced());
        
        target.SetActive(true);
    }

    IEnumerator ResetBounced()
    {
        yield return new WaitForSeconds(0.3f);
        bounced = 0;
        _freeTargeting = false;
    }
}
