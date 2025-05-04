using UnityEditor;
using UnityEngine;

public class DescriptionDrawerScriptableObject : ScriptableObject
{
    protected static string CustomDescriptionDrawer(string value, GUIContent label)
    {
        var style = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true,
            richText = true,
            alignment = TextAnchor.UpperLeft,
            margin = new RectOffset(6, 0, 0, 0)
        };

        GUI.enabled = false;
        EditorGUILayout.LabelField(label, new GUIContent(value), style);
        GUI.enabled = true;

        return value;
    }
}