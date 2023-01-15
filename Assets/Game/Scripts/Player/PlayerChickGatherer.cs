using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerChickGatherer : MonoBehaviour
{
    private FlockController flockController;
    
    void Awake()
    {
        flockController = GetComponentInParent<FlockController>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ChickController chickController))
        {
            flockController.AddFlockMember(chickController);
        }
    }
}
