using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private Rigidbody owner;
    private CapsuleCollider hitBox;
    private bool pickedUp = false;
    [SerializeField] private float hitPower = 50f;
    [SerializeField] private float launchModifier = 10f;
    [SerializeField] private float recoil = 50f;
    [SerializeField] private float recoilLaunchModifier = 10f;
    [SerializeField] private float hitRange = 2f;

    void Awake()
    {
        foreach (CapsuleCollider c in GetComponents<CapsuleCollider>())
        {
            if (c.isTrigger)
            {
                hitBox = c;
                break;
            }
        }

        if (!hitBox)
            throw new Exception("No hitbox!");
    }

    public void Pickup(Rigidbody newOwner)
    {
        pickedUp = true;
        owner = newOwner;
    }

    public void Drop()
    {
        pickedUp = false;
        owner = null;
    }

    public void Attack()
    {
        Vector3 direction = new Vector3 { [hitBox.direction] = 1 };
        float offset = hitBox.height / 2 - hitBox.radius;
        Vector3 localPoint0 = hitBox.center - direction * offset;
        Vector3 localPoint1 = hitBox.center + direction * offset;
        Vector3 point0 = transform.TransformPoint(localPoint0);
        Vector3 point1 = transform.TransformPoint(localPoint1);

        foreach (Collider c in Physics.OverlapCapsule(point0, point1, hitBox.radius))
        {
            if (c.TryGetComponent(out Rigidbody rb))
            {
                if (rb == owner)
                {
                    rb.AddExplosionForce(recoil, transform.position, hitRange, recoilLaunchModifier, ForceMode.Impulse);
                    continue;
                }

                rb.AddExplosionForce(hitPower, transform.position, hitRange, launchModifier, ForceMode.Impulse);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRange);
    }
}