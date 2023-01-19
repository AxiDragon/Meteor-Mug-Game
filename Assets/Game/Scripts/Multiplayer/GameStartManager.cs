using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameStartManager : MonoBehaviour
{
    [SerializeField] private float countdownTime = 3f;

    [Range(0, 4)] [SerializeField] private int requiredPlayersForStart = 2;

    [SerializeField] private TextMeshPro playerCountText;
    [SerializeField] private TextMeshPro timerText;
    private bool gameStarted;
    private int playerCount;
    private int playersInsideOfCollider;
    private float timer;
    private bool timing;

    private void Start()
    {
        timer = countdownTime;
        var playerInputManager = FindObjectOfType<PlayerInputManager>();
        playerCount = playerInputManager.playerCount;
        UpdatePlayerReadiness();
    }

    private void Update()
    {
        if (timing)
        {
            timer = Mathf.Max(0f, timer - Time.unscaledDeltaTime);
            timerText.text = Mathf.Ceil(timer).ToString("F0");

            if (timer == 0f && !gameStarted)
            {
                FindObjectOfType<GameManager>().TransitionFromLobby();
                gameStarted = true;
            }
        }
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

    private void UpdatePlayerReadiness()
    {
        playerCountText.text =
            playerCount < requiredPlayersForStart
                ? $"Need at least {requiredPlayersForStart} players!"
                : $"{playersInsideOfCollider} out of {playerCount}";

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

    public void OnPlayerJoined()
    {
        playerCount++;
        UpdatePlayerReadiness();
    }

    public void OnPlayerLeft()
    {
        playerCount--;
        UpdatePlayerReadiness();
    }
}