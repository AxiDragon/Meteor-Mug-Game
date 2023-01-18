using System;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityTimer;

[RequireComponent(typeof(AgentMover), typeof(Rigidbody), typeof(Collider))]
public class ChickController : MonoBehaviour
{
    public enum ChickState
    {
        Idle,
        Following,
        Thrown,
    }

    public ChickState currentChickState = ChickState.Idle;
    public float flockTimeout = 0.1f;

    [SerializeField] private float postThrowFlockTimeout = 1f;
    [SerializeField] private float maxGroundHitNormalDistance = .2f;
    [SerializeField] private float throwTimeOut = 4f;
    [HideInInspector] public bool held;
    private int startLayerMask;

    [HideInInspector] public float strikePower;
    [HideInInspector] public float strikeRange;

    [HideInInspector] public bool canStrike;

    public MeshRenderer colorRenderer;
    [HideInInspector] public Color defaultColor;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public TrailRenderer trail;
    [HideInInspector] public AgentMover agentMover;
    [HideInInspector] public FlockController owner;
    [SerializeField] private MMF_Player regularHit;
    [SerializeField] private MMF_Player chickHit;
    [SerializeField] private MMF_Player playerHit;
    [SerializeField] private MMF_Player baseHit;
    [SerializeField] private MMF_Player hitObject;
    [Header("Debug")] [SerializeField] private bool showPickupability = false;

    private float rbStartingMass;
    private Vector3 startingSize;

    private void Awake()
    {
        agentMover = GetComponent<AgentMover>();
        rb = GetComponent<Rigidbody>();
        defaultColor = colorRenderer.material.color;
        trail = GetComponent<TrailRenderer>();
        startLayerMask = gameObject.layer;
    }

    private void Start()
    {
        SetChickColor();
        rbStartingMass = rb.mass;
        startingSize = transform.localScale;
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
        currentChickState = ChickState.Following;
        agentMover.target = target;
    }

    public void TogglePhysics(bool physicsOn)
    {
        if (agentMover != null) agentMover.enabled = !physicsOn;
        rb.constraints = physicsOn ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeAll;
        trail.enabled = physicsOn;

        if (!physicsOn)
            ResetStats();
    }

    private void ResetStats()
    {
        rb.mass = rbStartingMass;
        transform.DOScale(startingSize, .25f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (currentChickState == ChickState.Thrown)
        {
            if (canStrike)
            {
                rb.useGravity = true;
                Strike(collision.gameObject);
            }

            foreach (var item in collision.contacts)
            {
                if (Vector3.Distance(item.normal, Vector3.up) < maxGroundHitNormalDistance)
                {
                    CancelInvoke(nameof(ChickThrowOver));
                    ChickThrowOver();
                }
            }
        }
    }

    public void StartChickThrowTimeOutInvoke()
    {
        Invoke(nameof(ChickThrowOver), throwTimeOut);
    }
    
    public void ChickThrowOver()
    {
        trail.enabled = false;
        flockTimeout = 0f;
        currentChickState = ChickState.Idle;
        gameObject.layer = startLayerMask;
    }

    private void Strike(GameObject hitObject)
    {
        bool hitFlickableObject = false;
        MMF_Player hitEffectPlayer = regularHit;

        if (hitObject.TryGetComponent(out ChickController cc))
        {
            this.hitObject.GetFeedbackOfType<MMF_Flicker>().BoundRenderer = cc.GetComponentInChildren<Renderer>();
            hitFlickableObject = true;
            hitEffectPlayer = chickHit;
        }

        if (hitObject.TryGetComponent(out FlockController flockController) && flockController != owner)
        {
            bool hitShielded = false;
            foreach (PowerUp powerUp in hitObject.GetComponents<PowerUp>())
            {
                if (powerUp.powerUpType == PowerUpType.Shield)
                {
                    
                }
            }

            if (!hitShielded)
            {
                flockController.ScatterFlock(strikePower * 10f, strikeRange * 3f);
                flockController.GetComponent<PlayerMovement>().Stun(.5f);
                this.hitObject.GetFeedbackOfType<MMF_Flicker>().BoundRenderer = flockController.GetComponentInChildren<Renderer>();
                hitEffectPlayer = playerHit;
                hitFlickableObject = true;
            }
        }

        if (hitObject.CompareTag("MoveableObject"))
        {
            hitFlickableObject = true;
            this.hitObject.GetFeedbackOfType<MMF_Flicker>().BoundRenderer = hitObject.GetComponent<Renderer>();
        }
        
        foreach (var c in Physics.OverlapSphere(transform.position, strikeRange))
        {
            if (c.TryGetComponent(out Rigidbody r))
            {
                if (r == rb)
                    continue;

                if (r.TryGetComponent(out ChickController chickController))
                {
                    if (chickController.owner == owner)
                        continue;
                    
                    chickController.TogglePhysics(true);
                    if (chickController.owner && !chickController.held)
                    {
                        chickController.owner.RemoveFlockMember(chickController);
                    }
                }

                r.AddExplosionForce(strikePower, transform.position, strikeRange, 1f, ForceMode.Impulse);
            }
        }
        
        baseHit.PlayFeedbacks();
        hitEffectPlayer.PlayFeedbacks();

        if (!hitFlickableObject)
            this.hitObject.GetFeedbackOfType<MMF_Flicker>().BoundRenderer = colorRenderer;
            
        this.hitObject.PlayFeedbacks();
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
        if (showPickupability)
        {
            if (flockTimeout == 0f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, 1.2f);
            }
        }

        if (Application.isPlaying && gameObject.layer != startLayerMask)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, .8f);
        }
    }
}