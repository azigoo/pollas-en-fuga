using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int eggs = 0;

    public TextMeshProUGUI eggText;

    void Awake()
    {
        instance = this;
    }

    public void AddEgg()
    {
        eggs++;

        eggText.text = "X " + eggs;
    }
}