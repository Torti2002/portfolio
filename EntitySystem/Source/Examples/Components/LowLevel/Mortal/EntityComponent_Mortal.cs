public class EntityComponent_Mortal : 
    EntityComponent, 
    IReceiveConfig<EntityComponentConfig_Mortal>, 
    IReceiveState<EntityComponentState_Mortal>,
    IReceiveTick
{
    // Values for this entity
    private float maxRegen = 1f;
    private float maxHealth = 1f;

    // Current Values
    private float health = 1f;
    private float regen = 1f;

    private event System.Action<float> OnDamaged; // amount


    // IReceiveConfig
    public void ApplyConfig(EntityComponentConfig_Mortal cfg)
    {
        if (cfg != null)
        {
            maxHealth = cfg.maxHealth;
            maxRegen = cfg.maxRegen;
        }
    }
    public System.Type GetConfigType() => typeof(EntityComponentConfig_Mortal);

    // IReceiveState
    public void SetState(EntityComponentState_Mortal state) => health = state.health;
    public EntityComponentState_Mortal GetState() => new EntityComponentState_Mortal { health = this.health };
    public System.Type GetStateType() => typeof(EntityComponentState_Mortal);

    // IReceiveTick
    public void Tick(float dt)
    {
        if (health > 0f && health < maxHealth) 
            health += dt * regen;
    }

    // Component-specific
    public void Die()
    {
        if (health <= 0f) health = 0f;
        UnityEngine.Debug.Log("[Mortal] " + entityGhost.entity.gameObject.name + " died.");

        UnityEngine.GameObject.Destroy(entityGhost.entity.gameObject);
    }

    public void Hurt(float damage)
    { 
        if (health <= 0){return;}
        OnDamaged?.Invoke(damage);
        health -= damage; 
        if (health <= 0)
        {
            Die();             
        }
        else
        {

        }
    }

    public bool IsDead()
    {
        return health <= 0f;
    }

    public float GetRegen() => regen;
    public float GetMaxHealth() => maxHealth;
}

[System.Serializable]
public class EntityComponentConfig_Mortal
{
    public float maxHealth = 100f;
    public float maxRegen = 1f;
}

[System.Serializable]
public class EntityComponentState_Mortal
{
    public float health = 1f;
}