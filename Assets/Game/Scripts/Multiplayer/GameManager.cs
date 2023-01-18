using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityTimer;

public class GameManager : MonoBehaviour
{
    private bool gameStartTransition = true;
    private int currentIndex = 0;
    private int lastLevelScene = 0;
    private CinemachineTargetGroup targetGroup;
    private PlayerInputManager playerInputManager;
    private RoundObjectiveManager roundObjectiveManager;
    [SerializeField] private ParticleSystem playerWinParticleSystemPrefab;
    private ParticleSystem playerWinParticleSystem;
    [SerializeField] private Transform playerParent;
    [SerializeField] private RectTransform title; //0, 700
    [SerializeField] private RectTransform objective;
    [SerializeField] private RectTransform playerScores;
    [SerializeField] private RectTransform screenWipe; //90, 270 (rotation)
    [SerializeField] private float tweenTime = 1f;
    [SerializeField] private Ease easeType;
    [SerializeField] private Color[] playerColors;
    [SerializeField] private MMF_Player roundWonFeedback;
    [SerializeField] private int requiredScoreToWin = 3;

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

        if (FindObjectOfType<GameStartManager>())
        {
            FindObjectOfType<GameStartManager>().OnPlayerJoined();
        }
    }

    public void OnPlayerLeft()
    {
        if (FindObjectOfType<GameStartManager>())
        {
            FindObjectOfType<GameStartManager>().OnPlayerLeft();
        }
    }

    public void TransitionFromLobby()
    {
        TransitionToNewLevel();
    }

    public void TransitionToNewLevel(bool canPlayerJoin = false, PlayerScoreManager roundWinner = null)
    {
        if (canPlayerJoin)
        {
            playerInputManager.EnableJoining();
        }
        else
        {
            playerInputManager.DisableJoining();
        }

        float fadeDelay = 0f;

        if (roundWinner != null)
        {
            FocusOnWinner(roundWinner.transform);
            fadeDelay = 1.5f;
            roundWonFeedback.PlayFeedbacks();
        }

        Timer.Register(fadeDelay, () => screenWipe.DOAnchorPosX(0f, tweenTime).SetEase(Ease.InOutSine).SetUpdate(true)
                .OnComplete(() => StartCoroutine(TransitionToNewLevelCoroutine(gameStartTransition, roundWinner))),
            useRealTime: true);
    }

    private void FocusOnWinner(Transform roundWinnerTransform)
    {
        for (int i = 0; i < targetGroup.m_Targets.Length; i++)
        {
            if (i == 0)
                targetGroup.m_Targets[i].target = roundWinnerTransform;
            else
                targetGroup.m_Targets[i].target = null;
        }
    }

    IEnumerator TransitionToNewLevelCoroutine(bool transition, PlayerScoreManager roundWinner = null)
    {
        yield return new WaitForEndOfFrame();

        foreach (var flockController in FindObjectsOfType<FlockController>())
        {
            flockController.ReleaseFlock();
        }

        foreach (var cc in FindObjectsOfType<ChickController>())
        {
            Destroy(cc.gameObject);
        }


        int newLevel = Random.Range(1, SceneManager.sceneCountInBuildSettings - 1);

        while (newLevel == lastLevelScene)
        {
            newLevel = Random.Range(1, SceneManager.sceneCountInBuildSettings - 1);
        }

        lastLevelScene = newLevel;

        bool gameWon = false;

        if (roundWinner != null)
        {
            roundWinner.RoundWon();
            if (SetCrownHolder(out PlayerScoreManager winner))
            {
                ClearPlayerPowerUps();
                gameWon = true;
                playerWinParticleSystem = Instantiate(playerWinParticleSystemPrefab, winner.transform);
                newLevel = SceneManager.sceneCountInBuildSettings - 1;
            }
        }

        if (gameStartTransition)
        {
            SetCrownHolder(out _);
            
            if (playerWinParticleSystem)
                Destroy(playerWinParticleSystem);
        }

        yield return new WaitForSeconds(1.5f);

        var levelLoading = SceneManager.LoadSceneAsync(newLevel);
        while (!levelLoading.isDone)
        {
            yield return null;
        }

        screenWipe.DOAnchorPosX(2000f, tweenTime).SetEase(Ease.InOutSine).SetUpdate(true);

        yield return new WaitForSeconds(.5f);


        if (gameStartTransition)
        {
            //tween title
            title.DOAnchorPosX(-700f, tweenTime).SetEase(easeType);
            //tween objective
            objective.DOAnchorPosY(200f, tweenTime).SetEase(easeType);
            //tween player score
            playerScores.DOAnchorPosY(-225f, tweenTime).SetEase(easeType);
            
            gameStartTransition = false;
        }

        SetUpPlayers();
        if (gameWon)
        {
            gameStartTransition = true;
            playerInputManager.EnableJoining();
            ResetPlayerScores();

            title.DOAnchorPosX(0f, tweenTime).SetEase(easeType);

            objective.DOAnchorPosY(400f, tweenTime).SetEase(easeType);

            playerScores.DOAnchorPosY(-435f, tweenTime).SetEase(easeType);
        }
        else
        {
            UpdateScoreRequirement();
        }

        foreach (var flockController in FindObjectsOfType<FlockController>())
        {
            flockController.ReleaseFlock();
        }
    }

    private void ClearPlayerPowerUps()
    {
        foreach (PlayerScoreManager playerScoreManager in FindObjectsOfType<PlayerScoreManager>())
        {
            foreach (PowerUp powerUp in playerScoreManager.GetComponents<PowerUp>())
            {
                Destroy(powerUp);
            }
        }
    }

    private void ResetPlayerScores()
    {
        foreach (PlayerScoreManager playerScoreManager in FindObjectsOfType<PlayerScoreManager>())
        {
            playerScoreManager.ResetScoreCircles();
            playerScoreManager.roundsWon = 0;
        }
    }

    private bool SetCrownHolder(out PlayerScoreManager leader)
    {
        int highestScore = 0;
        PlayerScoreManager[] playerScoreManagers = FindObjectsOfType<PlayerScoreManager>();

        foreach (PlayerScoreManager playerScoreManager in playerScoreManagers)
        {
            highestScore = Mathf.Max(playerScoreManager.roundsWon, highestScore);
            playerScoreManager.ToggleCrown(false);
        }

        bool tied = false;
        leader = null;

        foreach (PlayerScoreManager playerScoreManager in playerScoreManagers)
        {
            if (highestScore == playerScoreManager.roundsWon)
            {
                if (leader != null)
                {
                    tied = true;
                    break;
                }

                leader = playerScoreManager;
            }
        }

        if (!tied)
            leader.ToggleCrown(true);

        return highestScore == requiredScoreToWin;
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
            targetGroup.m_Targets[i].target = players[i].transform;
        }
    }

    private void UpdateScoreRequirement()
    {
        int p = playerInputManager.playerCount;
        roundObjectiveManager.ScoringTarget = FindObjectOfType<LevelObjectiveCount>().objectiveCount[p - 1];
        roundObjectiveManager.roundWon = false;
    }
}