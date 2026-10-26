using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movimento")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;

    [Header("Visuale")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float maxLookAngle = 85f;

    private CharacterController controller;
    private float verticalVelocity;
    private float pitch;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        LockCursor(true);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;

        // ESC libera il cursore, un clic lo blocca di nuovo
        if (keyboard.escapeKey.wasPressedThisFrame) LockCursor(false);
        if (mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked) LockCursor(true);

        if (Cursor.lockState != CursorLockMode.Locked) return;

        Look(mouse);
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

    private void Move(Keyboard keyboard)
    {
        // Input WASD
        float x = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
        float z = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);

        Vector3 direction = transform.right * x + transform.forward * z;
        direction = Vector3.ClampMagnitude(direction, 1f); // evita di andare più veloce in diagonale

        float speed = keyboard.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;

        // Gravità e salto
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f; // piccola spinta verso il basso per restare a terra
        }

        if (controller.isGrounded && keyboard.spaceKey.wasPressedThisFrame)
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