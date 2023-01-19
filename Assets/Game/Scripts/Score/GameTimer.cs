using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameTimer : MonoBehaviour
{
    public float timer;
    public bool timing;
    public UnityEvent onTimerCompleted;
    private TMP_Text timerText;

    private void Awake()
    {
        timerText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (timing)
        {
            timer = Mathf.Max(0f, timer - Time.unscaledDeltaTime);
            timerText.text = timer.ToString("F0") + "s";

            if (timer == 0f)
                onTimerCompleted?.Invoke();
        }
    }
}