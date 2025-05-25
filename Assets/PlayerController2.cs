using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Simple3DMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Input: A/D or Left/Right ve W/S veya Up/Down
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Dünya uzayında XZ düzleminde hareket vektörü
        Vector3 move = new Vector3(h, 0f, v) * speed;

        // Rigidbody ile direkt hız ata
        rb.linearVelocity = move;
    }
}