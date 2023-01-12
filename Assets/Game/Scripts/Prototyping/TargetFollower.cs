using System.Collections.Generic;
using UnityEngine;

namespace Prototyping
{
    public class TargetFollower : MonoBehaviour
    {
        [HideInInspector] public List<Vector3> positions = new();
        [SerializeField] private float secondDelay = 2f;
        [SerializeField] private float maxMoveDistance = 1f;
        private int pointDelay;
        [SerializeField] private float stoppingDistance = .2f;
        public Transform target;

        private void Awake()
        {
            pointDelay = (int)(secondDelay / Time.fixedDeltaTime);
        }

        void FixedUpdate()
        {
            if (Vector3.Distance(transform.position, target.position) < stoppingDistance * 1.1f)
                return;
        
            positions.Add(target.position + GetOffset());
        
            if (positions.Count > pointDelay)
            {
                transform.LookAt(positions[0]);
                transform.position = Vector3.MoveTowards(transform.position, positions[0],
                    maxMoveDistance * Time.fixedDeltaTime * 50f);
                positions.RemoveAt(0);
            }
        }

        private Vector3 GetOffset()
        {
            Vector3 differenceDirection = transform.position - target.position;
            differenceDirection.y = 0f;
            differenceDirection.Normalize();
        
            return differenceDirection * stoppingDistance;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            foreach (Vector3 pos in positions)
            {
                Gizmos.DrawSphere(pos, .02f);
            }

            if (target == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, stoppingDistance);
        }
    }
}