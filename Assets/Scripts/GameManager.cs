using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using DefaultNamespace;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public TextMeshPro gameStatus;
    public TextMeshPro levelIndicator;
    
    public GameObject scoreBoard;
    public Canvas countDownCanvas; 
    
    public GameObject humanPlayer;
    public GameObject aiPlayer1;
    public GameObject aiPlayer2;
    public GameObject aiPlayer3;

    public GameObject square1;
    public GameObject square2;
    public GameObject square3;
    public GameObject square4;
    
    public GameObject confettiSystem;
    

    public int MAX_LEVEL = 1;
    public int LEVEL_UP_WINS = 3;
    public int MAXMAL_LOSES = 3;
    
    public int currentLevel = 1;

    public int playerWins = 0;
    public int playerLoses = 0;
    
    public static GameManager Instance
    {
        get
        {
            if (!_instance)
                Debug.LogError("There needs to be one active gameManager script on a GameObject in your scene.");

            return _instance;
        }
    }

    public void Awake()
    {
        _instance = this;
        InitGame();
    }

    private static void InitGame()
    {
        Instance.levelIndicator.SetText("Level " + Instance.currentLevel);
    }

    public static GameObject getTargetSquareBasedOnPosition(Vector3 position)
    {
        GameObject[] squares = { Instance.square1, Instance.square2, Instance.square3, Instance.square4 };
        GameObject square = Array.Find(squares, sq =>
        {
            Vector3 size = sq.GetComponent<MeshRenderer>().bounds.size;

            Vector3 sqPosition = sq.transform.position;
            bool contains = !(Mathf.Abs(sqPosition.x - position.x) > size.x / 2 ||
                              Mathf.Abs(sqPosition.z - position.z) > size.z / 2);
            
            return contains;
        });
            
        return square;
    }

    public static GameObject getPlayerOnSquare(GameObject square)
    {
        if (square == null)
        {
            return null;
        }
        
        GameObject[] aiPlayers = { Instance.aiPlayer1, Instance.aiPlayer2, Instance.aiPlayer3 };
        GameObject player = Array.Find(aiPlayers, 
            p => p.GetComponent<AIPlayerController>().homeSquare.CompareTag(square.tag)
            );

        if (player)
        {
            return player;
        }
        else
        {
            return Instance.humanPlayer;
        }
    }
    
    public static void levelUp() 
    {
        Instance.currentLevel++;
        if (Instance.currentLevel > Instance.MAX_LEVEL){
            SceneManager.LoadScene("VictoryScreen");
            Instance.currentLevel = 1;
        }
        else
        {
            EventManager.TriggerEvent<CheeringEvent, Vector3>(Instance.confettiSystem.transform.position);
            Instance.confettiSystem.SetActive(true);
        }
        
        Instance.levelIndicator.SetText("Level " + Instance.currentLevel);
    }

    public static void UpdateWinLose(GameObject losePlayer)
    {
        bool playerWin;
        // human player lose the game
        if (GameObject.ReferenceEquals(losePlayer, Instance.humanPlayer))
        {
            playerWin = false;
            Instance.playerLoses++;
            if (Instance.playerLoses >= Instance.MAXMAL_LOSES)
            {
                // Load Lose Scene
                
            }
            
        }
        else // human player win the game
        {
            playerWin = true;
            Instance.playerWins++;
            if (Instance.playerWins >= Instance.LEVEL_UP_WINS)
            {
                // load next level
                levelUp();
                return;
            }
        }

        Instance.StartCoroutine(ShowWinLoseTransitions(playerWin));
    }

    static IEnumerator ShowWinLoseTransitions(bool win)
    {
        GameManager.SetPlayersState(PlayerState.Idle);
        
        String message = win ? "You Win!" : "You Lose!";
        // Update player score
        Instance.scoreBoard.GetComponent<ScoreboardNumSetManager>().SetScore(Instance.playerWins);
        
        // Reset the game (Avoid ball bouncing again after win or lose
        Instance.humanPlayer.GetComponent<PlayerController>().ResetStates();
        
        // Show animated win/lose text
        float waitSeconds = 2f;
        Instance.gameStatus.GetComponent<AnimateText>().ShowText(message, waitSeconds);
        yield return new WaitForSeconds(waitSeconds + 1);
        
        // Show count down
        int countDown = 3;
        Instance.countDownCanvas.GetComponent<CountDownController>().ShowCountDown(countDown);
        yield return new WaitForSeconds(countDown + 1);
        
        GameManager.SetPlayersState(PlayerState.Playing);
    }


    // Set player state to something other than Playering, will stop receiving controller inputs
    public static void SetPlayersState(PlayerState ps)
    {
        Instance.humanPlayer.GetComponent<PlayerController>().playerState = ps;
        Instance.aiPlayer1.GetComponent<AIPlayerController>().playerState = ps;
        Instance.aiPlayer2.GetComponent<AIPlayerController>().playerState = ps;
        Instance.aiPlayer3.GetComponent<AIPlayerController>().playerState = ps;
    }
}
