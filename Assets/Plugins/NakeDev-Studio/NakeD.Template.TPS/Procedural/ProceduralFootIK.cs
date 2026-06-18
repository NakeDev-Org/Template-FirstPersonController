using UnityEngine;

[RequireComponent(typeof(Animator))]
[DefaultExecutionOrder(10)]
public class ProceduralFootIK : MonoBehaviour
{
    [Header("Configurações de Colisão")]
    [SerializeField]
    [Tooltip("A camada (Layer) que representa o chão/terreno")]
    private LayerMask groundLayer = -1;

    [SerializeField]
    [Tooltip("Distância acima do pé para iniciar o raio")]
    private float raycastHeightOffset = 0.5f;

    [SerializeField]
    [Tooltip("Distância máxima que o raio tentará achar o chão para baixo")]
    private float raycastDistance = 1.0f;

    [SerializeField]
    [Tooltip("Ajuste fino de altura do pé para ele não afundar no chão")]
    private float footHeightOffset = 0.08f;

    [SerializeField]
    [Tooltip("Ângulo máximo de inclinação permitido para o pé")]
    private float maxFootTiltAngle = 45f;

    [SerializeField]
    [Tooltip("Comprimento aproximado do pé (calcanhar até a ponta) para compensar inclinações")]
    private float footLength = 0.22f;

    [Header("Pesos e Suavização")]
    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Peso geral da influência do IK")]
    private float globalIkWeight = 1.0f;

    [SerializeField]
    [Tooltip("Velocidade com que o pé alcança o chão")]
    private float footPositionSpeed = 22f;

    [SerializeField]
    [Tooltip("Velocidade com que o pé se alinha à inclinação do terreno")]
    private float footRotationSpeed = 20f;

    [SerializeField]
    [Tooltip("Velocidade de transição de ativação/desativação do peso do IK")]
    private float weightTransitionSpeed = 20f;

    [Header("Ajuste de Caminhada/Corrida")]
    [SerializeField]
    [Tooltip("O quanto a animação precisa levantar o pé acima da altura padrão para desligar o IK")]
    private float liftThreshold = 0.08f;

    [SerializeField]
    [Tooltip("Velocidade de movimento acima da qual o IK é desativado")]
    private float speedToDisableIk = 6f;

    [Header("Ajuste da Pélvis (Quadril)")]
    [SerializeField]
    [Tooltip("Se ativado, o corpo inteiro desce para permitir que os joelhos dobrem")]
    private bool enablePelvisAdjustment = true;

    [SerializeField]
    [Tooltip("Velocidade de transição da pélvis")]
    private float pelvisSpeed = 18f;

    [Header("Integração com Locomoção")]
    [SerializeField]
    [Tooltip("Desativa o IK automaticamente quando o personagem não estiver no chão")]
    private bool checkGrounded = true;

    [Header("Visualização (Scene View)")]
    [SerializeField]
    private float gizmoSize = 0.08f;

    public float GlobalIkWeight
    {
        get => globalIkWeight;
        set => globalIkWeight = Mathf.Clamp01(value);
    }
    public bool IsGrounded { get; set; } = true;
    public bool IsJumping { get; set; } = false;
    public bool IsSprinting { get; set; } = false;
    public bool IsAttacking { get; set; } = false;

    private class FootData
    {
        public Vector3 IkPosition;
        public Quaternion IkRotation;
        public float IkWeight;
        public float DefaultY;
    }

    private Animator _animator;
    private CharacterController _characterController;
    private Transform _rootTransform;

    private readonly FootData _leftFoot = new FootData();
    private readonly FootData _rightFoot = new FootData();

    private float _currentPelvisOffset;
    private Vector3 _previousPosition;
    private float _currentSpeed;

    private readonly RaycastHit[] _hitBuffer = new RaycastHit[10];

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _rootTransform = transform.root;
        _previousPosition = transform.position;

