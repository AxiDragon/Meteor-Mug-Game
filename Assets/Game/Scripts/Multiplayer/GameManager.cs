using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool firstTransition = true;
    private int currentIndex = 0;
    private CinemachineTargetGroup targetGroup;
    private PlayerInputManager playerInputManager;
    private RoundObjectiveManager roundObjectiveManager;
    [SerializeField] private Transform playerParent;
    [SerializeField] private RectTransform title;
    [SerializeField] private RectTransform joinPrompt;
    [SerializeField] private RectTransform objective; //175, 400
    [SerializeField] private RectTransform playerScores;
    [SerializeField] private RectTransform screenWipe; //90, 270 (rotation)
    [SerializeField] private float tweenTime = 1f;
    [SerializeField] private Ease easeType;
    [SerializeField] private Color[] playerColors;

    private void Awake()
    {
        targetGroup = FindObjectOfType<CinemachineTargetGroup>();
        playerInputManager = GetComponent<PlayerInputManager>();
        roundObjectiveManager = GetComponent<RoundObjectiveManager>();
        DontDestroyOnLoad(transform.parent.gameObject);
    }

    public void OnPlayerJoined()
    {
        Transform newPlayerTransform = null;

        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            bool alreadyAccountedFor = false;

            for (var i = 0; i < targetGroup.m_Targets.Length; i++)
            {
                if (player.transform == targetGroup.m_Targets[i].target)
                    alreadyAccountedFor = true;
            }

            if (alreadyAccountedFor)
                continue;

            newPlayerTransform = player.transform;
            newPlayerTransform.GetComponent<FlockController>().FlockColor =
                playerColors[playerInputManager.playerCount - 1];
        }

        if (newPlayerTransform != null)
        {
            targetGroup.m_Targets[currentIndex].target = newPlayerTransform;
            currentIndex++;
            newPlayerTransform.parent = playerParent;
        }
    }

    public void TransitionToNewLevel(bool canPlayerJoin = false)
    {
        if (canPlayerJoin)
        {
            playerInputManager.EnableJoining();
        }
        else
        {
            playerInputManager.DisableJoining();
        }

        screenWipe.DOAnchorPosX(1500f, tweenTime).SetEase(Ease.InOutSine)
            .OnComplete(() => StartCoroutine(TransitionToNewLevelCoroutine(firstTransition)));
    }

    IEnumerator TransitionToNewLevelCoroutine(bool transition)
    {
        foreach (var flockController in FindObjectsOfType<FlockController>())
        {
            flockController.ReleaseFlock();
        }

        foreach (var cc in FindObjectsOfType<ChickController>())
        {
            Destroy(cc.gameObject);
        }

        int newLevel = Random.Range(1, SceneManager.sceneCountInBuildSettings);
        var levelLoading = SceneManager.LoadSceneAsync(newLevel);
        while (!levelLoading.isDone)
        {
            yield return null;
        }

        screenWipe.DOAnchorPosX(3960f, tweenTime).SetEase(Ease.InOutSine);

        if (firstTransition)
        {
            //tween title
            title.DOAnchorPosX(-500f, tweenTime).SetEase(easeType);
            //tween join prompt
            joinPrompt.DOAnchorPosX(-1000f, tweenTime).SetEase(easeType);
            //tween objective
            objective.DOAnchorPosY(200f, tweenTime).SetEase(easeType);
            //tween player score
            playerScores.DOAnchorPosY(-225f, tweenTime).SetEase(easeType);
        }

        SetUpPlayers();
        UpdateScoreRequirement();

        firstTransition = false;

        foreach (var flockController in FindObjectsOfType<FlockController>())
        {
            flockController.ReleaseFlock();
        }
    }

    private void SetUpPlayers()
    {
        Transform spawnPositions = FindObjectOfType<SpawnPositions>().transform;

        List<PlayerScoreManager> players = FindObjectsOfType<PlayerScoreManager>().ToList();
        players.OrderByDescending(player => player.roundsWon);

        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.position = spawnPositions.GetChild(i).position;
            players[i].GetComponent<ChickThrower>().canThrow = true;
        }
    }

    private void UpdateScoreRequirement()
    {
        int p = playerInputManager.playerCount;
        roundObjectiveManager.ScoringTarget = FindObjectOfType<LevelObjectiveCount>().objectiveCount[p - 1];
        roundObjectiveManager.roundWon = false;
    }
}