using UnityEngine;

namespace JejakAsa
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PremanController : MonoBehaviour
    {
        [SerializeField] private PlayerInteraction player;
        [SerializeField, Min(0f)] private float detectionRadius = 7f;
        [SerializeField, Min(0f)] private float moveSpeed = 2.8f;
        [SerializeField, Min(0f)] private float timePenalty = 10f;
        [SerializeField, Min(0f)] private float contactCooldown = 2f;

        private CharacterController controller;
        private CharacterController playerController;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private float cooldownRemaining;
        private int contactCount;
        private float verticalSpeed;
        private bool collisionConfigured;
        private bool collisionWasIgnored;
        private readonly RaycastHit[] pathHits = new RaycastHit[32];
        private readonly Vector3[] corners = new Vector3[4];
        private Vector3 waypoint;
        private Vector3 nextWaypoint;
        private bool hasWaypoint;
        private bool hasNextWaypoint;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            if (player == null)
                player = FindFirstObjectByType<PlayerInteraction>();
            if (player != null)
            {
                playerController = player.GetComponent<CharacterController>();
                player.MissionReset += ResetPreman;
            }
        }

        private void OnEnable()
        {
            IgnorePlayerCollision();
        }

        private void IgnorePlayerCollision()
        {
            if (controller == null || playerController == null || !controller.enabled || !playerController.enabled
                || !controller.gameObject.activeInHierarchy || !playerController.gameObject.activeInHierarchy)
                return;
            if (!collisionConfigured)
            {
                collisionWasIgnored = Physics.GetIgnoreCollision(controller, playerController);
                collisionConfigured = true;
            }
            if (!Physics.GetIgnoreCollision(controller, playerController))
                Physics.IgnoreCollision(controller, playerController);
        }

        private void OnDisable()
        {
            if (collisionConfigured && controller != null && playerController != null
                && controller.gameObject.activeInHierarchy && playerController.gameObject.activeInHierarchy)
                Physics.IgnoreCollision(controller, playerController, collisionWasIgnored);
            collisionConfigured = false;
        }

        private void Update()
        {
            IgnorePlayerCollision();
            if (!controller.enabled || player == null || !player.isActiveAndEnabled || !player.IsMissionActive || player.IsMissionEnded
                || player.IsDialogueOpen || StoryNpcController.AnyDialogueOpen || SessionMenu.InputBlocked || Time.timeScale <= 0f)
                return;

            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
            Vector3 direction = player.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > detectionRadius * detectionRadius)
            {
                hasWaypoint = false;
                hasNextWaypoint = false;
                return;
            }

            Vector3 destination = player.transform.position;
            destination.y = transform.position.y;
            Collider obstacle = FindObstacle(transform.position, destination);
            if (obstacle == null)
            {
                hasWaypoint = false;
                hasNextWaypoint = false;
            }
            else
            {
                if (hasWaypoint && (waypoint - transform.position).sqrMagnitude < 0.01f)
                {
                    hasWaypoint = hasNextWaypoint;
                    waypoint = nextWaypoint;
                    hasNextWaypoint = false;
                }
                if (!hasWaypoint || FindObstacle(transform.position, waypoint) != null)
                    PlanDetour(obstacle, destination);
                if (hasWaypoint)
                    direction = waypoint - transform.position;
            }
            direction.y = 0f;

            if (controller.isGrounded && verticalSpeed < 0f)
                verticalSpeed = -2f;
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
            Vector3 velocity = direction.normalized * Mathf.Min(moveSpeed, direction.magnitude / Time.deltaTime);
            velocity.y = verticalSpeed;
            controller.Move(velocity * Time.deltaTime);

            if (playerController != null && playerController.enabled && cooldownRemaining <= 0f
                && Physics.ComputePenetration(controller, transform.position, transform.rotation,
                    playerController, player.transform.position, player.transform.rotation, out _, out _))
            {
                cooldownRemaining = contactCooldown;
                player.SetTimeRemaining(player.TimeRemaining - timePenalty);
                contactCount++;
                if (contactCount >= 3)
                    gameObject.SetActive(false);
            }
        }

        private Collider FindObstacle(Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;
            delta.y = 0f;
            float distance = delta.magnitude;
            if (distance < 0.001f)
                return null;

            Vector3 scale = transform.lossyScale;
            float radius = controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float halfHeight = Mathf.Max(radius, controller.height * Mathf.Abs(scale.y) * 0.5f);
            Vector3 center = from + transform.TransformVector(controller.center);
            Vector3 extent = Vector3.up * (halfHeight - radius);
            int count = Physics.CapsuleCastNonAlloc(center - extent + Vector3.up * 0.03f, center + extent,
                radius, delta / distance, pathHits, distance, ~0, QueryTriggerInteraction.Ignore);
            Collider nearest = null;
            float nearestDistance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Collider hit = pathHits[i].collider;
                if (hit == null || hit.transform.IsChildOf(transform) || hit.transform.IsChildOf(player.transform)
                    || Physics.GetIgnoreLayerCollision(gameObject.layer, hit.gameObject.layer))
                    continue;
                if (pathHits[i].distance < nearestDistance)
                {
                    nearest = hit;
                    nearestDistance = pathHits[i].distance;
                }
            }
            return nearest;
        }

        // shortcut: detours handle simple collider layouts; use baked navigation for complex maps.
        private void PlanDetour(Collider obstacle, Vector3 destination)
        {
            Bounds bounds = obstacle.bounds;
            float padding = controller.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z))
                + controller.skinWidth + 0.2f;
            float y = transform.position.y;
            corners[0] = new Vector3(bounds.min.x - padding, y, bounds.min.z - padding);
            corners[1] = new Vector3(bounds.min.x - padding, y, bounds.max.z + padding);
            corners[2] = new Vector3(bounds.max.x + padding, y, bounds.min.z - padding);
            corners[3] = new Vector3(bounds.max.x + padding, y, bounds.max.z + padding);
            float bestLength = float.PositiveInfinity;
            hasWaypoint = false;
            hasNextWaypoint = false;
            for (int i = 0; i < corners.Length; i++)
            {
                if (FindObstacle(transform.position, corners[i]) != null)
                    continue;
                for (int j = 0; j < corners.Length; j++)
                {
                    if (FindObstacle(corners[i], corners[j]) != null || FindObstacle(corners[j], destination) != null)
                        continue;
                    float length = Vector3.Distance(transform.position, corners[i])
                        + Vector3.Distance(corners[i], corners[j]) + Vector3.Distance(corners[j], destination);
                    if (length >= bestLength)
                        continue;
                    bestLength = length;
                    waypoint = corners[i];
                    nextWaypoint = corners[j];
                    hasWaypoint = true;
                    hasNextWaypoint = i != j;
                }
            }
        }


        private void ResetPreman()
        {
            bool wasHidden = contactCount >= 3;
            contactCount = 0;
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            transform.SetPositionAndRotation(initialPosition, initialRotation);
            controller.enabled = wasEnabled;
            cooldownRemaining = 0f;
            verticalSpeed = 0f;
            if (wasHidden)
                gameObject.SetActive(true);
            if (isActiveAndEnabled)
                IgnorePlayerCollision();
            hasWaypoint = false;
            hasNextWaypoint = false;
        }

        private void OnDestroy()
        {
            if (player != null)
                player.MissionReset -= ResetPreman;
        }
    }
}
