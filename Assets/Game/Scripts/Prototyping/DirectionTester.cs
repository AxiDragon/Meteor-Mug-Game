using UnityEngine;

public class DirectionTester : MonoBehaviour
{
    [SerializeField] private Vector3 direction;

    private void OnDrawGizmosSelected()
    {
        direction.Normalize();
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direction * 5f);
    }
}