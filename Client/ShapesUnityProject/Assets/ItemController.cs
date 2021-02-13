using System;
using Assets.Scripts;
using UniRx;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    private Vector3 _screenPoint;
    private Vector3 _offset;

    private bool _canMove = true;
    private IDisposable _positionChangeStream;
    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
        SignalRClientContext.Instance.PositionUpdated += OnPositionUpdated;
        SignalRClientContext.Instance.LockStateUpdated += OnLockStateUpdated;
    }

    private void OnPositionUpdated(object sender, PositionUpdateArgs e)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => { transform.position = e.Position; });
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            _ = SignalRClientContext.Instance.TryLock();
        }

        if (Input.GetMouseButtonUp(0))
        {
            _ = SignalRClientContext.Instance.ReleaseLockToServer();
        }
    }

    private void OnLockStateUpdated(object sender, LockArgs e)
    {
        Debug.Log($"got lock {e.AcruiredLock}");
        _canMove = e.AcruiredLock;
        if (_canMove)
        {
            OnMouseDown();
            _positionChangeStream?.Dispose();
            _positionChangeStream = transform.ObserveEveryValueChanged(t => t.position)
                .Subscribe(async p => { await SignalRClientContext.Instance.SetPositionToServer(p); }).AddTo(this);
        }
        else
        {
            _positionChangeStream?.Dispose();
        }
    }


    void OnMouseDown()
    {
        if (_canMove == false) return;

        var position = transform.position;
        _screenPoint = _mainCamera.WorldToScreenPoint(position);
        _offset = position - _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _screenPoint.z));
    }

    void OnMouseDrag()
    {
        if (_canMove == false) return;

        var cursorPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, _screenPoint.z);
        var cursorPosition = _mainCamera.ScreenToWorldPoint(cursorPoint) + _offset;
        transform.position = cursorPosition;
        _ =SignalRClientContext.Instance.SetPositionToServer(transform.position);
    }

    private void OnDestroy()
    {
        SignalRClientContext.Instance.PositionUpdated -= OnPositionUpdated;
        SignalRClientContext.Instance.LockStateUpdated -= OnLockStateUpdated;
    }
}