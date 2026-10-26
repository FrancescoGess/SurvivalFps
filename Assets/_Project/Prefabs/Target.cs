using UnityEngine;

using System.Collections;
 
// Feedback visivo al colpo (lampeggio) e, opzionalmente, distruzione alla morte.
// Sui nemici si disattiva la distruzione: se ne occupa EnemyAI.
[RequireComponent(typeof(Health))]
public class Target : MonoBehaviour
{
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private bool destroyOnDeath = true;

    private Health health;
    private Renderer targetRenderer;
    private Color originalColor;

    private void Awake()
    {
        health = GetComponent<Health>();
        targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null) originalColor = targetRenderer.material.color;
    }

    private void OnEnable()
    {
        health.OnDamaged += HandleDamaged;
        health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        health.OnDamaged -= HandleDamaged;
        health.OnDied -= HandleDied;
    }

    private void HandleDamaged(float amount)
    {
        if (targetRenderer == null) return;
        StopAllCoroutines();
        StartCoroutine(Flash());
    }

    private void HandleDied()
    {
        if (destroyOnDeath) Destroy(gameObject);
    }

    private IEnumerator Flash()
    {
        targetRenderer.material.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        targetRenderer.material.color = originalColor;
    }
}
