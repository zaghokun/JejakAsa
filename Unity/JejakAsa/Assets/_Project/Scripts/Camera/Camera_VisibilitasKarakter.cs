using UnityEngine;

namespace JejakAsa
{
    // Open overhead scenery when it crosses the player's silhouette in this
    // fixed isometric view. Keep its collider and shadow for spatial continuity.
    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public sealed class Camera_VisibilitasKarakter : MonoBehaviour
    {
        [SerializeField] private Transform subject;
        [SerializeField] private Renderer[] overheadScenery = new Renderer[0];
        [SerializeField] private Renderer[] attachedDetails = new Renderer[0];
        private UnityEngine.Rendering.ShadowCastingMode[] originalModes;
        private UnityEngine.Rendering.ShadowCastingMode[] originalDetailModes;

        private void OnEnable()
        {
            originalModes = new UnityEngine.Rendering.ShadowCastingMode[overheadScenery.Length];
            originalDetailModes = new UnityEngine.Rendering.ShadowCastingMode[attachedDetails.Length];
            for (int i = 0; i < overheadScenery.Length; i++)
                if (overheadScenery[i] != null) originalModes[i] = overheadScenery[i].shadowCastingMode;
            for (int i = 0; i < attachedDetails.Length; i++)
                if (attachedDetails[i] != null) originalDetailModes[i] = attachedDetails[i].shadowCastingMode;
        }

        private void LateUpdate()
        {
            if (subject == null || originalModes == null) return;
            for (int i = 0; i < overheadScenery.Length; i++)
            {
                var scenery = overheadScenery[i];
                if (scenery == null) continue;
                bool blocks = false;
                for (int sample = 0; sample < 3 && !blocks; sample++)
                {
                    // Scene actor origins sit at y=1, while their feet sit at y=0.
                    Vector3 point = subject.position + Vector3.up * (-.98f + sample * .8f);
                    Vector3 direction = point - transform.position;
                    float distance;
                    Bounds margin = scenery.bounds; margin.Expand(.12f);
                    blocks = margin.IntersectRay(new Ray(transform.position, direction.normalized), out distance)
                        && distance < direction.magnitude - .15f;
                }
                scenery.shadowCastingMode = blocks ? UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly : originalModes[i];
                if (i < attachedDetails.Length && attachedDetails[i] != null)
                    attachedDetails[i].shadowCastingMode = blocks ? UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly : originalDetailModes[i];
            }
        }

        private void OnDisable()
        {
            if (originalModes == null) return;
            for (int i = 0; i < overheadScenery.Length; i++)
                if (overheadScenery[i] != null) overheadScenery[i].shadowCastingMode = originalModes[i];
            for (int i = 0; i < attachedDetails.Length; i++)
                if (attachedDetails[i] != null) attachedDetails[i].shadowCastingMode = originalDetailModes[i];
        }
    }
}
