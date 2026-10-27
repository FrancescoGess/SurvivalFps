using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private SurvivalStats survivalStats; // opzionale: se vuoto, lo cerca sul Player

    [Header("Movimento")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;

    [Header("Accovacciamento (tasto C)")]
    [SerializeField] private float crouchHeight = 1f;            // altezza del collider da accovacciato
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float crouchCameraDrop = 0.7f;      // di quanto scende la camera
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [SerializeField] private bool toggleCrouch = false;          // false = tieni premuto, true = premi per alternare
    [SerializeField] private LayerMask obstacleMask = ~0;        // cosa impedisce di rialzarsi (soffitti)

    [Header("Visuale")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float maxLookAngle = 85f;

    // Letti da altri script (stamina, e in futuro rumore/IA dei nemici)
    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }

    // Quando è true (inventario aperto, menu...) il controller ignora tutto l'input
    public bool IsUiOpen { get; set; }

    private CharacterController controller;
    private float verticalVelocity;
    private float pitch;

    // Valori "in piedi" salvati all'avvio
    private float standingHeight;
    private Vector3 standingCenter;
    private float standingCameraY;
    private bool crouchToggled;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (survivalStats == null) survivalStats = GetComponent<SurvivalStats>();

        standingHeight = controller.height;
        standingCenter = controller.center;
        standingCameraY = cameraTransform.localPosition.y;

        // Un CharacterController non può essere più basso del doppio del suo raggio
        crouchHeight = Mathf.Clamp(crouchHeight, controller.radius * 2f, standingHeight);

        LockCursor(true);
    }

    private void OnDisable()
    {
        IsSprinting = false;
    }

    private void Update()
    {
        IsSprinting = false;
        if (IsUiOpen) return;

        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;

        // ESC libera il cursore, un clic lo blocca di nuovo
        if (keyboard.escapeKey.wasPressedThisFrame) LockCursor(false);
        if (mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked) LockCursor(true);

        if (Cursor.lockState != CursorLockMode.Locked) return;

        Look(mouse);
        HandleCrouch(keyboard);
        Move(keyboard);
    }

    private void Look(Mouse mouse)
    {
        // Il delta del mouse è già "per frame": non va moltiplicato per deltaTime
        Vector2 delta = mouse.delta.ReadValue() * mouseSensitivity;

        // Rotazione orizzontale: ruota tutto il corpo del player
        transform.Rotate(0f, delta.x, 0f);

        // Rotazione verticale: ruota solo la camera, con limite alto/basso
        pitch = Mathf.Clamp(pitch - delta.y, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleCrouch(Keyboard keyboard)
    {
        bool wantsToCrouch;
        if (toggleCrouch)
        {
            if (keyboard.cKey.wasPressedThisFrame) crouchToggled = !crouchToggled;
            wantsToCrouch = crouchToggled;
        }
        else
        {
            wantsToCrouch = keyboard.cKey.isPressed;
        }

        // Ci si abbassa subito, ci si rialza solo se sopra la testa c'è spazio
        if (wantsToCrouch) IsCrouching = true;
        else if (IsCrouching && CanStandUp()) IsCrouching = false;

        // Transizione morbida dell'altezza del collider
        float targetHeight = IsCrouching ? crouchHeight : standingHeight;
        float height = Mathf.Lerp(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        if (Mathf.Abs(height - targetHeight) < 0.01f) height = targetHeight;

        ApplyHeight(height);
    }

    private void ApplyHeight(float height)
    {
        // Il centro scende della metà della differenza: i piedi restano a terra
        controller.height = height;
        controller.center = standingCenter + Vector3.down * ((standingHeight - height) * 0.5f);

        // La camera segue l'altezza: più il collider è basso, più la camera scende
        float t = Mathf.InverseLerp(crouchHeight, standingHeight, height);
        Vector3 cameraPosition = cameraTransform.localPosition;
        cameraPosition.y = Mathf.Lerp(standingCameraY - crouchCameraDrop, standingCameraY, t);
        cameraTransform.localPosition = cameraPosition;
    }

    private bool CanStandUp()
    {
        // Lancia una sfera verso l'alto dalla testa attuale: se colpisce qualcosa, non c'è spazio
        float radius = controller.radius;
        float currentTop = controller.center.y + controller.height * 0.5f;
        Vector3 origin = transform.position + Vector3.up * (currentTop - radius);
        float distance = standingHeight - controller.height + 0.05f;

        return !Physics.SphereCast(origin, radius * 0.9f, Vector3.up, out _, distance,
                                   obstacleMask, QueryTriggerInteraction.Ignore);
    }

    private void Move(Keyboard keyboard)
    {
        // Input WASD
        float x = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
        float z = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);

        Vector3 direction = transform.right * x + transform.forward * z;
        direction = Vector3.ClampMagnitude(direction, 1f); // evita di andare più veloce in diagonale

        // Da accovacciati non si corre. Altrimenti: Shift + movimento + stamina disponibile
        bool isMoving = direction.sqrMagnitude > 0.01f;
        bool wantsToSprint = keyboard.leftShiftKey.isPressed && isMoving && !IsCrouching;
        bool staminaAllows = survivalStats == null || survivalStats.CanSprint;

        IsSprinting = wantsToSprint && staminaAllows;

        float speed = walkSpeed;
        if (IsCrouching) speed = crouchSpeed;
        else if (IsSprinting) speed = sprintSpeed;

        // Gravità e salto
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f; // piccola spinta verso il basso per restare a terra
        }

        // TryJump è l'ultima condizione: la stamina si consuma solo se il salto avviene davvero
        if (controller.isGrounded && !IsCrouching && keyboard.spaceKey.wasPressedThisFrame
            && (survivalStats == null || survivalStats.TryJump()))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = direction * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}