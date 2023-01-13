using System.Collections.Generic;
using UnityEngine;

namespace Prototyping
{
    public class PositionTracker : MonoBehaviour
    {
        [HideInInspector] public List<Vector3> positions = new();
        [SerializeField] private int maxPositionCount = 1000;

        private void FixedUpdate()
        {
            positions.Add(transform.position);

            if (positions.Count > maxPositionCount)
                positions.RemoveAt(0);
        }
    }
}