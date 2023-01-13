using System;
using UnityEngine;

namespace Prototyping
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private float hitPower = 50f;
        [SerializeField] private float launchModifier = 10f;
        [SerializeField] private float recoil = 50f;
        [SerializeField] private float recoilLaunchModifier = 10f;
        [SerializeField] private float hitRange = 2f;
        private CapsuleCollider hitBox;
        private Rigidbody owner;
        private bool pickedUp;

        private void Awake()
        {
            foreach (var c in GetComponents<CapsuleCollider>())
                if (c.isTrigger)
                {
                    hitBox = c;
                    break;
                }

            if (!hitBox)
                throw new Exception("No hitbox!");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, hitRange);
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
            var direction = new Vector3 { [hitBox.direction] = 1 };
            var offset = hitBox.height / 2 - hitBox.radius;
            var localPoint0 = hitBox.center - direction * offset;
            var localPoint1 = hitBox.center + direction * offset;
            var point0 = transform.TransformPoint(localPoint0);
            var point1 = transform.TransformPoint(localPoint1);

            foreach (var c in Physics.OverlapCapsule(point0, point1, hitBox.radius))
                if (c.TryGetComponent(out Rigidbody rb))
                {
                    if (rb == owner)
                    {
                        rb.AddExplosionForce(recoil, transform.position, hitRange, recoilLaunchModifier,
                            ForceMode.Impulse);
                        continue;
                    }

                    rb.AddExplosionForce(hitPower, transform.position, hitRange, launchModifier, ForceMode.Impulse);
                }
        }
    }
}