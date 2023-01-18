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
    private ChickController chickController;

    private void Awake()
    {
        chickController = GetComponent<ChickController>();
    }

    private void Update()
    {
        foreach (Collider coll in Physics.OverlapSphere(transform.position, magnetRange, affectedLayers))
        {
            if (coll.TryGetComponent(out Rigidbody hitRigidbody))
            {
                if (Attracted(hitRigidbody))
                {
                    float distance = Mathf.Max(Vector3.Distance(transform.position, hitRigidbody.position), 1f);
                    Vector3 direction = (transform.position - hitRigidbody.position).normalized;
                    hitRigidbody.AddForce(direction / distance * (magnetForce * magnetRange));
                }
            }
        }
    }

    private bool Attracted(Rigidbody hitRigidbody)
    {
        if (chickController == null)
            return true;

        if (hitRigidbody.TryGetComponent<ChickController>(out var hitChick))
        {
            if (hitChick.owner == chickController.owner)
                return false;
        }

        if (hitRigidbody.TryGetComponent<FlockController>(out var flockController))
        {
            if (hitChick.owner == flockController)
                return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }
}