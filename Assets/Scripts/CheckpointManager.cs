using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instance;

    private Vector3 ultimoCheckpoint;
    private bool hayCheckpoint = false;

    [Header("Posicion inicial de la gallina")]
    public Transform spawnInicial;

    void Awake()
    {
        instance = this;
    }

    public void SetCheckpoint(Vector3 posicion)
    {
        ultimoCheckpoint = posicion;
        hayCheckpoint = true;
    }

    public Vector3 GetCheckpoint()
    {
        if (hayCheckpoint)
            return ultimoCheckpoint;
        if (spawnInicial != null)
            return spawnInicial.position;
        Debug.LogError("CheckpointManager: no hay checkpoint activado y 'spawnInicial' no está asignado en el Inspector.");
        return Vector3.zero;
    }
}