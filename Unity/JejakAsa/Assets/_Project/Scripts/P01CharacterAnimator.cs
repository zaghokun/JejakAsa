using System;
using UnityEngine;

namespace JejakAsa
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class P01CharacterAnimator : MonoBehaviour
    {
        [SerializeField] private string characterName;
        private Animator animator;
        private Vector3 previousPosition;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            previousPosition = transform.position;
        }

        private void OnEnable()
        {
            StoryDialog_InteraksiNPC.SpeakerChanged += OnSpeakerChanged;
            previousPosition = transform.position;
        }

        private void OnDisable()
        {
            StoryDialog_InteraksiNPC.SpeakerChanged -= OnSpeakerChanged;
        }

        private void OnSpeakerChanged(string speaker)
        {
            if (string.Equals(characterName, speaker, StringComparison.OrdinalIgnoreCase))
                animator.CrossFadeInFixedTime("TalkStart", 0.12f, 0, 0f);
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("TalkStart"))
                animator.CrossFadeInFixedTime("Idle", 0.12f);
        }

        private void LateUpdate()
        {
            Vector3 displacement = transform.position - previousPosition;
            previousPosition = transform.position;
            if (Time.deltaTime <= 0f)
                return;

            displacement.y = 0f;
            float speedSquared = displacement.sqrMagnitude / (Time.deltaTime * Time.deltaTime);
            AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
            if (speedSquared < 0.01f && current.IsName("TalkStart"))
                return;

            string next = speedSquared > 27.56f ? "Run" : speedSquared > 0.01f ? "Walk" : "Idle";
            if (!current.IsName(next) && !animator.IsInTransition(0))
                animator.CrossFadeInFixedTime(next, 0.12f);
        }
    }
}
