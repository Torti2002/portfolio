public class EntityComponent_Inventory :
    EntityComponent,
    IReceiveConfig<EntityComponentConfig_Inventory>,
    IReceiveState<EntityComponentState_Inventory>
{
    private Inventory inventory;

    // IReceiveConfig
    public void ApplyConfig(EntityComponentConfig_Inventory cfg)
    {
        if (cfg != null)
        {
            inventory = cfg.inventory;
        }
    }

    public System.Type GetConfigType() => typeof(EntityComponentConfig_Inventory);

    public Inventory GetInventory()
    {
        if (inventory == null)
        {
            inventory = new Inventory();
        }
        return inventory;
    }

    // IReceiveState
    public EntityComponentState_Inventory GetState()
    {
        return new EntityComponentState_Inventory { inventory = GetInventory() };
    }

    public void SetState(EntityComponentState_Inventory s)
    {
        if (s != null)
        {
            inventory = s.inventory;
        }
    }

    public System.Type GetStateType() => typeof(EntityComponentState_Inventory);
}

[System.Serializable]
public class EntityComponentConfig_Inventory
{
    public Inventory inventory;
}

[System.Serializable]
public class EntityComponentState_Inventory
{
    public Inventory inventory;
}