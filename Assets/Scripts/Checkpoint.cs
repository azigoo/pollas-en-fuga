using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Particulas")]
    public ParticleSystem particulasInactivo; // particulas cuando NO esta activado

    private bool activado = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !activado)
            Activar();
    }

    void Activar()
    {
        activado = true;

        // Apaga las particulas
        if (particulasInactivo != null)
        {
            particulasInactivo.Stop();
            particulasInactivo.Clear();
        }

        // Guarda este checkpoint en el GameManager
        CheckpointManager.instance.SetCheckpoint(transform.position);
        Debug.Log("Checkpoint activado en: " + transform.position);
    }
}