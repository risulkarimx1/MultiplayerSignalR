using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using SigClient4;

namespace Assets.Scripts
{
    public class SignalRClientContext : IDisposable
    {
        public static EventHandler<MessageArgs> ReceivedMessage = delegate { };
        private static HubConnection _hubConnection;
        private static IHubProxy _chatHub;
        private static IDisposable _messageProxy;
        private static SignalRClientContext _instance;
        private SignalRClientContext()
        {
            // _hubConnection = new HubConnection("http://localhost:8080/chatHub");
            // _chatHub = _hubConnection.CreateHubProxy("ChatHub");
            // _messageProxy = _chatHub.On<string, string>("SendMessageToClient", MessageFromServer);
            // _hubConnection.Start().ConfigureAwait(false);
        }

        public static async UniTask<SignalRClientContext> Create()
        {
            if (_instance != null) return _instance;
            _hubConnection = new HubConnection("http://localhost:8080/chatHub");
            _chatHub = _hubConnection.CreateHubProxy("ChatHub");
            _messageProxy = _chatHub.On<string, string>("SendMessageToClient", MessageFromServer);
            await _hubConnection.Start().ConfigureAwait(false);
            _instance = new SignalRClientContext();
            return _instance;
        }

        public Task SendMessageAsync(string name, string message)
        {
            return _chatHub.Invoke("SendMessage", new object[] {name, message});
        }

        private static void MessageFromServer(string name, string message)
        {
            ReceivedMessage.Invoke(_instance, new MessageArgs() {Message = message, Name = name});
        }

        public void Dispose()
        {
            _hubConnection.Dispose();
            _messageProxy.Dispose();
        }
    }
}