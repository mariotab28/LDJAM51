using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Request
{
    public string name;
    public string likes;
    public string dislikes;
    public int mandatorySet;

}


public class RequestManager : MonoBehaviour
{
    [SerializeField] List<string> names;
    [Header("Category configuration")]
    [SerializeField] List<string> categories;
    [SerializeField] int numOfSets;

    #region Singleton declaration
    public static RequestManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    #endregion

    public Request GetRandomRequest()
    {
        Request r = new Request();
        r.name = names[Random.Range(0, names.Count)];
        r.likes = categories[Random.Range(0, categories.Count)];
        List<string> aux = new List<string>(categories);
        aux.Remove(r.likes);
        r.dislikes = aux[Random.Range(0, aux.Count)];
        int index = categories.IndexOf(r.likes);
        r.mandatorySet = Random.Range(0, numOfSets);

        return r;
    }
}
