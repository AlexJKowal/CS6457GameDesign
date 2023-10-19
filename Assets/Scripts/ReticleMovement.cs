using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticleMovement : MonoBehaviour
{

    public GameObject player;

    public GameObject human;

    public GameObject opponentSquare; // This should be replaced at some point with a smarter high-level targeting system
    public float reticleSpeed = 100f;
    public float reticleRadius = 3f;
    private MeshRenderer mesh;
    private Camera mainCam;

    private Vector3 reticleCenter;
    private float reticleAngle;

    private bool holdingBall;
    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        if (player.CompareTag("Player"))
        {
            PlayerController.OnHoldingBallChanged += UpdateReticleState;
        }
        else
        { 
            EnemyControlScript.OnHoldingBallChanged += UpdateReticleState;
            reticleCenter = human.transform.position;
        }
        
        mesh.enabled = false;
        mainCam = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        MoveReticle();
    }

    private void UpdateReticleState(bool isHoldingBall)
    {
        mesh.enabled = isHoldingBall;
        holdingBall = isHoldingBall;
    }

    void MoveReticle()
    {

        if (player.CompareTag("Player"))
        {
            CalculatePlayerReticle();
        }
        else
        {
            CalculateNPCReticle();
        }

    }

    void CalculatePlayerReticle()
    {
        //enable if using mouse
        /*  Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        float projection;
        Plane floor = new Plane(Vector3.up, Vector3.zero);

        if (floor.Raycast(ray, out projection))
        {
            Vector3 reticleDir = ray.GetPoint(projection);
            transform.position = reticleDir;
        }

        */

        // 2nd analogue stick
        float horizontal = Input.GetAxis("Horizontal2");
        float vertical = Input.GetAxis("Vertical2");

        Vector3 reticleDir = new Vector3();
        Vector3 direction = new Vector3(horizontal, 0f,  (-1) * vertical).normalized;

        if (direction.magnitude > 0.1f)
        {
            // Convert the direction from local to world space based on camera orientation
            reticleDir = Quaternion.Euler(0, mainCam.transform.eulerAngles.y, 0) * direction;

        }
        else
        {
            Vector3 differenceVector = (-transform.position + player.transform.position).normalized;
            reticleDir.x = 2 * differenceVector.x;
            reticleDir.y = 0f;
            reticleDir.z = 2 * differenceVector.z;
        }

        reticleDir *= reticleSpeed * Time.fixedDeltaTime;
        transform.position += reticleDir;
    }

    void CalculateNPCReticle()
    {
        //Currently will just fire straight at Player 1
        reticleCenter = human.transform.position;
        Vector3 reticleDir = new Vector3();

        if (holdingBall)
        {
            reticleAngle = (reticleAngle + (Time.fixedDeltaTime * reticleSpeed)) % 360;
            float rads = reticleAngle * Mathf.Deg2Rad;
            reticleDir.x = reticleCenter.x + reticleRadius * Mathf.Cos(rads);
            reticleDir.z = reticleCenter.z + reticleRadius * Mathf.Sin(rads);

        }
        else
        {
            transform.position = player.transform.position;
        }
        
        transform.position = reticleDir;
    }
}
