using UnityEngine;

namespace JejakAsa
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class Obstacle_Preman : MonoBehaviour
    {
        [SerializeField] private Mission_PengantaranBantuan player;
        [SerializeField, Min(0f)] private float detectionRadius = 7f;
        [SerializeField, Min(0f)] private float moveSpeed = 2.8f;
        [SerializeField, Min(0f)] private float timePenalty = 10f;
        [SerializeField, Min(0f)] private float contactCooldown = 2f;
        [SerializeField, Min(0f)] private float turnSpeed = 540f;
        [SerializeField] private Vector3[] patrolPoints = new Vector3[0];
        [SerializeField, Min(0f)] private float patrolSpeed = 1.8f;
        [SerializeField, Min(0f)] private float leashDistance;
        [SerializeField] private bool detectFromHome;
        [SerializeField] private bool requireLineOfSight;
        [SerializeField, Min(0)] private int retireAfterContacts = 3;
        [SerializeField, Min(0f)] private float retreatAfterContactSeconds;
        [SerializeField, Min(0f)] private float sharedContactGraceSeconds;

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
        private int patrolIndex;
        private float retreatRemaining;
        private bool returningHome;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            if (player == null)
                player = FindFirstObjectByType<Mission_PengantaranBantuan>();
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
                || player.IsDialogueOpen || StoryDialog_InteraksiNPC.AnyDialogueOpen || UI_MenuSesi.InputBlocked || Time.timeScale <= 0f)
                return;

            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
            retreatRemaining = Mathf.Max(0f, retreatRemaining - Time.deltaTime);
            Vector3 direction = player.transform.position - transform.position;
            direction.y = 0f;
            Vector3 homeToPlayer = player.transform.position - initialPosition; homeToPlayer.y = 0;
            Vector3 detection = detectFromHome ? homeToPlayer : direction;
            bool withinLeash = leashDistance <= 0 || homeToPlayer.sqrMagnitude <= leashDistance * leashDistance;
            bool chasing = retreatRemaining <= 0 && withinLeash && detection.sqrMagnitude <= detectionRadius * detectionRadius
                && (!requireLineOfSight || FindObstacle(transform.position, player.transform.position) == null);
            Vector3 destination;
            float activeSpeed;
            if (chasing)
            {
                returningHome = false;
                destination = player.transform.position;
                activeSpeed = moveSpeed;
            }
            else if (patrolPoints.Length > 0 || leashDistance > 0)
            {
                if (retreatRemaining > 0 || (leashDistance > 0 &&
                    Vector3.Distance(transform.position, initialPosition) > leashDistance)) returningHome = true;
                destination = returningHome || patrolPoints.Length == 0 ? initialPosition : patrolPoints[patrolIndex];
                Vector3 toPoint = destination - transform.position; toPoint.y = 0;
                if (toPoint.sqrMagnitude < .10f)
                {
                    if (returningHome) returningHome = false;
                    else if (patrolPoints.Length > 0) patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                }
                activeSpeed = patrolSpeed;
            }
            else
            {
                hasWaypoint = false;
                hasNextWaypoint = false;
                return;
            }

            destination.y = transform.position.y;
            direction = destination - transform.position;
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
            Vector3 velocity = direction.normalized * Mathf.Min(activeSpeed, direction.magnitude / Time.deltaTime);
            velocity.y = verticalSpeed;
            Vector3 beforeMove = transform.position;
            controller.Move(velocity * Time.deltaTime);
            Vector3 moved = transform.position - beforeMove; moved.y = 0;
            if (moved.sqrMagnitude > .000001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moved), turnSpeed * Time.deltaTime);

            if (retreatRemaining <= 0 && playerController != null && playerController.enabled && cooldownRemaining <= 0f
                && Physics.ComputePenetration(controller, transform.position, transform.rotation,
                    playerController, player.transform.position, player.transform.rotation, out _, out _))
            {
                cooldownRemaining = contactCooldown;
                if (!player.TryApplyPremanPenalty(timePenalty, sharedContactGraceSeconds)) return;
                contactCount++;
                retreatRemaining = retreatAfterContactSeconds;
                returningHome = retreatAfterContactSeconds > 0;
                if (retireAfterContacts > 0 && contactCount >= retireAfterContacts)
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
            bool wasHidden = retireAfterContacts > 0 && contactCount >= retireAfterContacts;
            contactCount = 0;
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            transform.SetPositionAndRotation(initialPosition, initialRotation);
            controller.enabled = wasEnabled;
            cooldownRemaining = 0f;
            retreatRemaining = 0f;
            returningHome = false;
            patrolIndex = 0;
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