        InitializeFootData(HumanBodyBones.LeftFoot, _leftFoot);
        InitializeFootData(HumanBodyBones.RightFoot, _rightFoot);
    }

    private void InitializeFootData(HumanBodyBones boneType, FootData footData)
    {
        Transform footBone = _animator.GetBoneTransform(boneType);
        if (footBone != null)
        {
            footData.DefaultY = footBone.position.y - transform.position.y;
        }
        else
        {
            footData.DefaultY = 0.1f;
        }

        footData.IkPosition = transform.position + Vector3.up * footData.DefaultY;
        footData.IkRotation = transform.rotation;
        footData.IkWeight = 0f;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime > 0.0001f ? Time.deltaTime : 0.016f;

        Vector3 velocity = (transform.position - _previousPosition) / deltaTime;
        _previousPosition = transform.position;

        float rawSpeed =
            _characterController != null
                ? _characterController.velocity.magnitude
                : velocity.magnitude;
        _currentSpeed = Mathf.Lerp(_currentSpeed, rawSpeed, deltaTime * 5f);

        if (_characterController != null)
        {
            IsGrounded = _characterController.isGrounded;
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (layerIndex != 0 || _animator == null)
            return;

        float speedWeightFactor = Mathf.Clamp01(1f - (_currentSpeed / speedToDisableIk));
        bool isLocomotionBypass =
            (!checkGrounded || IsGrounded) && !IsJumping && !IsSprinting && !IsAttacking;
        float stateWeightFactor = isLocomotionBypass ? speedWeightFactor : 0f;

        SolveFoot(AvatarIKGoal.LeftFoot, _leftFoot, stateWeightFactor);
        SolveFoot(AvatarIKGoal.RightFoot, _rightFoot, stateWeightFactor);

        if (enablePelvisAdjustment)
        {
            ApplyPelvisAdjustment(stateWeightFactor);
        }
    }

    private void SolveFoot(AvatarIKGoal footType, FootData foot, float stateWeightFactor)
    {
        Vector3 animFootPos = _animator.GetIKPosition(footType);
        Quaternion animFootRot = _animator.GetIKRotation(footType);

        float trueLiftHeight = (animFootPos.y - transform.position.y) - foot.DefaultY;
        float targetWeight = globalIkWeight * stateWeightFactor;
        float deltaTime = Time.deltaTime > 0.0001f ? Time.deltaTime : 0.016f;

        if (targetWeight > 0.01f)
        {
            Vector3 rayOrigin = animFootPos + Vector3.up * raycastHeightOffset;
            float sphereRadius = footLength * 0.4f;

            if (FindGround(rayOrigin, sphereRadius, out RaycastHit hit))
            {
                foot.IkWeight = Mathf.MoveTowards(
                    foot.IkWeight,
                    targetWeight,
                    deltaTime * weightTransitionSpeed
                );

                Vector3 charUp = transform.up;
                Vector3 groundNormal = hit.normal;
                float tiltAngle = Vector3.Angle(charUp, groundNormal);

                if (tiltAngle > maxFootTiltAngle)
                {
                    groundNormal = Vector3.Slerp(
                        charUp,
                        groundNormal,
                        maxFootTiltAngle / tiltAngle
                    );
                    tiltAngle = maxFootTiltAngle;
                }

                float slopeHeightCompensation =
                    Mathf.Sin(tiltAngle * Mathf.Deg2Rad) * (footLength * 0.5f);
                float correctedHitY = rayOrigin.y - hit.distance;

                if (hit.distance <= 0f)
                {
                    correctedHitY =
                        hit.point.y > transform.position.y ? hit.point.y : animFootPos.y;
                }
                else
                {
                    correctedHitY -= sphereRadius;
                }

                float stepArc = Mathf.Max(0f, trueLiftHeight);
                Vector3 targetPosition = animFootPos;
                targetPosition.y =
                    correctedHitY + footHeightOffset + slopeHeightCompensation + stepArc;

                Vector3 footForward = animFootRot * Vector3.forward;
                Vector3 projectedForward = Vector3
                    .ProjectOnPlane(footForward, groundNormal)
                    .normalized;
                Quaternion groundRotation = Quaternion.LookRotation(projectedForward, groundNormal);

                float rotationBlend = Mathf.Clamp01(stepArc / (liftThreshold * 2f));
                Quaternion targetRotation = Quaternion.Slerp(
                    groundRotation,
                    animFootRot,
                    rotationBlend
                );

                float smoothY = Mathf.Lerp(
                    foot.IkPosition.y,
                    targetPosition.y,
                    deltaTime * footPositionSpeed
                );
                foot.IkPosition = new Vector3(targetPosition.x, smoothY, targetPosition.z);
                foot.IkRotation = Quaternion.Slerp(
                    foot.IkRotation,
                    targetRotation,
                    deltaTime * footRotationSpeed
                );

                SetIK(footType, foot);

                if (gizmoSize > 0)
                    Debug.DrawLine(rayOrigin, hit.point, Color.green);
                return;
            }
        }

        foot.IkWeight = Mathf.MoveTowards(
            foot.IkWeight,
            0f,
            deltaTime * (weightTransitionSpeed * 0.75f)
        );

        float fallbackSmoothY = Mathf.Lerp(
            foot.IkPosition.y,
            animFootPos.y,
            deltaTime * footPositionSpeed
        );
        foot.IkPosition = new Vector3(animFootPos.x, fallbackSmoothY, animFootPos.z);

        foot.IkRotation = Quaternion.Slerp(
            foot.IkRotation,
            animFootRot,
            deltaTime * footRotationSpeed
        );

        SetIK(footType, foot);
    }

    private bool FindGround(Vector3 origin, float radius, out RaycastHit validHit)
    {
        validHit = default;

        int hitCount = Physics.SphereCastNonAlloc(
            origin,
            radius,
            Vector3.down,
            _hitBuffer,
            raycastDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (hitCount == 0)
            return false;

        float closestDistance = float.MaxValue;
        bool found = false;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = _hitBuffer[i];

            if (
                hit.collider.transform.root == _rootTransform
                || hit.collider.transform.IsChildOf(transform)
            )
                continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                validHit = hit;
                found = true;
            }
        }

        return found;
    }

    private void SetIK(AvatarIKGoal footType, FootData foot)
    {
        _animator.SetIKPosition(footType, foot.IkPosition);
        _animator.SetIKRotation(footType, foot.IkRotation);
        _animator.SetIKPositionWeight(footType, foot.IkWeight);
        _animator.SetIKRotationWeight(footType, foot.IkWeight);
    }

    private void ApplyPelvisAdjustment(float stateWeightFactor)
    {
        Vector3 bodyPosition = _animator.bodyPosition;
        float targetOffset = 0f;

        if (stateWeightFactor > 0.01f)
        {
            Vector3 animLeftPos = _animator.GetIKPosition(AvatarIKGoal.LeftFoot);
            Vector3 animRightPos = _animator.GetIKPosition(AvatarIKGoal.RightFoot);

            float leftOffset = _leftFoot.IkPosition.y - animLeftPos.y;
            float rightOffset = _rightFoot.IkPosition.y - animRightPos.y;

            float highestFootOffset = Mathf.Max(leftOffset, rightOffset);
            targetOffset = Mathf.Min(0f, highestFootOffset);
        }

        float deltaTime = Time.deltaTime > 0.0001f ? Time.deltaTime : 0.016f;
        float currentSpeed =
            targetOffset < _currentPelvisOffset ? pelvisSpeed * 1.2f : pelvisSpeed * 0.8f;

        _currentPelvisOffset = Mathf.Lerp(
            _currentPelvisOffset,
            targetOffset,
            deltaTime * currentSpeed
        );
        _currentPelvisOffset = Mathf.Clamp(_currentPelvisOffset, -0.8f, 0.3f);

        bodyPosition.y += _currentPelvisOffset;
        _animator.bodyPosition = bodyPosition;
    }

    public void DrawDebugGizmos()
    {
        Animator anim = GetComponent<Animator>();
        if (anim == null)
            return;

        Transform leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        Transform rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);

        DrawFootGizmo(leftFoot);
        DrawFootGizmo(rightFoot);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_leftFoot.IkPosition, gizmoSize);
            Gizmos.DrawWireSphere(_rightFoot.IkPosition, gizmoSize);
        }
    }

    private void DrawFootGizmo(Transform footTransform)
    {
        if (footTransform == null)
            return;

        Vector3 rayOrigin = footTransform.position + Vector3.up * raycastHeightOffset;
        Vector3 rayEnd = rayOrigin + Vector3.down * raycastDistance;
        float sphereRadius = footLength * 0.4f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(rayOrigin, rayEnd);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rayOrigin, sphereRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(rayEnd, sphereRadius);

        Gizmos.color = Color.green;
        Vector3 footOffsetPos = footTransform.position + Vector3.up * footHeightOffset;
        Gizmos.DrawWireCube(footOffsetPos, new Vector3(gizmoSize, gizmoSize, gizmoSize));
    }
}
