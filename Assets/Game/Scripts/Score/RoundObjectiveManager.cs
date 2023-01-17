using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using TMPro;
using UnityEngine;
using UnityTimer;

public class RoundObjectiveManager : MonoBehaviour
{
    private FlockController[] flockControllers = new [] { (FlockController)null };
    private GameManager gameManager;
    public bool roundWon = false;
    private int scoringTarget = 10;
    [SerializeField] private TMP_Text objectiveText;

    public int ScoringTarget
    {
        get => scoringTarget;
        set
        {
            scoringTarget = value; 
            objectiveText.text = ScoringTarget.ToString();
        }
    }

    private void Awake()
    {
        gameManager = GetComponent<GameManager>();
    }

    public void OnPlayerJoined()
    {
        RefreshFlockControllerArray();
    }
    
    public void OnPlayerLeft()
    {
        RefreshFlockControllerArray();
    }
    
    private void RefreshFlockControllerArray()
    {
        for (int i = 0; i < flockControllers.Length; i++)
        {
            if (flockControllers[i] != null) flockControllers[i].flock.CollectionChanged -= CheckScoring;
        }

        flockControllers = FindObjectsOfType<FlockController>();
        
        for (int i = 0; i < flockControllers.Length; i++)
        {
            if (flockControllers != null) flockControllers[i].flock.CollectionChanged += CheckScoring;
        }
    }

    private void CheckScoring(object sender, NotifyCollectionChangedEventArgs e)
    {
        for (int i = 0; i < flockControllers.Length; i++)
        {
            if (flockControllers[i].flock.Count >= ScoringTarget && !roundWon)
            {
                print(flockControllers[i].gameObject.name + "Has won!");
                flockControllers[i].GetComponent<PlayerScoreManager>().roundsWon++;
                roundWon = true;
                Timer.Register(.0001f, () => gameManager.TransitionToNewLevel());
            }
        }
    }
}
