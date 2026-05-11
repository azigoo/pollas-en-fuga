using UnityEngine;
using System.Collections;
using System;
using TMPro;

public class DialogoNivel : MonoBehaviour
{
    [Header("Historia")]
    [TextArea(3, 8)]
    [Tooltip("Separa bloques con |")]
    public string textoHistoria = "Escribe el dialogo aqui...";

    [Header("Referencias UI")]
    public TextMeshProUGUI historiaText;
    public TextMeshProUGUI saltarText;
    public RectTransform marcoHistoria;

    [Header("Tiempos")]
    public float duracionFade = 1f;
    public float tiempoEspera = 3f;
    public float delayPorCaracter = 0.035f;
    public int caracteresPorClick = 28;
    public string separadorBloques = "|";

    private bool saltando = false;
    private bool enFaseHistoria = false;
    private bool historiaEscribiendo = false;
    private bool historiaEsperandoClick = false;
    private bool historiaClickSiguiente = false;
    private int historiaApuroPendiente = 0;
    private CanvasGroup cgHistoria;

    void Start() { }

    public void Iniciar()
    {
        gameObject.SetActive(true);
        cgHistoria = marcoHistoria.GetComponent<CanvasGroup>();

        historiaText.text = "";
        saltarText.alpha = 1;
        cgHistoria.alpha = 0;

        saltando = false;
        enFaseHistoria = false;
        historiaEscribiendo = false;
        historiaApuroPendiente = 0;

        Time.timeScale = 0f;
        StartCoroutine(Secuencia());
    }

    void Update()
    {
        if (saltando) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            saltando = true;
            StopAllCoroutines();
            StartCoroutine(Saltar());
            return;
        }

        if (enFaseHistoria && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            if (historiaEsperandoClick)
            {
                historiaClickSiguiente = true;
                return;
            }
            if (historiaEscribiendo)
                historiaApuroPendiente += caracteresPorClick;
            else
            {
                saltando = true;
                StopAllCoroutines();
                StartCoroutine(Saltar());
            }
        }
    }

    IEnumerator Secuencia()
    {
        yield return StartCoroutine(FadeCanvasGroup(cgHistoria, 1f, duracionFade));
        enFaseHistoria = true;
        historiaEscribiendo = true;
        yield return StartCoroutine(EscribirHistoria());
        historiaEscribiendo = false;
        yield return new WaitForSecondsRealtime(tiempoEspera);
        enFaseHistoria = false;
        yield return StartCoroutine(FadeCanvasGroup(cgHistoria, 0f, duracionFade));
        Terminar();
    }

    IEnumerator Saltar()
    {
        saltarText.alpha = 0;
        cgHistoria.alpha = 0;
        enFaseHistoria = false;
        historiaEscribiendo = false;
        yield return null;
        Terminar();
    }

    void Terminar()
    {
        enFaseHistoria = false;
        historiaEscribiendo = false;
        saltarText.alpha = 0;
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    IEnumerator EscribirHistoria()
    {
        string sep = string.IsNullOrEmpty(separadorBloques) ? "|" : separadorBloques;
        string[] bloques = textoHistoria.Split(new[] { sep }, StringSplitOptions.RemoveEmptyEntries);

        for (int b = 0; b < bloques.Length; b++)
        {
            if (b > 0)
            {
                historiaEscribiendo = false;
                historiaApuroPendiente = 0;
                yield return StartCoroutine(EsperarClick());
                historiaEscribiendo = true;
            }
            yield return StartCoroutine(EscribirBloque(bloques[b].Trim()));
        }
    }

    IEnumerator EsperarClick()
    {
        historiaEsperandoClick = true;
        historiaClickSiguiente = false;
        while (!historiaClickSiguiente)
            yield return null;
        historiaEsperandoClick = false;
    }

    IEnumerator EscribirBloque(string full)
    {
        if (full.Length == 0) yield break;

        historiaText.text = "";
        int i = 0;

        while (i < full.Length)
        {
            while (historiaApuroPendiente > 0 && i < full.Length)
            {
                historiaApuroPendiente--;
                i++;
                historiaText.text = full.Substring(0, i);
            }
            if (i >= full.Length) break;

            yield return new WaitForSecondsRealtime(delayPorCaracter);
            i++;
            historiaText.text = full.Substring(0, i);
        }

        historiaText.text = full;
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