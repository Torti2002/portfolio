using UnityEngine;



public namespace EntitySystem.Core
{
    public abstract class Entity : MonoBehaviour
    {
        public EntityGhost entityGhost;
        public void AttachGhostEntity(EntityGhost _entityGhost)
        {
            if (_entityGhost != null)
            {
                entityGhost = _entityGhost;            
                if (entityGhost.entity != this)
                    entityGhost.AttachWorldEntity(this);        
            }
            else
            {
                Debug.LogError($"[Entity '{name}']: EntityGhost is null!");
            }
        }
    }    
}
