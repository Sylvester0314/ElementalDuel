using System.IO;

namespace Editor
{
    using UnityEditor;
    using UnityEngine;
    using System;
    using System.Linq;
    using System.Reflection;

    public class CreateScriptableObjectAsset : UnityEditor.ProjectWindowCallback.EndNameEditAction
    {
        public override void Action(int instanceId, string path, string resourceFile)
        {
            var type = Type.GetType(resourceFile) ??
                       Assembly.GetExecutingAssembly().GetType(resourceFile) ??
                       AppDomain.CurrentDomain.GetAssemblies()
                           .SelectMany(a => a.GetTypes())
                           .FirstOrDefault(t => t.Name == resourceFile);

            if (type == null || !typeof(ScriptableObject).IsAssignableFrom(type))
            {
                Debug.LogError($"无法创建 ScriptableObject：未找到类型 {resourceFile}");
                return;
            }

            var asset = CreateInstance(type);
            if (asset is IInitializableScriptableObject initializable)
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                initializable.Initialize(fileName);
            }

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}