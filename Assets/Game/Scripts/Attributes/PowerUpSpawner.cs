using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PowerUpSpawner : MonoBehaviour
{
    [SerializeField] private PowerUpPickup powerUpPrefab;
    private BoxCollider[] boxColliders;
    public bool active;

    [Tooltip("Average Per Minute")] [SerializeField]
    private float powerUpSpawningChance;

    [SerializeField] private float minimumSpawningDistanceFromPlayers;
    [SerializeField] private int spawnAttempts = 30;

    private void Awake()
    {
        boxColliders = GetComponents<BoxCollider>();
    }

    void FixedUpdate()
    {
        if (!active)
            return;
        
        if (Random.value < powerUpSpawningChance * (Time.fixedDeltaTime / 60f))
        {
            SpawnPowerUp();
        }
    }

    private void SpawnPowerUp()
    {
        Vector3 spawnPosition = GetSpawnPosition(out bool success);
        
        if (success)
            Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);
    }

    private Vector3 GetSpawnPosition(out bool success)
    {
        Vector3 position;
        bool validPosition = true;
        int attemptsMade = 0;

        do
        {
            attemptsMade++;
            validPosition = true;
            
            BoxCollider randomCollider = boxColliders[Random.Range(0, boxColliders.Length)];
            position = GetRandomPointInsideCollider(randomCollider);
            
            foreach (var p in FindObjectsOfType<PlayerMovement>())
            {
                if (Vector3.Distance(position, p.transform.position) < minimumSpawningDistanceFromPlayers)
                {
                    validPosition = false;
                    break;
                }
            }

        } while (!validPosition && attemptsMade < spawnAttempts);

        success = attemptsMade < spawnAttempts;
        return position;
    }
    
    public Vector3 GetRandomPointInsideCollider( BoxCollider boxCollider )
    {
        Vector3 extents = boxCollider.size / 2f;
        Vector3 point = new Vector3(
            Random.Range( -extents.x, extents.x ),
            Random.Range( -extents.y, extents.y ),
            Random.Range( -extents.z, extents.z )
        )  + boxCollider.center;
        return boxCollider.transform.TransformPoint( point );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, minimumSpawningDistanceFromPlayers);
        
    }
}