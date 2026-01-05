public namespace EntitySystem.Core
{
    // Komponente kann einen mit Zustand initlialisiert werden
    public interface IReceiveConfig<TConfig>
    {
        void ApplyConfig(TConfig config);
        System.Type GetConfigType();
    }
}