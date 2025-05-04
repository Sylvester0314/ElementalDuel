using System;

namespace Shared.Misc
{
    [Serializable]

    public class ServerConfig
    {
        public string serverScheme;
        public string serverHost;
        public int serverPort;
        public string socketServerKey;
        public string httpKey;
    }

    [Serializable]
    public class RegisterAccountPayload
    {
        public string uid;
    }

    [Serializable]
    public class Metadata
    {
        public string uid;
        public int score;
        public int avatar;
    }

    [Serializable]
    public class PlayerData
    {
        public string username;
        public Metadata metadata;
    }
}