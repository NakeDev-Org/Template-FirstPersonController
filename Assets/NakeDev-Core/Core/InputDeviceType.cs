namespace nakatimat.Core
{
    /// <summary>
    /// Classificação genérica do dispositivo de input atual, usada para escolher
    /// prompts/ícones de UI (ex: botão de interação) sem acoplar sistemas de UI ao Input System.
    /// </summary>
    public enum InputDeviceType
    {
        Keyboard,
        Xbox,
        PlayStation,
        Nintendo,
        GenericGamepad,
    }
}
