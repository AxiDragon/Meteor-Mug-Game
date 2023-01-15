using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreManager : MonoBehaviour
{
    private FlockController flockController;
    [SerializeField] private Image playerScoreUIPrefab;
    private Image playerScoreUI;
    private TextMeshProUGUI scoreText;
    private MMPositionShaker shaker;
    [SerializeField] private float positiveShakeIntensity = 20;
    [SerializeField] private float negativeShakeIntensity = 10;
    private int currentScore = 0;
    [SerializeField] private string playerScoreManagerTag;

    void Awake()
    {
        flockController = GetComponent<FlockController>();
    }

    private void Start()
    {
        playerScoreUI = Instantiate(playerScoreUIPrefab, GameObject.FindWithTag(playerScoreManagerTag).transform);
        scoreText = playerScoreUI.GetComponentInChildren<TextMeshProUGUI>();
        shaker = scoreText.GetComponent<MMPositionShaker>();

        playerScoreUI.color = flockController.FlockColor;
    }

    private void OnEnable()
    {
        flockController.onFlockChanged += UpdateScore;
    }

    private void OnDisable()
    {
        flockController.onFlockChanged -= UpdateScore;
    }

    private void UpdateScore(int score)
    {
        shaker.ShakeRange = currentScore < score ? positiveShakeIntensity : negativeShakeIntensity;
        currentScore = score;
        scoreText.text = score.ToString();
        shaker.Play();
    }
}