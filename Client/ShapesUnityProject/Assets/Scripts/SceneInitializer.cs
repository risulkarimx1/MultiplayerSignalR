using System;
using Cysharp.Threading.Tasks;
using SigClient4;
using UnityEngine;

namespace Assets.Scripts
{
    public class SceneInitializer : MonoBehaviour
    {
        private SignalRClientContext _clientContext;
        // Start is called before the first frame update
        async UniTask Awake()
        {
            _clientContext = await SignalRClientContext.CreateInstance();
        }

        private void Start()
        {
            SignalRClientContext.Instance.ReceivedMessage += OnMessageReceived;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                _clientContext.SendMessageAsync("risul", "from unity");
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                _clientContext.RequestToLock();
            }
        }

        private void OnMessageReceived(object sender, MessageArgs e)
        {
            Debug.Log($"{e.Name} - {e.Message}");
        }

        private void OnDestroy()
        {
            SignalRClientContext.Instance.ReceivedMessage -= OnMessageReceived;
            _clientContext.Dispose();
        }
    }
}
