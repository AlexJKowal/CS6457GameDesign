using UnityEngine;

// This file is obsolete.
public class DeletedBallTriggerTracker : MonoBehaviour
{
    public Transform playerTransform;  // Reference to the player's Transform
    public float slowMotionFactor = 0.5f;  // Slow-motion speed
    public float normalTime = 1f;  // Normal speed

    private string currentTrigger = "None";
    private string previousTrigger = "None";
    private Rigidbody ballRb;
    private bool isServed = false;
    private Vector3 initialPlayerPosition;
    private Vector3 initialBallPosition;
    private PauseManager pauseManager;


    void Start()
    {
        // Store initial positions
        initialPlayerPosition = playerTransform.position;
        initialBallPosition = transform.position;

        ballRb = GetComponent<Rigidbody>();
        ballRb.isKinematic = true;  // Make the ball static in the air until served

        pauseManager = FindObjectOfType<PauseManager>();
    }

    void Update()
    {
        // Serve the ball on space press
        if (Input.GetButton("Submit") && !isServed)
        {
            ballRb.isKinematic = false;
            isServed = true;
        }

        // Slow down time when holding space after serve
        if (pauseManager.checkPaused())
        {
            Debug.Log($"Game is paused");
        }
        else if (Input.GetButton("Submit") && isServed)
        {
            Debug.Log($"Slow-mo activated");
            Time.timeScale = slowMotionFactor;
        }
        else
        {
            // Debug.Log($"Slow-mo deactivated");
            Time.timeScale = normalTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Update the previous and current triggers
        previousTrigger = currentTrigger;
        currentTrigger = other.gameObject.tag;

        // Reset the game if the ball hits the "Edge"
        if (currentTrigger == "Edge")
        {
            ResetGame();
        }

        // Print to console
        Debug.Log($"Bounce: {currentTrigger}, Previous Bounce: {previousTrigger}");
    }

    void ResetGame()
    {
        // Reset the ball
        transform.position = initialBallPosition;
        ballRb.isKinematic = true;
        ballRb.velocity = Vector3.zero;
        isServed = false;

        // Reset the player
        playerTransform.position = initialPlayerPosition;
        
        // Invoke a ResetEvent so that other objects can handle their own reset behaviour
        EventManager.TriggerEvent<ResetEvent>();
    }

    void OnCollisionEnter(Collision c)
    {
        SquareLocation currentSquare;
        if (c.gameObject.CompareTag("square_one"))
        {
            currentSquare = SquareLocation.square_one;
        }
        else if (c.gameObject.CompareTag("square_two"))
        {
            currentSquare = SquareLocation.square_two;
        }
        else if (c.gameObject.CompareTag("square_three"))
        {
            currentSquare = SquareLocation.square_three;
        }
        else if (c.gameObject.CompareTag("square_four"))
        {
            currentSquare = SquareLocation.square_four;
        }
        else
        {
            currentSquare = SquareLocation.out_of_square;
        }

        bool isGroundCollision = c.gameObject.CompareTag("square_one") ||
                                 c.gameObject.CompareTag("square_two") ||
                                 c.gameObject.CompareTag("square_three") ||
                                 c.gameObject.CompareTag("square_four");

        if (c.impulse.magnitude > 0.25f)
        {
            EventManager.TriggerEvent<BallBounceEvent, Vector3, SquareLocation>(c.contacts[0].point, currentSquare);
        }
    }
}
