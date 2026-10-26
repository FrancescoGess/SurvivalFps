using UnityEngine;

// Zona che infligge danno continuo a chi la attraversa. Utile per testare la vita
// e, più avanti, per lava, radiazioni, gas tossici...
[RequireComponent(typeof(Collider))]
public class DamageZone : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 20f;

    private void Reset()
    {
        // Chiamato da Unity quando aggiungi il componente: rende il collider un trigger
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}
