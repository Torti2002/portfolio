[System.Serializable]
public abstract class EntityComponentConfigData
{
    public virtual string DisplayName() => GetType().Name;
    public abstract void AddTo(EntityGhost ghost);
}    