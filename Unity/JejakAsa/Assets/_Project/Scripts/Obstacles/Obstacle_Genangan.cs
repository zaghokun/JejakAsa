using System.Collections.Generic;
using UnityEngine;

namespace JejakAsa
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public sealed class Obstacle_Genangan : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float speedMultiplier = 0.5f;
        [SerializeField] private Vector2[] footprint = new Vector2[0];

        private readonly HashSet<Player_Gerak> players = new HashSet<Player_Gerak>();
        private BoxCollider trigger;

        internal float SpeedMultiplier => Mathf.Clamp01(speedMultiplier);
        internal bool TriggerEnabled => trigger != null && trigger.enabled && trigger.isTrigger;

        private void Awake()
        {
            trigger = GetComponent<BoxCollider>();
            trigger.isTrigger = true;
            Rigidbody body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        private void OnEnable()
        {
            Bounds bounds = trigger.bounds;
            Collider[] overlaps = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity,
                Physics.AllLayers, QueryTriggerInteraction.Collide);
            foreach (Collider other in overlaps)
                if (other is CharacterController && Overlaps(other))
                    AddPlayer(other);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isActiveAndEnabled && other is CharacterController)
                AddPlayer(other);
        }

        internal void AddPlayer(Collider other)
        {
            Player_Gerak player = other.GetComponent<Player_Gerak>();
            if (player != null && players.Add(player))
                player.EnterPuddle(this);
        }


        internal bool Overlaps(Collider other)
        {
            return TriggerEnabled && other.enabled && InsideFootprint(other)
                && !Physics.GetIgnoreLayerCollision(gameObject.layer, other.gameObject.layer)
                && !Physics.GetIgnoreCollision(trigger, other)
                && Physics.ComputePenetration(trigger, trigger.transform.position, trigger.transform.rotation,
                    other, other.transform.position, other.transform.rotation, out _, out _);
        }
        private bool InsideFootprint(Collider other)
        {
            if (footprint == null || footprint.Length < 3) return true;
            Vector3 local = transform.InverseTransformPoint(other.bounds.center);
            Vector2 point = new Vector2(local.x, local.z);
            float radius = Mathf.Min(other.bounds.extents.x, other.bounds.extents.z) /
                Mathf.Max(.001f, Mathf.Min(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z)));
            bool inside = false;
            for (int i = 0, j = footprint.Length - 1; i < footprint.Length; j = i++)
            {
                Vector2 a = footprint[j], b = footprint[i];
                if ((a.y > point.y) != (b.y > point.y) && point.x < (b.x-a.x)*(point.y-a.y)/(b.y-a.y)+a.x)
                    inside = !inside;
                Vector2 edge = b-a;
                float t = Mathf.Clamp01(Vector2.Dot(point-a,edge)/Mathf.Max(.000001f,edge.sqrMagnitude));
                if ((point-(a+edge*t)).sqrMagnitude <= radius*radius) return true;
            }
            return inside;
        }
        private void OnTriggerExit(Collider other)
        {
            if (!(other is CharacterController))
                return;
            Player_Gerak player = other.GetComponent<Player_Gerak>();
            if (player != null && !Overlaps(other) && players.Remove(player))
                player.ExitPuddle(this);
        }

        internal void ForgetPlayer(Player_Gerak player) => players.Remove(player);

        private void OnDisable()
        {
            foreach (Player_Gerak player in players)
                if (player != null)
                    player.ExitPuddle(this);
            players.Clear();
        }
    }
}
