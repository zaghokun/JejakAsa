using UnityEngine;

namespace JejakAsa
{
    [DisallowMultipleComponent]
    public sealed class FixedIsometricCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private Vector3 offset;
        private Quaternion orientation;

        private void Awake()
        {
            if (target == null)
            {
                Debug.LogError("FixedIsometricCamera requires a follow target.", this);
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
