using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform cameraTransform;
    public float minVerticalAngle = -40f;
    public float maxVerticalAngle = 80f;
    public float turnSpeed = 5f;

    private float verticalRotation = 0f;

    void Update()
    {
        // Player Movement
        MovePlayer();

        // Camera Orbit
        CameraControl();
    }

    void MovePlayer()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            // Gradually stop the player when no keys are pressed
            transform.position = Vector3.Lerp(transform.position, transform.position, 0.9f);
        }
    }

    void CameraControl()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        cameraTransform.RotateAround(transform.position, Vector3.up, mouseX * turnSpeed);

        verticalRotation -= mouseY * turnSpeed;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        Vector3 currentRotation = cameraTransform.localEulerAngles;
        currentRotation.x = verticalRotation;
        cameraTransform.localEulerAngles = currentRotation;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object has a tag "Ball"
        if (collision.gameObject.CompareTag("Ball"))
        {
            // Get the normal vector of the collision
            Vector3 collisionNormal = collision.contacts[0].normal;

            // Get the Rigidbody component of the bouncing ball
            Rigidbody ballRb = collision.gameObject.GetComponent<Rigidbody>();

            // Apply force to the bouncing ball with an upward component
            Vector3 forceDirection = -collisionNormal + new Vector3(0, 0.5f, 0);  // Upward force
            ballRb.AddForce(forceDirection.normalized * 500f);  // 500f is the force magnitude

        }
    }

}
