using System.Collections.Generic;
using Server.Managers;
using UnityEngine;

namespace Client.Logic.Request
{
    public class ActionRequestReceiver
    {
        private readonly GameManager _manager;
        private readonly Queue<ActionRequestWrapper> _requests;
        private bool _processing;

        public ActionRequestReceiver(GameManager manager)
        {
            _manager = manager;
            _requests = new Queue<ActionRequestWrapper>();
        }

        public void Enqueue(ActionRequestWrapper request)
        {
            _requests.Enqueue(request);

            if (_processing)
                return;

            _processing = true;
            Process();
        }

        public void Dequeue(string uniqueId)
        {
            if (_requests.Count == 0 || _requests.Peek().Request.UniqueId != uniqueId)
                return;
            
            var wrapper = _requests.Dequeue();
            wrapper.Request.Complete();
            
            RequestDebug(wrapper.Request, "Complete");
        }

        private async void Process()
        {
            while (true)
            {
                if (_requests.Count == 0)
                {
                    _processing = false;
                    return;
                }

                var request = _requests.Peek().Request;
                RequestDebug(request, "Start process");

                await request.Process(_manager);
            }
        }

        private void RequestDebug(IActionRequest request, string type)
        {
            var roomId = _manager.Room.id.Value.content;
            var uniqueId = request.UniqueId[..8];
            
            Debug.Log($"[{roomId}] [{uniqueId}] {type} the request (from {request.RequesterId}): \n{request}");
        }
    }
}