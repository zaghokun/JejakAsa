using UnityEngine;

namespace JejakAsa
{
    [DisallowMultipleComponent]
    public sealed class Camera_IsometrikTetap : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private Vector3 offset;
        private Quaternion orientation;

        private void Awake()
        {
            if (target == null)
            {
                Debug.LogError("Camera_IsometrikTetap requires a follow target.", this);
                enabled = false;
                return;
            }

            offset = transform.position - target.position;
            orientation = transform.rotation;
        }

        private void LateUpdate()
        {
            transform.SetPositionAndRotation(target.position + offset, orientation);
        }
    }
}
