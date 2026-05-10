using UnityEngine;
using System.Collections;
using System;
using TMPro;

public class IntroNivel : MonoBehaviour
{
    [Header("Titulo del nivel (reutilizable)")]
    [TextArea(1, 2)]
    public string textoTitulo = "La Gran Fuga";

    [Header("Historia (cambia por nivel)")]
    [TextArea(3, 8)]
    [Tooltip("Varios textos: sepáralos con \"Separador bloques\" (por defecto |). Ej.: primer párrafo|segundo párrafo")]
    public string textoHistoria = "Poli lleva toda su vida encerrada en la granja. Por años anelo su libertad, y el dia de hoy se decivio a escapar...";

    [Header("Referencias UI")]
    public TextMeshProUGUI tituloText;
    public TextMeshProUGUI historiaText;
    public TextMeshProUGUI saltarText;
    public RectTransform barraSuperior;
    public RectTransform barraInferior;
    public RectTransform marcoHistoria;

    [Header("Tiempos")]
    public float duracionFade = 1f;
    public float tiempoTitulo = 3f;
    public float duracionBarras = 2f;
    [Tooltip("Segundos con el texto ya completo antes del fade de salida.")]
    public float tiempoHistoria = 5f;

    [Header("Historia — escritura")]
    public float delayPorCaracter = 0.035f;
    public int caracteresPorClick = 28;
    [Tooltip("Mismo string en textoHistoria: bloque1|bloque2. Cambia si necesitas el carácter | en el texto.")]
    public string separadorBloquesHistoria = "|";

    [Header("Posicion barras")]
    public float supFuera = 320f;
    public float supDentro = -30f;
    public float infFuera = -320f;
    public float infDentro = 30f;

    private bool saltando = false;
    private bool enFaseHistoria = false;
    private bool historiaEscribiendo = false;
    private bool historiaEsperandoClickEntreBloques = false;
    private bool historiaClickSiguienteBloque = false;
    private int historiaApuroPendiente = 0;
    private CanvasGroup cgHistoria;

    void Start()
    {
        cgHistoria = marcoHistoria.GetComponent<CanvasGroup>();

        tituloText.text = textoTitulo;
        historiaText.text = "";

        barraSuperior.anchoredPosition = new Vector2(0, supFuera);
        barraInferior.anchoredPosition = new Vector2(0, infFuera);

        tituloText.alpha = 0;
        historiaText.alpha = 1f;
        saltarText.alpha = 1;
        cgHistoria.alpha = 0;

        Time.timeScale = 0f;
        StartCoroutine(SecuenciaIntro());
    }

    void Update()
    {
        if (saltando) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            saltando = true;
            StopAllCoroutines();
            StartCoroutine(SaltarIntro());
            return;
        }

