using UnityEngine;

namespace ithappy.Animals_FREE
{
    [DefaultExecutionOrder(-100)]
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform m_Player;

        [Header("Distancia y Altura")]
        [SerializeField] private float m_CamDistance = 8f;
        [SerializeField] private float m_HeightOffset = 2.5f;

        [Header("Rotación con Mouse (RMB)")]
        [SerializeField] private float m_SensitivityX = 3f;
        [SerializeField] private float m_SensitivityY = 2f;
        [SerializeField] private float m_MinPitch = 10f;
        [SerializeField] private float m_MaxPitch = 80f;
        [SerializeField] private float m_RotationSmoothing = 15f;

        [Header("Suavizado de Seguimiento")]
        [SerializeField] private float m_FollowSpeed = 10f;

        [Header("Colisión")]
        [SerializeField] private LayerMask m_CollisionLayers;
        [SerializeField] private float m_CollisionRadius = 0.3f;

        private float m_Yaw = 0f;
        private float m_Pitch = 30f;
        private float m_CurrentYaw = 0f;
        private float m_CurrentPitch = 30f;
        private Vector3 m_CurrentPosition;

        public bool IsRotating => Input.GetMouseButton(1);
        public Vector3 Target => transform.position + transform.forward * 20f;
        public float Yaw => m_CurrentYaw;

        private void Start()
        {
            if (m_Player == null) return;

            m_Yaw = m_Player.eulerAngles.y;
            m_CurrentYaw = m_Yaw;
            m_Pitch = 30f;
            m_CurrentPitch = 30f;

            m_CurrentPosition = CalculateDesiredPosition();
            transform.position = m_CurrentPosition;
            transform.LookAt(GetLookTarget());

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void LateUpdate()
        {
            if (m_Player == null) return;

            if (Input.GetMouseButton(1))
            {
                m_Yaw += Input.GetAxis("Mouse X") * m_SensitivityX;
                m_Pitch -= Input.GetAxis("Mouse Y") * m_SensitivityY;
                m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            m_CurrentYaw = Mathf.LerpAngle(m_CurrentYaw, m_Yaw, Time.deltaTime * m_RotationSmoothing);
            m_CurrentPitch = Mathf.LerpAngle(m_CurrentPitch, m_Pitch, Time.deltaTime * m_RotationSmoothing);

            Vector3 lookTarget = GetLookTarget();
            Vector3 desiredPosition = CalculateDesiredPosition();

            Vector3 dir = desiredPosition - lookTarget;
            float dirLen = dir.magnitude;
            if (dirLen > 1e-4f && Physics.SphereCast(lookTarget, m_CollisionRadius, dir / dirLen,
                    out RaycastHit hit, m_CamDistance, m_CollisionLayers))
            {
                desiredPosition = lookTarget + (dir / dirLen) *
                    Mathf.Max(hit.distance - m_CollisionRadius, 0.5f);
            }

            m_CurrentPosition = Vector3.Lerp(m_CurrentPosition, desiredPosition,
                Time.deltaTime * m_FollowSpeed);

            transform.position = m_CurrentPosition;
            transform.LookAt(lookTarget);
        }

        private Vector3 CalculateDesiredPosition()
        {
            var rotation = Quaternion.Euler(m_CurrentPitch, m_CurrentYaw, 0f);
            return GetLookTarget() + rotation * new Vector3(0f, 0f, -m_CamDistance);
        }

        private Vector3 GetLookTarget()
        {
            return m_Player.position + Vector3.up * m_HeightOffset;
        }

        public void SetInput(in Vector2 delta, float scroll) { }
    }
}
