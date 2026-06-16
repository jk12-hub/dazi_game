using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 游戏主管理器 — 状态机 + 协调各系统
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, GameOver, Paused }

    [Header("全局引用")]
    public GameData gameData;
    public TypingEngine typingEngine;
    public PlayerController player;
    public MonsterAI monster;
    public DistanceManager distanceManager;
    public UIManager uiManager;
    public FeedbackSystem feedbackSystem;

    [Header("场景引用")]
    public Camera mainCamera;
    public Transform worldRoot;
    private Transform cachedBgTransform; // 缓存避免每帧 Find

    [Header("状态")]
    [SerializeField] private GameState state = GameState.Playing;
    [SerializeField] private float elapsedGameTime = 0f;

    public GameState State => state;
    public float ElapsedTime => elapsedGameTime;

    [HideInInspector] public UnityEvent onGameOver;
    [HideInInspector] public UnityEvent onWin;

    void Start()
    {
        // 自动查找引用
        if (gameData == null) gameData = Resources.Load<GameData>("GameData/DefaultGameConfig");
        if (typingEngine == null) typingEngine = FindAnyObjectByType<TypingEngine>();
        if (player == null) player = FindAnyObjectByType<PlayerController>();
        if (monster == null) monster = FindAnyObjectByType<MonsterAI>();
        if (distanceManager == null) distanceManager = FindAnyObjectByType<DistanceManager>();
        if (uiManager == null) uiManager = FindAnyObjectByType<UIManager>();
        if (feedbackSystem == null) feedbackSystem = FindAnyObjectByType<FeedbackSystem>();
        if (mainCamera == null) mainCamera = Camera.main;

        // 程序化生成 Sprite 并应用到场景对象
        SetupSprites();

        // 缓存背景引用（避免每帧 Find）
        var bgObj = GameObject.Find("Background");
        if (bgObj != null) cachedBgTransform = bgObj.transform;

        StartGame();
    }

    void SetupSprites()
    {
        // 生成像素美术
        var playerSpr = SpriteGenerator.CreatePlayerSprite();
        var monsterSpr = SpriteGenerator.CreateMonsterSprite();
        var groundSpr = SpriteGenerator.CreateGroundSprite();
        var bgSpr = SpriteGenerator.CreateBackgroundSprite();

        // 应用到场景对象
        if (player != null)
        {
            var psr = player.GetComponent<SpriteRenderer>();
            if (psr != null) psr.sprite = playerSpr;
        }

        if (monster != null)
        {
            var msr = monster.GetComponent<SpriteRenderer>();
            if (msr != null) msr.sprite = monsterSpr;
        }

        // 查找并设置地面
        var groundObj = GameObject.Find("Ground");
        if (groundObj != null)
        {
            var gsr = groundObj.GetComponent<SpriteRenderer>();
            if (gsr != null) gsr.sprite = groundSpr;
        }

        // 查找并设置背景
        var bgObj = GameObject.Find("Background");
        if (bgObj != null)
        {
            var bgsr = bgObj.GetComponent<SpriteRenderer>();
            if (bgsr != null) bgsr.sprite = bgSpr;
        }

        // 查找并设置暗角（使用 UI Image，不需要 sprite）
        // Vignette 由 FeedbackSystem 通过 Image.color 控制
    }

    void Update()
    {
        if (state != GameState.Playing) return;

        elapsedGameTime += Time.deltaTime;

        // 检测游戏结束条件
        CheckGameOver();

        // 相机跟随玩家（平滑）
        UpdateCamera();

        // 背景视差滚动
        UpdateBackgroundParallax();
    }

    void CheckGameOver()
    {
        // 条件 1: 怪物追上玩家
        if (distanceManager != null && distanceManager.Distance <= 0)
        {
            GameOver(false);
            return;
        }

        // 条件 2: 到达终点（时间耗尽）
        if (gameData != null && gameData.totalGameTime > 0 && elapsedGameTime >= gameData.totalGameTime)
        {
            GameOver(true);
            return;
        }
    }

    void UpdateCamera()
    {
        if (mainCamera == null || player == null) return;

        float playerX = player.WorldX;
        float camZ = mainCamera.transform.position.z;
        Vector3 targetPos = new Vector3(playerX + 3f, 0, camZ);
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPos,
            Time.deltaTime * 3f
        );
    }

    void UpdateBackgroundParallax()
    {
        if (mainCamera == null || cachedBgTransform == null) return;

        float camX = mainCamera.transform.position.x;
        cachedBgTransform.position = new Vector3(camX * 0.3f, 1f, 5f);
    }
    }

    void StartGame()
    {
        state = GameState.Playing;
        elapsedGameTime = 0;

        typingEngine?.Reset();
        player?.Reset();
        monster?.Reset();
        feedbackSystem?.ResetAll();
    }

    void GameOver(bool survived)
    {
        state = GameState.GameOver;

        if (survived)
            onWin?.Invoke();
        else
            onGameOver?.Invoke();

        uiManager?.ShowGameOver(survived);

        Debug.Log(survived
            ? "🎉 玩家成功逃脱！坚持了整局！"
            : $"💀 被怪物抓住！存活了 {elapsedGameTime:F1} 秒");
    }

    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void Pause()
    {
        if (state == GameState.Playing)
        {
            state = GameState.Paused;
            Time.timeScale = 0;
        }
    }

    public void Resume()
    {
        if (state == GameState.Paused)
        {
            state = GameState.Playing;
            Time.timeScale = 1;
        }
    }
}
