using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using SigClient4;

namespace Assets.Scripts
{
    public class SignalRClientContext : IDisposable
    {
        public EventHandler<MessageArgs> ReceivedMessage = delegate { };
        private HubConnection _hubConnection;
        private IHubProxy _chatHub;
        private IDisposable _messageProxy;

        public SignalRClientContext()
        {
            _hubConnection = new HubConnection("http://localhost:8080/chatHub");
            _chatHub = _hubConnection.CreateHubProxy("ChatHub");
            _messageProxy = _chatHub.On<string, string>("SendMessageToClient", MessageFromServer);
            _hubConnection.Start().ConfigureAwait(false);
        }

        public Task SendMessageAsync(string name, string message)
        {
            return _chatHub.Invoke("SendMessage", new object[] {name, message});
        }

        private void MessageFromServer(string name, string message)
        {
            ReceivedMessage.Invoke(this, new MessageArgs() {Message = message, Name = name});
        }

        public void Dispose()
        {
            _hubConnection.Dispose();
            _messageProxy.Dispose();
        }
    }
}