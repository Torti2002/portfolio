public class EntityComponent_Attack : 
    EntityComponent, 
    IReceiveConfig<EntityComponentConfig_Attack>,
    IReceiveState<EntityComponentState_Attack>,
    IReceiveTick
{
    private float damage;
    private float attackSpeed;
    private float cooldown;

    // IReceiveConfig
    public void ApplyConfig(EntityComponentConfig_Attack cfg)
    {
        if (cfg != null)
        {
            damage = cfg.damage;
            attackSpeed = cfg.attackSpeed;
        }
    }
    public System.Type GetConfigType() => typeof(EntityComponentConfig_Attack);
    
    // IReceiveState
    public void SetState(EntityComponentState_Attack state) => this.cooldown = state.cooldown;
    public EntityComponentState_Attack GetState() => new EntityComponentState_Attack{cooldown = cooldown};
    public System.Type GetStateType() => typeof(EntityComponentState_Attack);

    // IReceiveTick
    public void Tick(float dt)
    {
        
    }

    // Component-specific
    public void Attack()
    {
        // Get target here
        Entity target = null;

        if (target != null && target.entityGhost != null && target.entityGhost.TryGet(out EntityComponent_Mortal mortal))
            mortal.Hurt(damage);
    }
}

[System.Serializable]
public class EntityComponentConfig_Attack
{
    public float damage;
    public float attackSpeed;
}

[System.Serializable]
public class EntityComponentState_Attack
{
    public float cooldown;
}
