using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vibrator : MonoBehaviour
{
    public float speed;
    public float amplitude;

    private Vector3 _startPos;
    private void Start()
    {
        _startPos = transform.position;
    }

    void Update()
    {
        transform.localPosition = _startPos + transform.forward * (Mathf.Sin(Time.time * speed) + amplitude);
    }
}
