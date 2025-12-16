using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1.0f;
    public float damagePerHit = 10f;

    [Header("Movement Settings")]
    public float runSpeed = 3.5f;
    public float fleeSpeed = 15f;
    public float fleeRunDistance = 200f;
    public float fleeDespawnDelay = 20f;

    NavMeshAgent agent;
    Animator animator;
    Transform player;
    PlayerStats playerStats;

    float lastAttackTime = -999f;
    bool fleeing;
    float fleeStartTime;
    Vector3 fleeStartPos;
    Vector3 fleeDirection;

    void Awake()
    {
        // Grab required components on this enemy
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.updateRotation = false;
    }

    void Start()
    {
        // Find the player using the Player tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerStats = playerObj.GetComponent<PlayerStats>();
        }

        // Make sure the enemy starts on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        // Basic movement setup
        agent.speed = runSpeed;
        agent.stoppingDistance = attackRange * 0.8f;
    }

    void Update()
    {
        // Set animation speed (runs faster when fleeing)
        animator.SetFloat("Speed", fleeing ? fleeSpeed : agent.velocity.magnitude);

        // If we don't have a player do nothing
        if (player == null)
            return;

        // If not fleeing and somehow not on the NavMesh do nothing
        if (!fleeing && !agent.isOnNavMesh)
            return;

        if (fleeing)
        {
            FleeUpdate();
        }
        else
        {
            ChaseAndAttackBehaviour();
        }

        // Make the enemy face the toward player or away when fleeing
        Vector3 lookDir;
        if (fleeing)
            lookDir = fleeDirection;
        else
            lookDir = player.position - transform.position;

        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    void ChaseAndAttackBehaviour()
    {
        Vector3 selfPos = transform.position;
        Vector3 playerPos = player.position;
        selfPos.y = 0f;
        playerPos.y = 0f;

        float dist = Vector3.Distance(selfPos, playerPos);

        // If too far chase the player
        if (dist > attackRange)
        {
            agent.isStopped = false;
            agent.speed = runSpeed;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            // Only attack if the cooldown is ready
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                animator.SetTrigger("Bite");

                // Apply damage to the player 
                if (playerStats != null)
                    playerStats.TakeDamage(damagePerHit);
            }
        }
    }

    void FleeUpdate()
    {
        // a straight line away from the player
        float dt = Time.deltaTime;
        transform.position += fleeDirection * fleeSpeed * dt;

        float distRun = Vector3.Distance(fleeStartPos, transform.position);
        float timeRun = Time.time - fleeStartTime;

        // Despawn after running far enough or after a set time
        if (distRun >= fleeRunDistance || timeRun >= fleeDespawnDelay)
            Destroy(gameObject);
    }

    public void RunAwayAndDespawn()
    {
        // Don't start fleeing twice
        if (fleeing) return;

        fleeing = true;
        fleeStartTime = Time.time;
        fleeStartPos = transform.position;

        // Pick a direction away from the player or fallback to backward direction
        if (player != null)
        {
            Vector3 selfPos = transform.position;
            Vector3 playerPos = player.position;
            Vector3 awayDir = (selfPos - playerPos).normalized;

            if (awayDir.sqrMagnitude < 0.0001f)
                awayDir = -transform.forward;

            fleeDirection = awayDir.normalized;
        }
        else
        {
            fleeDirection = -transform.forward;
        }

        // Turn off NavMesh control once fleeing is active
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
    }
}
