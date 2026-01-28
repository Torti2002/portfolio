[System.Serializable]
public sealed class EntityComponentConfigData_Inventory : EntityComponentConfigData
{
    public Inventory inventory;

    public override void AddTo(EntityGhost ghost)
    {
        var c = ghost.AddEntityComponent<EntityComponent_Inventory>();
        c.ApplyConfig(new EntityComponentConfig_Inventory { inventory = inventory });
    }

    public override string DisplayName()
    {
        return "Inventory";
    }
}    