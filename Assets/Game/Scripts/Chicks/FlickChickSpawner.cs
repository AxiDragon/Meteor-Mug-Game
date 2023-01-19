using System;
using Unity.Mathematics;
using UnityEngine;
using UnityTimer;
using Random = UnityEngine.Random;

public class FlickChickSpawner : MonoBehaviour
{
    [SerializeField] private ChickController flickChickPrefab;
    [SerializeField] private float timeBeforeFirstSpawn;
    [SerializeField] private float timeBetweenSpawns;
    [SerializeField] private float verticalOffset;
    private BoxCollider[] boxColliders;

    private void Awake()
    {
        boxColliders = GetComponents<BoxCollider>();
    }

    private void Start()
    {
        Timer.Register(timeBeforeFirstSpawn, SpawnInFlickChick);
    }

    private void SpawnInFlickChick()
    {
        Instantiate(flickChickPrefab, GetSpawnPosition(), quaternion.identity).TogglePhysics(true);
        Timer.Register(timeBetweenSpawns, SpawnInFlickChick);
    }

    private Vector3 GetSpawnPosition()
    {
        BoxCollider randomCollider = boxColliders[Random.Range(0, boxColliders.Length)];
        return GetRandomPointInsideCollider(randomCollider) + Vector3.up * verticalOffset;
    }

    public Vector3 GetRandomPointInsideCollider(BoxCollider boxCollider)
    {
        Vector3 extents = boxCollider.size / 2f;
        Vector3 point = new Vector3(
            Random.Range(-extents.x, extents.x),
            Random.Range(-extents.y, extents.y),
            Random.Range(-extents.z, extents.z)
        ) + boxCollider.center;
        return boxCollider.transform.TransformPoint(point);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position + Vector3.up * verticalOffset, 2f);
    }
}