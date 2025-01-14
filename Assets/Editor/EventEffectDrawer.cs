#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

[CustomPropertyDrawer(typeof(EventEffect), true)]
public class EventEffectDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Calculate rects
        Rect buttonRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // Get current type
        Type currentType = property.managedReferenceValue?.GetType();
        var derivedTypes = TypeCache.GetTypesDerivedFrom<EventEffect>().Where(t => !t.IsAbstract).ToArray();
        string[] typeNames = derivedTypes.Select(t => t.Name).ToArray();

        int currentIndex = Array.FindIndex(derivedTypes, t => t == currentType);
        if (currentIndex < 0) currentIndex = 0;

        // Draw the type selection button
        if (GUI.Button(buttonRect, currentType != null ? currentType.Name : "Select Type"))
        {
            GenericMenu menu = new GenericMenu();
            foreach (var type in derivedTypes)
            {
                menu.AddItem(new GUIContent(type.Name), type == currentType, () =>
                {
                    property.managedReferenceValue = Activator.CreateInstance(type);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }

        // Draw the properties of the selected type
        if (property.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            SerializedProperty iterator = property.Copy();
            bool enterChildren = true;
            float yOffset = position.y + EditorGUIUtility.singleLineHeight + 2;

            while (iterator.NextVisible(enterChildren))
            {
                if (!iterator.propertyPath.StartsWith(property.propertyPath + "."))
                    break;

                float height = EditorGUI.GetPropertyHeight(iterator, true);
                Rect propertyRect = new Rect(position.x, yOffset, position.width, height);
                EditorGUI.PropertyField(propertyRect, iterator, true);
                yOffset += height + 2;
                enterChildren = false;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (property.managedReferenceValue != null)
        {
            SerializedProperty iterator = property.Copy();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                if (!iterator.propertyPath.StartsWith(property.propertyPath + "."))
                    break;

                height += EditorGUI.GetPropertyHeight(iterator, true) + 2;
                enterChildren = false;
            }
        }

        return height;
    }
}
#endif
