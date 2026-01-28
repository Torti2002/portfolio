[System.Serializable]
public sealed class EntityComponentConfigData_Mortal : EntityComponentConfigData
{
    [UnityEngine.SerializeField] private float health = 100f;
    [UnityEngine.SerializeField] private float regen = 1f;

    public override void AddTo(EntityGhost ghost)
    {
        var c = ghost.AddEntityComponent<EntityComponent_Mortal>();
        c.ApplyConfig(new EntityComponentConfig_Mortal { health = health, regen = regen });
    }

    public override string DisplayName()
    {
        return "Mortal";
    }
}