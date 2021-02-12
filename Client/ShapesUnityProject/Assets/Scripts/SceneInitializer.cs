using Cysharp.Threading.Tasks;
using SigClient4;
using UnityEngine;

namespace Assets.Scripts
{
    public class SceneInitializer : MonoBehaviour
    {
        private SignalRClientContext _clientContext;
        // Start is called before the first frame update
        async UniTask Start()
        {
            _clientContext = await SignalRClientContext.Create();
            SignalRClientContext.ReceivedMessage += OnMessageReceived;
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
            SignalRClientContext.ReceivedMessage -= OnMessageReceived;
            _clientContext.Dispose();
        }
    }
}
