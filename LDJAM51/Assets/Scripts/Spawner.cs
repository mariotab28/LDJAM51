using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] Transform spawnParent;
    [SerializeField] AudioClip spawnClip;
    private int cont = 0;

    /*
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            cont++;
            GameManager.Instance.SpawnRandom(spawnParent);
        }
    }
    */

    public void Spawn()
    {
        GameManager.Instance.SpawnRandom(spawnParent);
        SoundManager.Instance.Play(spawnClip);
    }

    public void Spawn(ToyPieceData piece)
    {
        GameManager.Instance.Spawn(piece, spawnParent);
        SoundManager.Instance.Play(spawnClip);
    }
}
