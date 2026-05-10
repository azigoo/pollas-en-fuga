using UnityEngine;

public class Enemigo : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        Debug.Log("Colisión con: " + col.gameObject.name + " tag: " + col.gameObject.tag);

        if (col.gameObject.CompareTag("Player"))
        {
            GallinaVida vida = col.gameObject.GetComponent<GallinaVida>();
            if (vida != null)
                vida.RecibirDano();
            else
                Debug.Log("No encontró GallinaVida en la gallina");
        }
    }
}