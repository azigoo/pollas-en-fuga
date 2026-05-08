using UnityEngine;
using UnityEngine.AI;

public class ChickenNPC : MonoBehaviour
{
    public float walkRadius = 12f;
    public float waitTime = 2f;

    private NavMeshAgent agent;
    private Animator animator;

    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        SetNewDestination();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Animaciones
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Vert", speed);

        // Nuevo destino
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (timer >= waitTime)
            {
                SetNewDestination();
                timer = 0;
            }
        }
    }

    void SetNewDestination()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint =
                Random.insideUnitSphere * walkRadius;

            randomPoint.y = 0;

            randomPoint += transform.position;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                randomPoint,
                out hit,
                walkRadius,
                NavMesh.AllAreas))
            {
                // Evita destinos demasiado cercanos
                if (Vector3.Distance(transform.position, hit.position) > 4f)
                {
                    agent.SetDestination(hit.position);
                    return;
                }
            }
        }
    }
}