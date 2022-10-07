using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public struct SavedToy
{
    public ToyPieceData body, head, rightArm, leftArm, legs;
    public Request request;
}

public class GameManager : MonoBehaviour
{
    [SerializeField] List<ToyPieceData> toyPieces;
    [SerializeField] List<LevelInfo> levels;
    [SerializeField] List<ToySet> sets;
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
    public int level = 0;
    int toyCount = 0;

    // Triggers
    bool ready = false; // -> piece Generation
    bool pieceGenCompleted = false; // -> Building, start timer
    bool timeOut = false; // -> Cleaning / Game Over
    bool cleaningCompleted = false; // -> Piece Generation
    bool gameOver = false; // -> Stop game, show scores

    Request request;
    List<SavedToy> savedToys = new List<SavedToy>();
    List<ToyPieceData> pieceGenList = new List<ToyPieceData>();

    [Header("Game Logic Configuration")]
    [SerializeField] int maxPieces = 10;
    [SerializeField] [Range(0, 1)] float timeBetweenPieceSpawn = 0;
    [SerializeField] [Range(0, 10)] float totalSpawnTime = 2;
    [SerializeField] List<Spawner> spawners;
    [Space]
    [SerializeField] RequestPanelConfiguration requestPanel;
    [SerializeField] int buildingSeconds = 10;
    [Space]
    [SerializeField] GameObject bottomBound;
    [SerializeField] float cleaningTime = 3;
    [SerializeField] ToyBuilder toyBuilder;

    [Header("Callbacks")]
    public UnityEvent onGenerationStart;
    public UnityEvent onBuildStart;
    public UnityEvent onBuildFinished;
    public UnityEvent onBuildSuccess;
    public UnityEvent onGameOver;
    public UnityEvent onEndGameTransition;
    public UnityEvent onEndGameTransitionEnd;

    [Header("Score Menu Objects")]
    [SerializeField] private GameObject scoreMenuPanels;
    [SerializeField] private TMPro.TMP_Text kidNumText;
    [SerializeField] private TMPro.TMP_Text kidNameText;
    [SerializeField] private TMPro.TMP_Text likesText;
    [SerializeField] private TMPro.TMP_Text dislikesText;
    [SerializeField] private TMPro.TMP_Text satisfiedCountText;
    [SerializeField] private Animator satisfiedAnim;
    [SerializeField] private Animator unhappyAnim;
    [SerializeField] private GameObject retryButton;

