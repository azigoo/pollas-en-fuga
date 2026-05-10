using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Huevos")]
    public int eggs = 0;
    public TextMeshProUGUI eggText;

    [Header("Vidas")]
    public int vidasMaximas = 3;
    public int vidasActuales = 3;

    void Awake()
    {
        instance = this;
    }

    public void AddEgg()
    {
        eggs++;
        eggText.text = "X " + eggs;
    }

    public void QuitarVida()
    {
        vidasActuales--;
        VidaUI.instance.ActualizarVidas(vidasActuales);

        if (vidasActuales <= 0)
            GameOverTipo1();

    }

    public void SalioDelMapa()
    {
        GameOverTipo2();
    }

    void GameOverTipo1()
    {
        GameOverUI.instance.MostrarGameOver("Te faltaron huevos para escapar.");
    }

    void GameOverTipo2()
    {
        GameOverUI.instance.MostrarGameOver("¡Aun no estas lista, Poli!\nRegresa y junta todos los huevos\npara desafiar el destino.");
    }

    public void RespawnDesdeGameOver()
    {
        vidasActuales = vidasMaximas;
        if (VidaUI.instance != null)
            VidaUI.instance.RestablecerVidas();
        RespawnGallina();
    }

    void RespawnGallina()
    {
        if (CheckpointManager.instance == null)
        {
            Debug.LogError("GameManager: no hay CheckpointManager en la escena. Añade el componente a un GameObject.");
            return;
        }

        Vector3 pos = CheckpointManager.instance.GetCheckpoint();
        GameObject gallina = GameObject.FindGameObjectWithTag("Player");
        if (gallina == null)
        {
            Debug.LogError("GameManager: no se encontró ningún objeto con tag \"Player\".");
            return;
        }

        // CharacterController guarda posición interna: hay que desactivarlo un instante o el teleport no aplica.
        CharacterController cc = gallina.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            gallina.transform.position = pos;
            cc.enabled = true;
        }
        else
        {
            gallina.transform.position = pos;
        }
    }
}