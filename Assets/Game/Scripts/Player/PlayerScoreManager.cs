using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreManager : MonoBehaviour
{
    [SerializeField] private Image playerScoreUIPrefab;
    [SerializeField] private Image playerTransitionScoreUIPrefab;
    [SerializeField] private float positiveShakeIntensity = 20;
    [SerializeField] private float negativeShakeIntensity = 10;
    [SerializeField] private string playerScoreManagerTag;
    [SerializeField] private string playerTransitionScoreManagerTag;
    [SerializeField] private Color backgroundColorMultiply;
    [SerializeField] private Color scoreCircleColorMultiply;
    [SerializeField] private GameObject crown;

    [HideInInspector] public int roundsWon;
    private int currentScore;
    private FlockController flockController;
    private Image playerScoreUI;
    private Image playerTransitionScoreUI;
    private ScoreCircle[] scoreCircles;
    private TextMeshProUGUI scoreText;
    private MMPositionShaker shaker;

    private void Awake()
    {
        flockController = GetComponent<FlockController>();
    }

    private void Start()
    {
        playerScoreUI = Instantiate(playerScoreUIPrefab, GameObject.FindWithTag(playerScoreManagerTag).transform);
        scoreText = playerScoreUI.GetComponentInChildren<TextMeshProUGUI>();
        shaker = scoreText.GetComponent<MMPositionShaker>();
        playerScoreUI.color = flockController.FlockColor;

        playerTransitionScoreUI = Instantiate(playerTransitionScoreUIPrefab,
            GameObject.FindWithTag(playerTransitionScoreManagerTag).transform);
        playerTransitionScoreUI.color = flockController.FlockColor * backgroundColorMultiply;

        scoreCircles = playerTransitionScoreUI.GetComponentsInChildren<ScoreCircle>();

        ResetScoreCircles();
    }

    private void OnEnable()
    {
        flockController.onFlockChanged += UpdateScore;
    }

    private void OnDisable()
    {
        flockController.onFlockChanged -= UpdateScore;
    }

    public void ResetScoreCircles()
    {
        for (var i = 0; i < scoreCircles.Length; i++)
            scoreCircles[i].AssignStartingColor(flockController.FlockColor * scoreCircleColorMultiply);
    }

    private void UpdateScore(int score)
    {
        shaker.ShakeRange = currentScore < score ? positiveShakeIntensity : negativeShakeIntensity;
        currentScore = score;
        scoreText.text = score.ToString();
        shaker.Play();
    }

    public void RoundWon()
    {
        roundsWon++;

        playerTransitionScoreUI.GetComponent<MMRotationShaker>().Play();
        playerTransitionScoreUI.GetComponent<MMPositionShaker>().Play();
        scoreCircles[roundsWon - 1].TriggerScoring(flockController.FlockColor);
    }

    public void ToggleCrown(bool toggle = false)
    {
        crown.SetActive(toggle);
    }
}