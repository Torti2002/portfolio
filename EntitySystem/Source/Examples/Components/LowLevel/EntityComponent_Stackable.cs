using UnityEngine;
using System;



public namespace EntitySystem.Examples.Components.LowLevel
{
    public class EntityComponent_Stackable : 
        EntityComponent,
        IReceiveConfig<Config_Stackable>
    {
        public int maxQuantity = 1;
        public int quantity = 1;

        public void ApplyConfig(Config_Stackable config) => maxQuantity = config.maxQuantity;
        public Type GetConfigType() => typeof(Config_Stackable);
    }

    [System.Serializable]
    public class Config_Stackable
    {
        public int maxQuantity = 1;
    }
}