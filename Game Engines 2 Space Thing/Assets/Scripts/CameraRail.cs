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
    //public Rail[] paths;

#if UNITY_EDITOR
    [Header("Gizmo Settings")]
    public Color pointGizmoColor = Color.yellow;
    public Color railGizmoColor = Color.green;
    public Color bakedPointGizmoColor = Color.magenta;
    public float railGizmoScale = 1f;
    public bool drawNonBakedPointsIfBaked = true;

    private void OnDrawGizmos()
    {

        if (currentRail == null) return;
        Vector3 scale = Vector3.one * railGizmoScale;
            if ((currentRail.ArePointsBaked && drawNonBakedPointsIfBaked) || !currentRail.ArePointsBaked)
            {
                Gizmos.color = pointGizmoColor;
                for (int i = 0; i < currentRail.railPoints.Length; i++)
                    Gizmos.DrawSphere(currentRail.railPoints[i].position, railGizmoScale);
                Gizmos.color = railGizmoColor;
                for (float t = 0; t < 1f; t += currentRail.pointDensity)
                    Gizmos.DrawCube(currentRail.GetPointAtTime(t), scale);
            }
            if (!currentRail.ArePointsBaked) return;
            Gizmos.color = bakedPointGizmoColor;
            for (int i = 0; i < currentRail.BakedPoints.Length; i++)
                Gizmos.DrawCube(currentRail.BakedPoints[i], scale);
    }
#endif
    private void Start()
    {
        currentRail.BakePoints(GetComponent<LineRenderer>());
#if !UNITY_EDITOR
        Destroy(GetComponent<LineRenderer>());
#endif
    }
}

[System.Serializable]
public class Rail {
    public Transform[] railPoints;
    [Range(0.0001f, 1f)] public float pointDensity = 0.01f;

    private Vector3[] _bakedPoints;
    public Vector3[] BakedPoints => _bakedPoints;
    public bool ArePointsBaked => _bakedPoints != null;

    public Vector3 GetPointAtTime(float t)
    { //Cubic Bezier Curve Formula
        if (railPoints == null) return Vector3.zero;
        t = t.Clamp01();
        return Mathf.Pow(1f - t, 3f) * railPoints[0].position +
            3f * Mathf.Pow(1f - t, 2f) * t * railPoints[1].position +
            3f * (1 - t) * Mathf.Pow(t, 2f) * railPoints[2].position + 
            Mathf.Pow(t, 3f) * railPoints[3].position;
    }

    public void BakePoints(LineRenderer lineRenderer = null) {
        _bakedPoints = new Vector3[Mathf.RoundToInt(1f / pointDensity)];
        for (int i = 0; i < _bakedPoints.Length; i++)
            _bakedPoints[i] = GetPointAtTime(i * pointDensity);

#if UNITY_EDITOR
        if (lineRenderer)
        {
            lineRenderer.positionCount = _bakedPoints.Length;
            lineRenderer.SetPositions(_bakedPoints);
        }
#endif
    }
}
