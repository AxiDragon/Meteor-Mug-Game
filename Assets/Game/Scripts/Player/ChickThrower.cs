using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityTimer;

[RequireComponent(typeof(PlayerAimer))]
public class ChickThrower : MonoBehaviour
{
    [Header("Input")] [SerializeField] private float aimTurnInputDifferenceThreshold = .5f;
    [SerializeField] private float aimStartThreshold = .2f;
    [Header("Throwing")] public float throwingForce = 5f;
    [SerializeField] private Transform throwingPoint;
    [SerializeField] private float throwingCooldown = .1f;
    [SerializeField] private float upwardThrowAngle = 35f;
    [Header("Chick Striking")] public float strikePower;
    public float strikeRange;
    [Header("Other")] [SerializeField] private bool infiniteThrowsMode;
    [SerializeField] private float autoAimRange = 25f;
    [SerializeField] private float autoAimRadius = 2f;
    [SerializeField] private float throwCollisionImmunityTime = .1f;
    [SerializeField] private int thrownChickLayerMaskID;
    [SerializeField] private List<Collider> ignoreFromAutoAim;
    [SerializeField] private float verticalLineOffset = .1f;

    [HideInInspector] public float thrownChickSize = 1f;
    [HideInInspector] public float thrownChickMass = 1f;
    public bool canThrow = true;
    private PlayerAimer aimer;
    private ChickController aimingChick;
    [SerializeField] private LineRenderer aimLine;

    private Animator animator;

    private float flickTimer;
    private FlockController flockController;
    private bool isAiming;
    private ChickController mostRecentThrownChick;
    private MMF_Player throwSoundFeedback;

    private Vector2 previousAimInput;

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
        animator = GetComponent<Animator>();
        throwSoundFeedback = GetComponent<MMF_Player>();
    }

    private void Update()
    {
        UpdateLine();

        if (IsAiming && aimingChick) aimingChick.transform.localPosition = Vector3.zero;

        flickTimer = Mathf.Max(0f, flickTimer - Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        var throwingDirection = Quaternion.AngleAxis(-upwardThrowAngle, transform.right) * transform.forward *
                                throwingForce;

        Gizmos.DrawLine(throwingPoint.position, throwingPoint.position + throwingDirection);
        Gizmos.DrawWireSphere(transform.position, strikeRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(throwingPoint.position, autoAimRadius);
        Gizmos.DrawWireSphere(throwingPoint.position + throwingDirection.normalized * autoAimRange, autoAimRadius);
    }

    private void UpdateLine()
    {
        Vector3 basePosition = transform.position + Vector3.up * verticalLineOffset;
        aimLine.SetPosition(0, basePosition);
        aimLine.SetPosition(1,
            aimer.angledAimInput.magnitude > aimer.aimTurnInputThreshold
                ? basePosition + aimer.angledAimInput * 5f
                : basePosition);
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

        aimingChick.rb.mass *= thrownChickMass;
        aimingChick.transform.DOScale(aimingChick.transform.localScale * thrownChickSize, .25f);
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

    [UsedImplicitly]
    public void OnAim(InputValue value)
    {
        var inputValue = value.Get<Vector2>();

        if (IsFlicking(inputValue) && canThrow)
        {
            if (flockController.GetChickCount() > 0)
                Throw();
            else if (infiniteThrowsMode) ThrowSphere();
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
        var throwingDirection = GetThrowingDirection();
        aimingChick.rb.AddForce(throwingDirection * throwingForce, ForceMode.VelocityChange);

        aimingChick.canStrike = true;

        mostRecentThrownChick = aimingChick;
        
        throwSoundFeedback.PlayFeedbacks();

        Timer.Register(throwCollisionImmunityTime, () =>
        {
            if (mostRecentThrownChick.currentChickState != ChickController.ChickState.Idle)
                mostRecentThrownChick.gameObject.layer = thrownChickLayerMaskID;
        });

        animator.SetTrigger("throw");
    }

    private Vector3 GetThrowingDirection()
    {
        var direction = Quaternion.AngleAxis(-upwardThrowAngle, transform.right) * aimer.angledAimInput;
        List<AutoAimTarget> inAutoAimTargets = new();

        var ray = new Ray(throwingPoint.position, throwingPoint.position + direction.normalized);
        var hits = Physics.SphereCastAll(ray, autoAimRadius, autoAimRange);

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
            var modifiedDirection = direction;
            modifiedDirection.y = 0f;
            return modifiedDirection.normalized;
        }

        var closestTarget = inAutoAimTargets[0];
        var closestDistance = Mathf.Infinity;

        for (var i = 0; i < inAutoAimTargets.Count; i++)
        {
            var distance = Vector3.Distance(throwingPoint.position, inAutoAimTargets[i].transform.position);

            if (distance < autoAimRadius)
                continue;

            if (distance < closestDistance)
            {
                closestTarget = inAutoAimTargets[i];
                closestDistance = distance;
            }
        }

        var modifiedAutoAim = closestTarget.transform.position - transform.position;
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

        var throwSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var throwRb = throwSphere.AddComponent<Rigidbody>();
        throwSphere.transform.position = throwingPoint.position;

        var throwingDirection = Quaternion.AngleAxis(-upwardThrowAngle, transform.right) * aimer.angledAimInput;
        throwRb.AddForce(throwingDirection * throwingForce);
    }

    private bool IsFlicking(Vector2 inputValue)
    {
        return previousAimInput.magnitude - inputValue.magnitude > aimTurnInputDifferenceThreshold;
    }
}