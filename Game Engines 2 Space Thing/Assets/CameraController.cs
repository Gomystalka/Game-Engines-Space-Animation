using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform startPoint;

    private void Start()
    {
        transform.position = startPoint.position;
    }
}
