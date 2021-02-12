using System;
using Assets.Scripts;
using UniRx;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    private Vector3 screenPoint;
    private Vector3 offset;
    private bool _canMove = true;
    // Start is called before the first frame update

    private IDisposable positionChange;

    private void Start()
    {
        SignalRClientContext.Instance.PositionUpdated += OnPositionUpdated;
        SignalRClientContext.Instance.LockStateUpdated += OnLockStateUpdated;
    }

    private void OnPositionUpdated(object sender, PositionUpdateArgs e)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(()=>
        {
            transform.position = e.Position;
        });
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            SignalRClientContext.Instance.RequestToLock();
        }

        if (Input.GetMouseButtonUp(0))
        {
            SignalRClientContext.Instance.ReleaseLock();
        }
    }

    private void OnLockStateUpdated(object sender, LockArgs e)
    {
        Debug.Log($"got lock {e.AcruiredLock}");
        _canMove = e.AcruiredLock;
        if (_canMove)
        {
            OnMouseDown();
            positionChange  = transform.ObserveEveryValueChanged(x => x.position).Subscribe(p =>
            {
                SignalRClientContext.Instance.UpdatePositionInServer(p);
            }).AddTo(this);
            SignalRClientContext.Instance.LockStateUpdated += OnLockStateUpdated;
        }
        else
        {
            positionChange?.Dispose();
        }
    }


    void OnMouseDown()
    {
        if (_canMove == false) return;
        
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }

    void OnMouseDrag()
    {
        if(_canMove == false) return;
        
        Vector3 cursorPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint) + offset;
        transform.position = cursorPosition;
        SignalRClientContext.Instance.UpdatePositionInServer(transform.position);
    }

    private void OnDestroy()
    {
        SignalRClientContext.Instance.PositionUpdated -= OnPositionUpdated;
        SignalRClientContext.Instance.LockStateUpdated -= OnLockStateUpdated;
    }
}
