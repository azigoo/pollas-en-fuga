using UnityEngine;

public class CheckpointCinematico : MonoBehaviour
{
    [Header("Tipo de intro")]
    public IntroNivel introNivel;      // para intro completa con barras
    public DialogoNivel dialogoNivel;  // para solo dialogo

    private bool activado = false;

    void Start()
    {
        if (introNivel != null) introNivel.gameObject.SetActive(false);
        if (dialogoNivel != null) dialogoNivel.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !activado)
        {
            activado = true;
            CheckpointManager.instance.SetCheckpoint(transform.position);

            if (introNivel != null) introNivel.Iniciar();
            else if (dialogoNivel != null) dialogoNivel.Iniciar();
        }
    }
}
