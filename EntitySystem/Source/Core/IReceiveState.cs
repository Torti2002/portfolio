public namespace EntitySystem.Core
{
    // Komponente kann einen Laufzeit-Zustand wiederherstellen
    public interface IReceiveState<TState>
    {
        void SetState(TState state);
        TState GetState();
        System.Type GetStateType();
    }
}