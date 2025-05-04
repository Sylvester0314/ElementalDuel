using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nakama;
using ParrelSync;
using Shared.Classes;
using Shared.Misc;
using UnityEngine;

public class NakamaManager : MonoBehaviour
{
    public static NakamaManager Instance { get; private set; }

    [HideInInspector] 
    public PlayerData self;
    public IApiAccount Account;
    public ISession Session;

    public DeckData initialDeck;

    private IClient _client;
    private ServerConfig _config;
    private string _deviceId;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            PlayerPrefs.SetInt("HandbookContainer", 0);
            PlayerPrefs.Save();
        }
        else
            Destroy(gameObject);
    }

    public void Start()
    {
        StaticMisc.LoadJson("server-config", out _config);

        _client = new Nakama.Client(
            _config.serverScheme, _config.serverHost,
            _config.serverPort, _config.socketServerKey
        );

        _deviceId = ClonesManager.IsClone()
            ? ClonesManager.GetArgument()
            : SystemInfo.deviceUniqueIdentifier;

        initialDeck.deckName = ResourceLoader.GetLocalizedUIText("initial_deck_key");
        initialDeck.UniqueId = new Guid();
    }

    #region Log in/out

    public void LogoutAccount()
    {
        Session = null;
        Account = null;
    }

    public async Task CheckDeviceAccount()
    {
        try
        {
            Session = await _client.AuthenticateDeviceAsync(_deviceId, create: false);
            await GetSelfAccountData();
        }
        catch (Exception)
        {
            Session = null;
            Debug.Log("该设备未检测到绑定的账号");
        }
    }

    public async Task<bool> LoginAccount(string email, string password, Action<string> onError = null)
    {
        try
        {
            Session = await _client.AuthenticateEmailAsync(
                $"{email}@weird.adachi.top",
                password, create: false
            );
            await GetSelfAccountData();
            return true;
        }
        catch (ApiResponseException e)
        {
            onError?.Invoke(e.Message);
        }

        return false;
    }

    public async Task<bool> CreateNewAccount(string username, string password, Action<string> onError = null)
    {
        try
        {
            var payload = new Dictionary<string, string>
            {
                { "password", password },
                { "username", username },
                { "device_id", _deviceId }
            };
            var rpcResult = await _client.RpcAsync(
                _config.httpKey,
                "create_new_account",
                payload.ToJson()
            );

            var data = JsonUtility.FromJson<RegisterAccountPayload>(rpcResult.Payload);
            await LoginAccount(data.uid, password, onError);

            await SaveDeck(initialDeck);

            return true;
        }
        catch (ApiResponseException e)
        {
            onError?.Invoke(e.Message);
        }

        return false;
    }

    #endregion

    #region Storage CRUD

    public async Task WriteStorage(string collection, string key, Dictionary<string, string> value)
    {
        await WriteStorage(collection, key, value.ToJson());
    }

    public async Task WriteStorage(string collection, string key, string value)
    {
        var data = new WriteStorageObject
        {
            Collection = collection,
            Key = key,
            Value = value,
            PermissionRead = 2
        };
        var request = new IApiWriteStorageObject[] { data };

        await _client.WriteStorageObjectsAsync(Session, request);
    }

    public async Task<IApiStorageObjects> ReadStorage(
        string collection, string key, string userId = null
    )
    {
        userId ??= Session.UserId;
        var condition = new StorageObjectId
        {
            Collection = collection,
            Key = key,
            UserId = userId
        };

        var request = new IApiReadStorageObjectId[] { condition };
        return await _client.ReadStorageObjectsAsync(Session, request);
    }

    public async Task<IApiStorageObjectList> ReadAllStorage(
        string collection, string cursor = null
    )
    {
        return await _client.ListStorageObjectsAsync(
            Session, collection, 50, cursor
        );
    }

    public async Task DeleteStorage(string collection, string key, string userId = null)
    {
        userId ??= Session.UserId;
        var condition = new StorageObjectId
        {
            Collection = collection,
            Key = key,
            UserId = userId
        };

        await _client.DeleteStorageObjectsAsync(Session, new[] { condition });
    }

    #endregion

    #region Player Data Getter

    public async Task<Metadata> GetMetadata()
    {
        var account = await _client.GetAccountAsync(Session);
        return Nakama.TinyJson.JsonParser.FromJson<Metadata>(account.User.Metadata);
    }

    public async Task<PlayerData> GetPlayerData(string userId)
    {
        try
        {
            var payload = new Dictionary<string, string> { { "user_id", userId } };
            var response = await _client.RpcAsync(
                _config.httpKey, "get_metadata", payload.ToJson()
            );

            return Nakama.TinyJson.JsonParser.FromJson<PlayerData>(response.Payload);
        }
        catch (Exception e)
        {
            Debug.Log("Failed to load avatar: " + e.Message);
        }

        return null;
    }

    private async Task GetSelfAccountData()
    {
        Account = await _client.GetAccountAsync(Session);
        self = await GetPlayerData(Session.UserId);
    }
    
    #endregion

    #region Deck Operation

    public async Task<List<DeckData>> GetDecks()
    {
        var storage = await ReadStorage(
            "deck",
            Instance.self.metadata.uid
        );

        var data = storage.Objects.First().Value;
        var wrapper = JsonUtility.FromJson<NetworkListWrapper<RawDeckData>>(data);
        var decksTask = await Task.WhenAll(
            wrapper.value.Select(deck => deck.Parse())
        );
        
        return decksTask.ToList();
    }

    public async Task SaveDeck(DeckData deck)
    {
        var decks = await GetDecks();
        var index = decks.FindIndex(data => data.UniqueId == deck.UniqueId);
        
        if (index == -1) 
            decks.Add(deck);
        else
            decks[index] = deck;

        await WriteDeckStorage(decks);
    }
    
    public async Task RenameDeck(Guid uniqueId, string newName)
    {
        var decks = await GetDecks();
        var deck = decks.Where(deck => deck.UniqueId == uniqueId).FirstOrDefault();
        if (deck == null)
            return;
        
        deck.deckName = newName;
        await WriteDeckStorage(decks);
    }
    
    public async Task WriteDeckStorage(List<DeckData> decks)
    {
        var rawDeskList = new NetworkListWrapper<RawDeckData> (
            decks.Select(deck => deck.ToRaw()).ToList()
        );
        
        await WriteStorage("deck", self.metadata.uid, JsonUtility.ToJson(rawDeskList));
    }

    #endregion
}