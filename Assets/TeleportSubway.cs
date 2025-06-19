using UnityEngine;

public class TeleportSubway : MonoBehaviour
{
    public Transform subwayPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Oyuncu bu alana girdiğinde ışınlanma işlemi

            TrainController.Instance.AccelerateAway(); // Tren hızlanmaya başlasın
            other.transform.position = subwayPos.position;
            other.GetComponent<SimplePlayerController>().enabled = true;// Örnek olarak (0, 0, 0) konumuna ışınla
            Debug.Log("Oyuncu ışınlandı!");
            Game3.Instance.GetComponent<Game3>().enabled = false; // Game3'ü başlat
            Destroy(Game3.Instance.gameObject); // Işınlanma alanını yok et
            
        }
    }
}
