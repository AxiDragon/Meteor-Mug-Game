using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameStartManager : MonoBehaviour
{
    private int playerCount = 0;
    private int playersInsideOfCollider = 0;
    [SerializeField] private float countdownTime = 3f;
    [Range(0,4)]
    [SerializeField] private int requiredPlayersForStart = 2;
    private float timer;
    private bool timing;
    private bool gameStarted;
    public UnityEvent onGameStart;
    [SerializeField] private TextMeshPro playerCountText;
    [SerializeField] private TextMeshPro timerText;

    private void Start()
    {
        timer = countdownTime;
    }

    private void Update()
    {
        if (timing)
        {
            timer = Mathf.Max(0f, timer - Time.unscaledDeltaTime);
            timerText.text = Mathf.Ceil(timer).ToString("F0");

            if (timer == 0f && !gameStarted)
            {
                onGameStart?.Invoke();
                gameStarted = true;
            }
                
        }
    }

    private void UpdatePlayerReadiness()
    {
        playerCountText.text =
            playerCount < requiredPlayersForStart ? $"Need at least {requiredPlayersForStart} players!" : $"{playersInsideOfCollider} out of {playerCount}";

        if (AllPlayersReady())
        {
            timer = countdownTime;
            timing = true;
        }
        else
        {
            timing = false;
            timerText.text = "";
        }
    }

    private bool AllPlayersReady()
    {
        if (playerCount < requiredPlayersForStart)
            return false;

        return playersInsideOfCollider == playerCount;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            playersInsideOfCollider++;
            UpdatePlayerReadiness();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInsideOfCollider--;
            UpdatePlayerReadiness();
        }
    }

    public void OnPlayerJoined()
    {
        playerCount++;
        UpdatePlayerReadiness();
    }

    public void OnPlayerLeft()
    {
        playerCount--;
    }
}