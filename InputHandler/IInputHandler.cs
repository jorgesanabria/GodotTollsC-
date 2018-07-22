namespace MaquinaDeEstados.InputHandler
{
    public interface IInputHandler<TEnum>
    {
        bool IsActionPressed(TEnum action);
        bool IsActionJustPressed(TEnum action);
        bool IsActionReleased(TEnum action);
    }
}
