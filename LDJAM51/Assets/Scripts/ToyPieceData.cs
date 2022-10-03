using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ToyPiece", menuName = "ToyPiece")]
public class ToyPieceData : ScriptableObject
{
    public enum PieceType
    {
        BODY, 
        HEAD, 
        R_ARM, 
        L_ARM, 
        LEGS
    }

    public PieceType type;
    public Sprite sprite;
    public string[] tags;
}
