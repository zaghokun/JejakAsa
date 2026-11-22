using System.Collections.Generic;
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
        private Vector3 lastPosition;
        private readonly HashSet<PuddleZone> puddles = new HashSet<PuddleZone>();

        internal void EnterPuddle(PuddleZone puddle) => puddles.Add(puddle);
        internal void ExitPuddle(PuddleZone puddle) => puddles.Remove(puddle);

        private void OnDestroy()
        {
            foreach (PuddleZone puddle in puddles)
                if (puddle != null)
                    puddle.ForgetPlayer(this);
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            lastPosition = transform.position;
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
            if (!controller.enabled)
                return;
            if (transform.position != lastPosition)
                ReconcilePuddles();
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
            float speed = keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)
                ? moveSpeed * 2f : moveSpeed;
            float multiplier = 1f;
            foreach (PuddleZone puddle in puddles)
                if (puddle.isActiveAndEnabled && puddle.Overlaps(controller))
                    multiplier = Mathf.Min(multiplier, puddle.SpeedMultiplier);
            speed *= multiplier;
            Vector3 velocity = (cameraForward * input.y + cameraRight * input.x) * speed;
            if (velocity.sqrMagnitude > 0f)
                transform.rotation = Quaternion.LookRotation(velocity);

            if (controller.isGrounded && verticalSpeed < 0f)
                verticalSpeed = -2f;
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
            velocity.y = verticalSpeed;
            controller.Move(velocity * Time.deltaTime);
            lastPosition = transform.position;
        }

        private void ReconcilePuddles()
        {
            foreach (PuddleZone puddle in puddles)
                if (puddle != null)
                    puddle.ForgetPlayer(this);
            puddles.Clear();

            Physics.SyncTransforms();
            Bounds bounds = controller.bounds;
            Collider[] overlaps = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity,
                Physics.AllLayers, QueryTriggerInteraction.Collide);
            foreach (Collider other in overlaps)
            {
                PuddleZone puddle = other.GetComponent<PuddleZone>();
                if (puddle != null && puddle.isActiveAndEnabled && puddle.Overlaps(controller))
                    puddle.AddPlayer(controller);
            }
        }
    }
}
