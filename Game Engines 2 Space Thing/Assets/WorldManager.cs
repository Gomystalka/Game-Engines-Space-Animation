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
    public Vector2 usakiSpeedRange;
    public Fader fader;

    public static float RandomUsakiSpeed => Random.Range(instance.usakiSpeedRange.x, instance.usakiSpeedRange.y);
    public float maxUsakiSpeed = 2500f;
    public float minUsakiSpeed = 800f;
    public float usakiDecelerationRate = 10f;

    public Projectile laserPrefab;

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

    public void FireRandomBakuUsaAt(Transform transform)
    {
        StartCoroutine(PerformAfter(6f));
    }

    private IEnumerator PerformAfter(float delay) {
        yield return new WaitForSeconds(delay);
        CameraController.instance.orbit = false;
        CameraController.instance.positionSwitchTarget = null;
        CameraController.instance.SetCameraTarget(ShipAI.FireRandomBakuUsaAt(transform));
        CameraController.instance.moveRate = 200000f;
        yield return new WaitForSeconds(3f);
        fader.FadeIn();
        Debug.Log("AWA");
    }
}
