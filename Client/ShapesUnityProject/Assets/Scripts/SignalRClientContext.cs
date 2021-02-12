using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using SigClient4;

namespace Assets.Scripts
{
    public class SignalRClientContext : IDisposable
    {
        public EventHandler<MessageArgs> ReceivedMessage = delegate { };
        private static HubConnection _hubConnection;
        private static IHubProxy _chatHub;
        private static IDisposable _messageProxy;
        private static SignalRClientContext _instance;

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
            _messageProxy = _chatHub.On<string, string>("SendMessageToClient", _instance.MessageFromServer);

            return _instance;
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
            _messageProxy.Dispose();
        }
    }
}