using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AranaIA : MonoBehaviour
{
    [Header("Patrulla")]
    public Transform[] puntosPatrulla;
    public float velocidadPatrulla = 1.5f;

    [Header("Detección")]
    public float radioDeteccion = 5f;
    public float radioPerdida = 8f;

    [Header("Ataque")]
    public float radioAtaque = 1.2f;
    public float danioDelay = 0.4f;
    public float cooldownAtaque = 1.5f;

    [Header("Rotación")]
    public float velocidadRotacion = 10f;

    private NavMeshAgent agente;
    private Animator anim;
    private Transform gallina;

    private enum Estado { Patrullando, Persiguiendo, Atacando }
    private Estado estado = Estado.Patrullando;

    private int indicePunto = 0;
    private bool puedoAtacar = true;
    private bool danioAplicado = false;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null) gallina = obj.transform;

        agente.speed = velocidadPatrulla;
        agente.updateRotation = false; // Nosotros manejamos la rotación

        if (puntosPatrulla.Length > 0)
            agente.SetDestination(puntosPatrulla[0].position);
    }

    void Update()
    {
        if (gallina == null) return;

        float distancia = Vector3.Distance(transform.position, gallina.position);

        switch (estado)
        {
            case Estado.Patrullando:
                Patrullar();
                RotarHacia(agente.velocity.normalized); // Rota hacia donde camina
                if (distancia <= radioDeteccion)
                    CambiarEstado(Estado.Persiguiendo);
                break;

            case Estado.Persiguiendo:
                Perseguir(distancia);
                RotarHacia(agente.velocity.normalized); // Rota hacia donde camina
                break;

            case Estado.Atacando:
                // Rota hacia la gallina directamente
                Vector3 direccionGallina = (gallina.position - transform.position).normalized;
                direccionGallina.y = 0;
                RotarHacia(direccionGallina);
                break;
        }

        anim.SetBool("estaMoviendo", agente.velocity.magnitude > 0.1f);
    }

    // Toda la rotación pasa por aquí con el offset de 180°
    void RotarHacia(Vector3 direccion)
    {
        if (direccion.sqrMagnitude < 0.01f) return;

        Quaternion rotObjetivo = Quaternion.LookRotation(direccion);
        rotObjetivo *= Quaternion.Euler(0, 180, 0); // Corrige el modelo invertido
        transform.rotation = Quaternion.Slerp(transform.rotation, rotObjetivo, Time.deltaTime * velocidadRotacion);
    }

    void Patrullar()
    {
        if (puntosPatrulla.Length == 0) return;

        if (!agente.pathPending && agente.remainingDistance < 0.5f)
        {
            indicePunto = (indicePunto + 1) % puntosPatrulla.Length;
            agente.SetDestination(puntosPatrulla[indicePunto].position);
        }
    }

    void Perseguir(float distancia)
    {
        if (distancia <= radioAtaque && puedoAtacar)
        {
            CambiarEstado(Estado.Atacando);
            return;
        }

        if (distancia > radioPerdida)
        {
            CambiarEstado(Estado.Patrullando);
            return;
        }

        agente.SetDestination(gallina.position);
    }

    void CambiarEstado(Estado nuevo)
    {
        estado = nuevo;

        if (nuevo == Estado.Patrullando)
        {
            agente.speed = velocidadPatrulla;
            agente.isStopped = false;
            if (puntosPatrulla.Length > 0)
                agente.SetDestination(puntosPatrulla[indicePunto].position);
        }
        else if (nuevo == Estado.Persiguiendo)
        {
            agente.speed = velocidadPatrulla * 1.8f;
            agente.isStopped = false;
        }
        else if (nuevo == Estado.Atacando)
        {
            agente.isStopped = true;
            StartCoroutine(RealizarAtaque());
        }
    }

    IEnumerator RealizarAtaque()
    {
        puedoAtacar = false;
        danioAplicado = false;

        anim.SetBool("estaAtacando", true);

        yield return new WaitForSeconds(danioDelay);

        if (!danioAplicado)
        {
            float dist = Vector3.Distance(transform.position, gallina.position);
            if (dist <= radioAtaque * 1.3f)
            {
                GallinaVida vida = gallina.GetComponent<GallinaVida>();
                if (vida != null) vida.RecibirDano();
                danioAplicado = true;
            }
        }

        yield return new WaitForSeconds(cooldownAtaque - danioDelay);

        anim.SetBool("estaAtacando", false);
        puedoAtacar = true;

        float distFinal = Vector3.Distance(transform.position, gallina.position);
        CambiarEstado(distFinal <= radioDeteccion ? Estado.Persiguiendo : Estado.Patrullando);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioAtaque);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, radioPerdida);
    }
}