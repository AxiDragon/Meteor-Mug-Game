using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityTimer;

[RequireComponent(typeof(PlayerAimer))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speedTarget = 5f;
    [SerializeField] public float speed = 12f;
    [SerializeField] private float speedCap = 12f;
    public bool canMove = true;
    private PlayerAimer aimer;
    private Animator animator;
    private Transform cameraTransform;
    private bool isThrowing;
    private Vector3 moveInput;
    private Rigidbody rb;

    public bool CanMove
    {
        get => canMove;
        set
        {
            canMove = value;

            if (!canMove)
                moveInput = Vector3.zero;
        }
    }

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

        if (aimer.aimInput.magnitude > aimer.aimTurnInputThreshold || isThrowing)
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
        CheckSpeedCap();
    }

    private void CheckSpeedCap()
    {
        var xzVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        xzVelocity = Vector3.ClampMagnitude(xzVelocity, speedCap);
        rb.velocity = new Vector3(xzVelocity.x, rb.velocity.y, xzVelocity.z);
    }

    private float GetSpeedModifier()
    {
        var horizontalVelocity = Vector3.Scale(rb.velocity, new Vector3(1f, 0f, 1f)).magnitude;
        return Mathf.Min(3f, speedTarget / horizontalVelocity);
    }

    //Animation Event
    [UsedImplicitly]
    public void OnStartThrow()
    {
        isThrowing = true;
    }

    //Animation Event
    [UsedImplicitly]
    public void OnEndThrow()
    {
        isThrowing = false;
    }

    [UsedImplicitly]
    public void OnMove(InputValue value)
    {
        if (!CanMove)
            return;

        var inputValue = value.Get<Vector2>();
        moveInput = new Vector3(inputValue.x, 0f, inputValue.y);
    }

    public void Stun(float stunTime = 1f)
    {
        CanMove = false;
        Timer.Register(stunTime, () => CanMove = true);
    }
}