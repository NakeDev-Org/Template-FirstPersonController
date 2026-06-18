using nakatimat.CombatSystem.MeleeSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nakatimat.Player
{
    public class DebugWeaponSwitcher : MonoBehaviour
    {
        [Header("Ref")]
        [SerializeField]
        private CharacterEquipmentManager _weaponHandler;

        [Header("Testing Weapons")]
        [SerializeField]
        private TPSMeleeWeaponStats[] _testWeapons;

        private void Awake()
        {
            if (_weaponHandler == null)
            {
                _weaponHandler = GetComponent<CharacterEquipmentManager>();
            }
        }

        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (
                Keyboard.current.tKey.wasPressedThisFrame
                && _testWeapons != null
                && _testWeapons.Length > 0
            )
            {
                _weaponHandler.WeaponEquip(_testWeapons[0]);
            }

            if (
                Keyboard.current.pKey.wasPressedThisFrame
                && _testWeapons != null
                && _testWeapons.Length > 1
            )
            {
                _weaponHandler.WeaponEquip(_testWeapons[1]);
            }
        }
    }
}
