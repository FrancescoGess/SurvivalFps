using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject impactEffectPrefab; // opzionale

    [Header("Statistiche")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float range = 100f;
    [SerializeField] private float fireRate = 8f;        // colpi al secondo
    [SerializeField] private bool automatic = true;
    [SerializeField] private LayerMask hitMask = ~0;     // ~0 = tutti i layer

    [Header("Munizioni")]
    [SerializeField] private int magazineSize = 12;
    [SerializeField] private float reloadTime = 1.5f;

    private int currentAmmo;
    private float nextFireTime;
    private bool isReloading;

    private void Start()
    {
        currentAmmo = magazineSize;
        Debug.Log("Munizioni: " + currentAmmo + "/" + magazineSize);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;

        // Niente sparo se il cursore non è bloccato (evita colpi quando clicchi per rientrare nel gioco)
        if (Cursor.lockState != CursorLockMode.Locked) return;
        if (isReloading) return;

        if (keyboard.rKey.wasPressedThisFrame && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
            return;
        }

        bool wantsToShoot = automatic ? mouse.leftButton.isPressed : mouse.leftButton.wasPressedThisFrame;

        if (wantsToShoot && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                StartCoroutine(Reload()); // caricatore vuoto: ricarica automatica
            }
        }
    }

    private void Shoot()
    {
        nextFireTime = Time.time + 1f / fireRate;
        currentAmmo--;
        Debug.Log("Munizioni: " + currentAmmo + "/" + magazineSize);

        // Il raggio parte dal centro della camera e va dritto in avanti
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red, 1f); // visibile nella finestra Scene

            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            if (impactEffectPrefab != null)
            {
                // Orientiamo l'effetto lungo la normale della superficie colpita
                GameObject impact = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f);
            }
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * range, Color.yellow, 1f);
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Ricarica...");
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = magazineSize;
        isReloading = false;
        Debug.Log("Munizioni: " + currentAmmo + "/" + magazineSize);
    }
}
