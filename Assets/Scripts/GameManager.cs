using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Level
{
    public float Size { get; set; }
    public UnityEngine.Color CourtColor { get; set; }
    public float PowerPenalty { get; set; }
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public TextMeshPro gameStatus;

    public GameObject humanPlayer;
    public GameObject AI_Player1;
    public GameObject AI_Player2;
    public GameObject AI_Player3;

    public GameObject square1;
    public GameObject square2;
    public GameObject square3;
    public GameObject square4;
    
    public UnityEvent onLevelComplete;
    public UnityEvent onLevelLoaded;
    public GameObject levelCompleteText;
    public GameObject levelLoseText;
    
    

    public int maxLevel;

    public int currentLevel = 1;
    private Vector3 originalScale = new Vector3(1f,1f,1f);
    public float popInDuration = 0.5f;
    public AnimationCurve popInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private Dictionary<int, Level> _Levels = new Dictionary<int, Level>();
  
    public Dictionary<int, Level> Levels
    {
        get { return _Levels; }
        set { _Levels = value; }
    }
    
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
        if (_instance == null)
        {
            _instance = this;
            Debug.Log("initting");
            Levels.Add(1, new Level
            {
                Size = 1,
                CourtColor = new Color(0.1f, 0.7f, 0.5f, 1.0f),
                PowerPenalty = 1
            });

            Levels.Add(2, new Level
            {
                Size = 1.5f,
                CourtColor = new Color(0.1f, 0.5f, 0.5f, 1.0f),
                PowerPenalty = 0.85f
            });
            
            InitGame();
            
           // GameResult("Player 1");

            Debug.Log("Levels added");

        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        Debug.Log("End of awake");
        
        DontDestroyOnLoad(gameObject);
    }

    private static void InitGame()
    {
        // Assigning players to squares
        Instance.humanPlayer.GetComponent<PlayerController>().homeSquare = Instance.square1;
        Instance.AI_Player1.GetComponent<AIPlayerController>().homeSquare = Instance.square2;
        Instance.AI_Player2.GetComponent<AIPlayerController>().homeSquare = Instance.square3;
        Instance.AI_Player3.GetComponent<AIPlayerController>().homeSquare = Instance.square4;
        Instance.onLevelLoaded?.Invoke();
    }

    public static void ResetGame()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
        Instance.onLevelLoaded?.Invoke();
    }

    public IEnumerator TimerCoroutine(float duration, System.Action callback)
    {
        Debug.Log("time started");
        yield return new WaitForSeconds(duration);
        Debug.Log("time ended");
        callback?.Invoke();
    }
    
    private IEnumerator AnimateText(GameObject target, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / popInDuration;
            float curveValue = popInCurve.Evaluate(progress);
            target.transform.localScale = originalScale * curveValue;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private static void DisableText()
    {
        Debug.Log("setting disabled");
        Instance.levelCompleteText.SetActive(false);
        Instance.levelLoseText.SetActive(false);
    }

    public static GameObject getPlayerOnSquare(GameObject square)
    {
        GameObject[] AI_Players = { Instance.AI_Player1, Instance.AI_Player2, Instance.AI_Player3 };
        
        GameObject player = Array.Find(AI_Players, 
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

    public static void updateGameStatus(String message)
    {
        // just for demostration purpose, better than debug info
        Instance.gameStatus.SetText(message);
    }

    public static void levelUp() 
    {
        Instance.currentLevel++;
        if (Instance.currentLevel > Instance.maxLevel){
            SceneManager.LoadScene("VictoryScreen");
            Instance.currentLevel = 1;
        }
        else
        {
            Debug.Log("set complete true");
            Instance.levelCompleteText.SetActive(true);
            Instance.StartCoroutine(Instance.AnimateText(Instance.levelCompleteText, 3f));
            Debug.Log("done set true");
            Instance.StartCoroutine(Instance.TimerCoroutine(3f, DisableText));
            Debug.Log("done start coroutine");
            Instance.onLevelComplete?.Invoke();
            ResetGame();
        }
    }
    
    public static void redoLevel()
    {
        Instance.levelLoseText.SetActive(true);
        Instance.StartCoroutine(Instance.AnimateText(Instance.levelLoseText, 3f));
        Instance.StartCoroutine(Instance.TimerCoroutine(3f, DisableText));
        Instance.onLevelComplete?.Invoke();
        ResetGame();
        
    }
    
    // Can be square or player
    public static void updateGameResult(GameObject playerOrSquare)
    {
        GameObject square = playerOrSquare;
        String winner;
        if (playerOrSquare.tag.Contains("Player"))
        {
            if (GameObject.ReferenceEquals(playerOrSquare, Instance.humanPlayer))
            {
                square = playerOrSquare.GetComponent<PlayerController>().homeSquare;
                winner = "Player";

            }
            else
            {
                square = playerOrSquare.GetComponent<AIPlayerController>().homeSquare;
                winner = "AI";
            }
            GameResult(winner);
        }

    }

    public static void GameResult(String winPlayer)
    {
        Debug.Log("level result call");
        if (string.Compare(winPlayer, "Player") == 0)
        {
            Debug.Log("call lvl");
            levelUp();
        }
        else
        {
            redoLevel();
        }
    }
}
