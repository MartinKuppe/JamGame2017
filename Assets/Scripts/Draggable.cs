using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour
{
    public static GameObject DraggedInstance;

    Vector3 _startPosition;
    Vector3 _offsetToMouse;
    float _zDistanceToCamera;
    private bool _isDragged = false;

    #region Interface Implementations

    private void Update()
    {
    }

    public void OnMouseDown()
    {
        DraggedInstance = gameObject;
        _startPosition = transform.position;
        _zDistanceToCamera = Mathf.Abs(_startPosition.z - Camera.main.transform.position.z);

        _offsetToMouse = _startPosition - Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, _zDistanceToCamera)
        );
        _isDragged = true;
    }

    public void OnMouseDrag()
    {
        if (Input.touchCount > 1)
            return;

        transform.position = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, _zDistanceToCamera)
            ) + _offsetToMouse;
    }

    public void OnMouseUp()
    {
        DraggedInstance = null;
        _offsetToMouse = Vector3.zero;
        _isDragged = false;
    }

    #endregion
}