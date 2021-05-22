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

    private Collider _collider;
    [System.NonSerialized] public OneTimeEvent onHomeTargetReached = new OneTimeEvent();
    public float distanceThreshold = 5f;

    private void Awake()
    {
        onHomeTargetReached = new OneTimeEvent();
    }
    public float DistanceToTarget { get; private set; }

    void Start() => AssignRigidbody();

    public void AssignRigidbody()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    public void Update()
    {
        if (!homeTarget) return;
        _rb.velocity = transform.forward * projectileSpeed;
        _rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(homeTarget.position - transform.position), turnRate));
        DistanceToTarget = Vector3.Distance(transform.position, homeTarget.position);
        if (DistanceToTarget <= distanceThreshold)
        {
            Debug.Log("Fish");
            onHomeTargetReached.InvokeOneTime();
        }
    }

    private void OnDrawGizmos()
    {
        if (!homeTarget) return;
        Gizmos.color = DistanceToTarget <= distanceThreshold ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, homeTarget.position);
    }

    public void SetTriggerStatus(bool status) => _collider.isTrigger = status;
}
