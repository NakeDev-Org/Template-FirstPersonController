#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using nakatimat.TPS.Player.Modular;
using nakatimat.TPS.Player;

namespace nakatimat.Core.Editor
{
    public static class NakeDevMenu
    {
        // ==========================================
        // 🧍‍♂️ CRIADORES DE PLAYER (TEMPLATES)
        // ==========================================



        [MenuItem("NakeDev/Template/Spawn Player (Dummy)", false, 11)]
        public static void CreateSurvivalHorrorPlayer()
        {
            string prefabPath = "Assets/Plugins/NakeDev-Studio/NakeD.QuickSetup/PlayerPrefab/Player_Dummy_Survival.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab != null)
            {
                GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Selection.activeGameObject = player;
                Debug.Log("🔦 Sobrevivente (Dummy) instanciado na cena! Configure a malha 3D e o Avatar.");
            }
            else
            {
                Debug.LogError($"[NakeDev] Prefab Dummy não encontrado no caminho:\n{prefabPath}\nVerifique se o prefab foi criado corretamente!");
            }
        }

        // ==========================================
        // 💾 CRIADORES DE DADOS (ATALHOS)
        // ==========================================

        [MenuItem("NakeDev/Data/Create New Weapon Data", false, 50)]
        public static void CreateWeaponDataShortcut()
        {
            EditorApplication.ExecuteMenuItem(
                "Assets/Create/NakeCore/TPS/Combat/Weapons/Weapon Data"
            );
        }
    }
}
#endif
