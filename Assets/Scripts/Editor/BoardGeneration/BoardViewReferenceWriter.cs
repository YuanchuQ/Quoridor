// Writes generated cell references into a BoardView-like serialized object
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Quoridor.EditorTools
{
    /// <summary>
    /// Writes generated cell references into a BoardView-like serialized object.
    /// </summary>
    public static class BoardViewReferenceWriter
    {
        private static readonly string[] BoardViewCellFieldNames =
        {
            "cells",
            "cellViews",
            "cellViewReferences",
            "boardCells",
            "serializedCells",
            "cellList"
        };

        /// <summary>
        /// Attempts to write generated cell references into a serialized BoardView cell collection.
        /// </summary>
        public static bool TryWireBoardView(UnityObject boardView, IReadOnlyList<GameObject> generatedCells)
        {
            UnityObject target = ResolveBoardViewObject(boardView);
            if (target == null || generatedCells == null)
            {
                return false;
            }

            foreach (string fieldName in BoardViewCellFieldNames)
            {
                if (TryWriteCellCollection(target, fieldName, generatedCells))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryWriteCellCollection(UnityObject target, string fieldName, IReadOnlyList<GameObject> generatedCells)
        {
            FieldInfo field = FindField(target.GetType(), fieldName);
            if (field == null)
            {
                return false;
            }

            Type elementType = GetCollectionElementType(field.FieldType);
            if (elementType == null || !typeof(UnityObject).IsAssignableFrom(elementType))
            {
                return false;
            }

            var serializedObject = new SerializedObject(target);
            SerializedProperty cellCollection = serializedObject.FindProperty(fieldName);
            if (cellCollection == null || !cellCollection.isArray || cellCollection.propertyType == SerializedPropertyType.String)
            {
                return false;
            }

            Undo.RecordObject(target, "Wire BoardView Cells");
            cellCollection.arraySize = generatedCells.Count;

            for (int i = 0; i < generatedCells.Count; i++)
            {
                SerializedProperty element = cellCollection.GetArrayElementAtIndex(i);
                element.objectReferenceValue = ResolveCellReference(generatedCells[i], elementType);
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            return true;
        }

        private static UnityObject ResolveBoardViewObject(UnityObject boardView)
        {
            if (boardView is GameObject gameObject)
            {
                Component boardViewComponent = FindNamedComponent(gameObject, "BoardView");
                return boardViewComponent != null ? boardViewComponent : gameObject;
            }

            return boardView;
        }

        private static UnityObject ResolveCellReference(GameObject cellObject, Type elementType)
        {
            if (cellObject == null)
            {
                return null;
            }

            if (typeof(GameObject).IsAssignableFrom(elementType))
            {
                return cellObject;
            }

            if (typeof(Component).IsAssignableFrom(elementType))
            {
                return cellObject.GetComponentInChildren(elementType, true);
            }

            Component cellView = FindNamedComponent(cellObject, "Cell");
            return cellView != null ? cellView : cellObject;
        }

        private static Component FindNamedComponent(GameObject gameObject, string namePart)
        {
            Component[] components = gameObject.GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                if (component != null && component.GetType().Name.Contains(namePart, StringComparison.Ordinal))
                {
                    return component;
                }
            }

            return null;
        }

        private static Type GetCollectionElementType(Type collectionType)
        {
            if (collectionType.IsArray)
            {
                return collectionType.GetElementType();
            }

            if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return collectionType.GetGenericArguments()[0];
            }

            return null;
        }

        private static FieldInfo FindField(Type targetType, string fieldName)
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            Type currentType = targetType;
            while (currentType != null)
            {
                FieldInfo field = currentType.GetField(fieldName, Flags);
                if (field != null)
                {
                    return field;
                }

                currentType = currentType.BaseType;
            }

            return null;
        }
    }
}
