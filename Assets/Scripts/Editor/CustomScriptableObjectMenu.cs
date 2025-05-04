using UnityEditor;

namespace Editor
{
    public class CustomScriptableObjectMenu
    {
        [MenuItem("Assets/Create/New Skill", false, 1)]
        public static void CreateSkillAsset()
        {
            ScriptableObjectUtility.CreateAsset<SkillAsset>("New Skill.asset");
        }

        [MenuItem("Assets/Create/New Card", false, 2)]
        public static void CreateActionCardAsset()
        {
            ScriptableObjectUtility.CreateAsset<ActionCardAsset>("New Card.asset");
        }
        
        [MenuItem("Assets/Create/New Character", false, 3)]
        public static void CreateCharacterCardAsset()
        {
            ScriptableObjectUtility.CreateAsset<CharacterAsset>("New Character.asset");
        }
        
        [MenuItem("Assets/Create/New Status", false, 4)]
        public static void CreateStatusCardAsset()
        {
            ScriptableObjectUtility.CreateAsset<StatusCardAsset>("New Status.asset");
        }
    }
}