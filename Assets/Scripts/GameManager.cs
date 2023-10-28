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
    public UnityEvent onScore;

    public TextMeshPro gameStatus;

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
    

    public int maxLevel = 1;

    public int maxScore = 4;

    public int currentLevel = 1;
    private Vector3 originalScale = new Vector3(1f,1f,1f);
    public float popInDuration = 0.5f;
    public AnimationCurve popInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private Dictionary<int, Level> _Levels = new Dictionary<int, Level>();

    private Dictionary<string, int> _Scores = new Dictionary<string, int>();
    private Dictionary<string, Vector3> _InitialPositions = new Dictionary<string, Vector3>();
  
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
    
    public Dictionary<string, Vector3> InitialPositions
    {
        get { return _InitialPositions; }
        set { _InitialPositions = value; }
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
            DontDestroyOnLoad(gameObject);
            AddLevels();
            SetInitialScores();
            SetInitialPositions();
            InitGame();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

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
            
            Instance.levelCompleteText.SetActive(true);
            Instance.StartCoroutine(Instance.AnimateText(Instance.levelCompleteText, 3f));
            Instance.StartCoroutine(Instance.TimerCoroutine(3f, DisableText));
            Instance.onLevelComplete?.Invoke();
            ResetScores();
            ResetPositions();
        }
    }
    
    public static void redoLevel()
    {
        Instance.levelLoseText.SetActive(true);
        Instance.StartCoroutine(Instance.AnimateText(Instance.levelLoseText, 3f));
        Instance.StartCoroutine(Instance.TimerCoroutine(3f, DisableText));
        Instance.onLevelComplete?.Invoke();
        ResetScores();
        ResetPositions();

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
        else
        {
            ResetPositions();
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
    
    public static void ResetGame()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
        Instance.onLevelLoaded?.Invoke();
    }

    public static void ResetPositions()
    {
        EventManager.TriggerEvent<ResetEvent>();
        Instance.ballObject.transform.position = Instance.InitialPositions["Ball"];
        Instance.AI_Player1.transform.position = Instance.InitialPositions["AI1"];
        Instance.AI_Player2.transform.position = Instance.InitialPositions["AI2"];
        Instance.AI_Player3.transform.position = Instance.InitialPositions["AI3"];
        Instance.humanPlayer.transform.position = Instance.InitialPositions["Human"];
    }
    public static void ResetScores()
    {
        Instance.Scores["Player"] = 0;
        Instance.Scores["AI_Player1"] = 0;
        Instance.Scores["AI_Player2"] = 0;
        Instance.Scores["AI_Player3"] = 0;
        Instance.onScore?.Invoke();
        
    }

    public static void SetInitialPositions()
    {
        Instance.InitialPositions.Add("Ball", Instance.ballObject.transform.position);
        Instance.InitialPositions.Add("AI1",  Instance.AI_Player1.transform.position);
        Instance.InitialPositions.Add("AI2", Instance.AI_Player2.transform.position);
        Instance.InitialPositions.Add("AI3", Instance.AI_Player3.transform.position);
        Instance.InitialPositions.Add("Human", Instance.humanPlayer.transform.position);
    }
}
