using SigClient4;
using UnityEngine;

namespace Assets.Scripts
{
    public class SceneInitializer : MonoBehaviour
    {
        private SignalRClientContext _clientContext;
        // Start is called before the first frame update
        void Start()
        {
            _clientContext = new SignalRClientContext();
            _clientContext.ReceivedMessage += OnMessageReceived;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                _clientContext.SendMessageAsync("risul", "from unity");
            }
        }

        private void OnMessageReceived(object sender, MessageArgs e)
        {
            Debug.Log($"{e.Name} - {e.Message}");
        }

        private void OnDestroy()
        {
            _clientContext.ReceivedMessage -= OnMessageReceived;
            _clientContext.Dispose();
        }
    }
}
