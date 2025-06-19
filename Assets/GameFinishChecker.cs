using UnityEngine;

public class GameFinishChecker : MonoBehaviour
{
    public GameObject game1Object;
    public GameObject oyuncuciKis;   

    private Game1 game1Script;

    void Start()
    {
        if (game1Object != null)
        {
            game1Script = game1Object.GetComponent<Game1>();
        }

        if (oyuncuciKis != null)
        {
            oyuncuciKis.SetActive(false); // Baþta gizli olsun
        }
    }

    void Update()
    {
        if (game1Script != null && game1Script.isGameFinished)
        {
            if (oyuncuciKis != null && !oyuncuciKis.activeSelf)
            {
                oyuncuciKis.SetActive(true);
            }
        }
    }
}
