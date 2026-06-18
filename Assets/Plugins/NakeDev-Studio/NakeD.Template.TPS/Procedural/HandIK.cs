using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HandIK : MonoBehaviour
{
    [Header("Configurações de Colisão")]
    [SerializeField]
    [Tooltip("A camada (Layer) que representa paredes/obstáculos")]
    private LayerMask obstacleLayer;

    [SerializeField]
    [Tooltip("Distância máxima de alcance dos braços")]
    private float handReachDistance = 0.75f;

    [SerializeField]
    [Tooltip(
        "Ajuste fino de distância da mão em relação à parede (evita atravessar os dedos)"
    )]
    private float handOffset = 0.12f;

    [SerializeField]
    [Tooltip(
        "Raio da esfera (SphereCast) para detectar paredes, simulando o volume da mão"
    )]
    private float handSphereRadius = 0.08f;

    [SerializeField]
    [Tooltip(
        "Distância mínima para o IK ativar. Se o personagem esmagar a cara na parede, o IK desliga para evitar braços quebrados."
    )]
    private float handMinDistance = 0.2f;

    [SerializeField]
    [Tooltip("Ângulo Frontal (padrão 15 graus levemente para dentro)")]
    private float frontRayAngle = 15f;

    [SerializeField]
    [Tooltip("Ângulo Diagonal (padrão 45 graus)")]
    private float diagonalRayAngle = 45f;

    [SerializeField]
    [Tooltip("Ângulo Lateral (padrão 80 graus)")]
    private float sideRayAngle = 80f;

    [SerializeField]
    [Tooltip(
        "Ângulos Verticais para detectar mesas/objetos baixos (0 = reto, negativo = para baixo)"
    )]
    private float[] verticalRayAngles = { 0f, -25f, -50f };

    [Header("Pesos e Suavização")]
    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Peso geral da influência do IK das mãos")]
    private float globalHandWeight = 1.0f;

    [SerializeField]
    [Tooltip("Velocidade de transição para apoiar ou retirar a mão da parede")]
    private float handTransitionSpeed = 10f;

    [Header("Integração com Locomoção/Combate")]
    [SerializeField]
    [Tooltip(
        "Desativa o IK automaticamente quando o personagem não estiver no chão"
    )]
    private bool checkGrounded = true;

    [Header("Visualização (Scene View)")]
    [SerializeField]
    private float gizmoSize = 0.05f;

    // Propriedades públicas para integração externa
    public bool IsGrounded { get; set; } = true;
    public bool IsSprinting { get; set; } = false;
    public bool DisableLeftHandIK { get; set; } = false;
    public bool DisableRightHandIK { get; set; } = false;

    private Animator _animator;
    private CharacterController _characterController;

    private float _leftHandWeight;
    private float _rightHandWeight;

    private Vector3 _leftHandIkPos;
    private Vector3 _rightHandIkPos;
    private Quaternion _leftHandIkRot;
    private Quaternion _rightHandIkRot;

    // Pre-allocated array para evitar Garbage Collection no SphereCast
    private RaycastHit[] _raycastHits = new RaycastHit[10];

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        // Proteção: Roda a matemática de IK apenas na primeira camada que permitir IK Pass.
        if (layerIndex != 0)
            return;
        if (_animator == null)
            return;

        // Atualiza o estado de Grounded se o CharacterController existir
        if (_characterController != null)
        {
            IsGrounded = _characterController.isGrounded;
        }

        // 1. Obter posições originais dos ombros
        Transform leftShoulder = _animator.GetBoneTransform(
            HumanBodyBones.LeftShoulder
        );
        Transform rightShoulder = _animator.GetBoneTransform(
            HumanBodyBones.RightShoulder
        );

        Vector3 leftOrigin =
            leftShoulder != null
                ? leftShoulder.position
                : transform.position
                    + Vector3.up * 1.4f
                    - transform.right * 0.3f;
        Vector3 rightOrigin =
            rightShoulder != null
                ? rightShoulder.position
                : transform.position
                    + Vector3.up * 1.4f
                    + transform.right * 0.3f;

        // 2 & 3 & 4. Disparar o "radar" de 3 raios para cada braço e achar o melhor
        RaycastHit leftHit;
        bool leftHitSuccess = GetBestWallHit(leftOrigin, -1f, out leftHit);

        RaycastHit rightHit;
        bool rightHitSuccess = GetBestWallHit(rightOrigin, 1f, out rightHit);

        // 5. Ajustar as distâncias calculadas
        float leftAdjustedDistance = leftHitSuccess ? leftHit.distance : 0f;
        float rightAdjustedDistance = rightHitSuccess ? rightHit.distance : 0f;

        // 6. Resolver o posicionamento IK de cada mão
        SolveHand(
            AvatarIKGoal.LeftHand,
            leftHitSuccess,
            leftHit,
            leftAdjustedDistance,
            ref _leftHandIkPos,
            ref _leftHandIkRot,
            ref _leftHandWeight
        );
        SolveHand(
            AvatarIKGoal.RightHand,
            rightHitSuccess,
            rightHit,
            rightAdjustedDistance,
            ref _rightHandIkPos,
            ref _rightHandIkRot,
            ref _rightHandWeight
        );
    }

    private bool GetBestWallHit(
        Vector3 origin,
        float sideMultiplier,
        out RaycastHit bestHit
    )
    {
        bestHit = new RaycastHit();
        bool foundAny = false;
        float closestDist = float.MaxValue;

        float[] anglesToTest =
        {
            frontRayAngle,
            diagonalRayAngle,
            sideRayAngle,
        };

        foreach (float hAngle in anglesToTest)
        {
            foreach (float vAngle in verticalRayAngles)
            {
                // Rotação horizontal e vertical
                Quaternion hRot = Quaternion.AngleAxis(
                    hAngle * sideMultiplier,
                    transform.up
                );
                Quaternion vRot = Quaternion.AngleAxis(
                    -vAngle,
                    transform.right
                ); // Negativo agora vai para baixo
                Vector3 dir = hRot * vRot * transform.forward;

                RaycastHit hit;
                if (
                    CastRayIgnoringSelf(origin, dir, handReachDistance, out hit)
                )
                {
                    if (hit.distance < closestDist)
                    {
                        closestDist = hit.distance;
                        bestHit = hit;
                        foundAny = true;
                    }
                }
            }
        }

        return foundAny;
    }

    // Função auxiliar para ignorar colisões com o próprio personagem, usando SphereCastNonAlloc para zero GC
    private bool CastRayIgnoringSelf(
        Vector3 start,
        Vector3 direction,
        float distance,
        out RaycastHit hit
    )
    {
        int hitCount = Physics.SphereCastNonAlloc(
            start,
            handSphereRadius,
            direction,
            _raycastHits,
            distance,
            obstacleLayer
        );

        float closestDistance = float.MaxValue;
        bool foundValidHit = false;
        hit = new RaycastHit();

        for (int i = 0; i < hitCount; i++)
        {
            var h = _raycastHits[i];

            // Ignora se pertencer à mesma árvore de GameObjects do player
            // Também ignora se estiver mais perto do que a distância mínima (handMinDistance)
            if (
                h.collider != null
                && h.collider.transform.root != transform.root
                && h.distance > handMinDistance
            )
            {
                if (h.distance < closestDistance)
                {
                    closestDistance = h.distance;
                    hit = h;
                    foundValidHit = true;
                }
            }
        }

        return foundValidHit;
    }

    private void SolveHand(
        AvatarIKGoal hand,
        bool hasHit,
        RaycastHit hit,
        float adjustedDistance,
        ref Vector3 ikPosition,
        ref Quaternion ikRotation,
        ref float ikWeight
    )
    {
        if (hasHit)
        {
            // Calcula um peso dinâmico proporcional à proximidade da parede
            float distanceRange = handReachDistance - handOffset;
            float distanceFactor = 1f;
            if (distanceRange > 0.01f)
            {
                distanceFactor =
                    1f
                    - Mathf.Clamp01(
                        (adjustedDistance - handOffset) / distanceRange
                    );
                distanceFactor = Mathf.SmoothStep(0f, 1f, distanceFactor);
            }

            // Verifica se o IK desta mão está desativado por locomoção (no ar, correndo) ou combate/ações
            bool isHandDisabled =
                (checkGrounded && !IsGrounded)
                || IsSprinting
                || (hand == AvatarIKGoal.LeftHand && DisableLeftHandIK)
                || (hand == AvatarIKGoal.RightHand && DisableRightHandIK);

            float targetWeight = isHandDisabled
                ? 0f
                : (globalHandWeight * distanceFactor);

            // Interpola o peso do IK
            ikWeight = Mathf.MoveTowards(
                ikWeight,
                targetWeight,
                Time.deltaTime * handTransitionSpeed
            );

            // Posição final com offset para frente da parede
            Vector3 targetPosition = hit.point + hit.normal * handOffset;

            // Rotação Antitorção e Antiflip
            float facingWallFactor = Mathf.Abs(
                Vector3.Dot(transform.forward, hit.normal)
            );
            Vector3 referenceDirection = Vector3
                .Lerp(transform.forward, transform.up, facingWallFactor)
                .normalized;
            Vector3 projectedForward = Vector3.ProjectOnPlane(
                referenceDirection,
                hit.normal
            );

            if (projectedForward.sqrMagnitude < 0.001f)
            {
                projectedForward = transform.up;
            }
            else
            {
                projectedForward.Normalize();
            }

            if (Vector3.Dot(projectedForward, transform.up) < -0.1f)
            {
                projectedForward = -projectedForward;
            }

            Quaternion targetRotation = Quaternion.LookRotation(
                projectedForward,
                hit.normal
            );

            ikPosition = Vector3.Lerp(
                ikPosition,
                targetPosition,
                Time.deltaTime * handTransitionSpeed
            );
            ikRotation = Quaternion.Slerp(
                ikRotation,
                targetRotation,
                Time.deltaTime * handTransitionSpeed
            );

            _animator.SetIKPosition(hand, ikPosition);
            _animator.SetIKRotation(hand, ikRotation);
            _animator.SetIKPositionWeight(hand, ikWeight);
            _animator.SetIKRotationWeight(hand, ikWeight);

            // Ajuste do Cotovelo (Hint)
            AvatarIKHint hint =
                hand == AvatarIKGoal.LeftHand
                    ? AvatarIKHint.LeftElbow
                    : AvatarIKHint.RightElbow;
            Vector3 hintDir =
                -transform.up
                + (
                    hand == AvatarIKGoal.LeftHand
                        ? -transform.right
                        : transform.right
                );
            Vector3 shoulderPos =
                hand == AvatarIKGoal.LeftHand
                    ? _animator
                        .GetBoneTransform(HumanBodyBones.LeftShoulder)
                        .position
                    : _animator
                        .GetBoneTransform(HumanBodyBones.RightShoulder)
                        .position;

            Vector3 hintPos = shoulderPos + hintDir.normalized * 0.4f;
            _animator.SetIKHintPosition(hint, hintPos);
            _animator.SetIKHintPositionWeight(hint, ikWeight);
        }
        else
        {
            ikWeight = Mathf.MoveTowards(
                ikWeight,
                0f,
                Time.deltaTime * handTransitionSpeed
            );

            if (ikWeight > 0.01f)
            {
                Vector3 currentAnimPos = _animator.GetIKPosition(hand);
                Quaternion currentAnimRot = _animator.GetIKRotation(hand);

                ikPosition = Vector3.Lerp(
                    ikPosition,
                    currentAnimPos,
                    Time.deltaTime * handTransitionSpeed
                );
                ikRotation = Quaternion.Slerp(
                    ikRotation,
                    currentAnimRot,
                    Time.deltaTime * handTransitionSpeed
                );

                _animator.SetIKPosition(hand, ikPosition);
                _animator.SetIKRotation(hand, ikRotation);
                _animator.SetIKPositionWeight(hand, ikWeight);
                _animator.SetIKRotationWeight(hand, ikWeight);

                AvatarIKHint hint =
                    hand == AvatarIKGoal.LeftHand
                        ? AvatarIKHint.LeftElbow
                        : AvatarIKHint.RightElbow;
                _animator.SetIKHintPositionWeight(hint, ikWeight);
            }
        }
    }

    public void DrawDebugGizmos()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
        if (_animator == null)
            return;

        Gizmos.color = Color.cyan;
        if (_leftHandWeight > 0.05f)
            Gizmos.DrawWireSphere(_leftHandIkPos, gizmoSize);
        if (_rightHandWeight > 0.05f)
            Gizmos.DrawWireSphere(_rightHandIkPos, gizmoSize);

        // Desenhar os raios de debug dos ombros
        DrawShoulderGizmo(HumanBodyBones.LeftShoulder, -frontRayAngle);
        DrawShoulderGizmo(HumanBodyBones.LeftShoulder, -diagonalRayAngle);
        DrawShoulderGizmo(HumanBodyBones.LeftShoulder, -sideRayAngle);

        DrawShoulderGizmo(HumanBodyBones.RightShoulder, frontRayAngle);
        DrawShoulderGizmo(HumanBodyBones.RightShoulder, diagonalRayAngle);
        DrawShoulderGizmo(HumanBodyBones.RightShoulder, sideRayAngle);
    }

    private void DrawShoulderGizmo(HumanBodyBones shoulderBone, float hAngle)
    {
        Transform shoulder = _animator.GetBoneTransform(shoulderBone);
        Vector3 rayOrigin =
            shoulder != null
                ? shoulder.position
                : transform.position
                    + Vector3.up * 1.4f
                    + (
                        shoulderBone == HumanBodyBones.LeftShoulder
                            ? -transform.right * 0.3f
                            : transform.right * 0.3f
                    );

        foreach (float vAngle in verticalRayAngles)
        {
            Quaternion hRot = Quaternion.AngleAxis(hAngle, transform.up);
            Quaternion vRot = Quaternion.AngleAxis(-vAngle, transform.right); // Negativo agora vai para baixo
            Vector3 rayDirection = hRot * vRot * transform.forward;

            RaycastHit hit;
            if (
                CastRayIgnoringSelf(
                    rayOrigin,
                    rayDirection,
                    handReachDistance,
                    out hit
                )
            )
            {
                float weight =
                    (shoulderBone == HumanBodyBones.LeftShoulder)
                        ? _leftHandWeight
                        : _rightHandWeight;
                Gizmos.color = weight > 0.1f ? Color.green : Color.gray;

                Gizmos.DrawLine(rayOrigin, hit.point);
                Gizmos.DrawWireSphere(hit.point, 0.02f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(rayOrigin, rayDirection * handReachDistance);
            }
        }
    }
}
