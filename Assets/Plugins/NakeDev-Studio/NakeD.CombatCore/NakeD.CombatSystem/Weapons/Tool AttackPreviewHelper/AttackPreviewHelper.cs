#if UNITY_EDITOR
using nakatimat.ComboFramework.Data;
using UnityEditor.Animations;
using UnityEngine;

namespace nakatimat.AttackPreview
{
    public class AttackPreviewHelper : MonoBehaviour
    {
        [Header("Datas")]
        public ComboNode AttackData;
        public Animator _animator;

        [Header("Visual Reference")]
        public Transform slashPreviewTransform;

        [Header("Temporary Fixes")]
        public Vector3 hitBoxCenter;
        public Vector3 hitBoxSize;

        private void OnDrawGizmos()
        {
            if (AttackData == null)
            {
                return;
            }

            // Salva a matriz antiga e aplica a posição/rotação deste GameObject (o boneco da cena)
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            Gizmos.color = new Color(1, 0, 0, 0.8f);
            Gizmos.DrawWireCube(hitBoxCenter, hitBoxSize);

            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawCube(hitBoxCenter, hitBoxSize);

            Gizmos.matrix = oldMatrix;
        }

        public void ApplyDataToPreview()
        {
            slashPreviewTransform.localPosition = AttackData.vfxPosition;
            slashPreviewTransform.localRotation = Quaternion.Euler(AttackData.vfxRotation);
            slashPreviewTransform.localScale = Vector3.one * AttackData.vfxScale;

            hitBoxCenter = AttackData.hitBoxCenter;
            hitBoxSize = AttackData.hitBoxSize;

            AnimatorController controller =
                _animator.runtimeAnimatorController as AnimatorController;
            if (controller == null)
            {
                return;
            }

            AnimatorState state = controller.layers[0].stateMachine.states[0].state;
            state.motion = AttackData.previewClip;
        }

        public void SaveData()
        {
            if (AttackData == null || slashPreviewTransform == null)
            {
                return;
            }

            AttackData.vfxPosition = slashPreviewTransform.localPosition;
            AttackData.vfxRotation = slashPreviewTransform.localRotation.eulerAngles;
            AttackData.vfxScale = slashPreviewTransform.localScale.x;

            AttackData.hitBoxCenter = hitBoxCenter;
            AttackData.hitBoxSize = hitBoxSize;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(AttackData);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }
    }
}
#endif
