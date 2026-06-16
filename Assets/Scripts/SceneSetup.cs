using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 场景自动构建器 — 无需手动搭建任何东西，Play 即玩
/// RuntimeInitializeOnLoadMethod 确保在任何场景都会自动初始化
/// </summary>
public static class SceneAutoBuilder
{
    private static bool _built = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnSceneLoaded()
    {
        if (_built) return;
        _built = true;

        Debug.Log("[SceneAutoBuilder] 开始自动构建游戏场景...");

        // 清理场景中已有的默认物体（保留 Camera）
        var existingCam = Camera.main;

        // ===== 1. EventSystem =====
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // ===== 2. Main Camera =====
        var mainCam = existingCam;
        if (mainCam == null)
        {
            var camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            mainCam.tag = "MainCamera";
        }
        mainCam.orthographic = true;
        mainCam.orthographicSize = 7;
        mainCam.backgroundColor = new Color(0.08f, 0.04f, 0.18f);
        mainCam.transform.position = new Vector3(0, 0, -10);

        // ===== 3. Game World =====
        var worldRoot = new GameObject("GameWorld");

        // Background
        var bg = new GameObject("Background");
        bg.transform.SetParent(worldRoot.transform);
        var bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.sortingOrder = -100;
        bg.transform.position = new Vector3(0, 1, 5);
        bg.transform.localScale = new Vector3(12, 6, 1);

        // Ground
        var ground = new GameObject("Ground");
        ground.transform.SetParent(worldRoot.transform);
        var gSr = ground.AddComponent<SpriteRenderer>();
        gSr.sortingOrder = -10;
        gSr.drawMode = SpriteDrawMode.Tiled;
        gSr.size = new Vector2(40, 1);
        ground.transform.position = new Vector3(0, -3.5f, 0);
        ground.transform.localScale = Vector3.one;

        // ===== 4. Player =====
        var playerObj = new GameObject("Player");
        var psr = playerObj.AddComponent<SpriteRenderer>();
        psr.sortingOrder = 10;
        playerObj.transform.position = new Vector3(0, -2f, 0);
        var player = playerObj.AddComponent<PlayerController>();

        // ===== 5. Monster =====
        var monsterObj = new GameObject("Monster");
        var msr = monsterObj.AddComponent<SpriteRenderer>();
        msr.sortingOrder = 9;
        monsterObj.transform.position = new Vector3(10, -2.3f, 0);
        var monster = monsterObj.AddComponent<MonsterAI>();

        // ===== 6. Managers =====
        var dmObj = new GameObject("DistanceManager");
        var dm = dmObj.AddComponent<DistanceManager>();
        dm.player = player;
        dm.monster = monster;

        var teObj = new GameObject("TypingEngine");
        var te = teObj.AddComponent<TypingEngine>();

        var fbObj = new GameObject("FeedbackSystem");
        var fb = fbObj.AddComponent<FeedbackSystem>();

        // ===== 7. Canvas (UI) =====
        var canvasObj = new GameObject("GameCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // TopBar
        var topBar = MakeUI("TopBar", canvasObj.transform,
            new Vector2(0, 0.92f), new Vector2(1, 1), new Vector2(20, 0), new Vector2(-20, 0));
        var topHL = topBar.AddComponent<HorizontalLayoutGroup>();
        topHL.spacing = 30;
        topHL.childAlignment = TextAnchor.MiddleCenter;
        topHL.childForceExpandWidth = false;

        var cpmText = MakeText("CPM_Text", topBar.transform, "CPM: <b>0</b>", 28);
        var comboText = MakeText("Combo_Text", topBar.transform, "", 28);
        var speedText = MakeText("Speed_Text", topBar.transform, "Speed: <b>0.0</b>", 24);
        var timerText = MakeText("Timer_Text", topBar.transform, "⏱ 03:00", 28);

        // Distance Bar
        var distBar = MakeUI("DistanceBar", canvasObj.transform,
            new Vector2(0.15f, 0.83f), new Vector2(0.85f, 0.88f));
        var distSlider = distBar.AddComponent<Slider>();
        // Slider background
        var distBgGo = new GameObject("BG"); distBgGo.transform.SetParent(distBar.transform);
        var distBgImg = distBgGo.AddComponent<Image>();
        distBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
        distBgGo.GetComponent<RectTransform>().StretchFill();
        distSlider.targetGraphic = distBgImg;
        // Slider fill
        var distFillGo = new GameObject("Fill"); distFillGo.transform.SetParent(distBar.transform);
        var distFillImg = distFillGo.AddComponent<Image>();
        distFillImg.color = Color.green;
        var distFillRt = distFillGo.GetComponent<RectTransform>();
        distFillRt.StretchFill();
        distSlider.fillRect = distFillRt;
        distSlider.handleRect = distFillRt;
        distSlider.value = 0.5f;

        var distLabel = MakeText("DistLabel", distBar.transform, "✅ 6.0m", 20);
        var dlRt = distLabel.GetComponent<RectTransform>();
        dlRt.PinTopLeft(new Vector2(0, 15), new Vector2(400, 30));

        var mobLabel = MakeText("MobLabel", distBar.transform, "🐉", 24);
        var mobRt = mobLabel.GetComponent<RectTransform>();
        mobRt.PinMiddleLeft(new Vector2(-30, 0), new Vector2(40, 40));

        var playerLabel = MakeText("PlayerLabel", distBar.transform, "🏃", 24);
        var plLabelRt = playerLabel.GetComponent<RectTransform>();
        plLabelRt.PinMiddleRight(new Vector2(30, 0), new Vector2(40, 40));

        // Word Panel
        var wordPanel = MakeUI("WordPanel", canvasObj.transform,
            new Vector2(0.2f, 0.5f), new Vector2(0.8f, 0.68f));
        var wordBg = wordPanel.AddComponent<Image>();
        wordBg.color = new Color(0.05f, 0.05f, 0.1f, 0.85f);
        var wordVL = wordPanel.AddComponent<VerticalLayoutGroup>();
        wordVL.padding = new RectOffset(30, 30, 15, 15);
        wordVL.spacing = 8;
        wordVL.childAlignment = TextAnchor.MiddleCenter;
        wordVL.childForceExpandWidth = true;

        var currentWordText = MakeText("CurrentWord", wordPanel.transform, "run", 56, TextAlignmentOptions.Center, FontStyles.Bold);
        currentWordText.color = Color.white;

        var typedUntypedPanel = MakeUI("TypedUntyped", wordPanel.transform,
            Vector2.zero, Vector2.one);
        typedUntypedPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50);
        var tupHL = typedUntypedPanel.AddComponent<HorizontalLayoutGroup>();
        tupHL.childAlignment = TextAnchor.MiddleCenter;
        tupHL.spacing = 0;
        tupHL.childForceExpandWidth = false;

        var typedPartText = MakeText("TypedPart", typedUntypedPanel.transform, "", 52, TextAlignmentOptions.Center);
        typedPartText.enableRichText = true;
        var untypedPartText = MakeText("UntypedPart", typedUntypedPanel.transform, "", 52, TextAlignmentOptions.Center);
        untypedPartText.enableRichText = true;

        var inputPrompt = MakeText("InputPrompt", wordPanel.transform, "> type the word above", 22, TextAlignmentOptions.Center);
        inputPrompt.color = new Color(0.5f, 0.8f, 0.5f);

        var previewText = MakeText("PreviewWords", wordPanel.transform, "Next: ...", 18, TextAlignmentOptions.Center);
        previewText.color = new Color(0.4f, 0.4f, 0.5f);

        // Speed Gauge
        var gaugePanel = MakeUI("SpeedGauge", canvasObj.transform,
            new Vector2(0.02f, 0.72f), new Vector2(0.1f, 0.8f));
        var gaugeImg = gaugePanel.AddComponent<Image>();
        gaugeImg.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);
        gaugeImg.type = Image.Type.Filled;
        gaugeImg.fillMethod = Image.FillMethod.Horizontal;
        gaugeImg.fillAmount = 0.3f;
        var gaugeText = MakeText("GaugeText", gaugePanel.transform, "0 m/s", 14, TextAlignmentOptions.Center);
        gaugeText.GetComponent<RectTransform>().StretchFill();

