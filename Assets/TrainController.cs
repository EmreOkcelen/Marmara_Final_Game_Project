using UnityEngine;
using System.Collections;

public class TrainController : MonoBehaviour
{
    public float maxSpeed = 10f;
    public float duration = 5f;
    
    private Vector3 startPosition;
    public static TrainController Instance;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    // Giderek hızlanarak uzaklaş
    public void AccelerateAway()
    {
        StartCoroutine(AccelerateAwayCoroutine());
    }
    
    // Giderek yavaşlayarak gel
    public void DecelerateApproach()
    {
        StartCoroutine(DecelerateApproachCoroutine());
    }
    
    IEnumerator AccelerateAwayCoroutine()
    {
        float time = 0f;
        Vector3 initialPos = transform.position;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            
            // Hız giderek artar (quadratic easing)
            float speed = Mathf.Lerp(0f, maxSpeed, t * t);
            
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            yield return null;
        }
    }
    
    IEnumerator DecelerateApproachCoroutine()
    {
        float time = 0f;
        Vector3 initialPos = transform.position;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            
            // Hız giderek azalır (inverse quadratic)
            float speed = Mathf.Lerp(maxSpeed, 0f, t * t);
            
            Vector3 direction = (startPosition - transform.position).normalized;
            transform.Translate(direction * speed * Time.deltaTime);
            
            yield return null;
        }
        
        transform.position = startPosition;
    }
}
