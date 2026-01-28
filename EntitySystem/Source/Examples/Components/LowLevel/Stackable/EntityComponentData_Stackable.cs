[System.Serializable]
public sealed class EntityComponentConfigData_Stackable : EntityComponentConfigData
{
    public int maxQuantity = 1;

    public override void AddTo(EntityGhost ghost)
    {
        var c = ghost.AddEntityComponent<EntityComponent_Stackable>();
        c.ApplyConfig(new EntityComponentConfig_Stackable
        {
            maxQuantity = maxQuantity
        });
    }

    public override string DisplayName()
    {
        return "Stackable";
    }
}    