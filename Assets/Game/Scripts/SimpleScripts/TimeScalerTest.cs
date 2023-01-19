using UnityEngine;

public class TimeScalerTest : MonoBehaviour
{
    public float timeScale = 1f;

    private void Update()
    {
        Time.timeScale = timeScale;
    }
}