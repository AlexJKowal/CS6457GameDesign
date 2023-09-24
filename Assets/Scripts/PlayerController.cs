using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public Transform ballTransform;
    public Camera mainCamera;
    public float maxThrowForce = 20f;
    public float chargeRate = 10f;

    private Rigidbody playerRb;
    private Rigidbody ballRb;
    private bool isHoldingBall = false;
    private float chargeAmount = 0f;
    private bool justPickedUp = false;


    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        ballRb = ballTransform.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void Update()
    {
        RotatePlayer();
        HandleBall();
    }

    void RotatePlayer()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPos = mainCamera.WorldToScreenPoint(transform.position);

        Vector3 aimDirection = mousePos - playerScreenPos;
        float angle = Mathf.Atan2(aimDirection.x, aimDirection.y) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, angle, 0);
    }

    void MovePlayer()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Convert the direction from local to world space based on camera orientation
            Vector3 moveDir = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0) * direction;
            moveDir *= moveSpeed * Time.fixedDeltaTime;
            playerRb.MovePosition(transform.position + moveDir);
        }
    }


    void HandleBall()
    {
        float distanceToBall = Vector3.Distance(transform.position, ballTransform.position);

        // Debug message for proximity to ball
        Debug.Log("Distance to Ball: " + distanceToBall);

        // Pick up ball on mouse click if not already holding
        if (!isHoldingBall && Input.GetMouseButtonDown(0) && distanceToBall <= 1.5f)
        {
            Debug.Log("Picking up ball");  // Debug
            isHoldingBall = true;
            justPickedUp = true;
        }

        // If holding the ball
        if (isHoldingBall)
        {
            Debug.Log("Holding ball");  // Debug

            // Position the ball in front of the player
            ballTransform.position = transform.position + transform.forward;

            // Charge throw while holding the ball
            chargeAmount += Time.deltaTime * chargeRate;

            // Debug message for charge amount
            Debug.Log("Charge Amount: " + chargeAmount);

            // Release and throw ball on mouse click, but not if it was just picked up
            if (Input.GetMouseButtonDown(0) && !justPickedUp)
            {
                Debug.Log("Attempting to throw ball");  // Debug
                float finalThrowForce = Mathf.Clamp(chargeAmount, 0, maxThrowForce);
                ballRb.velocity = transform.forward * finalThrowForce;
                ResetBallHandling();
            }

            // Reset the justPickedUp flag
            justPickedUp = false;
        }
    }


    void ResetBallHandling()
    {
        isHoldingBall = false;
        chargeAmount = 0f;
        justPickedUp = false;
    }

}
