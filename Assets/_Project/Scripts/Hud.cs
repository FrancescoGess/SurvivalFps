using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Weapon weapon;
    [SerializeField] private SurvivalStats survivalStats;

    [Header("Vita e munizioni")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText; // opzionale
    [SerializeField] private TMP_Text ammoText;

    [Header("Sopravvivenza")]
    [SerializeField] private Slider hungerBar;
    [SerializeField] private Slider thirstBar;
    [SerializeField] private Slider staminaBar;

    private int lastAmmo;
    private int lastMagazine;
    private bool isReloading;

    private void OnEnable()
    {
        if (playerHealth != null) playerHealth.OnHealthChanged += UpdateHealth;

        if (weapon != null)
        {
            weapon.OnAmmoChanged += UpdateAmmo;
            weapon.OnReloadingChanged += UpdateReloading;
        }

        if (survivalStats != null)
        {
            survivalStats.Hunger.Changed += UpdateHunger;
            survivalStats.Thirst.Changed += UpdateThirst;
            survivalStats.Stamina.Changed += UpdateStamina;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null) playerHealth.OnHealthChanged -= UpdateHealth;

        if (weapon != null)
        {
            weapon.OnAmmoChanged -= UpdateAmmo;
            weapon.OnReloadingChanged -= UpdateReloading;
        }

        if (survivalStats != null)
        {
            survivalStats.Hunger.Changed -= UpdateHunger;
            survivalStats.Thirst.Changed -= UpdateThirst;
            survivalStats.Stamina.Changed -= UpdateStamina;
        }
    }

    private void Start()
    {
        // Valori iniziali, prima che arrivi il primo evento
        if (playerHealth != null) UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);

        if (survivalStats != null)
        {
            UpdateHunger(survivalStats.Hunger.Current, survivalStats.Hunger.Max);
            UpdateThirst(survivalStats.Thirst.Current, survivalStats.Thirst.Max);
            UpdateStamina(survivalStats.Stamina.Current, survivalStats.Stamina.Max);
        }
    }

    private void UpdateHealth(float current, float max)
    {
        SetBar(healthBar, current, max);
        if (healthText != null) healthText.text = Mathf.CeilToInt(current).ToString();
    }

    private void UpdateHunger(float current, float max) => SetBar(hungerBar, current, max);
    private void UpdateThirst(float current, float max) => SetBar(thirstBar, current, max);
    private void UpdateStamina(float current, float max) => SetBar(staminaBar, current, max);

    private void SetBar(Slider bar, float current, float max)
    {
        if (bar != null && max > 0f) bar.value = current / max;
    }

    private void UpdateAmmo(int current, int magazine)
    {
        lastAmmo = current;
        lastMagazine = magazine;
        RefreshAmmoText();
    }

    private void UpdateReloading(bool reloading)
    {
        isReloading = reloading;
        RefreshAmmoText();
    }

    private void RefreshAmmoText()
    {
        if (ammoText == null) return;
        ammoText.text = isReloading ? "Ricarica..." : lastAmmo + " / " + lastMagazine;
    }
}
