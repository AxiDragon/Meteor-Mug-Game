using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameTimer : MonoBehaviour
{
    public float timer = 0f;
    public bool timing = false;
    public UnityEvent onTimerCompleted;
    private TMP_Text timerText;
    
    void Awake()
    {
        timerText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (timing)
        {
            timer = Mathf.Max(0f, timer - Time.deltaTime);
            timerText.text = timer.ToString("F0") + "s";
            
            if (timer == 0f)
                onTimerCompleted?.Invoke();
        }
    }
}
