/// <summary>
/// Abstract base-class for all EntityComponents
/// </summary>
public abstract class EntityComponent
{
    public EntityGhost entityGhost { get; private set; }



    internal void AttachEntityGhost(EntityGhost _entityGhost)
    {
        entityGhost = _entityGhost;
    }
    internal void NotifyWorldAttached()
    {
        OnAttach();
    }
    internal void Detach()
    {
        OnDetach();
        entityGhost = null;
    }

    // Lifecycle-Hooks
    public virtual void OnInit() {}
    public virtual void OnDeath() { }    
    public virtual void OnAttach() {}
    public virtual void OnDetach() {}
}