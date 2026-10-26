using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    private enum State { Idle, Chase, Attack, Dead }

    [Header("Riferimenti")]
    [SerializeField] private Transform target; // se vuoto, cerca da solo il giocatore

    [Header("Rilevamento")]
    [SerializeField] private float detectionRange = 15f; // distanza a cui nota il giocatore
    [SerializeField] private float loseRange = 25f;      // distanza oltre cui lo perde

    [Header("Movimento")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float repathInterval = 0.2f; // ogni quanto aggiorna il percorso

    [Header("Attacco")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Morte")]
    [SerializeField] private float despawnDelay = 2f;

    private State state = State.Idle;
    private NavMeshAgent agent;
    private Health health;
    private Health targetHealth;
    private float nextAttackTime;
    private float nextRepathTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.OnDied += HandleDied;
        health.OnDamaged += HandleDamaged;
    }

    private void OnDisable()
    {
        health.OnDied -= HandleDied;
        health.OnDamaged -= HandleDamaged;
    }

    private void Start()
    {
        if (target == null)
        {
            FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
            if (player != null) target = player.transform;
        }

        if (target != null) targetHealth = target.GetComponent<Health>();

        agent.speed = chaseSpeed;
        agent.stoppingDistance = attackRange * 0.8f; // si ferma un po' prima del limite d'attacco
    }

    private void Update()
    {
        if (state == State.Dead) return;

        // Se non c'è il giocatore, o è morto, resta fermo
        if (target == null || (targetHealth != null && targetHealth.IsDead))
        {
            if (state != State.Idle) ChangeState(State.Idle);
            return;
        }

        float distance = FlatDistanceToTarget();

        switch (state)
        {
            case State.Idle:
                if (distance <= detectionRange) ChangeState(State.Chase);
                break;

            case State.Chase:
                if (distance > loseRange)
                {
                    ChangeState(State.Idle);
                }
                else if (distance <= attackRange)
                {
                    ChangeState(State.Attack);
                }
                else if (Time.time >= nextRepathTime)
                {
                    nextRepathTime = Time.time + repathInterval;
                    agent.SetDestination(target.position);
                }
                break;

            case State.Attack:
                FaceTarget();
                if (distance > attackRange + 0.5f) // margine per non oscillare tra Chase e Attack
                {
                    ChangeState(State.Chase);
                }
                else if (Time.time >= nextAttackTime)
                {
                    Attack();
                }
                break;
        }
    }

    private void ChangeState(State newState)
    {
        state = newState;

        switch (newState)
        {
            case State.Idle:
            case State.Attack:
                agent.isStopped = true;
                agent.ResetPath();
                break;

            case State.Chase:
                agent.isStopped = false;
                nextRepathTime = 0f; // ricalcola subito il percorso
                break;
        }
    }

    private void Attack()
    {
        nextAttackTime = Time.time + attackCooldown;
        if (targetHealth != null) targetHealth.TakeDamage(attackDamage);
    }

    private void FaceTarget()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion look = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationSpeed * Time.deltaTime);
    }

    private float FlatDistanceToTarget()
    {
        // Ignoriamo la differenza di altezza (es. quando il giocatore salta)
        Vector3 delta = target.position - transform.position;
        delta.y = 0f;
        return delta.magnitude;
    }

    private void HandleDamaged(float amount)
    {
        // Se lo colpisci da lontano, ti nota subito
        if (state == State.Idle && target != null) ChangeState(State.Chase);
    }

    private void HandleDied()
    {
        state = State.Dead;
        agent.enabled = false;

        // Senza collider i colpi successivi passano attraverso il corpo
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        Destroy(gameObject, despawnDelay);
    }

    // Mostra i raggi d'azione nella finestra Scene quando selezioni il nemico
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}