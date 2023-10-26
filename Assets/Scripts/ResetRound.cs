using UnityEngine;

public class ResetRound : MonoBehaviour
{
    public Transform playerTransform;  // Reference to the player's Transform
    public float slowMotionFactor = 0.5f;  // Slow-motion speed
    public float normalTime = 1f;  // Normal speed

    private string currentTrigger = "None";
    private string previousTrigger = "None";
    private Rigidbody ballRb;
    private Vector3 initialPlayerPosition;
    private Vector3 initialBallPosition;


    void Start()
    {
        // Store initial positions
        initialPlayerPosition = playerTransform.position;
        initialBallPosition = transform.position;

        ballRb = GetComponent<Rigidbody>();
        ballRb.isKinematic = true;  // Make the ball static in the air until served
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

        // Reset the player
        playerTransform.position = initialPlayerPosition;

        // Invoke a ResetEvent so that other objects can handle their own reset behaviour
        EventManager.TriggerEvent<ResetEvent>();
    }
}
