using UnityEngine;

public class Game2GreenArea : MonoBehaviour
{
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
        //if (other.CompareTag("Game2Controller"))
            Debug.Log("Oyuncu yeşil alana girdi!");
            // Oyuncunun yeşil alanda olduğunu belirt
            Game2.Instance.isPlayerInGreenArea = true;
        
    }
    void OnTriggerExit(Collider other)
    {
        //if (other.CompareTag("Game2Controller"))
            Debug.Log("Oyuncu yeşil alandan çıktı!");
            // Oyuncunun yeşil alanda olmadığını belirt
            Game2.Instance.isPlayerInGreenArea = false;
        
    }
}
