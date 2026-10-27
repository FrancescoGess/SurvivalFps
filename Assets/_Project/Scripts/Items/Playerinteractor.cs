using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Guarda davanti al giocatore: se c'è qualcosa di interagibile mostra il testo e, premendo E, lo attiva.
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactMask = ~0;

    private void Update()
    {
        // Con il cursore libero (inventario, pausa) non si interagisce
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            SetPrompt("");
            return;
        }

        IInteractable target = null;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask, QueryTriggerInteraction.Ignore))
        {
            target = hit.collider.GetComponentInParent<IInteractable>();
        }

        SetPrompt(target != null ? target.GetPrompt() : "");

        if (target != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            target.Interact(gameObject);
        }
    }

    private void SetPrompt(string text)
    {
        if (promptText != null) promptText.text = text;
    }
}
