using UnityEngine;

public class SpawnPositions : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            Gizmos.color = Color.red / i;
            Gizmos.DrawSphere(transform.GetChild(i).position, .2f);
        }
    }
}