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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position + Vector3.up * verticalOffset, 2f);
    }

    private void SpawnInFlickChick()
    {
        Instantiate(flickChickPrefab, GetSpawnPosition(), quaternion.identity).TogglePhysics(true);
        Timer.Register(timeBetweenSpawns, SpawnInFlickChick);
    }

    private Vector3 GetSpawnPosition()
    {
        var randomCollider = boxColliders[Random.Range(0, boxColliders.Length)];
        return GetRandomPointInsideCollider(randomCollider) + Vector3.up * verticalOffset;
    }

    public Vector3 GetRandomPointInsideCollider(BoxCollider boxCollider)
    {
        var extents = boxCollider.size / 2f;
        var point = new Vector3(
            Random.Range(-extents.x, extents.x),
            Random.Range(-extents.y, extents.y),
            Random.Range(-extents.z, extents.z)
        ) + boxCollider.center;
        return boxCollider.transform.TransformPoint(point);
    }
}