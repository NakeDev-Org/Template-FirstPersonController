using UnityEngine;
#if UNITY_6000_0_OR_NEWER
using Unity.Cinemachine;
#else
using Cinemachine;
#endif

namespace nakatimat.TPS.Player
{
    public class TargetingSystem : MonoBehaviour
    {
        [Header("Targeting Settings")]
        public float targetingRadius = 15f;

        [Tooltip(
            "Quantas vezes o Targeting Radius o inimigo precisa se afastar para quebrar a mira? 1.1 = 10% mais longe."
        )]
        public float loseTargetRadiusMultiplier = 1.1f;
        public LayerMask enemyLayer;
        public LayerMask obstacleLayer; // Camada de paredes e chão para quebrar a visão

        [Header("Target Switching")]
        public float switchThreshold = 0.5f;
        public float switchCooldown = 0.3f;
        private float _lastSwitchTime;

        [Header("Runtime")]
        [SerializeField]
        private Transform currentTarget;

        [Header("Cinemachine Support")]
        [Tooltip(
            "Arraste o GameObject da sua Câmera de Lock-On (Cinemachine) aqui."
        )]
        public GameObject lockOnVirtualCamera;

        [Header("Adaptive Camera")]
        public float minCombatFOV = 40f;
        public float maxCombatFOV = 65f;

        [Header("Debug/Visuals")]
        [SerializeField]
        private GameObject targetIndicatorPrefab; // Opcional
        private GameObject _currentIndicator;

        // Propriedade estática para fácil acesso pela Câmera
        public static Transform CurrentTarget { get; private set; }

        private void Start()
        {
            //if (InputManager.Input != null)
            //{
            //    InputManager.Input.onTargetToggle += ToggleTarget;
            //}
        }

        private void OnDestroy()
        {
            //if (InputManager.Input != null)
            //{
            //    InputManager.Input.onTargetToggle -= ToggleTarget;
            //}
        }

        private void Update()
        {
            if (currentTarget != null)
            {
                // Verifica se está vivo
                if (!currentTarget.gameObject.activeInHierarchy)
                {
                    ClearTarget();
                    return;
                }

                // Quebra por distância (agora com controle no Inspector)
                float dist = Vector3.Distance(
                    transform.position,
                    currentTarget.position
                );
                if (dist > targetingRadius * loseTargetRadiusMultiplier)
                {
                    ClearTarget();
                    return;
                }

                // Quebra por parede (Line of Sight)
                Vector3 dirToTarget =
                    (currentTarget.position + Vector3.up * 1.5f)
                    - (transform.position + Vector3.up * 1.5f);
                if (
                    Physics.Raycast(
                        transform.position + Vector3.up * 1.5f,
                        dirToTarget.normalized,
                        out RaycastHit hit,
                        dist,
                        obstacleLayer
                    )
                )
                {
                    // Tem uma parede bloqueando a visão
                    ClearTarget();
                    return;
                }

                if (_currentIndicator != null)
                {
                    // Mantém o indicador visual acima da cabeça do inimigo
                    _currentIndicator.transform.position =
                        currentTarget.position + Vector3.up * 2.2f;
                }

                if (lockOnVirtualCamera != null)
                {
#if UNITY_6000_0_OR_NEWER
                    var cam =
                        lockOnVirtualCamera.GetComponent<CinemachineCamera>();
#else
                    var cam =
                        lockOnVirtualCamera.GetComponent<CinemachineVirtualCamera>();
#endif
                    if (cam != null)
                    {
                        // FOV aumenta conforme a distância aumenta (até o limite de quebra)
                        float t =
                            dist
                            / (targetingRadius * loseTargetRadiusMultiplier);
                        float targetFOV = Mathf.Lerp(
                            minCombatFOV,
                            maxCombatFOV,
                            t
                        );
                        cam.Lens.FieldOfView = Mathf.Lerp(
                            cam.Lens.FieldOfView,
                            targetFOV,
                            Time.deltaTime * 3f
                        );
                    }
                }

                HandleTargetSwitching();
            }
        }

        private void HandleTargetSwitching()
        {
            //if (InputManager.Input == null)
            //    return;

            // O mouse ou o analógico direito controlam a troca
            float lookX = 0f; // InputManager.Input.GetLookAxis().x;

            if (
                Mathf.Abs(lookX) > switchThreshold
                && Time.time >= _lastSwitchTime + switchCooldown
            )
            {
                _lastSwitchTime = Time.time;
                SwitchTarget(Mathf.Sign(lookX));
            }
        }

        private void SwitchTarget(float direction)
        {
            // direction > 0 = direita, direction < 0 = esquerda
            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                targetingRadius,
                enemyLayer
            );

            Transform bestTarget = null;
            float smallestAngle = float.MaxValue;

            Transform camTransform =
                Camera.main != null ? Camera.main.transform : transform;

