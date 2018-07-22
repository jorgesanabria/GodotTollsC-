namespace MaquinaDeEstados.InputHandler
{
    public interface IAIInputHandler<TEnum>
    {
        void SetActionPressed(TEnum action);
        void SetActionReleased(TEnum action);
    }
}
