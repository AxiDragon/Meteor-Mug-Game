using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCircle : MonoBehaviour
{
    private bool triggered = false;
    private Image image;
    private Color startingColor;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    [ContextMenu("Trigger Circle")]
    public void TriggerTest()
    {
        TriggerScoring(Color.blue);
    }

    public void TriggerScoring(Color color)
    {
        if (triggered)
            return;

        triggered = true;

        GetComponent<MMPositionShaker>().Play();
        GetComponent<MMScaleShaker>().Play();
        image.color = color;
    }

    public void AssignStartingColor(Color color)
    {
        startingColor = color;
        image.color = color;
        triggered = false;
    }
}