    [Header("Sounds")]
    [SerializeField] AudioClip[] tickTack;
    [SerializeField] AudioClip stampSound;

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
#if UNITY_EDITOR
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
#endif
    }
    #endregion

    #region Logic

    public void SetReady(bool value)
    {
        ready = value;
    }

    public Request GetRequest()
    {
        return request;
    }

    public int GetToyCount()
    {
        return toyCount;
    }

    public int GetSetIndexFromCategory(string category)
    {
        int i = 0;
        bool found = false;

        while (i < sets.Count && !found)
        {
            foreach (var tag in sets[i].toyPieces[0].tags)
                if (tag == category)
                    found = true;

            if (!found) i++;
        }

        return i;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            return;
        }
        HandleState();
    }

    public bool CompareGameState(GameState state)
    {
        return this.state == state;
    }

    public void ResetScene()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // ====== WAITING ========
    void HandleWaiting()
    {
        if (ready)
        {
            // Load level info
            if (level < levels.Count)
            {
                LevelInfo info = levels[level];
                level++;

                request = info.request;

                // Build gen list
                BuildPieceList(info.includedSets, info.maxPieces);
            }
            else
            {
                // Generate new Request
                request = RequestManager.Instance.GetRandomRequest();

                // Build gen list
                BuildRandomPieceList(sets[request.mandatorySet], maxPieces);
            }

            

            // Start piece generation
            StartCoroutine(PieceGenerationRoutine());
            state = GameState.PIECE_GENERATION;
            onGenerationStart.Invoke();

            //ready = false;
        }
    }

    IEnumerator PieceGenerationRoutine()
    {
        requestPanel.gameObject.SetActive(true);
        requestPanel.Configure();
        yield return new WaitForSeconds(1);

        timeBetweenPieceSpawn = totalSpawnTime / pieceGenList.Count;
        int i = 0;
        while (i < pieceGenList.Count)
        {
            spawners[0].Spawn(pieceGenList[i]);
            i++;
            if (i < pieceGenList.Count)
            { spawners[1].Spawn(pieceGenList[i]); i++; }

            yield return new WaitForSeconds(timeBetweenPieceSpawn);
        }
        /*
        for (int i = 0; i < piecesPerSpawner; i++)
        {
            foreach (var spawner in spawners)
                spawner.Spawn();

            yield return new WaitForSeconds(timeBetweenPieceSpawn);
        }
        */
        requestPanel.gameObject.SetActive(false);
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
            onBuildStart.Invoke();

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
            onBuildFinished.Invoke();

            // Check toy is complete
            if (!toyBuilder.IsCompleted())
            {
                state = GameState.GAME_OVER;
                gameOver = true;
                onGameOver.Invoke();
                ResetGame();

                StartCoroutine(ScoreScreenRoutine());
            }
            else
            {
                // Save toy
                SavedToy toy = toyBuilder.GetToy();
                toy.request = request;
                savedToys.Add(toy);
                toyCount++;

                onBuildSuccess.Invoke();
                // Start cleaning
                state = GameState.CLEANING;
                StartCoroutine(CleaningRoutine());

                // Increase difficulty
                if (level >= levels.Count)
                {
                    maxPieces++;
                }
            }

            
        }
    }

    IEnumerator BuildingTimer()
    {
        int timer = buildingSeconds;

        while (timer > 0)
        {
            //Debug.Log("BUILDING TIME: " + timer);
            int tickClipIndex = timer % 2 == 0 ? 0 : 1;
            SoundManager.Instance.Play(tickTack[tickClipIndex]);
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

    void ResetGame()
    {
        level = 0;
        toyCount = 0;
    }

    IEnumerator ShowSavedPieceCoroutine(ToyPieceData piece)
    {
        float pTime = 0.15f;
        toyBuilder.AttachPiece(piece);
        yield return new WaitForSeconds(pTime);
    }

    private void ApplyTagsToScore(string[] tags, Request r, ref int score)
    {
        string likes = r.likes;
        string dislikes = r.dislikes;
        foreach (var tag in tags)
            if (tag.ToLower() == likes.ToLower()) score++;
            else if (tag.ToLower() == dislikes.ToLower()) score--;
    }

    private int CalculateScore(Request r, SavedToy toy)
    {
        int score = 0;

        ApplyTagsToScore(toy.head.tags, r, ref score);
        ApplyTagsToScore(toy.body.tags, r, ref score);
        ApplyTagsToScore(toy.rightArm.tags, r, ref score);
        ApplyTagsToScore(toy.leftArm.tags, r, ref score);
        ApplyTagsToScore(toy.legs.tags, r, ref score);

        return score;
    }

    IEnumerator ScoreScreenRoutine()
    {
        yield return new WaitForSeconds(2); // Wait for fired animation
        onEndGameTransition.Invoke();

        // Clean
        bottomBound.SetActive(false);
        toyBuilder.ResetToy();

        yield return new WaitForSeconds(cleaningTime);
        onEndGameTransitionEnd.Invoke();
        SoundManager.Instance.PlayMenuMusic();

        bottomBound.SetActive(true);
        cleaningCompleted = true;

        // Show Saved Toys Routine:
        // Activate panels
        scoreMenuPanels.SetActive(true);
        int toyCont = 1;
        int satisfied = 0;

        foreach (var toy in savedToys)
        {
            // ------------------- Request
            kidNumText.text = "kid num. " + toyCont;
            // Show piece -> wait
            yield return new WaitForSeconds(0.15f);
            yield return ShowSavedPieceCoroutine(toy.body);
            yield return ShowSavedPieceCoroutine(toy.head);
            yield return ShowSavedPieceCoroutine(toy.leftArm);
            yield return ShowSavedPieceCoroutine(toy.rightArm);
            yield return ShowSavedPieceCoroutine(toy.legs);

            // Show Request info
            Request r = toy.request;
            kidNameText.text = "kid's name: " + r.name;
            likesText.text = "likes: " + r.likes;
            dislikesText.text = "dislikes: " + r.dislikes;

            // Calculate satisfaction
            int score = CalculateScore(r, toy);

            // Show satisfaction graphic
            if (score > 2)
            {
                satisfied++;
                // show satisfied
                satisfiedAnim.gameObject.SetActive(true);
                satisfiedAnim.Play("IndicatorStamp");
                
                // Update satisfied count
                satisfiedCountText.text = "satisfied kids: " + satisfied;
            }
            else
            {
                // show unsatisfied
                unhappyAnim.gameObject.SetActive(true);
                unhappyAnim.Play("IndicatorStamp");
            }
            SoundManager.Instance.Play(stampSound);


            // Wait->Clean request
            yield return new WaitForSeconds(3);
            kidNumText.text = "kid num. ";
            kidNameText.text = "kid's name: ";
            likesText.text = "likes: ";
            dislikesText.text = "dislikes: ";
            toyBuilder.ResetToy();
            satisfiedAnim.gameObject.SetActive(false);
            unhappyAnim.gameObject.SetActive(false);

            // ------------------- Request
            toyCont++;
        }


        // Satisfied count bounce anim

        // Show retry button
        retryButton.SetActive(true);

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

    private void BuildPieceList(List<ToySet> sets, int maxPieces)
    {
        // clean gen list
        pieceGenList.Clear();

        foreach (var set in sets)
            foreach (var piece in set.toyPieces)
                pieceGenList.Add(piece);

        if (pieceGenList.Count < maxPieces)
            for (int i = pieceGenList.Count; i < maxPieces; i++)
                pieceGenList.Add(GetRandomPiece());

        // Shuffle list
        Shuffle(ref pieceGenList);
    }

    private void BuildRandomPieceList(ToySet mandatorySet, int maxPieces)
    {
        // clean gen list
        pieceGenList.Clear();

        foreach (var piece in mandatorySet.toyPieces)
            pieceGenList.Add(piece);

        for (int i = pieceGenList.Count; i < maxPieces; i++)
            pieceGenList.Add(GetRandomPiece());
    }


    public static void Shuffle<T>(ref List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    #endregion
}