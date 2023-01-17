using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RigidbodyMagnet : MonoBehaviour
{
    [SerializeField] private float magnetRange = 3f;
    [SerializeField] private float magnetForce = 400f;
    [SerializeField] private LayerMask affectedLayers;

    private void Update()
    {
        foreach (Collider c in Physics.OverlapSphere(transform.position, magnetRange, affectedLayers))
        {
            if (c.TryGetComponent(out Rigidbody r))
            {
                float distance = Mathf.Max(Vector3.Distance(transform.position, r.position), 1f);
                Vector3 direction = (transform.position - r.position).normalized;
                r.AddForce(direction / distance * magnetForce);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }
}
