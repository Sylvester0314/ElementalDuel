using System;
using System.Collections.Generic;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public class GameOverResponse : BaseResponse, IEquatable<GameOverResponse>
    {
        public List<Ulong> FailedPlayers;
        
        public GameOverResponse() { }

        public GameOverResponse(List<ulong> winners)
        {
            FailedPlayers = winners.Packing<ulong, Ulong>();
        }

        public override void Process()
        {
            var selfWin = !FailedPlayers.Unpacking().Contains(LocalId);
            var oppoWin = FailedPlayers.Count != 2 && !selfWin;
            
            Global.prompt.result.Display(selfWin);

            Global.Self.nameBar.SetGameResult(selfWin);
            Global.Opponent.nameBar.SetGameResult(oppoWin);
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            NetCodeMisc.SerializeList(serializer, ref FailedPlayers);
        }

        public bool Equals(GameOverResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return FailedPlayers.Equals(other.FailedPlayers);
        }
    }
}