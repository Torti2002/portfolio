using UnityEngine;
using System;
using System.Collections.Generic;



public namespace EntitySystem.Examples
{
    public static class Tables
    {
        #region componentTypeKey > componentType
        public static readonly Dictionary<string, Type> componentTypeKeyToComponentType = new Dictionary<string, Type>
        {        
            // EntityComponent
            { "EntityComponent_Attack",                     typeof(EntityComponent_Attack) },
            { "EntityComponent_Inventory",                  typeof(EntityComponent_Inventory) },
            { "EntityComponent_Mortal",                     typeof(EntityComponent_Mortal)},
            { "EntityComponent_Stackable",                  typeof(EntityComponent_Stackable)},
        };
        #endregion

        #region componentType > componentTypeKey
        public static readonly Dictionary<Type, string> componentTypeToComponentTypeKey = new Dictionary<Type, string>
        {
            // EntityComponent
            { typeof(EntityComponent_Attack),             "EntityComponent_Attack" },
            { typeof(EntityComponent_Inventory),          "EntityComponent_Inventory" },
            { typeof(EntityComponent_Mortal),             "EntityComponent_Mortal"},
            { typeof(EntityComponent_Stackable),          "EntityComponent_Stackable"},
        };
        #endregion

        public static Type ResolveTypeKey(string typeKey, Dictionary<string, Type> table) 
        {
            if (table.TryGetValue(typeKey, out var t)) return t;
            Debug.LogError($"Could not resolve typeKey: {typeKey}");
            return null;
        }
        public static string ResolveType(Type type, Dictionary<Type, string> table)
        {
            if (table.TryGetValue(type, out var t)) return t;
            Debug.LogError($"Could not resolve type: {type}");
            return null;
        }
        public static Type ResolveTypeKey(Type type, Dictionary<Type, Type> table)
        {
            if (table.TryGetValue(type, out var t)) return t;
            Debug.LogError($"Could not resolve type: {type}");
            return null;
        }
        public static string ResolveType(string typeKey, Dictionary<string, string> table)
        {
            if (table.TryGetValue(typeKey, out var t)) return t;
            Debug.LogError($"Could not resolve typeKey: {typeKey}");
            return null;
        }
    }
}