using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool IsDrag  { get; private set; }
    private Vector2 dragStartPoint;
    public Vector2 DeltaDrag { get { return (Vector2)transform.position - dragStartPoint; } }
    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStartPoint = transform.position;
        IsDrag = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDrag = false;
        dragStartPoint = transform.position;
    }

    void FixedUpdate()
    {
        if (IsDrag)
        {
            var mPos = InputController.GetMousePosition();
            if (mPos.HasValue)
            {
                transform.position = mPos.Value;
            }
        }
    }
}
