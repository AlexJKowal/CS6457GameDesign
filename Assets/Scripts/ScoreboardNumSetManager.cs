using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreboardNumSetManager : MonoBehaviour
{
    [SerializeField] private GameObject scoreBoardOnes;
    [SerializeField] private GameObject scoreBoardTens;
    private int count1;
    private int count10;
    private bool firstBounce = true;
    private Animator anim1;
    private Animator anim10;
    [SerializeField] private CapsuleCollider PlayerTrigger;
    [SerializeField] private CapsuleCollider EnemyTrigger1;
    private BallTriggerTracker gameReset;
    //[SerializeField] private CapsuleCollider EnemyTrigger2;  For more enemies
    //[SerializeField] private CapsuleCollider EnemyTrigger3;  For more enemies
    PrevBounce prevBounce;

    // Start is called before the first frame update
    void Start()
    {
        gameReset = GetComponent<BallTriggerTracker>();
        prevBounce = PrevBounce.firstshot;
        anim1 = scoreBoardOnes.GetComponent<Animator>();
        anim10 = scoreBoardTens.GetComponent<Animator>();
        count1 = 0;
        count10 = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider c)
    {
        if (firstBounce)
        {
            if (c.gameObject.CompareTag("Square1"))
            {
                prevBounce = PrevBounce.square1;
                firstBounce = false;
            }
            else if (c.gameObject.CompareTag("Square2"))
            {
                prevBounce = PrevBounce.square2;
                firstBounce = false;
            {
                prevBounce = PrevBounce.square3;
                firstBounce = false;
            }
            }
            else if (c.gameObject.CompareTag("Square3"))
            else if (c.gameObject.CompareTag("Square4"))
            {
                prevBounce = PrevBounce.square4;
                firstBounce = false;
            }
            else
            {
                gameReset.ResetGame();
            }
        }
        else
        {

        }
        if (c.CompareTag("Ball"))
        {
            if (count1 == 9)
            {
                count1 = 0;
                count10++;
                anim1.SetInteger("Count", count1);
                anim10.SetInteger("Count", count10);
            }
            else
            {
                count1++;
                anim1.SetInteger("Count", count1);
            }
        }
    }
}
