using UnityEngine;
using System;



public namespace EntitySystem.Examples.Components.LowLevel
{
    public class EntityComponent_Attack : EntityComponent, IReceiveConfig<EntityConfig_Init_Attack>
    {
        private float damage;
        private float attackSpeed;

        public void ApplyConfig(EntityConfig_Init_Attack cfg)
        {
            if (cfg != null)
            {
                damage = cfg.damage;
                attackSpeed = cfg.attackSpeed;
            }
        }

        public void Attack()
        {
            // Get target here
            Entity target = null;

            if (target != null && target.entityGhost != null && target.entityGhost.TryGet(out EntityComponent_Mortal mortal))
                mortal.Hurt(damage);
        }

        public System.Type GetConfigType() => typeof(EntityConfig_Init_Attack);
        public System.Type GetStateType() => typeof(EntityConfig_State_Attack);
    }

    [System.Serializable]
    public class EntityConfig_Init_Attack
    {
        public float damage;
        public float attackSpeed;
    }

    [Serializable]
    public class EntityConfig_State_Attack
    {
        public DateTime lastAttack;
    }
}