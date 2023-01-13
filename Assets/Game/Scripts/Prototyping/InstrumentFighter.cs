using UnityEngine;
using UnityEngine.InputSystem;

namespace Prototyping
{
    public class InstrumentFighter : MonoBehaviour
    {
        [SerializeField] private Transform hand;
        private Weapon currentWeapon;
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Weapon weapon))
            {
                if (currentWeapon != null)
                {
                    currentWeapon.transform.parent = null;
                    currentWeapon.Drop();
                }

                EquipWeapon(weapon);
            }
        }

        public void OnAttack(InputValue value)
        {
            if (value.isPressed && currentWeapon != null)
            {
                Debug.Log("Attacking!");
                currentWeapon.Attack();
            }
        }

        private void EquipWeapon(Weapon weapon)
        {
            weapon.Pickup(rb);
            currentWeapon = weapon;
            currentWeapon.transform.parent = hand;
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
        }
    }
}