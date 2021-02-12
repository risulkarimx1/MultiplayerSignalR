// using SignalRClient;

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using SigClient4;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public EventHandler<MessageArgs> MessageRec = delegate { };

    private HubConnection _hubConnection;
    private IHubProxy _chatHub;

    void Start()
    {
        _hubConnection = new HubConnection("http://localhost:8080/chatHub");
        _chatHub = _hubConnection.CreateHubProxy("ChatHub");
        _chatHub.On<string, string>("SendMessageToClient", FoundMessage);
        _hubConnection.Start().ConfigureAwait(false);
        Console.WriteLine("Connection success!");
    }
        
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log($"here");
            SendMessageAsync("risul", "from unity");
        }
    }


    public Task SendMessageAsync(string name, string message)
    {
        return _chatHub.Invoke("SendMessage", new object[] { name, message });
    }

    private void FoundMessage(string name, string message)
    {
        Debug.Log($"Message says {name}, {message}");
        MessageRec.Invoke(this, new MessageArgs() { Message = message, Name = name });
    }

    private void OnDestroy()
    {
        _hubConnection.Dispose();
    }
}
