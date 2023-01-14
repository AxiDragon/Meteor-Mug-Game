using Cinemachine;
using UnityEngine;

public class PlayerCameraGroupManager : MonoBehaviour
{
    private int currentIndex = 0;
    private CinemachineTargetGroup targetGroup;
    [SerializeField] private Gradient playerColorGradient;

    private void Awake()
    {
        targetGroup = FindObjectOfType<CinemachineTargetGroup>();
    }

    public void OnPlayerJoined()
    {
        Transform newPlayerTransform = null;

        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            bool alreadyAccountedFor = false;
            
            for (var i = 0; i < targetGroup.m_Targets.Length; i++)
            {
                if (player.transform == targetGroup.m_Targets[i].target)
                    alreadyAccountedFor = true;
            }

            if (alreadyAccountedFor)
                continue;
            
            newPlayerTransform = player.transform;
            newPlayerTransform.GetComponent<FlockController>().FlockColor =
                playerColorGradient.Evaluate(Random.Range(0f, 1f));
        }

        if (newPlayerTransform != null)
        {
            targetGroup.m_Targets[currentIndex].target = newPlayerTransform;
            currentIndex++;
        }
    }
}