using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Projection _projection;
    public GameObject ballPrefab;
    
    public float moveSpeed = 10f;
    private Rigidbody playerRb;
    private Rigidbody ballRb;
    public Transform ballTransform;
    public Camera mainCamera;
    
    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        ballRb = ballTransform.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        RotatePlayer();
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
}
