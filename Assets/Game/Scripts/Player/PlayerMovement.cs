using System;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerAimer))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speedTarget = 5f;
    [SerializeField] private float speed = 12f;
    [SerializeField] private float speedCap = 12f;
    private PlayerAimer aimer;
    private Transform cameraTransform;
    private Vector3 moveInput;
    private Rigidbody rb;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        aimer = GetComponent<PlayerAimer>();
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        var move = Quaternion.AngleAxis(cameraTransform.eulerAngles.y, Vector3.up) * moveInput;

        if (aimer.aimInput.magnitude > aimer.aimTurnInputThreshold)
        {
            transform.LookAt(transform.position + aimer.angledAimInput);
        }
        else
        {
            var velocityDirection = Vector3.Scale(rb.velocity, new Vector3(1f, 0f, 1f));

            if (velocityDirection.magnitude > .5f)
                transform.LookAt(transform.position + velocityDirection);
        }

        rb.AddForce(move * (GetSpeedModifier() * (speed * Time.deltaTime)), ForceMode.VelocityChange);
        
        animator.SetFloat("speed", rb.velocity.magnitude / 12f);
    }

    private void LateUpdate()
    {
        if (rb.velocity.magnitude > speedCap)
        {
            rb.velocity = rb.velocity.normalized * speedCap;
        }
    }

    private float GetSpeedModifier()
    {
        var horizontalVelocity = Vector3.Scale(rb.velocity, new Vector3(1f, 0f, 1f)).magnitude;
        return Mathf.Min(3f, speedTarget / horizontalVelocity);
    }

    public void OnMove(InputValue value)
    {
        var inputValue = value.Get<Vector2>();
        moveInput = new Vector3(inputValue.x, 0f, inputValue.y);
    }
}