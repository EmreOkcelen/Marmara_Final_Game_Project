using UnityEngine;
using System.Collections;

public class Game1 : MonoBehaviour
{
    [Header("Telefon Objesi ve Shake Ayarları")]
    public GameObject Phone;
    [Tooltip("Toplam sallanma süresi (saniye)")]
    public float shakeDuration = 0.5f;
    [Tooltip("Maksimum açı sapması (derece cinsinden)")]
    public float shakeAngleMagnitude = 3f;

    private Quaternion originalRot;
    private bool isShaking;

    void Start()
    {
        // Orijinal rotasyonu sakla
        originalRot = Phone.transform.localRotation;
    }

    void Update()
    {
        // Örnek tetikleme: boşluk tuşuna basınca çal
        if (Input.GetKeyDown(KeyCode.Space))
            PhoneRingAnimation();
    }

    // Sallamayı başlatan fonksiyon
    void PhoneRingAnimation()
    {
        if (!isShaking)
            StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float zAngle = Random.Range(-shakeAngleMagnitude, shakeAngleMagnitude);
            Phone.transform.localRotation = originalRot * Quaternion.Euler(0f, 0f, zAngle);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Süre bitince orijinal rotasyona dön
        Phone.transform.localRotation = originalRot;
        isShaking = false;
    }
}