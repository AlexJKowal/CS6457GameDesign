using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform ballTransform;
    public float followSpeed = 0.1f;  // Smoothing factor, smaller values result in slower following
    public float minY = -5f;  // Minimum y-position for the camera
    public float maxY = 5f;  // Maximum y-position for the camera

    private float smoothY;

    void Start()
    {
        // Initialize smoothY with the camera's current y-position
        smoothY = transform.position.y;
    }

    void Update()
    {
        // Apply simple smoothing to the y-position
        smoothY = Mathf.Lerp(smoothY, ballTransform.position.y, followSpeed);

        // Clamp the y-position to stay within defined boundaries
        float clampedY = Mathf.Clamp(smoothY, minY, maxY);

        // Update the camera's position
        transform.position = new Vector3(transform.position.x, clampedY, transform.position.z);
    }
}
