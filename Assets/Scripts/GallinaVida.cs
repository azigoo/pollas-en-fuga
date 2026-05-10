using UnityEngine;
using System.Collections;

public class GallinaVida : MonoBehaviour
{
    [Header("Invencibilidad tras recibir daño")]
    public float tiempoInvencible = 1.5f;
    private bool esInvencible = false;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Enemigo"))
            RecibirDano();
    }

    public void RecibirDano()
    {
        if (esInvencible) return;

        Debug.Log("Daño recibido");
        GameManager.instance.QuitarVida();
        StartCoroutine(TiempoInvencible());
    }

    IEnumerator TiempoInvencible()
    {
        esInvencible = true;
        yield return new WaitForSeconds(tiempoInvencible);
        esInvencible = false;
    }
}