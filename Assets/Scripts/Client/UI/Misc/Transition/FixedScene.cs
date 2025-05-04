using TMPro;
using UnityEngine;

namespace Client.UI.Misc.Transition
{
    public class FixedScene : MonoBehaviour
    {
        public static FixedScene Instance;

        public TextMeshProUGUI uidText;
        public DarkTransition dark;
        public RoomTransition room;

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void Initialize()
        {
            var uid = NakamaManager.Instance.self.metadata.uid;
            uidText.text = $"UID: {uid}";
            uidText.gameObject.SetActive(true);

            Open();
        }
    }
}