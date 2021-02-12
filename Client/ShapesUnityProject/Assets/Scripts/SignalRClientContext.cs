using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using UnityEngine;

namespace Assets.Scripts
{
    public class LockArgs : EventArgs
    {
        public bool AcruiredLock { get; set; }
    }

    public class MessageArgs : EventArgs
    {
        public string Name { get; set; }
        public string Message { get; set; }
    }

    public class PositionUpdateArgs : EventArgs
    {
        public Vector3 Position { get; set; }
    }

    public class SignalRClientContext : IDisposable
    {
        public EventHandler<MessageArgs> ReceivedMessage = delegate { };
        public EventHandler<LockArgs> LockStateUpdated = delegate { };
        public EventHandler<PositionUpdateArgs> PositionUpdated = delegate { };
        private static HubConnection _hubConnection;
        private static IHubProxy _chatHub;
        private static IDisposable _messageStream;
        private static IDisposable _lockStream;
        private static IDisposable _positionUpdateStream;
        private static SignalRClientContext _instance;


        public static bool HasLock = false;

        private SignalRClientContext()
        { 
        }

        public static SignalRClientContext Instance => _instance;

        public static async UniTask<SignalRClientContext> CreateInstance()
        {
            if (Instance != null) return Instance;
            _instance = new SignalRClientContext();
            _hubConnection = new HubConnection("http://localhost:8080/chatHub");
            _chatHub = _hubConnection.CreateHubProxy("ChatHub");
            await _hubConnection.Start().ConfigureAwait(false);
            _messageStream = _chatHub.On<string, string>("SendMessageToClient", _instance.MessageFromServer);
            _positionUpdateStream =
                _chatHub.On<float, float, float>("SendUpdatedPosition", _instance.GetPositionUpdate);
            _lockStream = _chatHub.On<bool>("SendLockStatus", _instance.GetLockStatus);
            return _instance;
        }

        private void GetPositionUpdate(float x, float y, float z)
        {
            PositionUpdated.Invoke(Instance, new PositionUpdateArgs(){ Position = new Vector3(x,y,z)});
        }

        public void RequestToLock()
        {
            if(HasLock) return;
            
            _chatHub.Invoke("TryLock");
        }

        public void ReleaseLock()
        {
            _chatHub.Invoke("ReleaseLock");
            HasLock = false;
        }
        
        private void GetLockStatus(bool acquiredLock)
        {
            HasLock = acquiredLock;
            LockStateUpdated.Invoke(Instance, new LockArgs(){AcruiredLock = acquiredLock});
        }

        public Task SendMessageAsync(string name, string message)
        {
            return _chatHub.Invoke("SendMessage", new object[] {name, message});
        }

        private void MessageFromServer(string name, string message)
        {
            ReceivedMessage.Invoke(Instance, new MessageArgs() {Message = message, Name = name});
        }
        
        

        public void Dispose()
        {
            _hubConnection.Dispose();
            _messageStream.Dispose();
            _lockStream.Dispose();
        }

        public void UpdatePositionInServer(Vector3 vector3)
        {
            _chatHub.Invoke("UpdatePosition", new object[] { vector3.x, vector3.y, vector3.z});
        }
    }
}