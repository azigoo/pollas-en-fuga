using UnityEngine;

namespace ithappy.Animals_FREE
{
    [RequireComponent(typeof(CreatureMover))]
    [DefaultExecutionOrder(0)]
    public class MovePlayerInput : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField] private string m_HorizontalAxis = "Horizontal";
        [SerializeField] private string m_VerticalAxis = "Vertical";
        [SerializeField] private string m_JumpButton = "Jump";
        [SerializeField] private KeyCode m_RunKey = KeyCode.LeftShift;

        [Header("Camera")]
        [SerializeField] private ThirdPersonCamera m_Camera;

        private CreatureMover m_Mover;
        private Vector2 m_Axis;
        private Vector3 m_MoveReference;
        private Vector3 m_LookTarget;
        private bool m_IsRun;
        private bool m_IsJump;

        private void Awake()
        {
            m_Mover = GetComponent<CreatureMover>();
        }

        private void LateUpdate()
        {
            GatherInput();
            SetInput();
        }

        public void GatherInput()
        {
            float h = Input.GetAxisRaw(m_HorizontalAxis);
            float v = Input.GetAxisRaw(m_VerticalAxis);

            m_IsRun = Input.GetKey(m_RunKey);
            m_IsJump = Input.GetButtonDown(m_JumpButton);

            if (m_Camera != null)
            {
                GetCameraPlanarBasis(m_Camera.transform, out Vector3 planarForward, out Vector3 planarRight);

                m_Axis = Vector2.ClampMagnitude(new Vector2(h, v), 1f);

                m_MoveReference = transform.position + planarForward * 10f;
                m_LookTarget = m_Camera.transform.position;
            }
            else
            {
                m_Axis = Vector2.ClampMagnitude(new Vector2(h, v), 1f);
                m_MoveReference = transform.position + transform.forward * 10f;
                m_LookTarget = m_MoveReference;
            }
        }

        /// <summary>Base horizontal tipo Roblox: solo yaw de la cámara, sin inclinar el plano de movimiento.</summary>
        private static void GetCameraPlanarBasis(Transform cameraTransform, out Vector3 planarForward, out Vector3 planarRight)
        {
            float yaw = cameraTransform.eulerAngles.y;
            Quaternion yawOnly = Quaternion.Euler(0f, yaw, 0f);
            planarForward = yawOnly * Vector3.forward;
            planarRight = yawOnly * Vector3.right;
        }

        public void SetInput()
        {
            if (m_Mover != null)
            {
                m_Mover.SetInput(in m_Axis, in m_MoveReference, in m_LookTarget, in m_IsRun, m_IsJump);
            }
        }

        public void BindMover(CreatureMover mover)
        {
            m_Mover = mover;
        }
    }
}
