using nakatimat.Core.Inspector;
using nakatimat.DamageSystem;
using nakatimat.TPS.Player;
using UnityEngine;

namespace nakatimat.Core.DebugTools
{
    public class GlobalDebugger : MonoBehaviour
    {
        public static GlobalDebugger Instance { get; private set; }

        [Separator("PLAYER TARGET", 0, 255, 255)]
        [Tooltip(
            "Arraste a Cápsula do Player aqui para que o GlobalDebugger possa desenhar os Gizmos dele e centralizar tudo."
        )]
        [SerializeField]
        private Transform targetPlayer;

        [Separator("DEBUG TOGGLES", 255, 0, 0)]
        [SerializeField]
        private bool enableAllLogs = true;

        [Space(10)]
        [SerializeField]
        private bool logStamina = true;

        [SerializeField]
        private bool logCombat = true;

        [SerializeField]
        private bool logSystem = true;

        [Separator("GIZMOS TOGGLES", 0, 255, 0)]
        [SerializeField]
        private bool drawPlayerPhysics = true;

        [SerializeField]
        private bool drawHandIK = true;

        [SerializeField]
        private bool drawFootIK = true;

        [SerializeField]
        private bool drawCombatHitboxes = true;

#if UNITY_EDITOR
        public struct DebugHitbox
        {
            public Vector3 center;
            public Vector3 halfExtents;
            public Quaternion rotation;
            public float expireTime;
        }

        private System.Collections.Generic.List<DebugHitbox> _debugHitboxes =
            new System.Collections.Generic.List<DebugHitbox>();

        public void RegisterHitbox(
            Vector3 center,
            Vector3 halfExtents,
            Quaternion rotation,
            float duration
        )
        {
            if (!drawCombatHitboxes)
                return;
            _debugHitboxes.Add(
                new DebugHitbox
                {
                    center = center,
                    halfExtents = halfExtents,
                    rotation = rotation,
                    expireTime = Time.time + duration,
                }
            );
        }
#endif

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            CharacterHealthManager.OnAnyDamageTaken += HandleDamageEvent;
        }

        private void OnDisable()
        {
            CharacterHealthManager.OnAnyDamageTaken -= HandleDamageEvent;
        }

        private void HandleDamageEvent(
            GameObject victim,
            float originalDamage,
            float finalDamage
        )
        {
            if (!enableAllLogs || !logCombat)
                return;

            bool isPlayer = victim.CompareTag("Player");
            string targetName = isPlayer
                ? "JOGADOR"
                : $"INIMIGO ({victim.name})";

            if (finalDamage == 0 && originalDamage > 0)
            {
                Debug.Log(
                    $"<color=yellow><b>[{targetName} PARRY/DEFESA PERFEITA]</b></color> Anulou completamente um ataque de {originalDamage} de dano!"
                );
            }
            else if (finalDamage < originalDamage)
            {
                Debug.Log(
                    $"<color=cyan><b>[{targetName} DEFENDEU]</b></color> Sofreu um golpe de {originalDamage}, defendeu, e tomou apenas <color=red>{finalDamage}</color> de dano!"
                );
            }
            else if (finalDamage > originalDamage)
            {
                string color = isPlayer ? "red" : "green";
                Debug.Log(
                    $"<color={color}><b>[{targetName} DANO CRÍTICO]</b></color> Tomou <color=red>{finalDamage}</color> de dano (Ataque base era {originalDamage})!"
                );
            }
            else
            {
                string color = isPlayer ? "red" : "green";
                Debug.Log(
                    $"<color={color}><b>[{targetName} TOMOU DANO]</b></color> Recebeu <color=red>{finalDamage}</color> de dano direto!"
                );
            }
        }

        public void LogStamina(string message)
        {
            if (!enableAllLogs || !logStamina)
                return;
            Debug.Log($"<color=cyan>[STAMINA]</color> {message}");
        }

        public void LogCombat(string message)
        {
            if (!enableAllLogs || !logCombat)
                return;
            Debug.Log($"<color=red>[COMBAT]</color> {message}");
        }

        public void LogSystem(string message)
        {
            if (!enableAllLogs || !logSystem)
                return;
            Debug.Log($"<color=yellow>[SYSTEM]</color> {message}");
        }

        private void OnDrawGizmos()
        {
            if (targetPlayer == null)
                return;

            // Player Physics
            if (drawPlayerPhysics)
            {
                CharacterController cc =
                    targetPlayer.GetComponent<CharacterController>();
                if (cc != null)
                {
                    Gizmos.color = Color.cyan;
                    Vector3 center = targetPlayer.position + cc.center;
                    float height = cc.height;
                    float radius = cc.radius;

                    Vector3 top = center + Vector3.up * (height / 2f - radius);
                    Vector3 bottom =
                        center - Vector3.up * (height / 2f - radius);

                    Gizmos.DrawWireSphere(top, radius);
                    Gizmos.DrawWireSphere(bottom, radius);
                }
            }

            // Hand IK
            if (drawHandIK)
            {
                var handIK = targetPlayer.GetComponent<HandIK>();
                if (handIK != null)
                {
                    handIK.DrawDebugGizmos();
                }
            }

            // Foot IK
            if (drawFootIK)
            {
                var footIK = targetPlayer.GetComponent<ProceduralFootIK>();
                if (footIK != null)
                {
                    footIK.DrawDebugGizmos();
                }
            }

            // Hitboxes
#if UNITY_EDITOR
            if (drawCombatHitboxes && Application.isPlaying)
            {
                for (int i = _debugHitboxes.Count - 1; i >= 0; i--)
                {
                    if (Time.time > _debugHitboxes[i].expireTime)
                    {
                        _debugHitboxes.RemoveAt(i);
                        continue;
                    }

                    Matrix4x4 oldMatrix = Gizmos.matrix;
                    Gizmos.matrix = Matrix4x4.TRS(
                        _debugHitboxes[i].center,
                        _debugHitboxes[i].rotation,
                        _debugHitboxes[i].halfExtents * 2f
                    );

                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

                    Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
                    Gizmos.DrawCube(Vector3.zero, Vector3.one);

                    Gizmos.matrix = oldMatrix;
                }
            }
#endif
        }
    }
}
