using UnityEngine;
/// <summary>
/// Abstract class to represent entities (and inheriting classes) in a 3D-World
/// </summary>
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