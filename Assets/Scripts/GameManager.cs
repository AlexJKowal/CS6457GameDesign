using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
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
    public UnityEvent onScore;

    public TextMeshPro gameStatus;
    public TextMeshPro levelIndicator;

    public GameObject humanPlayer;
    public GameObject AI_Player1;
    public GameObject AI_Player2;
    public GameObject AI_Player3;

    public GameObject ballObject;

    public GameObject square1;
    public GameObject square2;
    public GameObject square3;
    public GameObject square4;
    
    public UnityEvent onLevelComplete;
    public UnityEvent onLevelLoaded;
    public GameObject levelCompleteText;
    public GameObject levelLoseText;
    public GameObject confettiSystem;
    

    public int maxLevel = 1;

    public int maxScore = 4;

    public int currentLevel = 1;
    private Vector3 originalScale = new Vector3(1f,1f,1f);
    public float popInDuration = 0.5f;
    public AnimationCurve popInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private Dictionary<int, Level> _Levels = new Dictionary<int, Level>();

    private Dictionary<string, int> _Scores = new Dictionary<string, int>();
  
    public Dictionary<int, Level> Levels
    {
        get { return _Levels; }
        set { _Levels = value; }
    }
    
    public Dictionary<string, int> Scores
    {
        get { return _Scores; }
        set { _Scores = value; }
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
        _instance = this;
        DontDestroyOnLoad(gameObject);
        AddLevels();
        SetInitialScores();
        InitGame();
    }

    private static void InitGame()
    {
        Instance.onLevelLoaded?.Invoke();
        Instance.levelIndicator.SetText("Level " + Instance.currentLevel);
    }

    public IEnumerator TimerCoroutine(float duration, System.Action callback)
    {
        yield return new WaitForSeconds(duration);
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
        if (Instance.levelCompleteText != null)
        {
            Instance.levelCompleteText.SetActive(false);
        }
        if (Instance.levelLoseText != null)
        {
            Instance.levelLoseText.SetActive(false);
        }
    }

    private static void DisableConfetti()
    {
        if (Instance.confettiSystem != null)
        {
            Instance.confettiSystem.SetActive(false);
        }
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
            EventManager.TriggerEvent<CheeringEvent, Vector3>(Instance.confettiSystem.transform.position);
            Instance.confettiSystem.SetActive(true);

            Instance.levelCompleteText.SetActive(true);
            Instance.StartCoroutine(Instance.AnimateText(Instance.levelCompleteText, 5f));
            Instance.StartCoroutine(Instance.TimerCoroutine(5f, DisableText));
            Instance.StartCoroutine(Instance.TimerCoroutine(5f, DisableConfetti));
            Instance.onLevelComplete?.Invoke();
            ResetScores();
        }
        
        Instance.levelIndicator.SetText("Level " + Instance.currentLevel);
    }
    
    public static void redoLevel()
    {
        Instance.levelLoseText.SetActive(true);
        Instance.StartCoroutine(Instance.AnimateText(Instance.levelLoseText, 3f));
        Instance.StartCoroutine(Instance.TimerCoroutine(3f, DisableText));
        Instance.onLevelComplete?.Invoke();
        ResetScores();
    }
    
    // Can be square or player
    public static void updateScoreResult(GameObject playerOrSquare, GameObject lastTouched)
    {
        GameObject square = playerOrSquare;
        GameObject relatedPlayer = getPlayerOnSquare(square);
        string winner = "";
        string loser = relatedPlayer.name;
        if (lastTouched != null)
        {
            winner = getPlayerOnSquare(lastTouched).name;
        }
        else
        {
            winner = Instance.humanPlayer.name;
        }

        if (loser != winner)
        {
            Debug.Log(winner + " scores!");
            Instance.Scores[winner]++;
        }
        else if (loser.Length > 0)
        {
            Instance.Scores[loser]--;
        }
        
        Instance.onScore?.Invoke();
        CheckUpdateWinResult();

}

    public static void CheckUpdateWinResult()
    {
        if (Instance.Scores["Player"] >= Instance.maxScore){
            levelUp();
        }
        else if (false) //Not currently tracking NPC scores so no current loss condition
        {
            redoLevel();
        }
    }

    public void AddLevels()
    {
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
    }

    public void SetInitialScores()
    {
        Scores.Add("Player", 0);
        Scores.Add("AI_Player1", 0);
        Scores.Add("AI_Player2", 0);
        Scores.Add("AI_Player3", 0);
    }

    public static void ResetScores()
    {
        Instance.Scores["Player"] = 0;
        Instance.Scores["AIPlayer1"] = 0;
        Instance.Scores["AIPlayer2"] = 0;
        Instance.Scores["AIPlayer3"] = 0;
        Instance.onScore?.Invoke();
    }

    public static void SetPlayersState(PlayerState ps)
    {
        Instance.humanPlayer.GetComponent<PlayerController>().playerState = ps;
        Instance.AI_Player1.GetComponent<AIPlayerController>().playerState = ps;
        Instance.AI_Player2.GetComponent<AIPlayerController>().playerState = ps;
        Instance.AI_Player3.GetComponent<AIPlayerController>().playerState = ps;
    }
}
