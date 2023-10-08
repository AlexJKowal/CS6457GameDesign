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
    //[SerializeField] private GameObject ball;
    //[SerializeField] private BoxCollider trigger1;
    //[SerializeField] private BoxCollider trigger2;
    //[SerializeField] private BoxCollider trigger3;
    //[SerializeField] private BoxCollider trigger4;

    // Start is called before the first frame update
    void Start()
    {
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
