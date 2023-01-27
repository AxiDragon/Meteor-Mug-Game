using System.Collections;
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
    [SerializeField] private ParticleSystem playerWinParticleSystemPrefab;
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
    private int currentIndex;
    private bool gameStartTransition = true;
    private int lastLevelScene;
    private PlayerInputManager playerInputManager;
    private ParticleSystem playerWinParticleSystem;
    private RoundObjectiveManager roundObjectiveManager;
    private CinemachineTargetGroup targetGroup;

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
            var alreadyAccountedFor = false;

            for (var i = 0; i < targetGroup.m_Targets.Length; i++)
                if (player.transform == targetGroup.m_Targets[i].target)
                    alreadyAccountedFor = true;

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

        if (FindObjectOfType<GameStartManager>()) FindObjectOfType<GameStartManager>().OnPlayerJoined();
    }

    public void OnPlayerLeft()
    {
        if (FindObjectOfType<GameStartManager>()) FindObjectOfType<GameStartManager>().OnPlayerLeft();
    }

    public void TransitionFromLobby()
    {
        TransitionToNewLevel();
    }

    public void TransitionToNewLevel(bool canPlayerJoin = false, PlayerScoreManager roundWinner = null)
    {
        if (canPlayerJoin)
            playerInputManager.EnableJoining();
        else
            playerInputManager.DisableJoining();

        var fadeDelay = 0f;

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
        for (var i = 0; i < targetGroup.m_Targets.Length; i++)
            if (i == 0)
                targetGroup.m_Targets[i].target = roundWinnerTransform;
            else
                targetGroup.m_Targets[i].target = null;
    }

    private IEnumerator TransitionToNewLevelCoroutine(bool transition, PlayerScoreManager roundWinner = null)
    {
        yield return new WaitForEndOfFrame();

        foreach (var flockController in FindObjectsOfType<FlockController>()) flockController.ReleaseFlock();

        foreach (var cc in FindObjectsOfType<ChickController>()) Destroy(cc.gameObject);


        var newLevel = Random.Range(1, SceneManager.sceneCountInBuildSettings - 1);

        while (newLevel == lastLevelScene) newLevel = Random.Range(1, SceneManager.sceneCountInBuildSettings - 1);

        lastLevelScene = newLevel;

        var gameWon = false;

        if (roundWinner != null)
        {
            roundWinner.RoundWon();
            if (SetCrownHolder(out var winner))
            {
                ClearPlayerPowerUps();
                gameWon = true;
                playerWinParticleSystem = Instantiate(playerWinParticleSystemPrefab, winner.transform);
                var sm = FindObjectOfType<SoundManager>();
                sm.ToggleGameSoundtrack(false);
                sm.ToggleLobbySoundtrack(true);
                newLevel = SceneManager.sceneCountInBuildSettings - 1;
            }
        }

        if (gameStartTransition)
        {
            SetCrownHolder(out _);
            
            var sm = FindObjectOfType<SoundManager>();
            sm.ToggleGameSoundtrack(true);
            sm.ToggleLobbySoundtrack(false);

            if (playerWinParticleSystem)
                Destroy(playerWinParticleSystem);
        }

        yield return new WaitForSeconds(1.5f);

        var levelLoading = SceneManager.LoadSceneAsync(newLevel);
        while (!levelLoading.isDone) yield return null;

        screenWipe.DOAnchorPosX(2000f, tweenTime).SetEase(Ease.InOutSine).SetUpdate(true);

        SetUpPlayers();
        if (!gameWon)
            UpdateScoreRequirement();

        foreach (var flockController in FindObjectsOfType<FlockController>()) flockController.ReleaseFlock();

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

        if (gameWon)
        {
            gameStartTransition = true;
            playerInputManager.EnableJoining();
            ResetPlayerScores();

            title.DOAnchorPosX(0f, tweenTime).SetEase(easeType);

            objective.DOAnchorPosY(400f, tweenTime).SetEase(easeType);

            playerScores.DOAnchorPosY(-435f, tweenTime).SetEase(easeType);
        }
    }

    private void ClearPlayerPowerUps()
    {
        foreach (var playerScoreManager in FindObjectsOfType<PlayerScoreManager>())
        foreach (var powerUp in playerScoreManager.GetComponents<PowerUp>())
            Destroy(powerUp);
    }

    private void ResetPlayerScores()
    {
        foreach (var playerScoreManager in FindObjectsOfType<PlayerScoreManager>())
        {
            playerScoreManager.ResetScoreCircles();
            playerScoreManager.roundsWon = 0;
        }
    }

    private bool SetCrownHolder(out PlayerScoreManager leader)
    {
        var highestScore = 0;
        var playerScoreManagers = FindObjectsOfType<PlayerScoreManager>();

        foreach (var playerScoreManager in playerScoreManagers)
        {
            highestScore = Mathf.Max(playerScoreManager.roundsWon, highestScore);
            playerScoreManager.ToggleCrown();
        }

        var tied = false;
        leader = null;

        foreach (var playerScoreManager in playerScoreManagers)
            if (highestScore == playerScoreManager.roundsWon)
            {
                if (leader != null)
                {
                    tied = true;
                    break;
                }

                leader = playerScoreManager;
            }

        if (!tied)
            leader.ToggleCrown(true);

        return highestScore == requiredScoreToWin;
    }

    private void SetUpPlayers()
    {
        var spawnPositions = FindObjectOfType<SpawnPositions>().transform;

        var players = FindObjectsOfType<PlayerScoreManager>().ToList();
        players.OrderBy(player => player.roundsWon);
        var positionModifier = 0;

        for (var i = 0; i < players.Count; i++)
        {
            players[i].transform.position =
                spawnPositions.GetChild(spawnPositions.childCount - 1 - (i + positionModifier)).position;
            players[i].GetComponent<ChickThrower>().canThrow = true;
            targetGroup.m_Targets[i].target = players[i].transform;

            if (i == 0)
                positionModifier += 4 - playerInputManager.playerCount;
        }
    }

    private void UpdateScoreRequirement()
    {
        var p = playerInputManager.playerCount;
        roundObjectiveManager.ScoringTarget = FindObjectOfType<LevelObjectiveCount>().objectiveCount[p - 1];
        roundObjectiveManager.roundWon = false;
    }
}