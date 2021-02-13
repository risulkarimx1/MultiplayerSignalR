using System;
using Cysharp.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using UnityEngine;

namespace Assets.Scripts
{
    public class SignalRClientContext : IDisposable
    {
        public EventHandler<LockArgs> LockStateUpdated = delegate { };
        public EventHandler<PositionUpdateArgs> PositionUpdated = delegate { };
        private static HubConnection _hubConnection;
        private static IHubProxy _hub;
        private static IDisposable _lockStream;
        private static IDisposable _positionUpdateStream;
        private static SignalRClientContext _instance;

        private static bool _hasLock = false;

        private SignalRClientContext()
        {
        }

        public static SignalRClientContext Instance => _instance;

        public static async UniTask<SignalRClientContext> CreateInstance()
        {
            if (Instance != null) return Instance;
            _instance = new SignalRClientContext();
            _hubConnection = new HubConnection("http://localhost:8080/boards");
            _hub = _hubConnection.CreateHubProxy("BoardHub");
            await _hubConnection.Start().ConfigureAwait(false);
            _positionUpdateStream =
                _hub.On<float, float, float>("SendPositionFromServer", _instance.GetPositionFromServer);
            _lockStream = _hub.On<bool>("SendLockStatuesFromServer", _instance.GetLockStatusFromServer);
            return _instance;
        }

        public async UniTask TryLock()
        {
            if (_hasLock) return;

            await _hub.Invoke(nameof(TryLock)).ConfigureAwait(false);
        }

        private void GetLockStatusFromServer(bool acquiredLock)
        {
            _hasLock = acquiredLock;
            LockStateUpdated.Invoke(Instance, new LockArgs() {AcruiredLock = acquiredLock});
        }

        public async UniTask ReleaseLockToServer()
        {
            await _hub.Invoke(nameof(ReleaseLockToServer)).ConfigureAwait(false);
            _hasLock = false;
        }

        private void GetPositionFromServer(float x, float y, float z)
        {
            PositionUpdated.Invoke(Instance, new PositionUpdateArgs() {Position = new Vector3(x, y, z)});
        }

        public async UniTask SetPositionToServer(Vector3 vector3)
        {
            await _hub.Invoke(nameof(SetPositionToServer), new object[] {vector3.x, vector3.y, vector3.z})
                .ConfigureAwait(false);
        }

        public void Dispose()
        {
            _hubConnection.Dispose();
            _lockStream.Dispose();
        }
    }
}