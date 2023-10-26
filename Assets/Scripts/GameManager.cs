using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

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
        // Assigning players to squares
        Instance.humanPlayer.GetComponent<PlayerController>().homeSquare = Instance.square1;
        Instance.AI_Player1.GetComponent<AIPlayerController>().homeSquare = Instance.square2;
        Instance.AI_Player2.GetComponent<AIPlayerController>().homeSquare = Instance.square3;
        Instance.AI_Player3.GetComponent<AIPlayerController>().homeSquare = Instance.square4;
    }

    public static void ResetGame()
    {
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

    // Can be square or player
    public static void updateGameResult(GameObject playerOrSquare)
    {
        GameObject square = playerOrSquare;
        if (playerOrSquare.tag.Contains("Player"))
        {
            if (GameObject.ReferenceEquals(playerOrSquare, Instance.humanPlayer))
            {
                square = playerOrSquare.GetComponent<PlayerController>().homeSquare;
            }
            else
            {
                square = playerOrSquare.GetComponent<AIPlayerController>().homeSquare; 
            }
        }
        updateGameStatus("Player on " + square.tag + " is lost!!!!!!!");
    }
}
