using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform PlayerCamera;
    [SerializeField] private float Sensitivity = 0.1f;
    [SerializeField] private float MovementSpeed = 5f;
    [SerializeField] private float JumpForce = 5f;
    [SerializeField] private float Gravity = 9.81f;

    private CharacterController PlayerController;
    private float GravityForce;

    void Start()
    {
        PlayerController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        GetInput(out Vector3 moveInput, out Vector2 mouseInput);
        MovePlayer(moveInput);
        MovePlayerCamera(mouseInput);
    }

    void GetInput(out Vector3 moveInput, out Vector2 mouseInput)
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;

        float h = 0f, v = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  h = -1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h =  1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  v = -1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    v =  1f;

        moveInput = new Vector3(h, 0f, v);
        mouseInput = mouse.delta.ReadValue();
    }

    void MovePlayer(Vector3 moveInput)
    {
        if (PlayerController.isGrounded)
        {
            GravityForce = -2f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                GravityForce = JumpForce;
        }
        else
        {
            GravityForce -= Gravity * Time.deltaTime;  // fix: trước là * -2f → bay ngược lên
        }

        Vector3 moveDir = transform.TransformDirection(moveInput);
        PlayerController.Move(moveDir * MovementSpeed * Time.deltaTime);
        PlayerController.Move(new Vector3(0f, GravityForce, 0f) * Time.deltaTime);
    }

    void MovePlayerCamera(Vector2 mouseInput)
    {
        transform.Rotate(0f, mouseInput.x * Sensitivity, 0f);
        PlayerCamera.Rotate(-mouseInput.y * Sensitivity, 0f, 0f, Space.Self);
    }
}
