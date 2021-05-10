using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform startPoint;
    public Transform target;

    public float moveRate;
    public float rotationRate;

    public Vector3 targetOffset;

    public float positionSwitchTargetThreshold;
    public Transform positionSwitchTarget;
    public Vector3 positionSwitchTargetOffset;
    public Vector3 orbitOffset;

    public bool orbit;
    public UnityEngine.Events.UnityEvent onOrbitStart;

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
        if (positionSwitchTarget)
        {
            if (Vector3.Distance(positionSwitchTarget.position, transform.position) <= positionSwitchTargetThreshold)
            {
                SetCameraTarget(positionSwitchTarget);
                targetOffset = positionSwitchTargetOffset;
                if (!orbit)
                {
                    orbit = true;
                    onOrbitStart?.Invoke();
                }
                
            }
        }
        if (orbit) {
            realRotationRate /= 8f;
            Vector3 vv = positionSwitchTarget.position + orbitOffset;
            transform.RotateAround(vv, Vector3.up, realRotationRate * Time.deltaTime);
            transform.LookAt(positionSwitchTarget);

            //transform.position += (transform.forward * orbitOffset.z) + (transform.right * orbitOffset.x) + (transform.up * orbitOffset.y);
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(positionSwitchTarget.position), Time.deltaTime * rotationRate);
            return;
        }

        Vector3 newPos = target.position + (transform.forward * targetOffset.z) + (transform.right * targetOffset.x) + (transform.up * targetOffset.y);
        transform.position = Vector3.Lerp(transform.position, newPos, moveRate * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.transform.forward), Time.deltaTime * realRotationRate);
    }

    public void SetCameraTarget(Transform target) => this.target = target;

    private void OnDrawGizmos()
    {
        if (!positionSwitchTarget) return;
        Gizmos.color = Vector3.Distance(positionSwitchTarget.position, transform.position) <= positionSwitchTargetThreshold ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, positionSwitchTarget.position);
    }
}
