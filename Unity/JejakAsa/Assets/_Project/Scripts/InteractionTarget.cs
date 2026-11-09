using System.Collections.Generic;
using UnityEngine;

namespace JejakAsa
{
    [DisallowMultipleComponent]
    public sealed class InteractionTarget : MonoBehaviour
    {
        internal static readonly List<InteractionTarget> Active = new List<InteractionTarget>();

        [SerializeField] private string displayName = "Warga";
        [SerializeField, TextArea(2, 5)] private string dialogue = "Terima kasih sudah datang membantu warga.";

        public string DisplayName => displayName;
        public string Dialogue => dialogue;

        private void OnEnable()
        {
            Active.Add(this);
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }
    }
}
