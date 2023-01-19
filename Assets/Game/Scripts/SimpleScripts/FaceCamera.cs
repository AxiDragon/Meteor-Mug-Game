using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField] private bool flip = true;
    private Transform cam;

    private void Awake()
    {
        cam = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(flip ? transform.position - cam.position : cam.position);
    }
}