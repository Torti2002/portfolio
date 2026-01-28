using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "EntityAttributes", menuName = "Scriptable Objects/EntityAttributes")]
public class EntityAttributes : ScriptableObject
{
    public string displayName;
    public string entityId;
    public GameObject entityPrefab;

    [SerializeReference]
    public List<EntityComponentConfigData> components = new();
}