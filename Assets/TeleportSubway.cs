using UnityEngine;

public class TeleportSubway : MonoBehaviour
{
    public Transform subwayPos;
    public GameObject train;
    public GameObject game3; // Game3 prefab or instance reference


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
            train.GetComponent<TrainController>().AccelerateAway(); // Tren hızlanmaya başlasın
            other.transform.position = subwayPos.position;
            other.gameObject.GetComponent<SimplePlayerController>().enabled = true; // Örnek olarak (0, 0, 0) konumuna ışınla
            Debug.Log("Oyuncu ışınlandı!");
            game3.SetActive(false);// Game3'ü başlat
        }
        else
        {
            Destroy(other); // Eğer oyuncu değilse bu objeyi yok et
         }

    }
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Oyuncu bu alanda kaldığında trenin hızlanmasını sağla
            train.GetComponent<TrainController>().AccelerateAway();
            other.transform.position = subwayPos.position; // Oyuncuyu sürekli olarak aynı konumda tut
            other.gameObject.GetComponent<SimplePlayerController>().enabled = true; // Oyuncunun
            // hareket etmesine izin ver
            Debug.Log("Oyuncu ışınlandı ve tren hızlanıyor!");
            game3.SetActive(false); // Game3'ü başlat
        }
        
    }

}
