using UnityEngine;



public namespace EntitySystem.Examples.Components.LowLevel
{
    public class EntityComponent_Inventory :
    EntityComponent,
    IReceiveConfig<EntityConfig_Inventory>,
    IReceiveState<EntityState_Inventory>
    {
        private Inventory inventory;

        public void ApplyConfig(EntityConfig_Inventory cfg)
        {
            if (cfg != null)
            {
                inventory = cfg.inventory;
            }
        }

        public System.Type GetConfigType() => typeof(EntityConfig_Inventory);

        public Inventory GetInventory()
        {
            if (inventory == null)
            {
                inventory = new Inventory();
            }
            return inventory;
        }

        // IProvidesState
        public EntityState_Inventory GetState()
        {
            return new EntityState_Inventory { inventory = GetInventory() };
        }

        public void SetState(EntityState_Inventory s)
        {
            if (s != null)
            {
                inventory = s.inventory;
            }
        }

        public System.Type GetStateType() => typeof(EntityState_Inventory);
    }

    [System.Serializable]
    public class EntityConfig_Inventory
    {
        public Inventory inventory;
    }

    [System.Serializable]
    public class EntityState_Inventory
    {
        public Inventory inventory;
    }
}