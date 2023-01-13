using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityTimer;

[RequireComponent(typeof(AgentMover), typeof(Rigidbody))]
public class ChickController : MonoBehaviour
{
    public enum ChickState
    {
        Idle,
        Following,
        Thrown,
    }

    public ChickState currentChickState = ChickState.Idle;
    [HideInInspector] public float flockTimeout;
    [SerializeField] private float postThrowFlockTimeout = 2f;
    public Renderer colorRenderer;
    [HideInInspector] public Color defaultColor;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public AgentMover agentMover;
    private Transform followTarget;

    public ChickState CurrentChickState
    {
        get => currentChickState;
        set
        {
            currentChickState = value;
            switch (currentChickState)
            {
                case ChickState.Thrown:
                    flockTimeout = Mathf.Infinity;
                    break;
            }
        }
    }

    private void Awake()
    {
        agentMover = GetComponent<AgentMover>();
        rb = GetComponent<Rigidbody>();
        defaultColor = colorRenderer.material.color;
    }

    private void Update()
    {
        UpdateFlockTimeoutTimer();
    }

    private void UpdateFlockTimeoutTimer()
    {
        if (flockTimeout == 0)
            return;

        flockTimeout -= Time.deltaTime;

        if (flockTimeout <= 0f)
        {
            flockTimeout = 0f;
            agentMover.enabled = true;
        }
    }

    public void StartFollowing(Transform target)
    {
        CurrentChickState = ChickState.Following;
        agentMover.target = target;
    }

    public void TogglePhysics(bool physicsOn)
    {
        agentMover.enabled = !physicsOn;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (CurrentChickState == ChickState.Thrown)
        {
            flockTimeout = postThrowFlockTimeout;
        }
    }
}