using System.Collections.Generic;
using UnityEngine;

namespace JejakAsa
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public sealed class PuddleZone : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float speedMultiplier = 0.5f;

        private readonly HashSet<PlayerController> players = new HashSet<PlayerController>();
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
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && players.Add(player))
                player.EnterPuddle(this);
        }


        internal bool Overlaps(Collider other)
        {
            return TriggerEnabled && other.enabled
                && !Physics.GetIgnoreLayerCollision(gameObject.layer, other.gameObject.layer)
                && !Physics.GetIgnoreCollision(trigger, other)
                && Physics.ComputePenetration(trigger, trigger.transform.position, trigger.transform.rotation,
                    other, other.transform.position, other.transform.rotation, out _, out _);
        }
        private void OnTriggerExit(Collider other)
        {
            if (!(other is CharacterController))
                return;
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !Overlaps(other) && players.Remove(player))
                player.ExitPuddle(this);
        }

        internal void ForgetPlayer(PlayerController player) => players.Remove(player);

        private void OnDisable()
        {
            foreach (PlayerController player in players)
                if (player != null)
                    player.ExitPuddle(this);
            players.Clear();
        }
    }
}
