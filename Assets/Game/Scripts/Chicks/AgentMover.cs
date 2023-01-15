using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentMover : MonoBehaviour
{
    [HideInInspector] public NavMeshAgent navMeshAgent;
    public Transform target;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (target == null)
            return;

        navMeshAgent.SetDestination(target.position);
    }

    private void OnEnable()
    {
        navMeshAgent.enabled = true;
    }

    private void OnDisable()
    {
        navMeshAgent.enabled = false;
    }
}