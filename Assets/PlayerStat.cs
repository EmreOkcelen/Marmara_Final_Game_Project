using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStat : MonoBehaviour
{
    public static PlayerStat Instance;
    
    [Header("Health")]
    [SerializeField] Image healthBar;
    public float Health = 100f;
    public float PreviousHealth;

    [Header("Stamina")]
    [SerializeField] Image staminaBar;
    public float Stamina = 100f;
    public float PreviousStamina;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        healthBar.fillAmount = Health / 100f;
    }

    void Update()
    {
        HealthControl();
        StaminaControl();
    }

    void HealthControl()
    {
        if (Health != PreviousHealth)
        {
            healthBar.fillAmount = Health / 100f;
            PreviousHealth = Health;
        }
    }

    void StaminaControl()
    {
        if (Stamina != PreviousStamina)
        {
            staminaBar.fillAmount = Stamina / 100f;
            PreviousStamina = Stamina;
        }
    }
    
    public void TakeDamage(float damage)
    {
        Health -= damage;
    }

    public void RestoreHealth(float health)
    {
        Health += health;
    }

    public void UseStamina(float stamina)
    {
        Stamina -= stamina;
    }

    public void RestoreStamina(float stamina)
    {
        Stamina += stamina;
    }
}