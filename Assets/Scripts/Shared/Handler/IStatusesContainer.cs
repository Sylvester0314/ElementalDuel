using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Logic.Statuses;

namespace Shared.Handler
{
    public interface IStatusesContainer
    {
        public string UniqueId { get; }
        public void Initialize(string id, Global global);
        public Task Append(Status status);
        public void Preview(List<Status> statuses);
        public void CancelPreview();
    }
}