using UnityEngine;

[RequireComponent(typeof(Health))]
public class SurvivalStats : MonoBehaviour
{
    [Header("Statistiche")]
    [SerializeField] private Stat hunger = new Stat();
    [SerializeField] private Stat thirst = new Stat();
    [SerializeField] private Stat stamina = new Stat();

    [Header("Consumo (punti al secondo)")]
    [SerializeField] private float hungerDecayPerSecond = 0.15f;
    [SerializeField] private float thirstDecayPerSecond = 0.25f;

    [Header("Stamina")]
    [SerializeField] private float staminaDrainPerSecond = 20f;   // mentre corri
    [SerializeField] private float jumpStaminaCost = 25f;         // costo di un salto (una tantum)
    [SerializeField] private float staminaRegenPerSecond = 15f;   // quando riposi
    [SerializeField] private float staminaRegenDelay = 1f;        // attesa prima di recuperare
    [SerializeField] private float minStaminaToSprint = 10f;      // dopo l'esaurimento serve almeno questo per correre di nuovo

    [Header("Conseguenze (danni al secondo)")]
    [SerializeField] private float starvationDamagePerSecond = 2f;
    [SerializeField] private float dehydrationDamagePerSecond = 3f;

    public Stat Hunger => hunger;
    public Stat Thirst => thirst;
    public Stat Stamina => stamina;

    // FirstPersonController chiede qui il permesso di correre
    public bool CanSprint => !isExhausted;

    private Health health;
    private FirstPersonController movement;
    private float lastStaminaUseTime;
    private bool isExhausted;

    private void Awake()
    {
        health = GetComponent<Health>();
        movement = GetComponent<FirstPersonController>();
        ResetStats();
    }

    private void Update()
    {
        if (health.IsDead) return;

        float dt = Time.deltaTime;

        // Fame e sete calano col tempo
        hunger.Change(-hungerDecayPerSecond * dt);
        thirst.Change(-thirstDecayPerSecond * dt);

        UpdateStamina(dt);

        // Se una delle due è a zero, si perde vita
        if (hunger.IsEmpty) health.TakeDamage(starvationDamagePerSecond * dt);
        if (thirst.IsEmpty) health.TakeDamage(dehydrationDamagePerSecond * dt);
    }

    private void UpdateStamina(float dt)
    {
        bool sprinting = movement != null && movement.IsSprinting;

        if (sprinting)
        {
            stamina.Change(-staminaDrainPerSecond * dt);
            lastStaminaUseTime = Time.time;
        }
        else if (Time.time - lastStaminaUseTime >= staminaRegenDelay)
        {
            stamina.Change(staminaRegenPerSecond * dt);
        }

        // Isteresi: se si esaurisce, non si corre finché non si recupera un minimo
        if (stamina.IsEmpty) isExhausted = true;
        else if (stamina.Current >= minStaminaToSprint) isExhausted = false;
    }

    // Consuma la stamina in un colpo solo. Restituisce false (senza consumare nulla) se non ce n'è abbastanza.
    public bool TryUseStamina(float amount)
    {
        if (stamina.Current < amount) return false;

        stamina.Change(-amount);
        lastStaminaUseTime = Time.time; // ritarda il recupero, come per la corsa
        return true;
    }

    // Chiamato dal controller quando il giocatore vuole saltare
    public bool TryJump()
    {
        return TryUseStamina(jumpStaminaCost);
    }

    public void Eat(float amount)
    {
        hunger.Change(amount);
    }

    public void Drink(float amount)
    {
        thirst.Change(amount);
    }

    public void ResetStats()
    {
        hunger.Init();
        thirst.Init();
        stamina.Init();
        isExhausted = false;
        lastStaminaUseTime = -999f;
    }

    // Per provare: Play, poi clic destro sul titolo del componente nell'Inspector
    [ContextMenu("Debug: Mangia 25")]
    private void DebugEat() => Eat(25f);

    [ContextMenu("Debug: Bevi 25")]
    private void DebugDrink() => Drink(25f);

    [ContextMenu("Debug: Azzera fame")]
    private void DebugEmptyHunger() => hunger.Change(-hunger.Max);

    [ContextMenu("Debug: Azzera sete")]
    private void DebugEmptyThirst() => thirst.Change(-thirst.Max);
}
