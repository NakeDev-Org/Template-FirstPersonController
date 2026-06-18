#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using nakatimat.TPS.Player.Modular; // Para achar o PlayerManager

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
            player.AddComponent<PlayerManager>();
            player.AddComponent<nakatimat.TPS.Player.Modular.PlayerCombatManager>(); // Puxa Equipment e Stamina
            player.AddComponent<nakatimat.CombatSystem.MeleeSystem.CharacterCombatAnimator>(); // Puxa Animator
            player.AddComponent<nakatimat.RangedFramework.CharacterRangedCombat>(); // Puxa AimController
            player.AddComponent<nakatimat.DamageSystem.CharacterHealthManager>(); // Vida
            player.AddComponent<nakatimat.RangedFramework.AimRigTargetController>(); // Rigging

            Selection.activeGameObject = player;
            Debug.Log(
                "🔦 Sobrevivente criado! Configure o peso e o consumo alto de Stamina."
            );
        }

        [MenuItem("NakeDev/Template/Player/3. Hack & Slash", false, 12)]
        public static void CreateHackAndSlashPlayer()
        {
            GameObject player = new GameObject("Player_HackAndSlash");
            player.AddComponent<PlayerManager>();
            player.AddComponent<nakatimat.TPS.Player.Modular.PlayerCombatManager>();
            player.AddComponent<nakatimat.CombatSystem.MeleeSystem.CharacterCombatAnimator>();
            player.AddComponent<nakatimat.RangedFramework.CharacterRangedCombat>();
            player.AddComponent<nakatimat.DamageSystem.CharacterHealthManager>();
            player.AddComponent<nakatimat.RangedFramework.AimRigTargetController>();

            Selection.activeGameObject = player;
            Debug.Log("⚔️ Caçador criado! Pronto para combos velozes.");
        }

        // ==========================================
        // 💾 CRIADORES DE DADOS (ATALHOS)
        // ==========================================

        [MenuItem("NakeDev/Data/Create New Weapon Data", false, 50)]
        public static void CreateWeaponDataShortcut()
        {
            // Este comando "engana" a Unity e aperta o botão direito > Create do seu ScriptableObject pra você!
            EditorApplication.ExecuteMenuItem(
                "Assets/Create/NakeCore/TPS/Combat/Weapons/Weapon Data"
            );
        }

        [MenuItem("NakeDev/Data/Create New Combo Step Data", false, 51)]
        public static void CreateComboStepDataShortcut()
        {
            EditorApplication.ExecuteMenuItem(
                "Assets/Create/NakeCore/TPS/Combat/Animations/Attack Animation Data"
            );
        }
    }
}
#endif
