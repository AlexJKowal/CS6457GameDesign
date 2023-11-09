using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private bool _gamePaused = false;
    private CanvasGroup _pauseCanvasGroup;

    private void Start()
    {
        _pauseCanvasGroup = GetComponent<CanvasGroup>();
        
        _pauseCanvasGroup.interactable = false;
        _pauseCanvasGroup.blocksRaycasts = false;
        _pauseCanvasGroup.alpha = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        _gamePaused = !_gamePaused;
        Time.timeScale = _gamePaused ? 0f : 1;
            
        _pauseCanvasGroup.interactable = _gamePaused;
        _pauseCanvasGroup.blocksRaycasts = _gamePaused;
        _pauseCanvasGroup.alpha =_gamePaused ? 1f : 0;
    }
}
