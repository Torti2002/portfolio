using System;

public static class EntityFactory
{
    public static EntityGhost CreateGhostFromAttributes(EntityAttributes attrs)
    {
        var ghost = new EntityGhost
        {
            entityInstanceId = Guid.NewGuid().ToString("N"),
            entityTypeId = attrs.entityId
        };

        if (attrs.components != null)
        {
            foreach (var cfg in attrs.components) 
            {
                cfg?.AddTo(ghost);
                // Debug.Log($"[EntityFactory]: Added component [{cfg.GetType().Name}] with Config [{cfg}]");                
            }

        }

        ghost.InitAllComponents();
        return ghost;
    }
}    