using UnityEngine;
using UnityEngine.InputSystem;

// Повесь на Player. CameraRig — пустой дочерний объект на уровне глаз.
// Main Camera и CinemachineVirtualCamera лежат внутри CameraRig.
public class MouseLook : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.2f;
    [SerializeField] private float verticalClamp = 80f;
    [SerializeField] private Transform cameraRig;   // пустой объект — вертикальная ось

    private float _xRotation;

    private void Start()
    {
        // Фолбэк: если rig не назначен — берём камеру как раньше
        if (cameraRig == null)
            cameraRig = GetComponentInChildren<Camera>().transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Не перехватываем клик когда игра поставлена на паузу (GameOver/Victory)
        if (Mouse.current.leftButton.wasPressedThisFrame
            && Cursor.lockState == CursorLockMode.None
            && Time.timeScale > 0f)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 delta = Mouse.current.delta.ReadValue() * sensitivity;

        _xRotation -= delta.y;
        _xRotation = Mathf.Clamp(_xRotation, -verticalClamp, verticalClamp);

        cameraRig.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);  // вертикаль → CameraRig
        transform.Rotate(Vector3.up, delta.x);                            // горизонталь → Player
    }
}
