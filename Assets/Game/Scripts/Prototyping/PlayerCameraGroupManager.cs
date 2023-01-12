using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerCameraGroupManager : MonoBehaviour
{
    private CinemachineTargetGroup targetGroup;
    private int currentIndex = 1;

    private void Awake()
    {
        targetGroup = FindObjectOfType<CinemachineTargetGroup>();
    }

    public void OnPlayerJoined()
    {
        print("Trying");
        
        Transform newPlayerTransform = null;

        print(GameObject.FindGameObjectsWithTag("Player").Length);
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            for (int i = 0; i < targetGroup.m_Targets.Length; i++)
            {
                if (player.transform == targetGroup.m_Targets[i].target)
                    continue;
            }

            newPlayerTransform = player.transform;
        }

        if (newPlayerTransform != null)
        {
            targetGroup.m_Targets[currentIndex].target = newPlayerTransform;
            currentIndex++;
        }
    }
}