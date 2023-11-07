using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public void OnStartGame()
    {
        Debug.Log("load four square game");
        SceneManager.LoadScene(1);
    }

    public void OnGameOptions()
    {
        // to be implemented
    }

    public void OnExit()
    {
        Application.Quit();
    }
}
