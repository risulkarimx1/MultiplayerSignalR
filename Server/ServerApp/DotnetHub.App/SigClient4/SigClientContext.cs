using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace SigClient4
{
    public class MessageArgs : EventArgs
    {
        public string Name { get; set; }
        public string Message { get; set; }
    }
    
    public class SigClientContext : IDisposable
    {
        public EventHandler<MessageArgs> MessageRec = delegate{}; 
        
        private HubConnection _hubConnection;
        private IHubProxy _chatHub;

        public async Task CreateConnection()
        {
            _hubConnection = new HubConnection("http://localhost:8080/chatHub");
            _chatHub = _hubConnection.CreateHubProxy("ChatHub");
            _chatHub.On<string, string>("SendMessageToClient", FoundMessage);
            await _hubConnection.Start().ConfigureAwait(false);
            Console.WriteLine("Connection success!");
        }

        public Task SendMessageAsync(string name, string message)
        {
            return _chatHub.Invoke("SendMessage", new object[] {name, message});
        }

        private void FoundMessage(string name, string message)
        {
            Console.WriteLine($"Message says {name}, {message}");
            MessageRec.Invoke(this, new MessageArgs(){Message = message, Name = name });
        }

        public void Dispose()
        {
            _hubConnection.Dispose();
        }
    }
}