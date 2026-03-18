using UnityEngine;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Common.Attribute
{
    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public class ConditionalHideAttribute : PropertyAttribute
    {
        //The name of the bool field that will be in control
        public string conditionalSourceField = "";
        //TRUE = Hide in inspector / FALSE = Disable in inspector 
        public bool HideInInspector = false;
        object[] conditionalSourceValues;

        public ConditionalHideAttribute(string conditionalSourceField, params object[] conditionalSourceValues)
        {
            this.conditionalSourceField = conditionalSourceField;
            this.HideInInspector = true;
            this.conditionalSourceValues = conditionalSourceValues;
        }

        public ConditionalHideAttribute(string conditionalSourceField, bool HideInInspector, params object[] conditionalSourceValues)
        {
            this.conditionalSourceField = conditionalSourceField;
            this.HideInInspector = HideInInspector;
            this.conditionalSourceValues = conditionalSourceValues;
        }        

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(ConditionalHideAttribute), true)]
        public class ConditionalHidePropertyDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
                bool enabled = GetConditionalHideAttributeResult(condHAtt, property);
                bool wasEnabled = GUI.enabled;
                GUI.enabled = enabled;
                if (!condHAtt.HideInInspector || enabled)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }

                GUI.enabled = wasEnabled;
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
                bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

                if (!condHAtt.HideInInspector || enabled)
                {
                    return EditorGUI.GetPropertyHeight(property, label);
                }
                else
                {
                    return -EditorGUIUtility.standardVerticalSpacing;
                }
            }

            private bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHAtt, SerializedProperty property)
            {
                bool enabled = true;
                string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
                string conditionPath = propertyPath.Replace(property.name, condHAtt.conditionalSourceField); //changes the path to the conditionalsource property path
                SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

                if (sourcePropertyValue != null)
                {
                    object sourceValue = GetObjectValue(sourcePropertyValue);
                    enabled = condHAtt.conditionalSourceValues.Contains(sourceValue);
                }
                else
                {
                    Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + condHAtt.conditionalSourceField);
                }

                return enabled;
            }

            private object GetObjectValue(SerializedProperty property)
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        return property.intValue;
                    case SerializedPropertyType.Boolean:
                        return property.boolValue;
                    case SerializedPropertyType.Float:
                        return property.floatValue;
                    case SerializedPropertyType.String:
                        return property.stringValue;
                    case SerializedPropertyType.Enum:
                        return property.enumValueIndex;
                    case SerializedPropertyType.ObjectReference:
                        return property.objectReferenceValue;
                    default:
                        Debug.LogWarning("Unsupported property type: " + property.propertyType);
                        return null;
                }
            }
        }
#endif
    }
}
