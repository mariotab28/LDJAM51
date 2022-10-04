using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToyCountText : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text countText;

    public void UpdateText()
    {
        countText.text = GameManager.Instance.GetToyCount() + " TOYS CREATED";
    }
}
