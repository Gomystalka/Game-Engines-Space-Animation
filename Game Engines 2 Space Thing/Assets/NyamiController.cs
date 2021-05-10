using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NyamiController : MonoBehaviour
{
    private Animator[] _animators;
    public Transform target;

    public float speed;
    public float turnSpeed;

    public UnityEngine.Events.UnityEvent onTargetReached;
    private bool _hasTargetBeenReached;

    private void Start()
    {
        _animators = GetComponentsInChildren<Animator>();
    }

    private void Update()
    {
        bool moving = speed != 0 && target;
        if (!moving) return;

        if (transform.position != target.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * speed);
            _hasTargetBeenReached = false;
        }
        else {
            if (!_hasTargetBeenReached) {
                _hasTargetBeenReached = true;
                OnTargetReached();
            }
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.position), Time.deltaTime * turnSpeed);

        foreach (Animator anim in _animators)
            anim.SetBool("Moving", moving);
    }

    private void OnTargetReached() {
        onTargetReached?.Invoke();
    }

    public void SetParent(Transform transform) => transform.SetParent(transform);

    public void SetSpeed(float speed) => this.speed = speed;
}
