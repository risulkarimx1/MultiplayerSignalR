using Cysharp.Threading.Tasks;
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


        private void OnDestroy()
        {
            _clientContext.Dispose();
        }
    }
}