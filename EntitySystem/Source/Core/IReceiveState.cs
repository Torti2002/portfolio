/// <summary>
/// Komponente kann einen Laufzeit-Zustand wiederherstellen
/// </summary>
/// <typeparam name="TState"></typeparam>
public interface IReceiveState<TState>
{
    void SetState(TState state);
    TState GetState();
    System.Type GetStateType();
}