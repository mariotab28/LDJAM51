using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SerializablePiece
{
    public int type;
    public string spriteName;
    public string tags;

    public SerializablePiece(SerializablePiece piece)
    {
        type = piece.type;
        spriteName = piece.spriteName;
        tags = piece.tags;
    }
}
