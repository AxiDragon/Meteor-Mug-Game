using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using TMPro;
using UnityEngine;

public class RoundObjectiveManager : MonoBehaviour
{
    private FlockController[] flockControllers = new [] { (FlockController)null };
    private bool roundWon = false;
    public int scoringTarget = 10;
    [SerializeField] private TMP_Text objectiveText;

    private void Awake()
    {
        objectiveText.text = scoringTarget.ToString();
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
            if (flockControllers[i].flock.Count >= scoringTarget && !roundWon)
            {
                print(flockControllers[i].gameObject.name + "Has won!");
                roundWon = true;
            }
        }
    }
}
