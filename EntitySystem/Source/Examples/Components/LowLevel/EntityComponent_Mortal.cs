using UnityEngine;
using System;



public namespace EntitySystem.Examples.Components.LowLevel
{
    public class EntityComponent_Mortal : 
    EntityComponent, 
    IReceiveConfig<EntityConfig_Mortal>, 
    IReceiveState<EntityState_Mortal>,
    IReceiveTick
    {
        // Values for this entity
        public float regen = 1f;
        public float health = 1f;

        // Current Values
        public float currentHealth = 1f;
        public float currentRegen = 1f;

        public event Action<float> OnDamaged; // amount



        public void Tick(float dt)
        {
            if (currentHealth > 0f && currentHealth < health) 
                currentHealth += dt * regen;
        }

        public void ApplyConfig(EntityConfig_Mortal cfg)
        {
            if (cfg != null)
            {
                currentHealth = cfg.health;
                currentRegen = cfg.regen;
            }
        }
        public void Die()
        {
            if (currentHealth <= 0f) currentHealth = 0f;
            Debug.Log("[Mortal] " + entityGhost.entity.gameObject.name + " died.");

            GameObject.Destroy(entityGhost.entity.gameObject);
        }

        public void Hurt(float damage)
        { 
            if (currentHealth <= 0){return;}
            OnDamaged?.Invoke(damage);
            currentHealth -= damage; 
            if (currentHealth <= 0)
            {
                Die();             
            }
            else
            {

            }
        }

        public bool IsDead()
        {
            return currentHealth <= 0f;
        }
        public void SetState(EntityState_Mortal state) => currentHealth = state.currentHealth;
        public EntityState_Mortal GetState() => new EntityState_Mortal { currentHealth = this.currentHealth };


        public System.Type GetConfigType() => typeof(EntityConfig_Mortal);
        public System.Type GetStateType() => typeof(EntityState_Mortal);
    }

    [Serializable]
    public class EntityConfig_Mortal
    {
        public float health = 100f;
        public float regen = 1f;
    }

    [Serializable]
    public class EntityState_Mortal
    {
        public float currentHealth = 1f;
    }
}