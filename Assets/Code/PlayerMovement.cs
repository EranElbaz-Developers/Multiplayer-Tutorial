using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    public float movementSensitivity = 10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 force = Vector3.zero;
        if (Input.GetKey(KeyCode.A))
        {
            force += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            force += Vector3.right;
        }
        if (Input.GetKey(KeyCode.S))
        {
            force += Vector3.down;
        }
        if (Input.GetKey(KeyCode.W))
        {
            force += Vector3.up;
        }

        if (force.magnitude == 0)
        {
            rb.velocity = Vector3.zero;
        }
        else
        {
            rb.velocity = force * movementSensitivity;
        }
    }
}