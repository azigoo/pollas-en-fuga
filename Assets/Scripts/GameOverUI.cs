using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI instance;

    [Header("Referencias UI")]
    public CanvasGroup fondoOscuro;
    public TextMeshProUGUI mensajeText;
    public TextMeshProUGUI clickText;

    [Header("Fade")]
    public float duracionFade = 0.8f;

    private bool esperandoClick = false;

    void Awake()
    {
        instance = this;
        fondoOscuro.alpha = 0;
        mensajeText.alpha = 0;
        clickText.alpha = 0;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (esperandoClick && Input.GetMouseButtonUp(0))
        {
            esperandoClick = false;
            StartCoroutine(OcultarYRespawn());
        }
    }

    public void MostrarGameOver(string mensaje)
    {
        mensajeText.text = mensaje;
        StartCoroutine(MostrarSecuencia());
    }

    IEnumerator MostrarSecuencia()
    {
        Time.timeScale = 0f;

        // Fondo y mensaje aparecen con fade
        yield return StartCoroutine(FadeCanvasGroup(fondoOscuro, 0.7f, duracionFade));
        yield return StartCoroutine(FadeTexto(mensajeText, 1f, duracionFade));
        yield return StartCoroutine(FadeTexto(clickText, 1f, duracionFade * 0.5f));

        esperandoClick = true;
    }

    IEnumerator OcultarYRespawn()
    {
        // Todo desaparece con fade
        yield return StartCoroutine(FadeTexto(mensajeText, 0f, duracionFade * 0.5f));
        yield return StartCoroutine(FadeTexto(clickText, 0f, duracionFade * 0.5f));
        yield return StartCoroutine(FadeCanvasGroup(fondoOscuro, 0f, duracionFade * 0.5f));

        Time.timeScale = 1f;
        GameManager.instance.RespawnDesdeGameOver();
    }

    IEnumerator FadeTexto(TextMeshProUGUI texto, float targetAlpha, float duracion)
    {
        float tiempo = 0f;
        float inicio = texto.alpha;
        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            texto.alpha = Mathf.Lerp(inicio, targetAlpha, tiempo / duracion);
            yield return null;
        }
        texto.alpha = targetAlpha;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup grupo, float targetAlpha, float duracion)
    {
        float tiempo = 0f;
        float inicio = grupo.alpha;
        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            grupo.alpha = Mathf.Lerp(inicio, targetAlpha, tiempo / duracion);
            yield return null;
        }
        grupo.alpha = targetAlpha;
    }
}