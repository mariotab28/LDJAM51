using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragObject : MonoBehaviour
{
    private Vector3 mousePosition;
    private Vector3 offsetPosition;
    private Vector3 prevDragPosition;
    private float zCoord;

    public void OnMouseDown()
    {
        zCoord = transform.position.z;
        prevDragPosition = GetMouseWorldPos();
        offsetPosition = transform.position - prevDragPosition;

        //GameManager.Instance.SetDragging(this);
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;

        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private void OnMouseDrag()
    {
        Vector3 movePos = GetMouseWorldPos() + offsetPosition;
        transform.position = new Vector3(movePos.x, movePos.y, 4);
    }

    private void OnMouseUp()
    {
        DragRelease();

        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        Collider2D[] overlap = Physics2D.OverlapAreaAll(col.bounds.min, col.bounds.max);
        foreach (var item in overlap)
        {
            ToyBuilder drop = item.GetComponent<ToyBuilder>();
            if (drop) drop.HandleDrop(GetComponent<ToyPieceConfiguration>());
        }
    }

    public void DragRelease()
    {
        //GameManager.Instance.SetDragging(null);
    }
}
