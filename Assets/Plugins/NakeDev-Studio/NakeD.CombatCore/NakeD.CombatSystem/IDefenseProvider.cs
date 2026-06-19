using UnityEngine;

namespace nakatimat.DamageSystem
{
    public interface IDefenseProvider
    {
        /// <summary>
        /// Gets the current defense multiplier. 1f means no defense.
        /// Also outputs whether a successful parry occurred, so the DamageController can apply effects like stagger.
        /// </summary>
        float GetDefenseMultiplier(out bool parrySuccess);

        /// <summary>
        /// Optional: Callback to trigger any visual/logical feedback when a parry succeeds.
        /// </summary>
        void OnParrySuccess(GameObject attacker);
    }
}
