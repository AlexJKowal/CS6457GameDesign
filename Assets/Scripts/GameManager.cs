using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using DefaultNamespace;
using JetBrains.Annotations;
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

    private List<GameObject> balls = new List<GameObject>();
    private List<GameObject> s2Balls = new List<GameObject>();
    private List<GameObject> s3Balls = new List<GameObject>();
    private List<GameObject> s4Balls = new List<GameObject>();
    public int MAX_BALLS = 3;

    public static bool multiBallSetting;

    public GameObject ball;
    

    private int MAX_LEVEL = 3;
    private int LEVEL_UP_WINS = 3;
    private int MAX_LIVES = 3;
    
    public int currentLevel = 1;
    public int playerScore = 0;
    public int playerLives;
    
    private readonly object _lockObject = new object();
    private readonly object _lossCheckLockObject = new object();
    private float lastInstantiationTime = 0f;
    private readonly float instantiationCooldown = 1f;
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
        Instance.levelIndicator.SetText("Level " + Instance.currentLevel);
        Instance.playerLives = Instance.MAX_LIVES;
        balls.Add(ball);
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

    public static void AddBalls()
    {
        lock (Instance._lockObject)
        {
            if (Time.time - Instance.lastInstantiationTime > Instance.instantiationCooldown && Instance.balls.Count < Instance.MAX_BALLS)
            {
                String currentBallTarget = Instance.ball.GetComponent<BallThrowing>().targetSquare.tag;
                GameObject newBallLeft = Instantiate(Instance.ball, Instance.ball.transform.position,
                    Instance.ball.transform.rotation);
                GameObject newBallRight = Instantiate(Instance.ball, Instance.ball.transform.position,
                           Instance.ball.transform.rotation);
                GameObject targetLeft;
                GameObject targetRight;
                newBallLeft.tag = "CopyBall";
                newBallRight.tag = "CopyBall";
                Instance.balls.Add(newBallLeft);
                Instance.balls.Add(newBallRight);
                Instance.lastInstantiationTime = Time.time;
                BallThrowing btLeft = newBallLeft.GetComponent<BallThrowing>();
                BallThrowing btRight = newBallRight.GetComponent<BallThrowing>();
                btLeft.SetValues();
                btRight.SetValues();
                if (currentBallTarget == "Square2")
                {
                    targetLeft = Instance.square3;
                    targetRight = Instance.square4;
                }
                else if (currentBallTarget == "Square3")
                {
                    targetLeft = Instance.square2;
                    targetRight = Instance.square4;
                }
                else 
                {
                    targetLeft = Instance.square2;
                    targetRight = Instance.square3;
                }
                btLeft.ShotTheBallToTargetSquare(Instance.square1, targetLeft, 0.8f, null);
                btRight.ShotTheBallToTargetSquare(Instance.square1, targetRight, 0.8f, null);
                
            }
        }
    }

    public static void DestroyBall(GameObject ball)
    {
        lock (Instance._lockObject)
        {
            Instance.balls.Remove(ball);
            SortBallsInSquares();
            AssignAiBalls(ball);
            Destroy(ball);
        }
    }

    public static void UpdateWinLose(GameObject losePlayer)
    {
        lock (Instance._lossCheckLockObject)
        {
            bool playerWin;
            // human player lose the game
            if (ReferenceEquals(losePlayer, Instance.humanPlayer))
            {
                playerWin = false;
                Instance.playerLives--;

                if (Instance.playerLives == 0)
                {
                    // Load Lose Scene
                   Instance.StartCoroutine(EndLosingGame());
                    return;
                }

            }
            else // human player win the game
            {
                playerWin = true;
                Instance.playerScore++;
                if (Instance.playerScore >= Instance.LEVEL_UP_WINS)
                {
                    // load next level
                      Instance.StartCoroutine(LevelUp());
                      return;
                }
            }

            Instance.StartCoroutine(ShowWinLoseTransitions(playerWin));
        }
    }

    // End the game after 3 loses
    static IEnumerator EndLosingGame()
    {
        multiBallSetting = false;
        float waitSeconds = 2f;
        
        Instance.gameStatus.GetComponent<AnimateText>().ShowText("You Lose the Game.", waitSeconds);
        yield return new WaitForSeconds(waitSeconds + 1);
        
        SceneManager.LoadScene("Scenes/LoseScene");
    }

    static void LevelConfigReset()
    {
        Instance.playerScore = 0;
        Instance.playerLives = Instance.MAX_LIVES;
        Instance.currentLevel++;
        DestroyCopyBalls();

    }

    static IEnumerator LevelUp()
    {
        multiBallSetting = false;
        SetPlayersState(PlayerState.Idle);
        
        // Reset the game (Avoid ball bouncing again after win or lose
        Instance.humanPlayer.GetComponent<PlayerController>().ResetStates();
        
        // Show animated win/lose text
        float waitSeconds = 2f;
        
        LevelConfigReset();
        if (Instance.currentLevel > Instance.MAX_LEVEL){
            Instance.gameStatus.GetComponent<AnimateText>().ShowText("You Win!", waitSeconds);
            yield return new WaitForSeconds(waitSeconds + 1);
            
            SceneManager.LoadScene("VictoryScreen");
            Instance.currentLevel = 1;
        }
        else
        {
            Instance.gameStatus.GetComponent<AnimateText>().ShowText("Level Up!", waitSeconds);
            yield return new WaitForSeconds(waitSeconds + 1);
            
            EventManager.TriggerEvent<CheeringEvent, Vector3>(Instance.confettiSystem.transform.position);
            Instance.confettiSystem.SetActive(true);
            
            yield return new WaitForSeconds(2f);
            
            if (Instance.currentLevel == 2)
            {
                SceneManager.LoadScene("Scenes/LevelTwo");    
            } else if (Instance.currentLevel == 3) {
                SceneManager.LoadScene("Scenes/LevelThree");   
            } 
        }
    }

    static void DestroyCopyBalls()
    {
        lock (Instance._lockObject)
        {
            Instance.s2Balls.Clear();
            Instance.s3Balls.Clear();
            Instance.s4Balls.Clear();
            for (int i = Instance.balls.Count - 1; i >= 0; i--)
            {
                GameObject ball = Instance.balls[i];
                if (ball != null && ball.CompareTag("CopyBall"))
                {
                    Instance.balls.RemoveAt(i);
                    Destroy(ball);
                }
            }
            
        }

    }
    
    static IEnumerator ShowWinLoseTransitions(bool win)
    {
       
        SetPlayersState(PlayerState.Idle);
        DestroyCopyBalls();
        String message = win ? "You Score!" : 
            "Careful, " + (Instance.playerLives) + " lives left!";
        // Update player score
        Instance.scoreBoard.GetComponent<ScoreboardNumSetManager>().SetScore(Instance.playerScore);
        
        // Reset the game (Avoid ball bouncing again after win or lose
        Instance.humanPlayer.GetComponent<PlayerController>().ResetStates();
        
        // Show animated win/lose text
        float waitSeconds = 2f;
        Instance.gameStatus.GetComponent<AnimateText>().ShowText(message, waitSeconds);
        yield return new WaitForSeconds(waitSeconds + 1);
        
        // Show count down
        int countDown = 3;
        Instance.countDownCanvas.GetComponent<CountDownController>().ShowCountDown(countDown);
        yield return new WaitForSeconds(countDown);
        
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

    public static void SortBallsInSquares()
    {
        Instance.s2Balls.Clear();
        Instance.s3Balls.Clear();
        Instance.s4Balls.Clear();
        int count = 0;
        foreach (GameObject ball in Instance.balls)
        {
            if (ball == null)
            {
                continue;
            }
            if (ball.GetComponent<BallThrowing>().targetSquare != null)
            {
                
                if (ball.GetComponent<BallThrowing>().targetSquare.tag == "Square2")
                {
                    Instance.s2Balls.Add(ball);
                }
                else if (ball.GetComponent<BallThrowing>().targetSquare.tag == "Square3")
                {
                    Instance.s3Balls.Add(ball);
                }
                else if (ball.GetComponent<BallThrowing>().targetSquare.tag == "Square4")
                {
                    Instance.s4Balls.Add(ball);
                }
            }
        }
        
        foreach(List<GameObject> ballList in new List<List<GameObject>>
                {
                    Instance.s2Balls, Instance.s3Balls, Instance.s4Balls
                })
        {
            ballList.Sort((a, b) =>
            {
                int bounceCompare = b.GetComponent<BallThrowing>()._bounced.CompareTo(a.GetComponent<BallThrowing>()._bounced);
                return bounceCompare;
            });
        }
    }

    public static void AssignAiBalls([CanBeNull] GameObject removedBall)
    {
        Dictionary<GameObject, List<GameObject>> aiPlayerMappings = new Dictionary<GameObject, List<GameObject>>
        {
            { Instance.aiPlayer1, Instance.s2Balls },
            { Instance.aiPlayer2, Instance.s3Balls },
            { Instance.aiPlayer3, Instance.s4Balls }
        };

        foreach (KeyValuePair<GameObject, List<GameObject>> pair in aiPlayerMappings)
        {
            AIPlayerController AIController = pair.Key.GetComponent<AIPlayerController>();
            if (pair.Value.Count > 0)
            {
                GameObject TopBallObject = pair.Value[0];
                BallThrowing TopBallClass = TopBallObject.GetComponent<BallThrowing>();
              
                if (AIController.ball == removedBall || TopBallClass != null && AIController.ball.GetComponent<BallThrowing>()._bounced <=
                    TopBallClass._bounced)
                {
                    if (TopBallClass != null)
                    {
                        AIController.ball = TopBallObject;
                        AIController.ballRbody = TopBallObject.GetComponent<Rigidbody>();
                    }
                    else
                    {
                        AIController.ball = Instance.ball;
                        AIController.ballRbody = Instance.ball.GetComponent<Rigidbody>();
                    }
                }
            }
        }
    }

    public static void BallShotToNewTarget(GameObject ball, GameObject targetSquare)
    {
        SortBallsInSquares();
        AssignAiBalls(null);
    }
}
