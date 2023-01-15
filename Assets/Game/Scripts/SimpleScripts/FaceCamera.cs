using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform cam;
    [SerializeField] private bool flip = true;

    private void Awake()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        transform.LookAt(flip ? transform.position - cam.position : cam.position);
    }
}