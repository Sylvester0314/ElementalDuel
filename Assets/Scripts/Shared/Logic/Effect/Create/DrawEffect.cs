using Server.Logic.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using Server.GameLogic;
using Shared.Enums;
using Shared.Handler;
using Shared.Logic.Statuses;
using Sirenix.OdinInspector;

namespace Shared.Logic.Effect
{
    public enum DrawMode
    {
        Top,
        Bottom,
        SpecificName,
        SpecificType
    }
    
    [Serializable]
    public class DrawEffect : BaseNonTargetCreateEffect
    {
        public DrawMode drawMode;
        
        public int amount;

        [ShowIf("drawMode", DrawMode.SpecificName)]
        public string specificName;

        [ShowIf("drawMode", DrawMode.SpecificType)]
        public Property specificType;

        public DrawEffect() { }
        
        public DrawEffect(Site site, DrawMode drawMode, int amount)
        {
            this.site = site;
            this.amount = amount;
            this.drawMode = drawMode;
        }

        protected override IReadOnlyList<BaseEvent> GenerateEvents(IEventSource source, IEventGenerator via)
        {
            var events = new List<BaseEvent>();

            if (site != Site.Self)
                events.Add(Generate(source.Opponent, source, via));
            if (site != Site.Opponent)
                events.Add(Generate(source.Belongs, source, via));

            return events;
        }

        private BaseEvent Generate(PlayerLogic player, IEventSource source, IEventGenerator via)
        {
            var deck = player.DeckCard;

            var (drew, overdrew) = drawMode switch
            {
                DrawMode.Top            => deck.Draw(amount),
                DrawMode.Bottom         => deck.Draw(amount, bottom: true),
                DrawMode.SpecificName   => deck.DrawBySpecificName(amount, specificName),
                DrawMode.SpecificType   => deck.DrawBySpecificType(amount, specificType),
                _                       => throw new ArgumentOutOfRangeException()
            };

            return new DrawEvent(source, player.ActiveCharacter, via, drew, overdrew);
        }

        public override IReadOnlyList<BaseEvent> ResponseEvent(Status handler, BaseEvent e)
            => GenerateEvents(handler, e.Via).Concat(AutoConsume(handler)).ToList();
    }
}