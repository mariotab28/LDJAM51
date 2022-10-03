using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]List<ToyPieceData> toyPieces;
    [SerializeField] Transform spawnContainer;
    [Header("Piece Prefabs")]
    [SerializeField] ToyPieceConfiguration bodyPF, headPF, rightArmPF, leftArmPF, legsPF;

    // Piece Database
    [SerializeField] SerializablePiece auxPiece;
    [SerializeField] List<SerializablePiece> pieceDatabase = new List<SerializablePiece>();

    #region Singleton declaration
    public static GameManager Instance { get; private set; }
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

    public ToyPieceData GetRandomPiece()
    {
        return toyPieces[Random.Range(0, toyPieces.Count)];
    }

    public ToyPieceConfiguration Spawn(ToyPieceData pieceData, Transform spawnParent)
    {
        ToyPieceConfiguration toyPiece = null;
        switch (pieceData.type)
        {
            case ToyPieceData.PieceType.BODY:
                toyPiece = Instantiate(bodyPF, spawnParent);
                break;
            case ToyPieceData.PieceType.HEAD:
                toyPiece = Instantiate(headPF, spawnParent);
                break;
            case ToyPieceData.PieceType.R_ARM:
                toyPiece = Instantiate(rightArmPF, spawnParent);
                break;
            case ToyPieceData.PieceType.L_ARM:
                toyPiece = Instantiate(leftArmPF, spawnParent);
                break;
            case ToyPieceData.PieceType.LEGS:
                toyPiece = Instantiate(legsPF, spawnParent);
                break;
            default:
                break;
        }
        if (toyPiece) toyPiece.Init(pieceData);

        return toyPiece;
    }

    public ToyPieceConfiguration SpawnRandom(Transform spawnParent)
    {
        ToyPieceData pieceData = GetRandomPiece();
        return Spawn(pieceData, spawnParent);
    }

    public ToyPieceConfiguration SpawnAtPosition(ToyPieceData pieceData, Vector3 position)
    {
        ToyPieceConfiguration toyPiece = Spawn(pieceData, spawnContainer);
        toyPiece.transform.position = position;

        return toyPiece;
    }

    #region Loading Database

    public void LoadPiecesData()
    {
        Debug.Log("LOADING...");

        // Clear previous data
        pieceDatabase.Clear();

        // Read CSV file
        List<Dictionary<string, object>> data = CSVReader.Read("PieceDatabase");
        // Load data from each card in CSV file
        for (int i = 0; i < data.Count; i++)
        {
            // Load auxCard with the data
            auxPiece.type = int.Parse(data[i]["Type"].ToString(), System.Globalization.NumberStyles.Integer); ;
            auxPiece.spriteName = data[i]["Sprite"].ToString();
            auxPiece.tags = data[i]["Tags"].ToString();

            // Add the card to database
            AddPiece(auxPiece);
        }

        Debug.Log("Finished loading card database.");
    }

    private void AddPiece(SerializablePiece piece)
    {
        SerializablePiece newPiece = new SerializablePiece(piece);

        pieceDatabase.Add(newPiece);

        // Create Scriptable Object
        AddScriptableObject(newPiece);
    }

    private void AddScriptableObject(SerializablePiece piece)
    {
        ToyPieceData data = ScriptableObject.CreateInstance<ToyPieceData>();

        // Overwrite data with data from the serialized card
        data.type = (ToyPieceData.PieceType)piece.type;
        data.sprite = Resources.Load<Sprite>("Piece Sprites/" + piece.spriteName);
        data.tags = piece.tags.Split('0');

        // Increment cardIdCount
        //cardIdCount++;

        // Create new Asset file with this object
        string path = "Assets/ToyPieces/" + data.name + ".asset";
        AssetDatabase.CreateAsset(data, path);
    }
    #endregion
}