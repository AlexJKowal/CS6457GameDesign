using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] tutorialTips;
    private int currentTipIndex;
    void Start()
    {
        currentTipIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            tutorialTips[currentTipIndex].SetActive(false);

            currentTipIndex++;

            if (currentTipIndex >= tutorialTips.Length)
            {
                currentTipIndex = 0;
            }

            tutorialTips[currentTipIndex].SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SceneManager.LoadScene("StartScreen");
        }

    }
}
