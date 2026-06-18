namespace nakatimat.Core.Interfaces
{
    public interface ICombatAddon
    {
        bool IsMeleeStance { get; }
        bool IsBlocking { get; }
        bool HasEnoughStamina(float cost);
        bool TryConsumeStamina(float cost);
    }
}
