using System;
using Shared.Enums;
using Shared.Logic.Statuses;

namespace Shared.Handler
{
    public interface IGameEntity
    {
        public void CancelPreviewStatus();

        public void HidePreviewComponents();

        public void DoAction(CharacterCard character, Element element, Action feedbackAction = null);
    }

    public interface IStatusEntity
    {
        public void RefreshLiftHint(Status status);

        public void Discard();
    }
}