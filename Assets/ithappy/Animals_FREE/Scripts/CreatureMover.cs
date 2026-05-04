using System;
using UnityEngine;

namespace ithappy.Animals_FREE
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    public class CreatureMover : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField]
        private float m_WalkSpeed = 1f;
        [SerializeField]
        private float m_RunSpeed = 4f;
        [SerializeField, Range(0f, 7200f)]
        private float m_RotateSpeed = 540f;
        [SerializeField]
        private Space m_Space = Space.Self;
        [SerializeField, Tooltip("Altura aproximada del salto en metros (apex).")]
        private float m_JumpHeight = 5.2f;
        [SerializeField, Tooltip("Gravedad vertical mientras subes (mayor = subida más rápida y mismo apex si el impulso usa el mismo valor).")]
        private float m_RiseGravityMultiplier = 1.5f;
        [SerializeField, Tooltip("Gravedad vertical al caer (> subida = caída más rápida, menos tiempo en el aire).")]
        private float m_FallGravityMultiplier = 2.75f;
        [SerializeField, Tooltip("Mismo nombre que el eje Jump del Input Manager (para jump cut).")]
        private string m_JumpButtonName = "Jump";
        [SerializeField, Tooltip("Si sueltas Jump en la subida, se suma este extra al multiplicador de gravedad (corta el arco).")]
        private float m_JumpCutGravityAdd = 0.9f;
        [SerializeField]
        private bool m_EnableJumpCut = true;

        [Header("Animator")]
        [SerializeField]
        private string m_VerticalID = "Vert";
        [SerializeField]
        private string m_StateID = "State";
        [SerializeField]
        private LookWeight m_LookWeight = new(1f, 0.3f, 0.7f, 1f);

        private Transform m_Transform;
        private CharacterController m_Controller;
        private Animator m_Animator;

        private MovementHandler m_Movement;
        private AnimationHandler m_Animation;

        private Vector2 m_Axis;
        private Vector3 m_MoveReference;
        private Vector3 m_LookTarget;
        private bool m_IsRun;
        private bool m_IsJump;
        private bool m_IsMoving;

        public Vector2 Axis => m_Axis;
        public Vector3 Target => m_LookTarget;
        public bool IsRun => m_IsRun;

        private void OnValidate()
        {
            m_WalkSpeed = Mathf.Max(m_WalkSpeed, 0f);
            m_RunSpeed = Mathf.Max(m_RunSpeed, m_WalkSpeed);

            m_Movement?.SetStats(m_WalkSpeed / 3.6f, m_RunSpeed / 3.6f, m_RotateSpeed, m_JumpHeight, m_Space,
                m_RiseGravityMultiplier, m_FallGravityMultiplier, m_JumpCutGravityAdd, m_EnableJumpCut, m_JumpButtonName);
        }

        private void Awake()
        {
            m_Transform = transform;
            m_Controller = GetComponent<CharacterController>();
            m_Animator = GetComponent<Animator>();

            m_Movement = new MovementHandler(m_Controller, m_Transform, m_WalkSpeed, m_RunSpeed, m_RotateSpeed, m_JumpHeight, m_Space,
                m_RiseGravityMultiplier, m_FallGravityMultiplier, m_JumpCutGravityAdd, m_EnableJumpCut, m_JumpButtonName);
            m_Animation = new AnimationHandler(m_Animator, m_VerticalID, m_StateID);
        }

        private void LateUpdate()
        {
            m_Movement.Move(Time.deltaTime, in m_Axis, in m_MoveReference, m_IsRun, m_IsMoving, m_IsJump, out var animAxis, out var isAir);
            m_Animation.Animate(in animAxis, m_IsRun ? 1f : 0f, Time.deltaTime);
        }

        private void OnAnimatorIK()
        {
            m_Animation.AnimateIK(in m_LookTarget, m_LookWeight);
        }

        /// <summary>Compatibilidad: un solo punto se usa para movimiento e IK.</summary>
        public void SetInput(in Vector2 axis, in Vector3 target, in bool isRun, in bool isJump)
        {
            SetInput(in axis, in target, in target, in isRun, in isJump);
        }

        /// <summary>
        /// moveReferenceWorld: punto delante del personaje en la dirección de referencia del movimiento (p. ej. player + yawCam * forward * 10).
        /// lookAtWorld: punto hacia el que mira el IK (p. ej. posición de la cámara).
        /// </summary>
        public void SetInput(in Vector2 axis, in Vector3 moveReferenceWorld, in Vector3 lookAtWorld, in bool isRun, in bool isJump)
        {
            m_MoveReference = moveReferenceWorld;
            m_LookTarget = lookAtWorld;
            m_IsRun = isRun;
            m_IsJump = isJump;

            if (axis.sqrMagnitude < Mathf.Epsilon)
            {
                m_Axis = Vector2.zero;
                m_IsMoving = false;
            }
            else
            {
                m_Axis = Vector3.ClampMagnitude(axis, 1f);
                m_IsMoving = true;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y > m_Controller.stepOffset)
            {
                m_Movement.SetSurface(hit.normal);
            }
        }

        [Serializable]
        private struct LookWeight
        {
            public float weight;
            public float body;
            public float head;
            public float eyes;

            public LookWeight(float weight, float body, float head, float eyes)
            {
                this.weight = weight;
                this.body = body;
                this.head = head;
                this.eyes = eyes;
            }
        }

        #region Handlers
        private class MovementHandler
        {
            private readonly CharacterController m_Controller;
            private readonly Transform m_Transform;

            private float m_WalkSpeed;
            private float m_RunSpeed;
            private float m_RotateSpeed;
            private float m_JumpHeight;
            private float m_RiseGravityMult;
            private float m_FallGravityMult;
            private float m_JumpCutGravityAdd;
            private bool m_EnableJumpCut;
            private string m_JumpButtonName;

            private Space m_Space;

            private Vector3 m_Normal = Vector3.up;
            private Vector3 m_GravityVelocity;

            public MovementHandler(CharacterController controller, Transform transform, float walkSpeed, float runSpeed, float rotateSpeed, float jumpHeight, Space space,
                float riseGravityMult, float fallGravityMult, float jumpCutGravityAdd, bool enableJumpCut, string jumpButtonName)
            {
                m_Controller = controller;
                m_Transform = transform;

                m_WalkSpeed = walkSpeed;
                m_RunSpeed = runSpeed;
                m_RotateSpeed = rotateSpeed;
                m_JumpHeight = jumpHeight;
                m_Space = space;
                m_RiseGravityMult = Mathf.Max(0.01f, riseGravityMult);
                m_FallGravityMult = Mathf.Max(0.01f, fallGravityMult);
                m_JumpCutGravityAdd = Mathf.Max(0f, jumpCutGravityAdd);
                m_EnableJumpCut = enableJumpCut;
                m_JumpButtonName = string.IsNullOrEmpty(jumpButtonName) ? "Jump" : jumpButtonName;

                m_GravityVelocity = Vector3.zero;
            }

            public void SetStats(float walkSpeed, float runSpeed, float rotateSpeed, float jumpHeight, Space space,
                float riseGravityMult, float fallGravityMult, float jumpCutGravityAdd, bool enableJumpCut, string jumpButtonName)
            {
                m_WalkSpeed = walkSpeed;
                m_RunSpeed = runSpeed;
                m_RotateSpeed = rotateSpeed;
                m_JumpHeight = jumpHeight;
                m_Space = space;
                m_RiseGravityMult = Mathf.Max(0.01f, riseGravityMult);
                m_FallGravityMult = Mathf.Max(0.01f, fallGravityMult);
                m_JumpCutGravityAdd = Mathf.Max(0f, jumpCutGravityAdd);
                m_EnableJumpCut = enableJumpCut;
                m_JumpButtonName = string.IsNullOrEmpty(jumpButtonName) ? "Jump" : jumpButtonName;
            }

            public void SetSurface(in Vector3 normal)
            {
                m_Normal = normal;
            }

            public void Move(float deltaTime, in Vector2 axis, in Vector3 moveReferenceWorld, bool isRun, bool isMoving, bool isJump, out Vector2 animAxis, out bool isAir)
            {
                Vector3 deltaRef = moveReferenceWorld - m_Transform.position;
                Vector3 cameraLook = deltaRef.sqrMagnitude > 1e-6f
                    ? Vector3.Normalize(deltaRef)
                    : m_Transform.forward;

                ConvertMovement(in axis, in cameraLook, out var movement);

                CalculateGravity(deltaTime, isJump, out isAir);

                Vector3 moveOnPlane = Vector3.ProjectOnPlane(movement, Vector3.up);
                if (isMoving && moveOnPlane.sqrMagnitude > 1e-4f)
                {
                    ApplyFacing(deltaTime, moveOnPlane.normalized);
                }

                Displace(deltaTime, in movement, isRun);

                if (m_Controller.isGrounded)
                {
                    m_Normal = Vector3.up;
                }

                GenAnimationAxis(in movement, out animAxis);
            }

            private void ConvertMovement(in Vector2 axis, in Vector3 targetForward, out Vector3 movement)
            {
                Vector3 forward;
                Vector3 right;

                if (m_Space == Space.Self)
                {
                    forward = new Vector3(targetForward.x, 0f, targetForward.z);
                    if (forward.sqrMagnitude < 1e-6f)
                    {
                        forward = Vector3.forward;
                    }
                    else
                    {
                        forward.Normalize();
                    }

                    right = Vector3.Cross(Vector3.up, forward).normalized;
                }
                else
                {
                    forward = Vector3.forward;
                    right = Vector3.right;
                }

                movement = axis.x * right + axis.y * forward;
                movement = Vector3.ProjectOnPlane(movement, m_Normal);
            }

            private void Displace(float deltaTime, in Vector3 movement, bool isRun)
            {
                Vector3 displacement = (isRun ? m_RunSpeed : m_WalkSpeed) * movement;
                displacement += m_GravityVelocity;
                displacement *= deltaTime;

                m_Controller.Move(displacement);
            }

            private void CalculateGravity(float deltaTime, bool isJump, out bool isAir)
            {
                bool isGrounded = m_Controller.isGrounded;

                if (isGrounded && m_GravityVelocity.y < 0f)
                {
                    m_GravityVelocity.y = -2f;
                }

                if (!isGrounded)
                {
                    float mult = m_GravityVelocity.y > 0.001f ? m_RiseGravityMult : m_FallGravityMult;
                    if (m_EnableJumpCut && m_GravityVelocity.y > 0.001f && !Input.GetButton(m_JumpButtonName))
                    {
                        mult += m_JumpCutGravityAdd;
                    }

                    m_GravityVelocity += Physics.gravity * mult * deltaTime;
                }

                if (isJump && isGrounded)
                {
                    float gRise = -Physics.gravity.y * m_RiseGravityMult;
                    m_GravityVelocity.y = Mathf.Sqrt(2f * m_JumpHeight * gRise);
                }

                isAir = !isGrounded;
            }

            private void ApplyFacing(float deltaTime, in Vector3 horizontalForward)
            {
                Quaternion from = m_Transform.rotation;
                Quaternion to = Quaternion.LookRotation(horizontalForward, Vector3.up);
                float maxDeg = m_RotateSpeed * deltaTime;
                m_Transform.rotation = Quaternion.RotateTowards(from, to, maxDeg);
            }

            private void GenAnimationAxis(in Vector3 movement, out Vector2 animAxis)
            {
                if (m_Space == Space.Self)
                {
                    animAxis = new Vector2(Vector3.Dot(movement, m_Transform.right), Vector3.Dot(movement, m_Transform.forward));
                }
                else
                {
                    animAxis = new Vector2(Vector3.Dot(movement, Vector3.right), Vector3.Dot(movement, Vector3.forward));
                }
            }
        }

        private class AnimationHandler
        {
            private readonly Animator m_Animator;
            private readonly string m_VerticalID;
            private readonly string m_StateID;

            private readonly float k_InputFlow = 4.5f;

            private float m_FlowState;
            private Vector2 m_FlowAxis;

            public AnimationHandler(Animator animator, string verticalID, string stateID)
            {
                m_Animator = animator;
                m_VerticalID = verticalID;
                m_StateID = stateID;
            }

            public void Animate(in Vector2 axis, float state, float deltaTime)
            {
                m_Animator.SetFloat(m_VerticalID, m_FlowAxis.magnitude);
                m_Animator.SetFloat(m_StateID, Mathf.Clamp01(m_FlowState));

                m_FlowAxis = Vector2.ClampMagnitude(m_FlowAxis + k_InputFlow * deltaTime * (axis - m_FlowAxis).normalized, 1f);
                m_FlowState = Mathf.Clamp01(m_FlowState + k_InputFlow * deltaTime * Mathf.Sign(state - m_FlowState));
            }

            public void AnimateIK(in Vector3 target, in LookWeight lookWeight)
            {
                m_Animator.SetLookAtPosition(target);
                m_Animator.SetLookAtWeight(lookWeight.weight, lookWeight.body, lookWeight.head, lookWeight.eyes);
            }
        }
        #endregion
    }
}
