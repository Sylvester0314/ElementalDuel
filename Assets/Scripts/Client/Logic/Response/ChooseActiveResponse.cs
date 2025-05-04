using System;
using System.Collections.Generic;
using System.Linq;
using Server.GameLogic;
using Shared.Enums;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public class ChooseActiveResponse : BaseResponse, IEquatable<ChooseActiveResponse>
    {
        public List<Ulong> Players;

        public ChooseActiveResponse()
        {
            Players = Ulong.EmptyList;
        }
        
        public ChooseActiveResponse(List<PlayerLogic> players)
        {
            Players = players.Select(player => player.Id).Packing<ulong, Ulong>();
        }

        public override void Process()
        {
            string entry;
            
            if (!Players.Unpacking().Contains(LocalId))
                entry = "waiting_opponent_select_active";   
            else
            {
                Global.GetZone<CharacterZone>(Site.Self).ChooseActiveCharacter();
                entry = "select_new_active_character";   
            }

            var selectHint = ResourceLoader.GetLocalizedUIText(entry);
            
            Global.prompt.banner.FixedAnimate(selectHint);
            Global.BlockingResponse = this;
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            NetCodeMisc.SerializeList(serializer, ref Players);
        }

        public bool Equals(ChooseActiveResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return Players.Equals(other.Players);
        }
    }
}