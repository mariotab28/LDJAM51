using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestPanelConfiguration : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text nameText;
    [SerializeField] TMPro.TMP_Text likesText;
    [SerializeField] TMPro.TMP_Text dislikesText;

    public void Configure()
    {
        Request r = GameManager.Instance.GetRequest();
        nameText.text = "KID'S NAME: " + r.name;
        likesText.text = "LIKES: " + r.likes;
        dislikesText.text = "DISLIKES: " + r.dislikes;
    }
}
