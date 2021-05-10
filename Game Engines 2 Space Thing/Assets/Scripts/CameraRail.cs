using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRail : MonoBehaviour
{
    /*
     * Planned Features
     *  -Path Connections
     *  -Camera Direction across all connected paths
     *  -Path Events (Probably Editor)
     *  -One value (0 - 100) from start of first path to the end of the last path
     */
    public Rail currentRail;
    public Rail[] rails;
    public float speed;

    private float _time;
    private int _railIndex;

#if UNITY_EDITOR
    [Header("Gizmo Settings")]
    public Color pointGizmoColor = Color.yellow;
    public Color railGizmoColor = Color.green;
    public Color bakedPointGizmoColor = Color.magenta;
    public float railGizmoScale = 1f;
    public bool drawNonBakedPointsIfBaked = true;

    private void OnDrawGizmos()
    {
        foreach (Rail rail in rails)
        {
            if (rail == null) return;
            Vector3 scale = Vector3.one * railGizmoScale;
                Gizmos.color = pointGizmoColor;
                for (int i = 0; i < rail.railPoints.Length; i++)
                    Gizmos.DrawSphere(rail.railPoints[i].position, railGizmoScale);

                Gizmos.color = railGizmoColor;
                for (float t = 0; t < 1f; t += rail.pointDensity)
                    Gizmos.DrawCube(rail.GetPointAtTime(t), scale);
            if (!rail.ArePointsBaked) return;
            Gizmos.color = bakedPointGizmoColor;
            for (int i = 0; i < rail.BakedPoints.Length; i++)
                Gizmos.DrawCube(rail.BakedPoints[i], scale);
        }
    }
#endif
    private void Start()
    {
        foreach (Rail rail in rails)
        {
            if (!rail) continue;
            rail.BakePoints();
        }
        _railIndex = 0;
        BeginRailMotion();

#if !UNITY_EDITOR
        Destroy(GetComponent<LineRenderer>());
#endif
    }

    private void BeginRailMotion() {
        _railIndex = 0;
        currentRail = rails[_railIndex];
        transform.position = currentRail.BakedPoints[0];
        //transform.LookAt(currentRail.BakedPoints[1]);
    }

    private void LateUpdate()
    {
        if (speed == 0) return;
        _time += Time.deltaTime * speed;
        if (_time > 1f)
            _time = 1f;
        Vector3 nextPosition = currentRail.GetPointAtTime(_time);
        Vector3 nextPoint = _time == 0 ? currentRail.BakedPoints[1] : currentRail.GetPointAtTime(_time + Time.deltaTime * speed);
        transform.position = Vector3.MoveTowards(transform.position, nextPosition, _time);
        //Quaternion lookRot = Quaternion.LookRotation(nextPoint, );
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, Time.deltaTime * 1000f);
        //transform.LookAt(nextPoint);
        if (_time >= 1f) {
            if (_railIndex + 1 < rails.Length)
            {
                _railIndex++;
                _time = 0f;
            }
            currentRail = rails[_railIndex];
        }
    }
}
