using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Weapon weapon;

    [Header("Elementi UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText; // opzionale
    [SerializeField] private TMP_Text ammoText;

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
    }

    private void OnDisable()
    {
        if (playerHealth != null) playerHealth.OnHealthChanged -= UpdateHealth;
        if (weapon != null)
        {
            weapon.OnAmmoChanged -= UpdateAmmo;
            weapon.OnReloadingChanged -= UpdateReloading;
        }
    }

    private void Start()
    {
        // Valore iniziale, prima che arrivi il primo evento
        if (playerHealth != null) UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void UpdateHealth(float current, float max)
    {
        if (healthBar != null) healthBar.value = current / max;
        if (healthText != null) healthText.text = Mathf.CeilToInt(current).ToString();
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