using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Shared.Classes;
using Shared.Misc;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI.Misc.Transition
{
    [Serializable]
    public class RoomTransitionInformation
    {
        public PlayerInformation owner;
        public List<string> characters;
        public List<string> uniqueIds;
        public List<string> statusIds;
        public List<string> zoneIds;
        public int actionCardsCount;

        public async Task<GamePlayerInformation> Converter()
        {
            var playerData = await NakamaManager.Instance
                .GetPlayerData(owner.nakamaId);
            
            var path = $"Assets/Sources/Avatars/Avatar_{playerData.metadata.avatar}.png";
            var avatar = await ResourceLoader.LoadSprite(path);
            
            var assets = new List<CharacterAsset>();
            for (var i = 0; i < 3; i++)
            {
                var id = characters[i];
                var asset = await ResourceLoader.LoadSoAsset<CharacterAsset>(id);
                assets.Add(asset);
            }

            return new GamePlayerInformation
            {
                ActinCardsCount = actionCardsCount,
                ClientId = owner.clientId,
                Name = playerData.username,
                Avatar = avatar,
                Assets = assets,
                UniqueIds = uniqueIds,
                StatusIds = statusIds,
                ZoneIds = zoneIds
            };
        }
    }

    public class GamePlayerInformation
    {
        public int ActinCardsCount;
        public ulong ClientId;
        public string Name;
        public Sprite Avatar;
        public List<CharacterAsset> Assets;
        public List<string> UniqueIds;
        public List<string> StatusIds;
        public List<string> ZoneIds;
    }

    public class RoomTransition : AbstractTransition
    {
        public PlayerInformationDisplay selfDisplay;
        public PlayerInformationDisplay oppoDisplay;
        public GameObject loading;
        public Image loadingProgress;

        [HideInInspector] 
        public List<GamePlayerInformation> Information = new();

        private Tween _fakeLoadingTween;

        // Start Loading
        public override IEnumerator FadeIn(float duration)
        {
            yield return base.FadeIn(duration);

            loading.SetActive(true);
            loadingProgress.fillAmount = 0;

            _fakeLoadingTween = loadingProgress.DOFillAmount(0.943f, 4f);
        }

        // Loading Finish Callback
        public override IEnumerator FadeOut(float duration)
        {
            _fakeLoadingTween?.Kill();
            _fakeLoadingTween = loadingProgress.DOFillAmount(1, 2f);

            yield return _fakeLoadingTween.WaitForCompletion();

            yield return base.FadeOut(duration);
        }

        public override async Task Initialize(object data = default)
        {
            if (data is not string raw)
                return;

            var wrapper = JsonUtility.FromJson<NetworkListWrapper<RoomTransitionInformation>>(raw);
            var task = await Task.WhenAll(
                wrapper.value
                    .Select(information => information.Converter())
            );
            
            Information = task.ToList();
            Information.SplitBy(out var splitInformation, info =>
                info.ClientId == NetworkManager.Singleton.LocalClientId
            );
            Information.Add(splitInformation.First());

            selfDisplay.SetTransitionDisplay(Information[0]);
            oppoDisplay.SetTransitionDisplay(Information[1]);
        }
    }
}