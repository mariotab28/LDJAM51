using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToyPieceConfiguration : MonoBehaviour
{
    public ToyPieceData data;
    SpriteRenderer spriteR;

    private void Awake()
    {
        spriteR = GetComponent<SpriteRenderer>();
    }

    public void Init(ToyPieceData data)
    {
        this.data = data;

        spriteR.sprite = data.sprite;
    }
}
