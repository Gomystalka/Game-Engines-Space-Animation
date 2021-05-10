using UnityEngine;

[System.Serializable]
public class Rail : MonoBehaviour
{
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

    public void BakePoints(LineRenderer lineRenderer = null)
    {
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