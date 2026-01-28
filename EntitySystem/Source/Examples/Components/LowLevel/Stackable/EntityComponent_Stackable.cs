public class EntityComponent_Stackable : 
    EntityComponent,
    IReceiveConfig<EntityComponentConfig_Stackable>,
    IReceiveState<EntityComponentState_Stackable>
{
    private int maxQuantity = 1;
    private int quantity = 1;

    // IReceiveConfig
    public void ApplyConfig(EntityComponentConfig_Stackable cfg) => maxQuantity = cfg.maxQuantity;
    public System.Type GetConfigType() => typeof(EntityComponentConfig_Stackable);

    // IReceiveState
    public void SetState(EntityComponentState_Stackable state) => this.quantity = state.quantity;
    public EntityComponentState_Stackable GetState() => new EntityComponentState_Stackable();
    public System.Type GetStateType() => typeof(EntityComponentState_Stackable);

    // Component-specific
    public int GetQuantity() => quantity;
    public int GetMaxQuantity() => maxQuantity;
    public void Add(int amount) => quantity += amount;
    public void Remove(int amount) => quantity -= amount;
}

[System.Serializable]
public class EntityComponentConfig_Stackable
{
    public int maxQuantity = 1;
}

[System.Serializable]
public class EntityComponentState_Stackable
{
    public int quantity;
}