using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Common.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class ConditionalShowAttribute : PropertyAttribute
    {
        //The name of the bool field that will be in control
        public string ConditionalSourceField = "";
        //TRUE = Hide in inspector / FALSE = Disable in inspector 
        public bool HideInInspector = false;

        public ConditionalShowAttribute(string conditionalSourceField)
        {
            this.ConditionalSourceField = conditionalSourceField;
            this.HideInInspector = false;
        }

        public ConditionalShowAttribute(string conditionalSourceField, bool hideInInspector)
        {
            this.ConditionalSourceField = conditionalSourceField;
            this.HideInInspector = hideInInspector;
        }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ConditionalShowAttribute))]
    public class ConditionalHidePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalShowAttribute condHAtt = (ConditionalShowAttribute)attribute;
            bool enabled = GetConditionalShowAttributeResult(condHAtt, property);
    
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
            ConditionalShowAttribute condHAtt = (ConditionalShowAttribute)attribute;
            bool enabled = GetConditionalShowAttributeResult(condHAtt, property);
    
            if (!condHAtt.HideInInspector || enabled)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }
    
        private bool GetConditionalShowAttributeResult(ConditionalShowAttribute condHAtt, SerializedProperty property)
        {
            bool enabled = true;
            string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
            string conditionPath = propertyPath.Replace(property.name, condHAtt.ConditionalSourceField); //changes the path to the conditionalsource property path
            SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);
    
            if (sourcePropertyValue != null)
            {
                enabled = !sourcePropertyValue.boolValue;
            }
            else
            {
                Debug.LogWarning("Attempting to use a ConditionalShowAttribute but no matching SourcePropertyValue found in object: " + condHAtt.ConditionalSourceField);
            }
    
            return enabled;
        }
    }
#endif
    }
}
