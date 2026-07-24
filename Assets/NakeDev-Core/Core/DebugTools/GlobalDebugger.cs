using nakatimat.Core.Inspector;
using UnityEngine;

namespace nakatimat.Core.DebugTools
{
    public class GlobalDebugger : MonoBehaviour
    {
        public static GlobalDebugger Instance { get; private set; }

        [InspectorLine("PLAYER TARGET", 0, 255, 255)]
        [Tooltip(
            "Arraste a Cápsula do Player aqui para que o GlobalDebugger possa desenhar os Gizmos dele e centralizar tudo."
        )]
        [SerializeField]
        private Transform targetPlayer;

        [InspectorLine("DEBUG TOGGLES", 255, 0, 0)]
        [SerializeField]
        private bool enableAllLogs = true;

        [Space(10)]
        [SerializeField]
        private bool logSystem = true;

        [InspectorLine("GIZMOS TOGGLES", 0, 255, 0)]
        [SerializeField]
        private bool drawPlayerPhysics = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
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
        }
    }
}

