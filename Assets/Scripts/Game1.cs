using UnityEngine;
using System.Collections;

public class Game1 : MonoBehaviour
{
    [Header("Telefon Objesi ve Shake Ayarları")]
    public GameObject Phone;
    [Tooltip("Maksimum açı sapması (derece cinsinden)")]
    public float shakeAngleMagnitude = 3f;
    
    [Header("Ses Ayarları")]
    [Tooltip("Telefon zil sesi")]
    public AudioClip ringTone;
    
    private Quaternion originalRot;
    private bool isShaking;
    private AudioSource audioSource;

    void Start()
    {
        // Orijinal rotasyonu sakla
        originalRot = Phone.transform.localRotation;
        
        // AudioSource component'ini ekle ve 3D ses için ayarla
        audioSource = Phone.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = Phone.AddComponent<AudioSource>();
        }
        
        audioSource.clip = ringTone;
        audioSource.spatialBlend = 1.0f; // 3D ses için
        audioSource.loop = true; // Sesi sürekli tekrarla
        audioSource.playOnAwake = false;
        audioSource.volume = 1.0f; // Ses seviyesi
    }

    void Update()
    {
        // Boşluk tuşuna basınca çalmaya başla/durdur
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isShaking)
                StopPhoneRing();
            else
                PhoneRingAnimation();
        }
    }

    // Sallamayı başlatan fonksiyon
    void PhoneRingAnimation()
    {
        if (!isShaking)
        {
            isShaking = true;
            audioSource.Play();
            StartCoroutine(ShakeCoroutine());
        }
    }
    
    // Sallamayı durduran fonksiyon
    void StopPhoneRing()
    {
        isShaking = false;
        audioSource.Stop();
        Phone.transform.localRotation = originalRot;
    }

    private IEnumerator ShakeCoroutine()
    {
        while (isShaking)
        {
            float zAngle = Random.Range(-shakeAngleMagnitude, shakeAngleMagnitude);
            Phone.transform.localRotation = originalRot * Quaternion.Euler(0f, 0f, zAngle);

            yield return null;
        }

        // Sallama bitince orijinal rotasyona dön
        Phone.transform.localRotation = originalRot;
    }
}