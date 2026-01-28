/// <summary>
/// Komponente kann einen mit Zustand initlialisiert werden
/// </summary>
/// <typeparam name="TConfig"></typeparam>
public interface IReceiveConfig<TConfig>
{
    void ApplyConfig(TConfig config);
    System.Type GetConfigType();
}