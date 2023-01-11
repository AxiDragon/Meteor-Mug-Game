using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFighter : MonoBehaviour
{
    private Rigidbody rb;
    private Weapon currentWeapon = null;
    [SerializeField] private Transform hand;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentWeapon != null)
        {
            currentWeapon.Attack();
        }
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

    void EquipWeapon(Weapon weapon)
    {
        weapon.Pickup(rb);
        currentWeapon = weapon;
        currentWeapon.transform.parent = hand;
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
    }
}
