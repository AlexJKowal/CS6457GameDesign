using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private bool paused = false;

    private float originalTimeScale;

    void Start()
    {
        originalTimeScale = Time.timeScale;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = originalTimeScale;
    }

    public bool checkPaused()
    {
        return paused;
    }
    void Update()
    {
       // print(paused);
        if (Input.GetKeyUp(KeyCode.P))
        {
            if (paused)
            {
                this.ResumeGame();
                paused = false;
            }
            else
            {
                this.PauseGame();
                paused = true;
            }
        }
    }
}
