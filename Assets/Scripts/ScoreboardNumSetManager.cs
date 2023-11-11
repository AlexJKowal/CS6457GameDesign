using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

public class ScoreboardNumSetManager : MonoBehaviour
{
    [SerializeField] private GameObject scoreBoardOnes;
    [SerializeField] private GameObject scoreBoardTens;
    private int count1;
    private int count10;
    private Animator anim1;
    private Animator anim10;

    // Start is called before the first frame update
    void Start()
    {
        anim1 = scoreBoardOnes.GetComponent<Animator>();
        anim10 = scoreBoardTens.GetComponent<Animator>();
        count1 = 0;
        count10 = 0;
    }

    public int GetScore()
    {
        return count10 * 10 + count1;
    }

    public void SetScore(int score)
    {
        count10 = (int)Mathf.Floor(score / 10);
        count1 = score % 10;
  
        anim1.SetInteger("Count", count1);
        anim10.SetInteger("Count", count10);
    }
    
}
