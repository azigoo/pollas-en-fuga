using UnityEngine;

public class LimiteMapa : MonoBehaviour
{
        void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameManager.instance.SalioDelMapa();
    }
    
}
