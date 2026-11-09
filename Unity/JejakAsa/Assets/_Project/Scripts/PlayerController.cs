using UnityEngine;
using UnityEngine.InputSystem;

namespace JejakAsa
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform movementCamera;
        [SerializeField, Min(0.01f)] private float moveSpeed = 3.5f;

        private CharacterController controller;
        private Vector3 cameraForward;
        private Vector3 cameraRight;
        private float verticalSpeed;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (movementCamera == null)
            {
                Debug.LogError("PlayerController requires a movement camera.", this);
                enabled = false;
                return;
            }

            // The camera has a fixed orientation; movement only uses its yaw.
            Quaternion yaw = Quaternion.Euler(0f, movementCamera.eulerAngles.y, 0f);
            cameraForward = yaw * Vector3.forward;
            cameraRight = yaw * Vector3.right;
        }

        private void Update()
        {
            Vector2 input = Vector2.zero;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                input.x = ((keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) ? 1f : 0f)
                    - ((keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) ? 1f : 0f);
                input.y = ((keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) ? 1f : 0f)
                    - ((keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) ? 1f : 0f);
            }

            input = Vector2.ClampMagnitude(input, 1f);
            Vector3 velocity = (cameraForward * input.y + cameraRight * input.x) * moveSpeed;
            if (velocity.sqrMagnitude > 0f)
                transform.rotation = Quaternion.LookRotation(velocity);

            if (controller.isGrounded && verticalSpeed < 0f)
                verticalSpeed = -2f;
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
            velocity.y = verticalSpeed;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