        // Message Text
        var msgText = MakeText("MessageText", canvasObj.transform, "", 32, TextAlignmentOptions.Center);
        var msgRt = msgText.GetComponent<RectTransform>();
        msgRt.anchorMin = new Vector2(0.15f, 0.42f); msgRt.anchorMax = new Vector2(0.85f, 0.52f);
        msgRt.sizeDelta = Vector2.zero;
        msgText.color = new Color(1, 0.8f, 0.1f);
        msgText.alpha = 0;

        // Red Vignette
        var vignetteObj = MakeUI("VignetteOverlay", canvasObj.transform,
            Vector2.zero, Vector2.one);
        var vignetteImg = vignetteObj.AddComponent<Image>();
        vignetteImg.color = new Color(0.6f, 0.02f, 0.02f, 0f);
        vignetteImg.raycastTarget = false;
        var vignetteGroup = vignetteObj.AddComponent<CanvasGroup>();
        vignetteGroup.alpha = 0;
        vignetteGroup.blocksRaycasts = false;

        // Shake Container
        var shakeTarget = MakeUI("ShakeContainer", canvasObj.transform,
            Vector2.zero, Vector2.one);
        topBar.transform.SetParent(shakeTarget.transform);
        wordPanel.transform.SetParent(shakeTarget.transform);
        distBar.transform.SetParent(shakeTarget.transform);
        gaugePanel.transform.SetParent(shakeTarget.transform);
        msgText.transform.SetParent(shakeTarget.transform);

