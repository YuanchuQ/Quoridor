using System;
using UnityEditor;
using UnityEngine;

namespace Quoridor.EditorTools
{
    /// <summary>
    /// Applies serialized coordinate data to generated cell prefab instances.
    /// </summary>
    public static class CellInstanceConfigurator
    {
        /// <summary>
        /// Writes board coordinate fields on Cell-like components.
        /// </summary>
        public static void TryConfigureCellInstance(GameObject instance, int x, int y)
        {
            Component[] components = instance.GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                if (component == null || !component.GetType().Name.Contains("Cell", StringComparison.Ordinal))
                {
                    continue;
                }

                var serializedObject = new SerializedObject(component);
                bool changed = false;

                changed |= TrySetInt(serializedObject, "coordinateX", x);
                changed |= TrySetInt(serializedObject, "coordinateY", y);
                changed |= TrySetInt(serializedObject, "x", x);
                changed |= TrySetInt(serializedObject, "y", y);
                changed |= TrySetInt(serializedObject, "gridX", x);
                changed |= TrySetInt(serializedObject, "gridY", y);
                changed |= TrySetInt(serializedObject, "boardX", x);
                changed |= TrySetInt(serializedObject, "boardY", y);

                if (changed)
                {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(component);
                }
            }
        }

        private static bool TrySetInt(SerializedObject serializedObject, string propertyName, int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.propertyType != SerializedPropertyType.Integer)
            {
                return false;
            }

            property.intValue = value;
            return true;
        }
    }
}
