public namespace EntitySystem.Core.Components
{
    /// <summary>
    /// Abstract base-class for all EntityComponents (for Creatures, Items, etc.)
    /// </summary>
    public abstract class EntityComponent
    {
        public EntityGhost entityGhost { get; private set; }



        internal void AttachEntityGhost(EntityGhost _entityGhost)
        {
            entityGhost = _entityGhost;
            // OnAttach nicht hier!
        }
        internal void NotifyWorldAttached()
        {
            // OnAttach jetzt hier, erst wenn MonoBehaviour erzeugt
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
}