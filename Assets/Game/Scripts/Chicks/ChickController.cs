using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityTimer;
using Random = UnityEngine.Random;

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
    public float flockTimeout;
    
    [SerializeField] private float postThrowFlockTimeout = 1f;
    [SerializeField] private float maxGroundHitNormalDistance = .2f;
    [HideInInspector] public bool held;

    [HideInInspector] public float strikePower;
    [HideInInspector] public float strikeRange;
    
    [HideInInspector] public bool canStrike;
    
    public MeshRenderer colorRenderer;
    [HideInInspector] public Color defaultColor;
    [HideInInspector] public Rigidbody rb;
    private TrailRenderer trail;
    
    [HideInInspector] public AgentMover agentMover;
    [SerializeField] private Transform followTarget;
    [HideInInspector] public FlockController owner;

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
        trail = GetComponent<TrailRenderer>();
    }

    private void Start()
    {
        SetChickColor();
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
            
            if (!held)
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
            if (canStrike)
            {
                Strike();
            }
            
            foreach (var item in collision.contacts)
            {
                if (Vector3.Distance(item.normal, Vector3.up) < maxGroundHitNormalDistance)
                {
                    // print("Hit the ground");
                    flockTimeout = postThrowFlockTimeout;
                    CurrentChickState = ChickState.Idle;
                }
                
                // Debug.DrawRay(item.point, item.normal * 100, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), 10f);
            }
        }
    }

    private void Strike()
    {
        foreach (var c in Physics.OverlapSphere(transform.position, strikeRange))
        {
            if (c.TryGetComponent(out Rigidbody r))
            {
                if (r == rb)
                    continue;

                if (r.TryGetComponent(out ChickController chickController))
                {
                    chickController.TogglePhysics(true);
                    if (chickController.owner)
                    {
                        chickController.owner.RemoveFlockMember(chickController);
                    }
                }
                
                r.AddExplosionForce(strikePower, transform.position, strikeRange, 1f, ForceMode.Impulse);
            }
        }

        canStrike = false;
    }

    public void SetChickColor(Color? colorValue = null)
    {
        Color color;
        
        if (colorValue == null)
        {
            color = defaultColor;
        }
        else
        {
            color = (Color)colorValue;
        }

        Material material = colorRenderer.material;
        material.DOColor(color, .5f);

        for (int i = 0; i < trail.colorGradient.colorKeys.Length; i++)
        {
            trail.colorGradient.colorKeys[i].color = color;
        }
    }

    public void SetTimeout(float time = 0f)
    {
        if (time > 0f)
        {
            flockTimeout = time;
        }
        else
        {
            flockTimeout = postThrowFlockTimeout;
        }
    }

    private void OnDrawGizmos()
    {
        if (flockTimeout == 0f)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 1.2f);
        }
    }
}