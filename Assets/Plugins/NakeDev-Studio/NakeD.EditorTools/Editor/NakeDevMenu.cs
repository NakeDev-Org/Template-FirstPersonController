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

        [MenuItem("NakeDev/Template/Player/1. Walking Simulator", false, 10)]
        public static void CreateWalkingSimPlayer()
        {
            GameObject player = new GameObject("Player_WalkingSimulator");
            player.AddComponent<PlayerManager>();

            Selection.activeGameObject = player;
            Debug.Log("🧍 Caminhante criado! Configure as câmeras lentas.");
        }

        [MenuItem("NakeDev/Template/Player/2. Survival Horror", false, 11)]
        public static void CreateSurvivalHorrorPlayer()
        {
            GameObject player = new GameObject("Player_SurvivalHorror");
            
            // Unity Core
            player.AddComponent<Animator>();
            player.AddComponent<CharacterController>();

            // NakeDev Core Locomotion (PlayerManager's [RequireComponent] will automatically add:
            // InputReader, PlayerLocomotion, PlayerAnimationUpdater, and TPSCameraAimController)
            player.AddComponent<PlayerManager>();
            
            // Aiming / Interacting
            player.AddComponent<TargetingSystem>();

            Selection.activeGameObject = player;
            Debug.Log("🔦 Sobrevivente criado! Locomoção, inputs e animações básicas adicionadas.");
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
