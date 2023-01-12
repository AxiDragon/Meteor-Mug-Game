using UnityEngine;
using UnityEngine.InputSystem;

namespace Prototyping
{
    [RequireComponent(typeof(PlayerAimer))]
    public class PlayerMovement : MonoBehaviour
    {
        private Rigidbody rb;
        private Vector3 moveInput;
        private PlayerAimer aimer;
        [SerializeField] private float speedTarget = 5f;
        [SerializeField] private float speed = 12f;
        private Transform cameraTransform;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            aimer = GetComponent<PlayerAimer>();
            cameraTransform = Camera.main.transform;
        }

        void Update()
        {
            Vector3 move = Quaternion.AngleAxis(cameraTransform.eulerAngles.y, Vector3.up) * moveInput;

            if (aimer.aimInput.magnitude > aimer.aimTurnInputThreshold)
            {
                transform.LookAt(transform.position + aimer.angledAimInput);
            }
            else
            {
                Vector3 velocityDirection = Vector3.Scale(rb.velocity, new Vector3(1f, 0f, 1f));

                if (velocityDirection.magnitude > .5f)
                    transform.LookAt(transform.position + velocityDirection);
            }

            rb.AddForce(move * (GetSpeedModifier() * (speed * 60f * Time.deltaTime)));
        }

        float GetSpeedModifier()
        {
            float horizontalVelocity = Vector3.Scale(rb.velocity, new Vector3(1f, 0f, 1f)).magnitude;
            return Mathf.Min(3f, (speedTarget / horizontalVelocity));
        }

        public void OnMove(InputValue value)
        {
            Vector2 inputValue = value.Get<Vector2>();
            moveInput = new Vector3(inputValue.x, 0f, inputValue.y);
        }


    }
}