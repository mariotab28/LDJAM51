using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public struct SavedToy
{
    public ToyPieceData body, head, rightArm, leftArm, legs;
    public Request request;
}

public class GameManager : MonoBehaviour
{
    [SerializeField] List<ToyPieceData> toyPieces;
    [SerializeField] Transform spawnContainer;
    [Header("Piece Prefabs")]
    [SerializeField] ToyPieceConfiguration bodyPF, headPF, rightArmPF, leftArmPF, legsPF;

    // Piece Database -----------------
    [Header("Piece Database Creation")]
    [SerializeField] SerializablePiece auxPiece;
    [SerializeField] List<SerializablePiece> pieceDatabase = new List<SerializablePiece>();

    // Logic -----------------
    public enum GameState
    {
        WAITING, PIECE_GENERATION, BUILDING, CLEANING, GAME_OVER
    }
    public GameState state = GameState.WAITING;

    // Triggers
    bool ready = false; // -> piece Generation
    bool pieceGenCompleted = false; // -> Building, start timer
    bool timeOut = false; // -> Cleaning / Game Over
    bool cleaningCompleted = false; // -> Piece Generation
    bool gameOver = false; // -> Stop game, show scores

    Request request;
    List<SavedToy> savedToys;

    [Header("Game Logic Configuration")]
    [SerializeField] int piecesPerSpawner = 10;
    [SerializeField] [Range(0, 1)] float timeBetweenPieceSpawn = 0;
    [SerializeField] List<Spawner> spawners;
    [Space]
    [SerializeField] int buildingSeconds = 10;
    [Space]
    [SerializeField] GameObject bottomBound;
    [SerializeField] float cleaningTime = 3;
    [SerializeField] ToyBuilder toyBuilder;

    [Header("Callbacks")]
    public UnityEvent onGenerationStart;

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

    #region Piece Spawning
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
    #endregion

    #region Loading Database

    public void LoadPiecesData()
    {
        Debug.Log("LOADING...");

        // Clear previous data
        pieceDatabase.Clear();
        toyPieces.Clear();

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
        data.tags = piece.tags.Split('_');

        // Increment cardIdCount
        //cardIdCount++;

        // Create new Asset file with this object
        string path = "Assets/ToyPieces/" + data.sprite.name + ".asset";
        AssetDatabase.CreateAsset(data, path);

        toyPieces.Add(data);
    }
    #endregion

    #region Logic

    public void SetReady(bool value)
    {
        ready = value;
    }

    private void Update()
    {
        HandleState();
    }

    public bool CompareGameState(GameState state)
    {
        return this.state == state;
    }

    // ====== WAITING ========
    void HandleWaiting()
    {
        if (ready)
        {
            // Generate new Request
            request = RequestManager.Instance.GetRandomRequest();

            // Start piece generation
            StartCoroutine(PieceGenerationRoutine());
            state = GameState.PIECE_GENERATION;
            onGenerationStart.Invoke();

            //ready = false;
        }
    }

    IEnumerator PieceGenerationRoutine()
    {

        for (int i = 0; i < piecesPerSpawner; i++)
        {
            foreach (var spawner in spawners)
                spawner.Spawn();

            yield return new WaitForSeconds(timeBetweenPieceSpawn);
        }

        pieceGenCompleted = true;
    }

    // =======================

    // ====== GENERATION ========
    void HandlePieceGeneration()
    {
        if (pieceGenCompleted)
        {
            Debug.Log("GENERATION FINISHED");
            state = GameState.BUILDING;
            pieceGenCompleted = false;

            // Start building timer
            StartCoroutine(BuildingTimer());
        }
    }

    // =======================

    // ====== BUILDING ========
    void HandleBuilding()
    {
        if (timeOut)
        {
            timeOut = false;
            Debug.Log("Building time finished!!!");

            // Check toy is complete
            if (!toyBuilder.IsCompleted())
            {
                state = GameState.GAME_OVER;
                gameOver = true;
            }
            /*else
            {*/
                // Save toy
                /*SavedToy toy = toyBuilder.GetToy();
                toy.request = request;
                savedToys.Add(toy);*/

                // Start cleaning
                state = GameState.CLEANING;
                StartCoroutine(CleaningRoutine());
            //}

            
        }
    }

    IEnumerator BuildingTimer()
    {
        int timer = buildingSeconds;

        while (timer > 0)
        {
            Debug.Log("BUILDING TIME: " + timer);
            timer--;
            yield return new WaitForSeconds(1);
        }
        timeOut = true;
    }

    // =======================

    // ====== CLEANING ========
    void HandleCleaning()
    {
        if (cleaningCompleted)
        {
            state = GameState.WAITING;
            cleaningCompleted = false;
        }
    }

    IEnumerator CleaningRoutine() 
    {
        bottomBound.SetActive(false);

        yield return new WaitForSeconds(cleaningTime);

        bottomBound.SetActive(true);
        cleaningCompleted = true;

        toyBuilder.ResetToy();
    }

    // =======================

    // ====== GAME OVER ========
    void HandleGameOver()
    {

    }

    // =======================

    void HandleState()
    {
        switch (state)
        {
            case GameState.WAITING:
                HandleWaiting();
                break;
            case GameState.PIECE_GENERATION:
                HandlePieceGeneration();
                break;
            case GameState.BUILDING:
                HandleBuilding();
                break;
            case GameState.CLEANING:
                HandleCleaning();
                break;
            case GameState.GAME_OVER:
                HandleGameOver();
                break;
            default:
                break;
        }
    }



    #endregion
}