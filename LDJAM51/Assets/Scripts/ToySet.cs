using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ToySet", menuName = "ToySet")]
public class ToySet : ScriptableObject
{
    public List<ToyPieceData> toyPieces;
}
