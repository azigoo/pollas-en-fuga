using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VidaUI : MonoBehaviour
{
    public static VidaUI instance;

    [Header("Iconos de vida")]
    public Image[] iconosVida;

    [Header("Fade")]
    public float duracionFade = 0.5f;

    [Header("Tiempos de recuperacion (segundos)")]
    [Tooltip("0 = vida [2] (5s si solo cayo [2], o 5s despues de que [1] ya volvio si cayeron [2] y [1]). 1 = vida [1] (10s).")]
    public float[] tiemposRecuperacion = { 5f, 10f };

    void Awake()
    {
        instance = this;
    }

public void ActualizarVidas(int vidasActuales)
{
    int indicePerdido = vidasActuales;
    if (indicePerdido >= 0 && indicePerdido < iconosVida.Length)
    {
        StartCoroutine(FadeIcono(iconosVida[indicePerdido], false));

        // Si se pierden las 3 vidas no se recupera nada
        if (vidasActuales == 0) return;

        // Solo recupera si NO es la primera vida (indice 0)
        if (indicePerdido > 0)
        {
            float tiempoEspera = indicePerdido == 2 ? tiemposRecuperacion[0] : tiemposRecuperacion[1];
            StartCoroutine(RecuperarVida(indicePerdido, tiempoEspera));
        }
    }
}

IEnumerator RecuperarVida(int indice, float espera)
{
    if (indice == 2)
    {
        yield return new WaitForSeconds(espera);
        if (GameManager.instance.vidasActuales == 0) yield break;

        // Cayeron [2] y [1]: [1] tarda su tiempo (10s) en otra corrutina; [2] solo vuelve 5s despues de que [1] este visible
        if (iconosVida[1].color.a < 0.9f)
        {
            while (iconosVida[1].color.a < 0.999f)
                yield return null;
            yield return new WaitForSeconds(espera);
        }
    }
    else
    {
        yield return new WaitForSeconds(espera);
    }

    if (GameManager.instance.vidasActuales == 0) yield break;

    if (iconosVida[indice].color.a < 0.1f)
    {
        GameManager.instance.vidasActuales++;
        Image img = iconosVida[indice];
        Color c = img.color;
        c.a = 0f;
        img.color = c;
        yield return StartCoroutine(FadeIcono(img, true));
        Debug.Log("Vida recuperada, vidas actuales: " + GameManager.instance.vidasActuales);
    }
}

    IEnumerator FadeIcono(Image icono, bool aparecer)
    {
        float tiempo = 0f;
        Color color = icono.color;
        float alphaInicio = color.a;
        float alphaFinal = aparecer ? 1f : 0f;

        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            color.a = Mathf.Lerp(alphaInicio, alphaFinal, tiempo / duracionFade);
            icono.color = color;
            yield return null;
        }

        color.a = alphaFinal;
        icono.color = color;
    }
    public void RestablecerVidas()
{
    StopAllCoroutines();
    foreach (Image icono in iconosVida)
    {
        Color color = icono.color;
        color.a = 1f;
        icono.color = color;
    }
}
}