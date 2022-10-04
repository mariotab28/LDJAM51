using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelInfo", menuName = "LevelInfo")]
public class LevelInfo : ScriptableObject
{
    public Request request;
    public List<ToySet> includedSets;
    public int maxPieces;
}
