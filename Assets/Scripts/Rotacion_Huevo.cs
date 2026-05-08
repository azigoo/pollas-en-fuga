using UnityEngine;

public class Rotacion_Huevo : MonoBehaviour
{
    public float rotationSpeed = 10f;
    public float floatHeight = 1f;
    public float floatSpeed = 1f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);



        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        
        transform.position = new Vector3(
            transform.position.x, 
            newY, 
            transform.position.z
        );
    }
}
