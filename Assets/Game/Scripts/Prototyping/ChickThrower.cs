using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityTimer;

namespace Prototyping
{
    [RequireComponent(typeof(PlayerAimer))]
    public class ChickThrower : MonoBehaviour
    {
        private FlockController flockController;
        [SerializeField] private float aimTurnInputDifferenceThreshold = .5f;
        [SerializeField] private float throwingForce = 5f;
        [SerializeField] private Transform throwingPoint;
        [SerializeField] private float throwingCooldown = .1f;
        private bool canThrow = true;
        private PlayerAimer aimer;
        private Vector2 previousAimInput;
        private LineRenderer aimLine;

        void Awake()
        {
            aimer = GetComponent<PlayerAimer>();
            flockController = GetComponent<FlockController>();
            aimLine = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            aimLine.SetPosition(0, transform.position);
            aimLine.SetPosition(1,
                aimer.angledAimInput.magnitude > aimer.aimTurnInputThreshold
                    ? transform.position + aimer.angledAimInput * 5f
                    : transform.position);
        }

        public void OnAim(InputValue value)
        {
            Vector2 inputValue = value.Get<Vector2>();

            if (IsFlicking(inputValue) && HasChicks() && canThrow)
            {
                canThrow = false;
                Timer.Register(throwingCooldown, () => canThrow = true);
                print("Attempting to throw!");
                ChickController thrownChick = flockController.flock[0];
                flockController.RemoveFlockMember(0);
                thrownChick.transform.position = throwingPoint.position;
                thrownChick.Throw(aimer.angledAimInput * throwingForce, flockController);
            }

            previousAimInput = inputValue;
        }

        private bool HasChicks()
        {
            return flockController.flock.Count > 0;
        }

        private bool IsFlicking(Vector2 inputValue)
        {
            return (previousAimInput.magnitude - inputValue.magnitude) > aimTurnInputDifferenceThreshold;
        }
    }
}