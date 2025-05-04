namespace Editor
{
    using UnityEditor;
    using UnityEngine;
    using System.IO;

    public static class ScriptableObjectUtility
    {
        public static void CreateAsset<T>(string defaultFileName) where T : ScriptableObject
        {
            var path = GetSelectedPathOrFallback();
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, defaultFileName));

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<CreateScriptableObjectAsset>(),
                assetPath,
                null,
                typeof(T).Name
            );
        }

        private static string GetSelectedPathOrFallback()
        {
            if (Selection.activeObject == null)
                return "Assets";
            
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                path = Path.GetDirectoryName(path);
            return path;
        }
    }
}