using UnityEngine;
using UnityEngine.InputSystem;

// Повесь на Player вместе с MouseLook
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float verticalSpeed = 3f;

    private void Update()
    {
        if (Time.timeScale <= 0f) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        // Используем forward/right самого Player.
        // Player крутится только горизонтально (MouseLook), его forward = горизонтальное направление взгляда.
        // Не зависит от тряски Cinemachine, в отличие от Main Camera.
        Vector3 forward = transform.forward;
        Vector3 right   = transform.right;

        Vector3 move = Vector3.zero;

        if (kb.wKey.isPressed) move += forward;
        if (kb.sKey.isPressed) move -= forward;
        if (kb.dKey.isPressed) move += right;
        if (kb.aKey.isPressed) move -= right;

        if (kb.spaceKey.isPressed)    move += Vector3.up    * verticalSpeed / moveSpeed;
        if (kb.leftCtrlKey.isPressed) move -= Vector3.up    * verticalSpeed / moveSpeed;

        transform.position += move * moveSpeed * Time.deltaTime;
    }
}
