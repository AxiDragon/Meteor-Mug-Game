using MoreMountains.Feedbacks;
using UnityEngine;

public class FeelTest : MonoBehaviour
{
    public MMFeedbacks shakeFeedback;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(20f, 20f, 200f, 50f), "Do Stuffs"))
        {
        }
    }
}