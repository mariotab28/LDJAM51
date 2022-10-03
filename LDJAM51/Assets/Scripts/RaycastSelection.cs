using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastSelection : MonoBehaviour
{
    Camera cam;

    Ray ray;
    RaycastHit hit;
    float maxDistance = 1000;


    Vector3 pos;
    Vector3 v3, offset;
    float dist = 0;
    GameObject draggingGO;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        CheckClickOnObject();

        if(draggingGO != null) MoveObject();

        if (Input.GetMouseButtonUp(0)) draggingGO = null;
    }

    private void MoveObject()
    {
        pos = Input.mousePosition;
        v3 = new Vector3(pos.x, pos.y, dist);
        v3 = cam.ScreenToWorldPoint(v3);

        draggingGO.transform.position = v3 + offset;
    }

    void CheckClickOnObject()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pos = Input.mousePosition;

            ray = cam.ScreenPointToRay(pos);
            Debug.DrawRay(ray.origin,
                ray.direction * maxDistance, Color.green, 2f);

            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                draggingGO = hit.transform.gameObject;
                if (draggingGO)
                {
                    Debug.Log("Dragging => " + draggingGO.name);
                    // Apply distance
                    dist = draggingGO.transform.position.z - cam.transform.position.z;
                    v3 = new Vector3(pos.x, pos.y, dist);
                    v3 = cam.ScreenToWorldPoint(v3);
                    // Calculate offset
                    offset = draggingGO.transform.position - v3;
                }
            }
        }
    }
}
