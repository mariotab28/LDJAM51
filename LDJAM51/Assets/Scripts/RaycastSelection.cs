using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastSelection : MonoBehaviour
{
    Camera cam;

    Ray ray;
    RaycastHit hit;
    float maxDistance = 1000;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin,
                ray.direction * maxDistance, Color.green, 2f);

            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                if (hit.transform != null)
                {
                    Debug.Log("Click => " + hit.transform.name);
                }
            }
        }
    }
}
