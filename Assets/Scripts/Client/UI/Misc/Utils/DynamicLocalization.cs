using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class DynamicLocalization : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string tableName;

    public void SetLocalizedString(string keyName)
    {
        LocalizationSettings.StringDatabase.GetLocalizedStringAsync(tableName, keyName).Completed += handle =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                textComponent.text = handle.Result;
            }
            else
            {
                Debug.LogError($"Failed to load localized text for key: {keyName}");
            }
        };
    }
}
