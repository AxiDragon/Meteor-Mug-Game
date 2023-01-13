using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityTimer;

[RequireComponent(typeof(PlayerAimer))]
public class ChickThrower : MonoBehaviour
{
    [Tooltip("Input")]
    [SerializeField] private float aimTurnInputDifferenceThreshold = .5f;
    [SerializeField] private float aimStartThreshold = .2f;
    [Tooltip("Throwing")]
    [SerializeField] private float throwingForce = 5f;
    [SerializeField] private Transform throwingPoint;
    [SerializeField] private float throwingCooldown = .1f;
    [SerializeField] private float upwardThrowAngle = 35f;
    [Tooltip("Other")]
    [SerializeField] private bool infiniteThrowsMode = false;
    private float flickTimer = 0f;
    private PlayerAimer aimer;
    private LineRenderer aimLine;
    private bool canThrow = true;
    private bool isAiming = false;
    private FlockController flockController;
    private Vector2 previousAimInput;
    private ChickController aimingChick;

    public bool IsAiming
    {
        get => isAiming;
        set
        {
            if (value == isAiming)
                return;

            // if (value == false && flickTimer < flickTime)
            // {
            //     flickTime += Time.deltaTime * 2f;
            //     return;
            // }

            flickTimer = 0f;
            
            isAiming = value;
            
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
        if (!HasChicks())
        {
            IsAiming = false;
            return;
        }
        
        aimingChick = flockController.RemoveFlockMember(0);
        aimingChick.transform.parent = throwingPoint;
        aimingChick.agentMover.enabled = false;
        aimingChick.agentMover.target = null;
    }
    
    private void StopAiming()
    {
        if (!aimingChick)
            return;
        
        flockController.flock.Insert(0, aimingChick);
        aimingChick.transform.parent = null;
        aimingChick = null;
    }

    public void OnAim(InputValue value)
    {
        var inputValue = value.Get<Vector2>();

        
        if (IsFlicking(inputValue) && canThrow)
        {
            if (HasChicks())
            {
                ThrowChick();
            }
            else if (infiniteThrowsMode)
            {
                ThrowSphere();
            }
        }

        IsAiming = inputValue.magnitude > aimStartThreshold;
        previousAimInput = inputValue;
    }

    private void ThrowChick()
    {
        canThrow = false;
        Timer.Register(throwingCooldown, () => canThrow = true);

        aimingChick.transform.parent = null;
        Vector3 throwingDirection = Quaternion.AngleAxis(-upwardThrowAngle, transform.right) * aimer.angledAimInput;
        aimingChick.rb.AddForce(throwingDirection * throwingForce, ForceMode.Impulse);

        aimingChick = null;
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

    private bool HasChicks()
    {
        return flockController.flock.Count > 0;
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
    }
}