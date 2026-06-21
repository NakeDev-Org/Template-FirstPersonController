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
            string[] guids = AssetDatabase.FindAssets("Player_Dummy_Survival t:Prefab");
            if (guids.Length > 0)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                
                if (prefab != null)
                {
                    GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    Selection.activeGameObject = player;
                    Debug.Log("🔦 Sobrevivente (Dummy) instanciado na cena! Configure a malha 3D e o Avatar.");
                    return;
                }
            }
            
            Debug.LogError("[NakeDev] Prefab Dummy ('Player_Dummy_Survival') não encontrado no projeto. Verifique se a Framework foi importada corretamente!");
        }

    }
}
#endif
