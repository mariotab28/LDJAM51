using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] Transform spawnParent;
    private int cont = 0;

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            cont++;
            GameManager.Instance.SpawnRandom(spawnParent);
        }
    }
}
