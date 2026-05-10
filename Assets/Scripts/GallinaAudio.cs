using UnityEngine;
using System.Collections;

public class GallinaAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public float minTiempo = 3f; // tiempo mínimo entre cacareos
    public float maxTiempo = 7f; // tiempo máximo entre cacareos

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(RepetirSonido());
    }

    IEnumerator RepetirSonido()
    {
        while (true)
        {
            float espera = Random.Range(minTiempo, maxTiempo);
            yield return new WaitForSeconds(espera);
            audioSource.Play();
        }
    }
}