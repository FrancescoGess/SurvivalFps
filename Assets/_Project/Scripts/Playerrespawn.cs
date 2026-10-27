using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CharacterController))]
public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint; // opzionale: se vuoto usa la posizione di partenza
    [SerializeField] private float respawnDelay = 2f;

    private Health health;
    private CharacterController controller;
    private FirstPersonController movement;
    private Weapon weapon;
    private SurvivalStats survivalStats;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        health = GetComponent<Health>();
        controller = GetComponent<CharacterController>();
        movement = GetComponent<FirstPersonController>();
        weapon = GetComponent<Weapon>();
        survivalStats = GetComponent<SurvivalStats>();

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void OnEnable()
    {
        health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        health.OnDied -= HandleDied;
    }

    private void HandleDied()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        Debug.Log("Sei morto!");

        // Il giocatore non può muoversi né sparare mentre è morto
        if (movement != null) movement.enabled = false;
        if (weapon != null) weapon.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        Vector3 position = respawnPoint != null ? respawnPoint.position : startPosition;
        Quaternion rotation = respawnPoint != null ? respawnPoint.rotation : startRotation;

        // Per teletrasportare un CharacterController va disattivato un attimo,
        // altrimenti riporta l'oggetto nella posizione precedente
        controller.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        controller.enabled = true;

        health.ResetHealth();
        if (survivalStats != null) survivalStats.ResetStats();

        if (movement != null) movement.enabled = true;
        if (weapon != null) weapon.enabled = true;
    }
}
