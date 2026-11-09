using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JejakAsa
{
    [DefaultExecutionOrder(-10)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerInteraction : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float interactionDistance = 1.8f;
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_Text dialogueTitle;
        [SerializeField] private TMP_Text dialogueBody;
        [SerializeField] private Button closeButton;

        private PlayerController movement;
        private bool movementWasEnabled;

        public InteractionTarget CurrentTarget { get; private set; }
        public InteractionTarget DialogueTarget { get; private set; }
        public bool IsDialogueOpen { get; private set; }

        private void Awake()
        {
            movement = GetComponent<PlayerController>();
            if (promptPanel == null || promptText == null || dialoguePanel == null
                || dialogueTitle == null || dialogueBody == null || closeButton == null)
            {
                Debug.LogError("PlayerInteraction requires its prompt and dialogue UI references.", this);
                enabled = false;
                return;
            }

            promptPanel.SetActive(false);
            dialoguePanel.SetActive(false);
            closeButton.onClick.AddListener(CloseDialogue);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (IsDialogueOpen)
            {
                if (DialogueTarget == null || !DialogueTarget.isActiveAndEnabled
                    || (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)))
                    CloseDialogue();
                return;
            }

            InteractionTarget nearest = FindNearestTarget();
            if (!ReferenceEquals(nearest, CurrentTarget))
            {
                CurrentTarget = nearest;
                promptPanel.SetActive(nearest != null);
                if (nearest != null)
                    promptText.text = "[E] Bicara dengan " + nearest.DisplayName;
            }

            if (keyboard != null && keyboard.eKey.wasPressedThisFrame && CurrentTarget != null)
            {
                DialogueTarget = CurrentTarget;
                CurrentTarget = null;
                IsDialogueOpen = true;
                movementWasEnabled = movement.enabled;
                movement.enabled = false;
                promptPanel.SetActive(false);
                dialogueTitle.text = DialogueTarget.DisplayName;
                dialogueBody.text = DialogueTarget.Dialogue;
                dialoguePanel.SetActive(true);
                closeButton.Select();
            }
        }

        private InteractionTarget FindNearestTarget()
        {
            InteractionTarget nearest = null;
            float nearestDistance = interactionDistance * interactionDistance;
            Vector3 origin = transform.position;
            foreach (InteractionTarget candidate in InteractionTarget.Active)
            {
                if (candidate == null || !candidate.isActiveAndEnabled)
                    continue;
                Vector3 offset = candidate.transform.position - origin;
                offset.y = 0f;
                float distance = offset.sqrMagnitude;
                if (distance <= nearestDistance && (nearest == null || distance < nearestDistance))
                {
                    nearest = candidate;
                    nearestDistance = distance;
                }
            }
            return nearest;
        }

        private void CloseDialogue()
        {
            if (!IsDialogueOpen)
                return;
            IsDialogueOpen = false;
            DialogueTarget = null;
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            if (movement != null)
                movement.enabled = movementWasEnabled;
        }

        private void OnDisable()
        {
            CloseDialogue();
            CurrentTarget = null;
            if (promptPanel != null)
                promptPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(CloseDialogue);
        }
    }
}
