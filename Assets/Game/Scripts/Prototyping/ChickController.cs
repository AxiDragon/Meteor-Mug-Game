using System.Collections.Generic;
using UnityEngine;
using UnityTimer;

namespace Prototyping
{
    [RequireComponent(typeof(TargetFollower), typeof(Rigidbody))]
    public class ChickController : MonoBehaviour
    {
        public ChickState currentChickState = ChickState.Idle;
        [HideInInspector] public List<FlockController> incompatibleFlockControllers = new List<FlockController>();
        private TargetFollower targetFollower;
        private Rigidbody rb;
    
        public enum ChickState
        {
            Idle,
            Following,
            Thrown,
            Stunned
        }

        private void Awake()
        {
            targetFollower = GetComponent<TargetFollower>();
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            targetFollower.enabled = false;
            rb.isKinematic = true;
        }

        public void StartFollowing(Transform target)
        {
            currentChickState = ChickState.Following;
            rb.isKinematic = true;
            targetFollower.enabled = true;
            targetFollower.target = target;
        }

        public void Throw(Vector3 force, FlockController flockController)
        {
            rb.isKinematic = false;
            targetFollower.target = null;
            targetFollower.enabled = false;
            rb.AddForce(force, ForceMode.Impulse);
            
            incompatibleFlockControllers.Add(flockController);
            Timer.Register(1f, () => incompatibleFlockControllers.Remove(flockController));
        }
    }
}
