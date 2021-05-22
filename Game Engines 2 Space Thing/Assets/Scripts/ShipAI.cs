using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpaceGame.Interfaces;

[RequireComponent(typeof(Rigidbody))]
public class ShipAI : MonoBehaviour, IBehaviour
{
    public static List<ShipAI> usakiGroup = new List<ShipAI>();
    public static List<ShipAI> nyamiGroup = new List<ShipAI>();
    public const float kNyamiFlightSpeed = 5f;

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

    public Team team;

    [Header("Flocking")]
    public float hitboxSize;
    public float maximumFlockingDistance;
    public Vector3 positionOffset;
    public bool doNotFlock;
    public float maximumFlightSpeed;

    public Vector3 RealPosition => transform.position + positionOffset;
    private Vector3 _currentDirection;

    private Animator _anim;

    [Header("Weapons")]
    public Vector3 projectileSourceOffset;
    public Vector3 ProjectileSource => transform.position + (transform.forward * projectileSourceOffset.z) + (transform.right * projectileSourceOffset.x) + (transform.up * projectileSourceOffset.y);

    public Transform bakuUsa;

    public OneTimeEvent onFlockTargetReached;

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        if (randomizeTarget)
        {
            target = targetDummy;
            targetDummy.position = WorldManager.QueryRandomWorldPoint();
        }

        if (team == Team.Usada)
        {
            usakiGroup.Add(this);
            flySpeed = WorldManager.RandomShipSpeed;
        }
        else {
            nyamiGroup.Add(this);
            flySpeed = WorldManager.RandomShipSpeed;
        }

        _currentDirection = transform.forward;
        _anim = GetComponent<Animator>();

        bakuUsa = transform.FindChildInChildrenByName("BakuUsa");
        if (bakuUsa)
            bakuUsa.GetComponentInChildren<TrailRenderer>().enabled = false;
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
            case AIState.Flocking:
                OnFlock();
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
            _currentState = AIState.Flocking;
    }

    public void OnInCombat()
    {
        if (target) {
            transform.LookAt(target);
            FireAtTarget();
        }
    }

    private float _nextFire = 0f;
    public float fireRate;

    private void FireAtTarget() {
        if (!target) return;
        if (Time.time >= _nextFire) {
            _nextFire = Time.time + 1f / fireRate;
            if(_anim)
                _anim.SetTrigger("Fire");

            for (int i = 0; i < 2; i++) {
                Projectile proj = Instantiate(WorldManager.instance.laserPrefab, ProjectileSource, Quaternion.LookRotation(transform.forward));
                proj.AssignRigidbody();
                proj.Velocity = transform.forward * 100f;
                Destroy(proj.gameObject, 6f);
            }
        }
    }

    public void OnFlock()
    {
        //if (team != Team.Usada) return;

        if (Vector3.Distance(transform.position, target.position) <= rangeThreshold)
        {
            OnFlockTargetReached();
            FireAtTarget();
            return;
        }

        short shipCount = 0;
        Vector3 collisionAvoidance = Vector3.zero;
        float flightSpeed = WorldManager.RandomShipSpeed;
        Vector3 flockCenter = Vector3.zero;
        Vector3 directionToTarget = target.position - RealPosition;
        List<ShipAI> shipGroup = team == Team.Usada ? usakiGroup : nyamiGroup;

        foreach (ShipAI ship in shipGroup) {
            if (ship == this) continue;
            if (ship.doNotFlock) continue;
            float distance = Vector3.Distance(ship.RealPosition, RealPosition);
            Vector3 direction = RealPosition - ship.RealPosition;

            if (distance <= maximumFlockingDistance) {
                if (distance < hitboxSize)
                    collisionAvoidance += direction;

                flockCenter += ship.RealPosition;
                flightSpeed += ship.flySpeed;
                shipCount++;
            }
        }
        if (shipCount == 0) return;
        flockCenter /= shipCount;
        flockCenter += directionToTarget;

        flySpeed = flightSpeed / shipCount;
        if (flySpeed > maximumFlightSpeed)
            flySpeed = maximumFlightSpeed;

        _currentDirection = flockCenter + collisionAvoidance - RealPosition;
        if (_currentDirection != Vector3.zero)
        {
            Quaternion rotationTowardsTarget = Quaternion.LookRotation(_currentDirection);
            Quaternion rot = Quaternion.Lerp(transform.rotation, rotationTowardsTarget, rotationRate * Time.fixedDeltaTime);
            _rigidBody.MoveRotation(rot);
        }
    }

    private void OnFlockTargetReached() {
        //flySpeed -= Time.deltaTime * WorldManager.instance.usakiDecelerationRate;
        //flySpeed = Mathf.Clamp(flySpeed, WorldManager.instance.minUsakiSpeed, WorldManager.instance.maxUsakiSpeed);
        onFlockTargetReached?.InvokeOneTime();
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
            //_rigidBody.Translate(0, 0, Time.deltaTime * flySpeed);
            _rigidBody.velocity = transform.forward * flySpeed * Time.fixedDeltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(RealPosition, Vector3.one * hitboxSize);

        Gizmos.color = Color.magenta;
        Gizmos.DrawCube(ProjectileSource, Vector3.one * 0.5f);
    }

    public static Transform FireRandomBakuUsaAt(Transform transform) {
        ShipAI a = usakiGroup[Random.Range(0, usakiGroup.Count)];
        if (!a || !a.bakuUsa) return transform;
        a.bakuUsa.SetParent(null);
        a.bakuUsa.gameObject.AddComponent<Rigidbody>().useGravity = false;
        TrailRenderer trail = a.bakuUsa.GetComponentInChildren<TrailRenderer>();
        if (trail)
            trail.enabled = true;
        Projectile proj = a.bakuUsa.gameObject.AddComponent<Projectile>();
        proj.AssignRigidbody();
        proj.transform.LookAt(transform);
        proj.turnRate = 20f;
        proj.projectileSpeed = WorldManager.instance.bakuHatsuSpeed;
        proj.Velocity = transform.forward * WorldManager.instance.bakuHatsuSpeed;
        proj.SetTriggerStatus(true);
        proj.homeTarget = transform;
        return a.bakuUsa;
    }
}

public enum AIState { 
    Idle,
    Targeting,
    InCombat,
    Flocking,
    Returning
}

public enum Team { 
    Usada,
    Nyami
}

[System.Serializable]
public class OneTimeEvent : UnityEngine.Events.UnityEvent {
    private bool _eventFired;
    public bool HasEventFired => _eventFired;

    public void InvokeOneTime()
    {
        if (_eventFired) return;
        //if (GetPersistentEventCount() <= 0) return;
        _eventFired = true;
        Invoke();
    }

    public void Reset() => _eventFired = false;
}