        if (enFaseHistoria && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            if (historiaEsperandoClickEntreBloques)
            {
                historiaClickSiguienteBloque = true;
                return;
            }
            if (historiaEscribiendo)
                historiaApuroPendiente += caracteresPorClick;
            else
            {
                saltando = true;
                StopAllCoroutines();
                StartCoroutine(SaltarHistoria());
            }
        }
    }

    IEnumerator SecuenciaIntro()
    {
        // 1. Barras entran solas
        yield return StartCoroutine(MoverBarras(supFuera, supDentro, infFuera, infDentro, duracionBarras, entrada: true));

        // 2. Titulo aparece con fade
        yield return StartCoroutine(FadeTexto(tituloText, 1f, duracionFade));

        // 3. Titulo visible
        yield return new WaitForSecondsRealtime(tiempoTitulo);

        // 4. Titulo se desvanece
        yield return StartCoroutine(FadeTexto(tituloText, 0f, duracionFade));

        // 5. Barras salen
        yield return StartCoroutine(MoverBarras(supDentro, supFuera, infDentro, infFuera, duracionBarras, entrada: false));

        // 6. Marco de historia + escritura
        yield return StartCoroutine(FadeCanvasGroup(cgHistoria, 1f, duracionFade));
        enFaseHistoria = true;
        historiaEscribiendo = true;
        historiaApuroPendiente = 0;
        yield return StartCoroutine(EscribirHistoria());
        historiaEscribiendo = false;
        yield return new WaitForSecondsRealtime(tiempoHistoria);
        enFaseHistoria = false;
        yield return StartCoroutine(FadeCanvasGroup(cgHistoria, 0f, duracionFade));

        TerminarIntro();
    }

    IEnumerator SaltarIntro()
    {
        tituloText.alpha = 0;
        saltarText.alpha = 0;
        cgHistoria.alpha = 0;
        enFaseHistoria = false;
        historiaEscribiendo = false;
        historiaEsperandoClickEntreBloques = false;

        float desdeSup = barraSuperior.anchoredPosition.y;
        float desdeInf = barraInferior.anchoredPosition.y;
        yield return StartCoroutine(MoverBarras(desdeSup, supFuera, desdeInf, infFuera, 0.35f, entrada: false));

        TerminarIntro();
    }

    IEnumerator SaltarHistoria()
    {
        enFaseHistoria = false;
        yield return StartCoroutine(FadeCanvasGroup(cgHistoria, 0f, duracionFade * 0.35f));
        TerminarIntro();
    }

    void TerminarIntro()
    {
        enFaseHistoria = false;
        historiaEscribiendo = false;
        historiaEsperandoClickEntreBloques = false;
        saltarText.alpha = 0;
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    // entrada: arranca rapido y frena suave
    // salida: arranca suave y acelera al irse
    IEnumerator MoverBarras(float desdeSup, float hastaSup, float desdeInf, float hastaInf, float duracion, bool entrada)
    {
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(tiempo / duracion);
            float t = entrada ? EaseOutSine(u) : EaseInCubic(u);

            barraSuperior.anchoredPosition = new Vector2(0, Mathf.Lerp(desdeSup, hastaSup, t));
            barraInferior.anchoredPosition = new Vector2(0, Mathf.Lerp(desdeInf, hastaInf, t));

            yield return null;
        }

        barraSuperior.anchoredPosition = new Vector2(0, hastaSup);
        barraInferior.anchoredPosition = new Vector2(0, hastaInf);
    }

    IEnumerator EscribirHistoria()
    {
        string sep = string.IsNullOrEmpty(separadorBloquesHistoria) ? "|" : separadorBloquesHistoria;
        string[] bloques = textoHistoria.Split(new[] { sep }, StringSplitOptions.RemoveEmptyEntries);

        for (int b = 0; b < bloques.Length; b++)
        {
            if (b > 0)
            {
                historiaEscribiendo = false;
                historiaApuroPendiente = 0;
                yield return StartCoroutine(EsperarClickSiguienteBloque());
                historiaEscribiendo = true;
            }

            yield return StartCoroutine(EscribirUnBloqueHistoria(bloques[b].Trim()));
        }
    }

    IEnumerator EsperarClickSiguienteBloque()
    {
        historiaEsperandoClickEntreBloques = true;
        historiaClickSiguienteBloque = false;
        while (!historiaClickSiguienteBloque)
            yield return null;
        historiaEsperandoClickEntreBloques = false;
    }

    IEnumerator EscribirUnBloqueHistoria(string full)
    {
        if (full.Length == 0)
            yield break;

        historiaText.text = "";
        int i = 0;
        int len = full.Length;

        while (i < len)
        {
            while (historiaApuroPendiente > 0 && i < len)
            {
                historiaApuroPendiente--;
                i++;
                historiaText.text = full.Substring(0, i);
            }
            if (i >= len)
                break;

            yield return new WaitForSecondsRealtime(delayPorCaracter);
            i++;
            historiaText.text = full.Substring(0, i);
        }

        historiaText.text = full;
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

    // Suave al llegar, sin brusquedad
    static float EaseOutSine(float t) => Mathf.Sin(t * Mathf.PI * 0.5f);
    // Arranca suave y acelera al irse
    static float EaseInCubic(float t) => t * t * t;
}