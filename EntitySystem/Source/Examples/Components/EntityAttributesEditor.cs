#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;



[CustomEditor(typeof(EntityAttributes))]
public class EntityAttributesEditor : Editor
{
    private Type[] _configTypes;

    private void OnEnable()
    {
        _configTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return Array.Empty<Type>(); }
            })
            .Where(t => typeof(EntityComponentConfigData).IsAssignableFrom(t) && !t.IsAbstract)
            .OrderBy(t => t.Name)
            .ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Add Component Config", EditorStyles.boldLabel);

        if (_configTypes == null || _configTypes.Length == 0)
        {
            EditorGUILayout.HelpBox("No EntityComponentConfigData types found.", MessageType.Warning);
        }
        else
        {
            if (GUILayout.Button("Add..."))
            {
                var menu = new GenericMenu();
                foreach (var t in _configTypes)
                {
                    menu.AddItem(new GUIContent(t.Name), false, () => AddConfigInstance(t));
                }
                menu.ShowAsContext();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void AddConfigInstance(Type type)
    {
        var attrs = (EntityAttributes)target;
        Undo.RecordObject(attrs, "Add Component Config");

        var instance = (EntityComponentConfigData)Activator.CreateInstance(type);
        attrs.components ??= new System.Collections.Generic.List<EntityComponentConfigData>();
        attrs.components.Add(instance);

        EditorUtility.SetDirty(attrs);
    }
}
#endif