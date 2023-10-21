using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GroundVisualState : MonoBehaviour
{
    
    private UnityAction<Vector3, SquareLocation> ballBounceEventListener;
    private UnityAction<GameObject, SquareLocation> ballCaughtEventListener;
    private UnityAction resetEventListener;
    
    private int BounceCount = 0;

    private Renderer renderer;
    private Material material;

    private bool newBounce = false;

    // Start is called before the first frame update
    void Start()
    {
        renderer = gameObject.GetComponent<Renderer>();
        material = renderer.material;
    }

    // Update is called once per frame
    void Update()
    {
        if (newBounce)
        {
            if (BounceCount == 0)
            {
                material.SetColor("_BaseColor", Color.white);  
            }
            else if (BounceCount == 1)
            {
                material.SetColor("_BaseColor", Color.yellow);  
            }
            else if (BounceCount == 2)
            {
                material.SetColor("_BaseColor", Color.red);  
            }
            else
            {
                material.SetColor("_BaseColor", Color.black);  
            }

            newBounce = false;
        }
       
    }
    
    void Awake()
    {
        ballBounceEventListener = new UnityAction<Vector3, SquareLocation>(BallBounceEventHandler);
        ballCaughtEventListener = new UnityAction<GameObject, SquareLocation>(BallCaughtEventHandler);
        resetEventListener = new UnityAction(ResetEventHandler);
    }

    void OnEnable()
    {
        EventManager.StartListening<BallBounceEvent, Vector3, SquareLocation>(ballBounceEventListener);
        EventManager.StartListening<BallCaughtEvent, GameObject, SquareLocation>(ballCaughtEventListener);
        EventManager.StartListening<ResetEvent>(resetEventListener);
    }

    void OnDisable()
    {
        EventManager.StopListening<BallBounceEvent, Vector3, SquareLocation>(ballBounceEventListener);
        EventManager.StopListening<BallCaughtEvent, GameObject, SquareLocation>(ballCaughtEventListener);
        EventManager.StopListening<ResetEvent>(resetEventListener);
    }

    void BallBounceEventHandler(Vector3 location, SquareLocation square)
    {
        if (!gameObject.CompareTag(square.ToString()))
        {
            BounceCount = 0;
            newBounce = true;
        }
        else
        {
            BounceCount++;
            newBounce = true;
        }
    }

    void ResetEventHandler()
    {
        BounceCount = 0;
        newBounce = true;
    }

    void BallCaughtEventHandler(GameObject g, SquareLocation s)
    {
        ResetEventHandler();
    }
    
}