            foreach (var hit in hits)
            {
                if (
                    !hit.gameObject.activeInHierarchy
                    || hit.transform == currentTarget
                )
                    continue;

                Vector3 dirToTarget = (
                    hit.transform.position - transform.position
                ).normalized;

                // Evita mirar através das paredes
                float dist = Vector3.Distance(
                    transform.position,
                    hit.transform.position
                );
                if (
                    Physics.Raycast(
                        transform.position + Vector3.up * 1.5f,
                        dirToTarget,
                        dist,
                        obstacleLayer
                    )
                )
                {
                    continue;
                }

                Vector3 localDir = camTransform.InverseTransformDirection(
                    dirToTarget
                );

                // Verifica se o inimigo está do lado que o jogador empurrou o controle
                if (
                    (direction > 0 && localDir.x > 0.1f)
                    || (direction < 0 && localDir.x < -0.1f)
                )
                {
                    float angle = Vector3.Angle(
                        camTransform.forward,
                        dirToTarget
                    );
                    if (angle < smallestAngle)
                    {
                        smallestAngle = angle;
                        bestTarget = hit.transform;
                    }
                }
            }

            if (bestTarget != null)
            {
                currentTarget = bestTarget;
                CurrentTarget = bestTarget;
            }
        }

        public void SetTargeting(bool active, bool isAutoBlockLock = false)
        {
            if (active)
            {
                if (currentTarget == null)
                    FindNearestTarget(isAutoBlockLock);
            }
            else
            {
                ClearTarget();
            }
        }

        private void ToggleTarget()
        {
            if (currentTarget != null)
            {
                ClearTarget();
            }
            else
            {
                FindNearestTarget(false);
            }
        }

        private void FindNearestTarget(bool strictMeleeLock = false)
        {
            // Se for um Auto-Lock de defesa, restringimos a distância e o ângulo para não puxar inimigos longe
            float currentMaxRadius = strictMeleeLock
                ? targetingRadius * 0.4f
                : targetingRadius;
            float currentMaxAngle = strictMeleeLock ? 45f : 90f;

            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                currentMaxRadius,
                enemyLayer
            );

            float bestScore = float.MaxValue;
            Transform bestTarget = null;

            Transform camTransform =
                Camera.main != null ? Camera.main.transform : transform;

            foreach (var hit in hits)
            {
                if (hit.gameObject.activeInHierarchy)
                {
                    float dist = Vector3.Distance(
                        transform.position,
                        hit.transform.position
                    );

                    Vector3 dirToTarget = (
                        hit.transform.position - transform.position
                    ).normalized;

                    // 1. Line of Sight Check (Não travar através da parede)
                    if (
                        Physics.Raycast(
                            transform.position + Vector3.up * 1.5f,
                            dirToTarget,
                            dist,
                            obstacleLayer
                        )
                    )
                    {
                        continue;
                    }

                    // 2. Cálculo do Peso (Menor Score é melhor)
                    // Distância real + Ângulo que o inimigo está em relação à câmera
                    Vector3 camDirToTarget = (
                        hit.transform.position - camTransform.position
                    ).normalized;
                    float angle = Vector3.Angle(
                        camTransform.forward,
                        camDirToTarget
                    );

                    // Ignora inimigos que estão totalmente nas suas costas/câmera (ângulo muito grande)
                    if (angle > currentMaxAngle)
                        continue;

                    // O Score mistura a distância (metros) com o ângulo.
                    // Multiplicamos o ângulo por um peso para que quem está no meio da tela seja mais priorizado
                    float score = dist + (angle * 0.5f);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestTarget = hit.transform;
                    }
                }
            }

            if (bestTarget != null)
            {
                currentTarget = bestTarget;
                CurrentTarget = bestTarget;
                CreateIndicator();

                if (lockOnVirtualCamera != null)
                {
                    lockOnVirtualCamera.SetActive(true);
#if UNITY_6000_0_OR_NEWER
                    var cam =
                        lockOnVirtualCamera.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                    if (cam != null)
                    {
                        cam.LookAt = currentTarget;
                        cam.Follow = currentTarget; // Faz a câmera ancorar no inimigo para orbitá-lo!
                    }
#else
                    var cam =
                        lockOnVirtualCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>();
                    if (cam != null)
                    {
                        cam.LookAt = currentTarget;
                        cam.Follow = currentTarget; // Faz a câmera ancorar no inimigo para orbitá-lo!
                    }
#endif
                }
            }
        }

        private void ClearTarget()
        {
            currentTarget = null;
            CurrentTarget = null;
            if (_currentIndicator != null)
            {
                Destroy(_currentIndicator);
            }

            if (lockOnVirtualCamera != null)
            {
                lockOnVirtualCamera.SetActive(false);
            }
        }

        public Transform GetCurrentTarget()
        {
            return currentTarget;
        }

        private void CreateIndicator()
        {
            if (_currentIndicator != null)
                Destroy(_currentIndicator);

            if (targetIndicatorPrefab != null)
            {
                _currentIndicator = Instantiate(targetIndicatorPrefab);
            }
            else
            {
                // Fallback: Cria uma esferazinha vermelha temporária
                _currentIndicator = GameObject.CreatePrimitive(
                    PrimitiveType.Sphere
                );
                _currentIndicator.transform.localScale = Vector3.one * 0.3f;
                Destroy(_currentIndicator.GetComponent<Collider>()); // Remove o colisor para não interferir nos ataques

                var renderer = _currentIndicator.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                    // Torna o material Unlit para brilhar
                    renderer.material.shader = Shader.Find("Unlit/Color");
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, targetingRadius);
        }
    }
}
