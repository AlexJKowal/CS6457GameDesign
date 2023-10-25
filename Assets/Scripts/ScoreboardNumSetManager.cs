using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreboardNumSetManager : MonoBehaviour
{
    [SerializeField] private GameObject scoreBoardOnes;
    [SerializeField] private GameObject scoreBoardTens;
    private int count1;
    private int count10;
    private Animator anim1;
    private Animator anim10;
    //private BallTriggerTracker gameReset;
    PrevBounce prevBounce;

    // Start is called before the first frame update
    void Start()
    {
        //gameReset = GetComponent<BallTriggerTracker>();
        prevBounce = PrevBounce.firstbounce;
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
        switch (prevBounce)
        {
            case PrevBounce.firstbounce:
                if (c.gameObject.CompareTag("Square1"))
                {
                    Debug.Log("Square1 FirstBounce");
                    prevBounce = PrevBounce.square1;
                }
                else if (c.gameObject.CompareTag("Square2"))
                {
                    Debug.Log("Square2 FirstBounce");
                    prevBounce = PrevBounce.square2;
                }
                else if (c.gameObject.CompareTag("Square3"))
                {
                    Debug.Log("Square3 FirstBounce");
                    prevBounce = PrevBounce.square3;
                }
                else if (c.gameObject.CompareTag("Square4"))
                {
                    Debug.Log("Square4 FirstBounce");
                    prevBounce = PrevBounce.square4;
                }
                else
                {
                    //gameReset.ResetGame();
                }
                break;
            case PrevBounce.square1:
                if(c.gameObject.CompareTag("Player") || c.gameObject.CompareTag("Untagged"))
                {
                    prevBounce = PrevBounce.firstbounce;
                }
                else
                {
                    Debug.Log("Square1 SecondBounce");
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
                    Debug.Log(count1);
                    Debug.Log(count10);
                    prevBounce = PrevBounce.firstbounce;
                }
                break;
            case PrevBounce.square2:
                if (c.gameObject.CompareTag("Player") || c.gameObject.CompareTag("Untagged"))
                {
                    prevBounce = PrevBounce.firstbounce;
                }
                else
                {
                    Debug.Log("Square2 SecondBounce");
                    if (count1 >= 8)
                    {
                        count1 = (count1 + 2) % 10;
                        count10++;
                        anim1.SetInteger("Count", count1);
                        anim10.SetInteger("Count", count10);
                    }
                    else
                    {
                        count1 += 2;
                        anim1.SetInteger("Count", count1);
                    }
                    Debug.Log(count1);
                    Debug.Log(count10);
                    prevBounce = PrevBounce.firstbounce;
                }
                break;
            case PrevBounce.square3:
                if (c.gameObject.CompareTag("Player") ||c.gameObject.CompareTag("Untagged"))
                {
                    prevBounce = PrevBounce.firstbounce;
                }
                else
                {
                    Debug.Log("Square3 SecondBounce");
                    if (count1 >= 7)
                    {
                        count1 = (count1 + 3) % 10;
                        count10++;
                        anim1.SetInteger("Count", count1);
                        anim10.SetInteger("Count", count10);
                    }
                    else
                    {
                        count1 += 3;
                        anim1.SetInteger("Count", count1);
                    }
                    Debug.Log(count1);
                    Debug.Log(count10);
                    prevBounce = PrevBounce.firstbounce;
                }
                break;
            case PrevBounce.square4:
                if (c.gameObject.CompareTag("Player") || c.gameObject.CompareTag("Untagged"))
                {
                    prevBounce = PrevBounce.firstbounce;
                }
                else
                {
                    Debug.Log("Square4 SecondBounce");
                    if (count1 >= 6)
                    {
                        count1 = (count1 + 4) % 10;
                        count10++;
                        anim1.SetInteger("Count", count1);
                        anim10.SetInteger("Count", count10);
                    }
                    else
                    {
                        count1 += 4;
                        anim1.SetInteger("Count", count1);
                    }
                    Debug.Log(count1);
                    Debug.Log(count10);
                    prevBounce = PrevBounce.firstbounce;
                }
                break;
        }
    }
}
