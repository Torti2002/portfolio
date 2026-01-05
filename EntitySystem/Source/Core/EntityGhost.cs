using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;



public namespace EntitySystem.Core
{
    /// <summary>
    /// Entity on data-level
    /// IReceiveTick is used to tick all components wich implement IReceiveTick
    /// </summary>
    [SerializeField]
    public class EntityGhost : IReceiveTick
    {
        public string entityInstanceId;                     // -> generated GUID foreach instance of an Entity
        public string entityTypeId;                         // for example "Berry", "WoodPickaxe", "Stegosaurus"
        [NonSerialized] public readonly Dictionary<Type, object> byType = new();
        [NonSerialized] public Entity entity;
        
        // States forach Component
        public List<StateEntry> stateEntries = new();

        [Serializable]
        public class StateEntry
        {
            public string version;
            public string typeKey;
            public string stateRaw;

            public StateEntry(string _version, string _typeKey, string _stateRaw)
            {
                version = _version;
                typeKey = _typeKey;
                stateRaw = _stateRaw;
            }
        }

        public void Tick(float dt) 
        { 
            foreach (var c in byType.Values)
                if (c is IReceiveTick t)
                    t.Tick(dt);
        }

        // --- UTILS ---
        /// <summary>
        /// Iterates over all components of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="act"></param>
        public void ForEach<T>(Action<T> act) where T : class
        {
            foreach (var kv in byType)
                if (kv.Value is T t) act(t);
        }

        /// <summary>
        /// Attaches an Entity to this EntityGhost
        /// </summary>
        /// <param name="e"></param>
        public void AttachWorldEntity(Entity _entity)
        {
            if (_entity == null)
            {
                Debug.LogError($"[EntityGhost '{entityInstanceId}({entityTypeId})']: Entity is null!");
                return;
            }

            entity = _entity;

            if (entity.entityGhost != this)
                entity.AttachGhostEntity(this);

            foreach (var kv in byType)
                if (kv.Value is EntityComponent cc)
                    cc.NotifyWorldAttached();
        }


        /// <summary>
        /// Adds an EntityComponent
        /// </summary>
        /// <param name="comp"></param>
        public void AddEntityComponent(EntityComponent comp)
        {
            if (comp == null) return;
            byType[comp.GetType()] = comp;
            comp.AttachEntityGhost(this);
        }


        // Method wich adds a entitycomponent and returns it
        public T AddEntityComponent<T>() where T : EntityComponent, new()
        {
            var comp = new T();
            AddEntityComponent((EntityComponent)comp);
            return comp;
        }

        /// <summary>
        /// Returns the component of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comp"></param>
        /// <returns></returns>
        public bool TryGet<T>(out T comp) where T : EntityComponent
        {
            if (byType.TryGetValue(typeof(T), out var o) && o is T c)
            {
                comp = c;
                return true;
            }
            comp = null;
            return false;
        }

        /// <summary>
        /// Initializes all components with the OnInit() method
        /// </summary>
        public void InitAllComponents()
        {
            foreach (var kv in byType)
                if (kv.Value is EntityComponent cc) 
                    cc.OnInit();
        }

        // --- Write/Save ---
        public void OverrideStates()
        {
            if (stateEntries == null) stateEntries = new List<StateEntry>();
            stateEntries.Clear();

            foreach (var kv in byType)
            {
                var compObj = kv.Value;
                var compType = compObj.GetType();

                // Alle Interfaces der Komponente durchgehen
                foreach (var itf in compType.GetInterfaces())
                {
                    if (itf.IsGenericType && itf.GetGenericTypeDefinition() == typeof(IReceiveState<>))
                    {
                        var getState = itf.GetMethod("GetState");
                        if (getState == null) continue;

                        var stateObj = getState.Invoke(compObj, null);
                        if (stateObj == null) continue;

                        string json = JsonConvert.SerializeObject(stateObj, EntityJson.Settings);

                        string typeKey = compType.Name;
                        AddOrReplaceRaw(typeKey, json);
                    }
                }
            }
        }
        
        public void UpdateState(EntityComponent comp)
        {
            var compObj = (object)comp;
            var compType = compObj.GetType();

            foreach (var itf in compType.GetInterfaces())
            {
                if (itf.IsGenericType && itf.GetGenericTypeDefinition() == typeof(IReceiveState<>))
                {
                    var getState = itf.GetMethod("GetState");
                    if (getState == null) continue;

                    var stateObj = getState.Invoke(compObj, null);
                    if (stateObj == null) continue;

                    string json = JsonConvert.SerializeObject(stateObj, EntityJson.Settings);
                    string typeKey = compType.Name;
                    AddOrReplaceRaw(typeKey, json);
                    return;
                }
            }
        }

        // --- Read/Load ---
        public bool TryGetState<T>(string typeKey, out T dto)
        {
            var raw = GetStateRawByTypeKey(typeKey);
            if (string.IsNullOrEmpty(raw))
            {
                dto = default;
                return false;
            }

            dto = JsonConvert.DeserializeObject<T>(raw, EntityJson.Settings);
            return dto != null;
        }

        // --- UTILS ---
        public string GetStateRawByTypeKey(string typeKey)
        {
            if (stateEntries == null) return null;
            foreach (var e in stateEntries)
            {
                if (e.typeKey == typeKey)
                    return e.stateRaw;
            }
            return null;
        }

        public void AddOrReplaceRaw(string typeKey, string json)
        {
            if (stateEntries == null)
            {
                stateEntries = new List<StateEntry>();
            }


            int i = stateEntries.FindIndex(e => e.typeKey == typeKey);
            if (i >= 0)
            {
                stateEntries[i].stateRaw = json;
                stateEntries[i].version = Application.version;
            }
            else
            {
                stateEntries.Add(new StateEntry(Application.version, typeKey, json));
            }
        }
    }

    public interface IReceiveTick
    {
        void Tick(float dt);
    }
}