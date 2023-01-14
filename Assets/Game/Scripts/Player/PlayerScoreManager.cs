using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreManager : MonoBehaviour
{
    private FlockController flockController;
    [SerializeField] private Image playerScoreUIPrefab;
    private Image playerScoreUI;
    private TextMeshProUGUI scoreText;
    [SerializeField] private string playerScoreManagerTag;
    
    void Awake()
    {
        flockController = GetComponent<FlockController>();
    }

    private void Start()
    {
        playerScoreUI = Instantiate(playerScoreUIPrefab, GameObject.FindWithTag(playerScoreManagerTag).transform);
        scoreText = playerScoreUI.GetComponentInChildren<TextMeshProUGUI>();
        
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
        scoreText.text = score.ToString();
    }
}
