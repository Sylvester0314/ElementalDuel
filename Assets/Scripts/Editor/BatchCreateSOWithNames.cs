using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Editor
{
    [CreateAssetMenu(fileName = "NameListConfig", menuName = "Custom/Cards Name List")]
    public class NameListConfig : ScriptableObject
    {
        public List<string> names;
    }
    
    public class CardAssetsCreator : EditorWindow
    {
        private NameListConfig _nameListConfig;
        private const string FolderPath = "Assets/SOAssets/ActionCards/new";

        [MenuItem("Tools/Batch Create Assets")]
        public static void ShowWindow()
        {
            GetWindow<CardAssetsCreator>("Create ActionCard Assets");
        }

        private void OnGUI()
        {
            _nameListConfig = (NameListConfig)EditorGUILayout.ObjectField(
                "Name List Config",
                _nameListConfig,
                typeof(NameListConfig),
                false
            );
            
            if (GUILayout.Button("Create ActionCard Assets"))
            {
                CreateActionCardAssets();
            }
        }

        private void CreateActionCardAssets()
        {
            foreach (var value in _nameListConfig.names)
            {
                var card = CreateInstance<ActionCardAsset>();
                card.Initialize(value);

                var assetPath = $"{FolderPath}/{value}.asset";
                AssetDatabase.CreateAsset(card, assetPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}