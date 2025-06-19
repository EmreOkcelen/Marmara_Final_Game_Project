using UnityEngine;
using System.Collections;

public class Game1 : MonoBehaviour
{
    [Header("Game Control")]
    [Tooltip("Game1 başlatılsın mı?")]
    public bool isGameStarted = false;
    public bool isGameFinished = false; // Game1 bitince true olacak
    
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


        audioSource.clip = ringTone;
        audioSource.spatialBlend = 1.0f; // 3D ses için
        audioSource.loop = true; // Sesi sürekli tekrarla
        audioSource.playOnAwake = false;
        audioSource.volume = 1.0f; // Ses seviyesi
    }

    void Update()
    {
        // Game1 başlatıldıysa ve player telefonun etrafında ise
        if (isGameStarted)
        {
            PhoneRingAnimation(); // Telefonu çaldır
        }

        // Game1 bitmemişse ve player telefonun etrafında değilse
        if (isGameStarted && isGameFinished == false && !isShaking)
        {
            // Telefonu sallamayı durdur
            StopPhoneRing();
        }        
        
    }

    /// <summary>
    /// Game1'i başlatan fonksiyon - Bu çağrılana kadar game çalışmaz
    /// </summary>
    public void StartGame1()
    {
        isGameStarted = true;

        // AudioSource'u yeniden etkinleştir (eğer FinishGame1() ile devre dışı bırakıldıysa)
        if (audioSource != null)
        {
            audioSource.enabled = true;
            audioSource.Play(); // Başlangıçta sesi çal
        }

        
        
        Debug.Log("Game1 başlatıldı! Space tuşu ile telefonu çaldırabilirsiniz.");
    }

    /// <summary>
    /// Game1'i durduran fonksiyon
    /// </summary>
    public void StopGame1()
    {
        isGameStarted = false;
        StopPhoneRing(); // Eğer çalıyorsa durdur
        Debug.Log("Game1 durduruldu!");
    }

    /// <summary>
    /// Game1'i tamamen bitiren ve tüm aktiviteleri durduran fonksiyon
    /// Bu çağrıldıktan sonra Game1 ile ilgili hiçbir şey çalışmaz
    /// </summary>
    public void FinishGame1()
    {
        // Oyunu durdur
        isGameStarted = false;
        isGameFinished = true; // Game1 bitince true olacak

        // Tüm coroutine'ları durdur
        StopAllCoroutines();

        // Ses ve animasyonu durdur
        StopPhoneRing();

        // AudioSource'u tamamen devre dışı bırak
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.enabled = false;
        }

        // Telefonu orijinal pozisyonuna getir
        if (Phone != null)
        {
            Phone.transform.localRotation = originalRot;
        }

        // Durumu sıfırla
        isShaking = false;

        Debug.Log("Game1 tamamen bitirildi! Yeniden başlatmak için StartGame1() çağrılmalı.");
        
             
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