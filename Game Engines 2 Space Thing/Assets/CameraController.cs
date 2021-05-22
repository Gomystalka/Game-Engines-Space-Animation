using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform startPoint;
    public Transform target;

    public float moveRate;
    public float rotationRate;
    public bool smoothCameraMotion;

    public Vector3 targetOffset;

    public static CameraController instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        transform.position = startPoint.position;
    }

    private void FixedUpdate()
    {
        if (!target) return;
        //transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * moveRate);
        Vector3 targetOffset = this.targetOffset;
        float realRotationRate = rotationRate;

        Vector3 newPos = target.position + (transform.forward * targetOffset.z) + (transform.right * targetOffset.x) + (transform.up * targetOffset.y);
        transform.position = smoothCameraMotion ? Vector3.Lerp(transform.position, newPos, moveRate * Time.deltaTime) : newPos;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.transform.forward), Time.deltaTime * realRotationRate);
    }

    public void SetCameraTarget(Transform target) => this.target = target;
    public void SetCameraMovementSpeed(float moveRate) => this.moveRate = moveRate;
    public void SetCameraRotationSpeed(float rotationRate) => this.rotationRate = rotationRate;

    public void SetCameraTargetOffsetZ(float z) => targetOffset.z = z;
    public void SetCameraSpeeds(float movementSpeed, float rotationSpeed) {
        moveRate = movementSpeed;
        rotationRate = rotationSpeed;
    }

    public void SetCameraTargetOffset(Vector3 offset) => targetOffset = offset; 
}
