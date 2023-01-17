using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCircle : MonoBehaviour
{
    private bool triggered = false;
    [SerializeField] private float colorTransitionTime = 1f;

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
        GetComponent<Image>().DOColor(color, colorTransitionTime).SetEase(Ease.InQuint);
    }
}
