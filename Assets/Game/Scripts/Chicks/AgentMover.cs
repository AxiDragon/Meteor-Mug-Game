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
        if (target == null || !navMeshAgent.isOnNavMesh)
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