using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityTimer;

[RequireComponent(typeof(PlayerAimer))]
public class ChickThrower : MonoBehaviour
{
    [Header("Input")] [SerializeField] private float aimTurnInputDifferenceThreshold = .5f;
    [SerializeField] private float aimStartThreshold = .2f;
    [Header("Throwing")] [SerializeField] private float throwingForce = 5f;
    [SerializeField] private Transform throwingPoint;
    [SerializeField] private float throwingCooldown = .1f;
    [SerializeField] private float upwardThrowAngle = 35f;
    [Header("Chick Striking")] public float strikePower;
    public float strikeRange;
    [Header("Other")] [SerializeField] private bool infiniteThrowsMode = false;
    [SerializeField] private float autoAimRange = 25f;
    [SerializeField] private float autoAimRadius = 2f;
    [SerializeField] private float throwCollisionImmunityTime = .1f;
    [SerializeField] private int thrownChickLayerMaskID;
    [SerializeField] private List<Collider> ignoreFromAutoAim;

    private float flickTimer = 0f;
    private PlayerAimer aimer;
    private LineRenderer aimLine;
    public bool canThrow = true;
    private bool isAiming = false;

    private Animator animator;
    private FlockController flockController;

    private Vector2 previousAimInput;
    private ChickController aimingChick;
    private ChickController mostRecentThrownChick;

    public bool IsAiming
    {
        get => isAiming;
        set
        {
            if (value == isAiming)
                return;

            flickTimer = 0f;

            isAiming = value;
            animator.SetBool("aiming", IsAiming);

            if (isAiming)
                StartAiming();
            else
                StopAiming();
        }
    }

    private void Awake()
    {
        aimer = GetComponent<PlayerAimer>();
        flockController = GetComponent<FlockController>();
        aimLine = GetComponent<LineRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateLine();

        if (IsAiming && aimingChick)
        {
            aimingChick.transform.localPosition = Vector3.zero;
        }

        flickTimer = Mathf.Max(0f, flickTimer - Time.deltaTime);
    }

    private void UpdateLine()
    {
        aimLine.SetPosition(0, transform.position);
        aimLine.SetPosition(1,
            aimer.angledAimInput.magnitude > aimer.aimTurnInputThreshold
                ? transform.position + aimer.angledAimInput * 5f
                : transform.position);
    }

    public void StartAiming()
    {
        if (flockController.GetChickCount() <= 0)
        {
            IsAiming = false;
            return;
        }

        aimingChick = flockController.flock[0];
        aimingChick.TogglePhysics(true);
        aimingChick.held = true;
        aimingChick.transform.parent = throwingPoint;

        aimingChick.rb.useGravity = false;
        aimingChick.strikePower = strikePower;
        aimingChick.strikeRange = strikeRange;
        aimingChick.flockTimeout = Mathf.Infinity;
    }

    private void StopAiming()
    {
        if (!aimingChick)
            return;

        aimingChick.TogglePhysics(false);
        aimingChick.rb.useGravity = true;
        aimingChick.held = false;
        aimingChick.transform.parent = null;
        aimingChick.flockTimeout = 0f;
        aimingChick = null;
    }

    public void OnAim(InputValue value)
    {
        var inputValue = value.Get<Vector2>();

        if (IsFlicking(inputValue) && canThrow)
        {
            if (flockController.GetChickCount() > 0)
            {
                Throw();
            }
            else if (infiniteThrowsMode)
            {
                ThrowSphere();
            }
        }

        IsAiming = inputValue.magnitude > aimStartThreshold;
        previousAimInput = inputValue;
    }

    private void Throw()
    {
        SetThrowingCooldown(throwingCooldown);

        ThrowChick();

        ResetPostThrow();
    }

    private void ResetPostThrow()
    {
        flockController.RemoveFlockMember(aimingChick);
        aimingChick = null;
    }

    private void ThrowChick()
    {
        if (aimingChick == null)
            return;

        aimingChick.agentMover.target = null;
        aimingChick.transform.parent = null;
        aimingChick.currentChickState = ChickController.ChickState.Thrown;
        aimingChick.StartChickThrowTimeOutInvoke();

        aimingChick.rb.velocity = Vector3.zero;
        Vector3 throwingDirection = GetThrowingDirection();
        aimingChick.rb.AddForce(throwingDirection * throwingForce, ForceMode.VelocityChange);

        aimingChick.canStrike = true;

        mostRecentThrownChick = aimingChick;

        Timer.Register(throwCollisionImmunityTime, () =>
        {
            if (mostRecentThrownChick.currentChickState != ChickController.ChickState.Idle)
            {
                mostRecentThrownChick.gameObject.layer = thrownChickLayerMaskID;
            }
        });

        animator.SetTrigger("throw");
    }

    private Vector3 GetThrowingDirection()
    {
        Vector3 direction = Quaternion.AngleAxis(-upwardThrowAngle, transform.right) * aimer.angledAimInput;
        List<AutoAimTarget> inAutoAimTargets = new();

        Ray ray = new Ray(throwingPoint.position, throwingPoint.position + direction.normalized);
        RaycastHit[] hits = Physics.SphereCastAll(ray, autoAimRadius, autoAimRange);

        foreach (var c in hits)
        {
            if (ignoreFromAutoAim.Contains(c.collider))
                continue;

            if (c.collider.gameObject == gameObject)
                continue;

            if (c.collider.TryGetComponent(out AutoAimTarget t))
            {
                if (t.TryGetComponent(out ChickController cc) && cc.owner == flockController)
                    continue;

                inAutoAimTargets.Add(t);
            }
        }

        if (inAutoAimTargets.Count == 0)
        {
            Vector3 modifiedDirection = direction;
            modifiedDirection.y = 0f;
            return modifiedDirection.normalized;
        }

        AutoAimTarget closestTarget = inAutoAimTargets[0];
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < inAutoAimTargets.Count; i++)
        {
            float distance = Vector3.Distance(throwingPoint.position, inAutoAimTargets[i].transform.position);
            
            if (distance < autoAimRadius)
                continue;
            
            if (distance < closestDistance)
            {
                closestTarget = inAutoAimTargets[i];
                closestDistance = distance;
            }
        }

        Vector3 modifiedAutoAim = closestTarget.transform.position - transform.position;
        modifiedAutoAim.y = 0f;
        return modifiedAutoAim.normalized;
    }

    private void SetThrowingCooldown(float cooldown)
    {
        canThrow = false;
        Timer.Register(cooldown, () => canThrow = true);
    }

    private void ThrowSphere()
    {
        canThrow = false;
        Timer.Register(throwingCooldown, () => canThrow = true);

        GameObject throwSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Rigidbody throwRb = throwSphere.AddComponent<Rigidbody>();
        throwSphere.transform.position = throwingPoint.position;

        Vector3 throwingDirection = Quaternion.AngleAxis(-upwardThrowAngle, transform.right) * aimer.angledAimInput;
        throwRb.AddForce(throwingDirection * throwingForce);
    }

    private bool IsFlicking(Vector2 inputValue)
    {
        return previousAimInput.magnitude - inputValue.magnitude > aimTurnInputDifferenceThreshold;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 throwingDirection = Quaternion.AngleAxis(-upwardThrowAngle, transform.right) * transform.forward *
                                    throwingForce;

        Gizmos.DrawLine(throwingPoint.position, throwingPoint.position + throwingDirection);
        Gizmos.DrawWireSphere(transform.position, strikeRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(throwingPoint.position, autoAimRadius);
        Gizmos.DrawWireSphere(throwingPoint.position + throwingDirection.normalized * autoAimRange, autoAimRadius);
    }
}