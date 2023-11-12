using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RandomCollectible : MonoBehaviour
{

    public GameObject squareBound;

    public float moveSpeed;

    public int probabilityFactor;
    
    public float defaultBoostAmount = 1.5f;
    public float defaultBoostDuration = 1f; // Measured in # of rounds
    private UnityAction<GameObject, SquareLocation> ballCaughtEventListener;
    private UnityAction<SquareLocation, ShotType> ballHitEventListener;

    private bool randomChanceCheck = true;

    private Vector3 targetLocation;
    
    private Vector3 originalPosition;

    private bool initialized = false;
    
    private MeshCollider squareCollider;
    private MeshRenderer collectibleRenderer;
    private CapsuleCollider collectibleCollider;
    
    private int squareCounter;
  
    
    
    
    // Start is called before the first frame update
    void Start()
    {
      
        squareCollider = squareBound.GetComponent<MeshCollider>();
        collectibleCollider = GetComponent<CapsuleCollider>();
        collectibleRenderer = GetComponent<MeshRenderer>();
        originalPosition = gameObject.transform.position;
        initialized = true;

        collectibleCollider.enabled = false;
        collectibleRenderer.enabled = false;
    }
    
    void Awake()
    {
        ballCaughtEventListener = new UnityAction<GameObject, SquareLocation>(BallCaughtEventHandler);
        ballHitEventListener = new UnityAction<SquareLocation, ShotType>(BallHitEventHandler);
        EventManager.StartListening<BallCaughtEvent, GameObject, SquareLocation>(ballCaughtEventListener);
        EventManager.StartListening<BallHitEvent, SquareLocation, ShotType>(ballHitEventListener);
    }

    void OnEnable()
    {
        if (initialized)
        {
            transform.position = originalPosition;
        }
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (collectibleCollider.enabled)
        {
            SetTarget();
        }

    }

    void FixedUpdate()
    {
        if (collectibleCollider.enabled)
        {
            CirculateSquare();
        }
    }


    void CirculateSquare()
    {
        Vector3 newRootPosition = Vector3.LerpUnclamped(this.transform.position, targetLocation, moveSpeed);
        transform.position = newRootPosition;
    }

    void SetTarget()
    {
        
        if (targetLocation == null || (targetLocation - this.transform.position).magnitude < 1)
        {
            squareCounter++;
            
            if (squareCounter > 4)
            {
                squareCounter = 1;
            }

            Bounds boundSquare = squareCollider.bounds;

            float x = targetLocation.x;
            float z = targetLocation.z;
        
            if (squareCounter == 1){ 
            
                x = boundSquare.max.x;
                z = boundSquare.max.z;
        
            }
            else if (squareCounter == 2)
            {
                x = boundSquare.max.x;
                z = boundSquare.min.z;
            }
            else if (squareCounter == 3)
            {
                x = boundSquare.min.x;
                z = boundSquare.min.z;
            }
            else if (squareCounter == 4)
            {
                x = boundSquare.min.x;
                z = boundSquare.max.z;
            }

            targetLocation.x = x;
            targetLocation.y = originalPosition.y;
            targetLocation.z = z;
        }
    }
    
    void RandomChanceAppearance()
    {
        int roll = Random.Range(0, 100);
        if (roll >= probabilityFactor)
        {
            collectibleCollider.enabled = true;
            collectibleRenderer.enabled = true;
        }
    }
    
    void BallHitEventHandler(SquareLocation square, ShotType shot)
    {
        if (square == SquareLocation.square_one && randomChanceCheck) //Change upon adding more npcs to generalize
        {
            RandomChanceAppearance();
        }
    }
    
    void BallCaughtEventHandler(GameObject catcher, SquareLocation square)
    {
        if (square == SquareLocation.square_one)
        {
            ResetCollectable();
        }
        
    }

    void ResetCollectable()
    {
        collectibleCollider.enabled = false;
        collectibleRenderer.enabled = false;
        transform.position = originalPosition;
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("Player"))
        {
            EventManager.TriggerEvent<StatBoostEvent, GameObject, BoostType, float, float>(
                c.gameObject,
                BoostType.speed,
                defaultBoostAmount,
                defaultBoostDuration
                );

            ResetCollectable();
            randomChanceCheck = false;
            ScalePlayer(c.gameObject, true);
            StartCoroutine(ShrinkPlayer(c.gameObject, false, 10f));
        }
    }

    private IEnumerator ShrinkPlayer(GameObject player, bool enLarge, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        ScalePlayer(player, false);
        randomChanceCheck = true;
    }

    // fix the flying issue when eat the collectible
    private void ScalePlayer(GameObject player, bool enLarge)
    {
        if (enLarge)
        {
            player.transform.localScale += new Vector3(1f, 1f, 1f);
        }
        else
        {
            player.transform.localScale -= new Vector3(1f, 1f, 1f);
        }
    }
}
