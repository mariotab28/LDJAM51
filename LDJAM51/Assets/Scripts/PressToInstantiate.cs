using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressToInstantiate : MonoBehaviour
{
    [SerializeField] Transform spawnParent;
    [SerializeField] List<GameObject> objectsToSpawn;

    private int cont = 0;

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            cont++;
            int id = Random.Range(0, objectsToSpawn.Count);
            GameObject go = Instantiate(objectsToSpawn[id], spawnParent);
            go.name = go.name + "_" + cont;
        }
    }
}
