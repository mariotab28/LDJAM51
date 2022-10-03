using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToyBuilder : MonoBehaviour
{
    [SerializeField] GameObject spritePF;
    Transform headAnchor, bodyAnchor, rightArmAnchor, leftArmAnchor, legsAnchor;
    ToyPieceData headData, bodyData, rightArmData, leftArmData, legsData;

    private void Awake()
    {
        bodyAnchor = transform.GetChild(0);
        headAnchor = transform.GetChild(1);
        rightArmAnchor = transform.GetChild(2);
        leftArmAnchor = transform.GetChild(3);
        legsAnchor = transform.GetChild(4);
    }

    public void HandleDrop(ToyPieceConfiguration dropped)
    {
        switch (dropped.data.type)
        {
            case ToyPieceData.PieceType.BODY:
                HandleAnchorState(dropped.data, ref bodyData, bodyAnchor);
                break;
            case ToyPieceData.PieceType.HEAD:
                HandleAnchorState(dropped.data, ref headData, headAnchor);
                break;
            case ToyPieceData.PieceType.R_ARM:
                HandleAnchorState(dropped.data, ref rightArmData, rightArmAnchor);
                break;
            case ToyPieceData.PieceType.L_ARM:
                HandleAnchorState(dropped.data, ref leftArmData, leftArmAnchor);
                break;
            case ToyPieceData.PieceType.LEGS:
                HandleAnchorState(dropped.data, ref legsData, legsAnchor);
                break;
            default:
                break;
        }

        Destroy(dropped.gameObject);
    }

    private void HandleAnchorState(ToyPieceData droppedData, ref ToyPieceData anchorData, Transform anchor)
    {
        if (anchorData != null) DettachPiece(anchorData, anchor);
        AttachPiece(droppedData, anchor);
        anchorData = droppedData;
    }

    public void AttachPiece(ToyPieceData data, Transform anchor)
    {
        GameObject spriteGO = Instantiate(spritePF, anchor);
        SpriteRenderer renderer = spriteGO.GetComponent<SpriteRenderer>();
        renderer.sprite = data.sprite;
    }

    public void DettachPiece(ToyPieceData data, Transform anchor)
    {
        Destroy(anchor.GetChild(0).gameObject);
        GameManager.Instance.SpawnAtPosition(data, anchor.position);
    }

    public void ResetToy()
    {
        if (headAnchor.childCount > 0)
        {
            GameObject head = headAnchor.GetChild(0).gameObject;
            Destroy(head);
        }
        headData = null;

        if (bodyAnchor.childCount > 0)
        {
            GameObject body = bodyAnchor.GetChild(0).gameObject;
            Destroy(bodyAnchor.GetChild(0).gameObject);
        }
        bodyData = null;

        if (rightArmAnchor.childCount > 0)
        {
            GameObject rightArm = rightArmAnchor.GetChild(0).gameObject;
            Destroy(rightArmAnchor.GetChild(0).gameObject);
        }
        rightArmData = null;

        if (leftArmAnchor.childCount > 0)
        {
            GameObject leftArm = leftArmAnchor.GetChild(0).gameObject;
            Destroy(leftArmAnchor.GetChild(0).gameObject);
        }
        leftArmData = null;

        if (legsAnchor.childCount > 0)
        {
            GameObject legs = legsAnchor.GetChild(0).gameObject;
            Destroy(legsAnchor.GetChild(0).gameObject);
        }
        legsData = null;
    }
}