        // Game Over Panel
        var goPanel = MakeUI("GameOverPanel", canvasObj.transform,
            Vector2.zero, Vector2.one);
        goPanel.SetActive(false);
        var goBg = goPanel.AddComponent<Image>();
        goBg.color = new Color(0, 0, 0, 0.85f);
        var goVL = goPanel.AddComponent<VerticalLayoutGroup>();
        goVL.padding = new RectOffset(0, 0, 300, 0);
        goVL.spacing = 20;
        goVL.childAlignment = TextAnchor.MiddleCenter;
        goVL.childForceExpandWidth = true;

        var goTitle = MakeText("GOTitle", goPanel.transform, "💀 GAME OVER", 64, TextAlignmentOptions.Center, FontStyles.Bold);
        var goStats = MakeText("GOStats", goPanel.transform, "", 28, TextAlignmentOptions.Center);

        var goBtnGo = new GameObject("RestartButton");
        goBtnGo.transform.SetParent(goPanel.transform);
        var goBtn = goBtnGo.AddComponent<Button>();
        goBtnGo.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 60);
        var goBtnImg = goBtnGo.AddComponent<Image>();
        goBtnImg.color = new Color(0.8f, 0.3f, 0.3f);
        MakeText("RestartText", goBtnGo.transform, "🔄 重新开始", 28, TextAlignmentOptions.Center);
        goBtn.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex));

        // Volume
        var volObj = MakeUI("VolumeControl", canvasObj.transform,
            new Vector2(0.85f, 0.02f), new Vector2(0.98f, 0.06f));
        var volHL = volObj.AddComponent<HorizontalLayoutGroup>();
        volHL.spacing = 8;
        volHL.childForceExpandWidth = false;
        MakeText("VolLabel", volObj.transform, "🔊", 18);
        var volSlider = volObj.AddComponent<Slider>();
        volSlider.minValue = 0; volSlider.maxValue = 1; volSlider.value = 1;
        volSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 20);

        // ===== 8. Audio =====
        var audioObj = new GameObject("AudioSource");
        var audioSrc = audioObj.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;

        // ===== 9. GameManager =====
        var gmObj = new GameObject("GameManager");
        var gameManager = gmObj.AddComponent<GameManager>();
        gameManager.mainCamera = mainCam;
        gameManager.worldRoot = worldRoot.transform;

        // ===== 10. Wire References =====
        var uiManager = canvasObj.AddComponent<UIManager>();
        uiManager.currentWordText = currentWordText;
        uiManager.typedPartText = typedPartText;
        uiManager.untypedPartText = untypedPartText;
        uiManager.previewWordsText = previewText;
        uiManager.inputPromptText = inputPrompt;
        uiManager.cpmText = cpmText;
        uiManager.comboText = comboText;
        uiManager.timerText = timerText;
        uiManager.speedText = speedText;
        uiManager.distanceSlider = distSlider;
        uiManager.distanceFill = distFillImg;
        uiManager.distanceLabel = distLabel;
        uiManager.speedGaugeFill = gaugeImg;
        uiManager.speedGaugeText = gaugeText;
        uiManager.gameOverPanel = goPanel;
        uiManager.gameOverTitle = goTitle;
        uiManager.gameOverStats = goStats;
        uiManager.restartButton = goBtn;
        uiManager.volumeSlider = volSlider;
        uiManager.typingEngine = te;
        uiManager.player = player;
        uiManager.monster = monster;
        uiManager.distanceManager = dm;
        uiManager.gameManager = gameManager;

        fb.distanceManager = dm;
        fb.monster = monster;
        fb.typingEngine = te;
        fb.vignetteGroup = vignetteGroup;
        fb.vignetteImage = vignetteImg;
        fb.screenShakeTarget = shakeTarget.GetComponent<RectTransform>();
        fb.messageText = msgText;
        fb.audioSource = audioSrc;

        player.typingEngine = te;
        player.distanceManager = dm;
        monster.distanceManager = dm;

        gameManager.typingEngine = te;
        gameManager.player = player;
        gameManager.monster = monster;
        gameManager.distanceManager = dm;
        gameManager.uiManager = uiManager;
        gameManager.feedbackSystem = fb;

        // ScriptableObjects (runtime create if no .asset file exists)
        var gameData = Resources.Load<GameData>("GameData/DefaultGameConfig");
        if (gameData == null) gameData = ScriptableObject.CreateInstance<GameData>();
        gameManager.gameData = gameData;
        te.gameData = gameData;
        player.gameData = gameData;
        monster.gameData = gameData;
        dm.gameData = gameData;

        var wordPool = Resources.Load<WordPool>("WordPools/EnglishWords");
        if (wordPool == null) wordPool = ScriptableObject.CreateInstance<WordPool>();
        te.wordPool = wordPool;

        Debug.Log("[SceneAutoBuilder] ✅ 游戏场景构建完成！开始打字跑吧！");
    }

    // ===== Helpers =====

    static GameObject MakeUI(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        Vector2? offsetMin = null, Vector2? offsetMax = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin ?? Vector2.zero;
        rt.offsetMax = offsetMax ?? Vector2.zero;
        return go;
    }

    static TMP_Text MakeText(string name, Transform parent, string content, int fontSize,
        TextAlignmentOptions alignment = TextAlignmentOptions.Left,
        FontStyles fontStyle = FontStyles.Normal)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 40);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.fontStyle = fontStyle;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        // 自动使用 TMP 的默认字体（TMP Essential Resources 自带）
        var defaultFont = TMP_Settings.defaultFontAsset;
        if (defaultFont != null)
            tmp.font = defaultFont;
        else
            Debug.LogWarning($"[SceneSetup] TMP 默认字体未找到，TMP_Text '{name}' 可能不显示。请在 Window > TextMeshPro > Import TMP Essential Resources 中导入。");

        var fitter = go.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return tmp;
    }
}

/// <summary>RectTransform 扩展</summary>
public static class RectTransformExtensions
{
    public static void StretchFill(this RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    public static void PinTopLeft(this RectTransform rt, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
    }

    public static void PinMiddleLeft(this RectTransform rt, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(0, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
    }

    public static void PinMiddleRight(this RectTransform rt, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        rt.anchorMin = new Vector2(1, 0.5f); rt.anchorMax = new Vector2(1, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
    }
}
