using UnityEngine;

public class CubeMover : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;   // gradi al secondo
    [SerializeField] private float moveDistance = 3f;     // ampiezza del movimento
    [SerializeField] private float moveSpeed = 1f;        // velocità del movimento

    private Vector3 startPosition;

    private void Start()
    {
        // Salvo la posizione iniziale del cubo
        startPosition = transform.position;
        Debug.Log("CubeMover avviato su: " + gameObject.name);
    }

    private void Update()
    {
        // Rotazione continua attorno all'asse Y
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);

        // Movimento avanti e indietro lungo l'asse X
        float offset = Mathf.PingPong(Time.time * moveSpeed, moveDistance);
        transform.position = startPosition + new Vector3(offset, 0f, 0f);
    }
}