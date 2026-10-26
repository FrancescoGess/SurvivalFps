using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    private float currentHealth;
    private Renderer targetRenderer;
    private Color originalColor;

    private void Awake()
    {
        currentHealth = maxHealth;
        targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null) originalColor = targetRenderer.material.color;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " colpito! Vita rimasta: " + Mathf.Max(currentHealth, 0f));

        if (targetRenderer != null)
        {
            StopAllCoroutines();
            StartCoroutine(Flash());
        }

        if (currentHealth <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Flash()
    {
        targetRenderer.material.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        targetRenderer.material.color = originalColor;
    }
}