using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [SerializeField] private PowerUpPickup powerUpPrefab;
    public bool active;

    [Tooltip("Average Per Minute")] [SerializeField]
    private float powerUpSpawningChance;

    [SerializeField] private float minimumSpawningDistanceFromPlayers;
    [SerializeField] private int spawnAttempts = 30;
    private BoxCollider[] boxColliders;

    private void Awake()
    {
        boxColliders = GetComponents<BoxCollider>();
    }

    private void FixedUpdate()
    {
        if (!active)
            return;

        if (Random.value < powerUpSpawningChance * (Time.fixedDeltaTime / 60f)) SpawnPowerUp();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, minimumSpawningDistanceFromPlayers);
    }

    private void SpawnPowerUp()
    {
        var spawnPosition = GetSpawnPosition(out var success);

        if (success)
            Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);
    }

    private Vector3 GetSpawnPosition(out bool success)
    {
        Vector3 position;
        var validPosition = true;
        var attemptsMade = 0;

        do
        {
            attemptsMade++;
            validPosition = true;

            var randomCollider = boxColliders[Random.Range(0, boxColliders.Length)];
            position = GetRandomPointInsideCollider(randomCollider);

            foreach (var p in FindObjectsOfType<PlayerMovement>())
                if (Vector3.Distance(position, p.transform.position) < minimumSpawningDistanceFromPlayers)
                {
                    validPosition = false;
                    break;
                }
        } while (!validPosition && attemptsMade < spawnAttempts);

        success = attemptsMade < spawnAttempts;
        return position;
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