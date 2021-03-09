using UnityEngine;
using SpaceGame.Interfaces;

[RequireComponent(typeof(Rigidbody))]
public class ShipAI : MonoBehaviour, IBehaviour
{
    private Rigidbody _rigidBody;

    private AIState _currentState = AIState.Idle;

    [Header("Ship Settings")]
    public float flySpeed = 1000f;
    public float rotationRate = 10f;

    [Header("Behaviour")]
    public Transform target;
    public bool randomizeTarget;
    public Transform targetDummy;
    public float rangeThreshold = 10f;

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        if (randomizeTarget)
        {
            target = targetDummy;
            targetDummy.position = WorldManager.QueryRandomWorldPoint();
        }
    }

    void Update()
    {
        //UpdateBehaviourTree();
    }

    private void FixedUpdate()
    {
        UpdateBehaviourTree();
        ApplyThrust();
    }

    private void UpdateBehaviourTree() {
        switch (_currentState)
        {
            case AIState.Targeting:
                OnTargeting();
                break;
            case AIState.InCombat:
                OnInCombat();
                break;
            case AIState.LowHP:
                OnLowHP();
                break;
            case AIState.Returning:
                OnReturning();
                break;
            default: //Idle
                OnIdle();
                break;
        }
    }

    public void OnDamaged()
    {
    }

    public void OnIdle()
    {
        if (target)
            _currentState = AIState.Targeting;
    }

    public void OnInCombat()
    {
    }

    public void OnLowHP()
    {
    }

    public void OnReturning()
    {
    }

    public void OnTargeting()
    {
        if (!target) { _currentState = AIState.Idle; return; }
        if (randomizeTarget)
        {
            if (Vector3.Distance(transform.position, targetDummy.position) < rangeThreshold)
                targetDummy.position = WorldManager.QueryRandomWorldPoint();
        }
        Quaternion rot = Quaternion.LookRotation(target.position - transform.position);
        rot = Quaternion.RotateTowards(transform.rotation, rot, rotationRate * Time.fixedDeltaTime);
        _rigidBody.MoveRotation(rot);
    }

    private void ApplyThrust() {
        if (_currentState != AIState.Idle)
            _rigidBody.velocity = transform.forward * flySpeed * Time.fixedDeltaTime;
    }
}

public enum AIState { 
    Idle,
    Targeting,
    InCombat,
    LowHP,
    Returning
}
