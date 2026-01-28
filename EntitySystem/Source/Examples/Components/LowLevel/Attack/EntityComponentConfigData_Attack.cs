[System.Serializable]
public sealed class EntityComponentConfigData_Attack : EntityComponentConfigData
{
    [UnityEngine.SerializeField] private float damage = 1f;
    [UnityEngine.SerializeField] private float attackSpeed = 1f;

    public override void AddTo(EntityGhost ghost)
    {
        var c = ghost.AddEntityComponent<EntityComponent_Attack>();
        c.ApplyConfig(new EntityComponentConfig_Attack { damage = damage, attackSpeed = attackSpeed});
    }

    public override string DisplayName()
    {
        return "Attack";
    }
}