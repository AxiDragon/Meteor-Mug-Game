using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Transform cameraTransform;
    [SerializeField] private float speedTarget = 5f;
    public float speed = 12f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(x, 0f, z);
        move = Quaternion.AngleAxis(cameraTransform.eulerAngles.y, Vector3.up) * move;
        
        transform.LookAt(transform.position + Vector3.Scale(rb.velocity, new Vector3(1f, 0f, 1f)));
        rb.AddForce(move * (GetSpeedModifier() * (speed * 60f * Time.deltaTime)));
    }

    float GetSpeedModifier()
    {
        float horizontalVelocity = Vector3.Scale(rb.velocity, new Vector3(1f, 0f, 1f)).magnitude;
        return Mathf.Min(3f, (speedTarget / horizontalVelocity));
    }
}