using System;
using System.Reflection;
using UnityEditor;

// https://gist.github.com/starikcetin/583a3b86c22efae35b5a86e9ae23f2f0

internal static class EditorUtils
{
    private const BindingFlags AllBindingFlags = (BindingFlags)(-1);

    /// <summary>
    /// Returns attributes of type <typeparamref name="TAttribute"/> on <paramref name="serializedProperty"/>.
    /// </summary>
    public static TAttribute[] GetAttributes<TAttribute>(this SerializedProperty serializedProperty, bool inherit)
        where TAttribute : Attribute
    {
        if (serializedProperty == null)
        {
            throw new ArgumentNullException(nameof(serializedProperty));
        }

        var targetObjectType = serializedProperty.serializedObject.targetObject.GetType();

        if (targetObjectType == null)
        {
            throw new ArgumentException($"Could not find the {nameof(targetObjectType)} of {nameof(serializedProperty)}");
        }

        var path = serializedProperty.propertyPath.Split('.');
        while(path.Length > 1)
        {
            var fieldInfo = targetObjectType.GetField(path[0], AllBindingFlags);
            if (fieldInfo != null)
            {
                targetObjectType = fieldInfo.FieldType;
            }
            else
            {
                var propertyInfo = targetObjectType.GetProperty(path[0], AllBindingFlags);
                if (propertyInfo != null)
                {
                    targetObjectType = propertyInfo.PropertyType;
                }
                else
                {
                    throw new ArgumentException($"Could not find the field or property of {nameof(serializedProperty)}");
                }
            }
            path = path[1..];
        }
        
        if(path.Length == 1)
        {
            var fieldInfo = targetObjectType.GetField(path[0], AllBindingFlags);
            if (fieldInfo != null)
            {
                return (TAttribute[])fieldInfo.GetCustomAttributes<TAttribute>(inherit);
            }

            var propertyInfo = targetObjectType.GetProperty(path[0], AllBindingFlags);
            if (propertyInfo != null)
            {
                return (TAttribute[])propertyInfo.GetCustomAttributes<TAttribute>(inherit);
            }   
        }
        else throw new ArgumentException($"Could not find the field or property of {nameof(serializedProperty)}");

        return Array.Empty<TAttribute>();
    }
}