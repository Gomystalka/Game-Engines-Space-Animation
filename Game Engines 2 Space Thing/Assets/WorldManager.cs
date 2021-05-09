using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-20)]
public class WorldManager : MonoBehaviour
{
    public static WorldManager instance;

    [Header("Settings")]
    public float radius = 10f;
    public Vector3 centerOffset;

    private void Awake()
    {
        instance = Utilities.CreateSingleton(instance, this);
    }

    public static Vector3 QueryRandomWorldPoint() {
        if (!instance) return Vector3.zero;
        return instance.transform.position + instance.centerOffset + (Random.insideUnitSphere * instance.radius);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 1f, 0.4f);
        Gizmos.DrawSphere(transform.position + centerOffset, radius);
    }
}
