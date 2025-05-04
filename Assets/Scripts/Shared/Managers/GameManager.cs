using Unity.Netcode;

namespace Shared.Managers
{
    public abstract class GameManager : NetworkBehaviour
    {
        public static GameManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            Initialize();
        }

        protected abstract void Initialize();
    }
}