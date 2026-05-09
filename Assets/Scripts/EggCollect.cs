using UnityEngine;

public class EggCollect : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.AddEgg();

            Destroy(gameObject);
        }
    }
}