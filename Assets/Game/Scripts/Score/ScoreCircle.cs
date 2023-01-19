using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCircle : MonoBehaviour
{
    private Image image;
    private Color startingColor;
    private bool triggered;

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