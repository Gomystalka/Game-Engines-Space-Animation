using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpaceGame.Interfaces;

[RequireComponent(typeof(Rigidbody))]
public class ShipAI : MonoBehaviour, IBehaviour
{
    public static List<ShipAI> usakiGroup = new List<ShipAI>();

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

    public Vector3 RealPosition => transform.position + positionOffset;
    private Vector3 _currentDirection;

    private Animator _anim;

    [Header("Weapons")]
    public Vector3 projectileSourceOffset;
    public Vector3 ProjectileSource => transform.position + (transform.forward * projectileSourceOffset.z) + (transform.right * projectileSourceOffset.x) + (transform.up * projectileSourceOffset.y);

    public Transform bakuUsa;

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
            flySpeed = WorldManager.RandomUsakiSpeed;
        }

        _currentDirection = transform.forward;
        _anim = GetComponent<Animator>();

        bakuUsa = transform.FindChildInChildrenByName("BakuUsa");
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
            _currentState = team == Team.Nyami ? AIState.Targeting : AIState.Flocking;
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
            _anim.SetTrigger("Fire");

            for (int i = 0; i < 2; i++) {
                Projectile proj = Instantiate(WorldManager.instance.laserPrefab, ProjectileSource, Quaternion.LookRotation(transform.forward));
                proj.AssignRigidbody();
                proj.Velocity = transform.forward * 100f;
                Destroy(proj.gameObject, 6f);
            }
        }
    }

    private bool _readyToFire = false;

    public void OnFlock()
    {
        if (team != Team.Usada) return;
        if (Vector3.Distance(transform.position, target.position) <= rangeThreshold)
        {
            OnFlockTargetReached();
            _readyToFire = true;
            FireAtTarget();
            return;
        }


        short usakiCount = 0;
        Vector3 collisionAvoidance = Vector3.zero;
        float flightSpeed = WorldManager.RandomUsakiSpeed;
        Vector3 flockCenter = Vector3.zero;
        Vector3 directionToTarget = target.position - RealPosition;

        foreach (ShipAI usaki in usakiGroup) {
            if (usaki == this) continue;
            float distance = Vector3.Distance(usaki.RealPosition, RealPosition);
            Vector3 direction = RealPosition - usaki.RealPosition;

            if (distance <= maximumFlockingDistance) {
                if (distance < hitboxSize)
                    collisionAvoidance += direction;

                flockCenter += usaki.RealPosition;
                flightSpeed += usaki.flySpeed;
                usakiCount++;
            }
        }
        if (usakiCount == 0) return;
        flockCenter /= usakiCount;
        flockCenter += directionToTarget;

        flySpeed = flightSpeed / usakiCount;
        if (flySpeed > WorldManager.instance.maxUsakiSpeed)
            flySpeed = WorldManager.instance.maxUsakiSpeed;

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
    }

    private void OnMinimumSpeedReached() {
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
        Projectile proj = a.bakuUsa.gameObject.AddComponent<Projectile>();
        proj.AssignRigidbody();
        proj.transform.LookAt(transform);
        proj.Velocity = transform.forward * 100f;
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