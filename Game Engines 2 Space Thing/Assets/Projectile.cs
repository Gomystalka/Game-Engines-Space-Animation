using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody _rb;
    public float turnRate = 1500f;
    public float projectileSpeed = 100f;

    public Transform homeTarget;
    public Vector3 Velocity {
        get => _rb.velocity;
        set => _rb.velocity = value;
    }

    void Start() => AssignRigidbody();

    public void AssignRigidbody() => _rb = GetComponent<Rigidbody>();

    public void Update()
    {
        if (!homeTarget) return;
        _rb.velocity = transform.forward * projectileSpeed;
        _rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(homeTarget.position - transform.position), turnRate));
    }
}
