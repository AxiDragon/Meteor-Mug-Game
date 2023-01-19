using System.Collections.Specialized;
using TMPro;
using UnityEngine;

public class RoundObjectiveManager : MonoBehaviour
{
    public bool roundWon;
    [SerializeField] private TMP_Text objectiveText;
    private FlockController[] flockControllers = { (FlockController)null };
    private GameManager gameManager;
    private int scoringTarget = 10;

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
        for (var i = 0; i < flockControllers.Length; i++)
            if (flockControllers[i] != null)
                flockControllers[i].flock.CollectionChanged -= CheckScoring;

        flockControllers = FindObjectsOfType<FlockController>();

        for (var i = 0; i < flockControllers.Length; i++)
            if (flockControllers != null)
                flockControllers[i].flock.CollectionChanged += CheckScoring;
    }

    private void CheckScoring(object sender, NotifyCollectionChangedEventArgs e)
    {
        for (var i = 0; i < flockControllers.Length; i++)
            if (flockControllers[i].flock.Count >= ScoringTarget && !roundWon)
            {
                var winner = flockControllers[i].GetComponent<PlayerScoreManager>();
                roundWon = true;
                gameManager.TransitionToNewLevel(roundWinner: winner);
            }
    }
}