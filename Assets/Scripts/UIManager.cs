using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI 管理器 — 绑定所有 UI 元素的更新
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("打字面板")]
    public TMP_Text currentWordText;
    public TMP_Text typedPartText;
    public TMP_Text untypedPartText;
    public TMP_Text previewWordsText;
    public TMP_Text inputPromptText;

    [Header("状态栏")]
    public TMP_Text cpmText;
    public TMP_Text comboText;
    public TMP_Text timerText;
    public TMP_Text speedText;

    [Header("距离条")]
    public Slider distanceSlider;
    public Image distanceFill;
    public TMP_Text distanceLabel;

    [Header("速度仪表盘")]
    public Image speedGaugeFill;
    public TMP_Text speedGaugeText;

    [Header("面板")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverTitle;
    public TMP_Text gameOverStats;
    public Button restartButton;

    [Header("音效音量")]
    public Slider volumeSlider;

    [Header("引用")]
    public TypingEngine typingEngine;
    public PlayerController player;
    public MonsterAI monster;
    public DistanceManager distanceManager;
    public GameManager gameManager;
    public GameData gameData;

    private float gameTime;

    void Start()
    {
        if (typingEngine == null) typingEngine = FindAnyObjectByType<TypingEngine>();
        if (player == null) player = FindAnyObjectByType<PlayerController>();
        if (monster == null) monster = FindAnyObjectByType<MonsterAI>();
        if (distanceManager == null) distanceManager = FindAnyObjectByType<DistanceManager>();
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        if (gameData == null) gameData = Resources.Load<GameData>("GameData/DefaultGameConfig");

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(v => AudioListener.volume = v);
        }
    }

    void Update()
    {
        if (gameManager != null && gameManager.State == GameManager.GameState.GameOver)
            return;

        gameTime = Time.timeSinceLevelLoad;

        UpdateWordPanel();
        UpdateStatusBar();
        UpdateDistanceBar();
        UpdateSpeedGauge();
        UpdateTimer();
    }

    void UpdateWordPanel()
    {
        if (typingEngine == null) return;

        string word = typingEngine.CurrentWord;
        int idx = typingEngine.TypedIndex;

        // 在 currentWordText 中高亮已输入部分
        if (currentWordText != null)
        {
            if (idx > 0 && idx < word.Length)
            {
                currentWordText.text = $"<color=#00FF88>{word.Substring(0, idx)}</color><color=#AAAAAA>{word.Substring(idx)}</color>";
            }
            else if (idx >= word.Length)
            {
                currentWordText.text = $"<color=#00FF88>{word}</color>";
            }
            else
            {
                currentWordText.text = word;
            }
        }

        // 兼容：如果使用了 typed/untyped 分离方案
        if (typedPartText != null)
        {
            typedPartText.text = idx > 0 ? $"<color=#00FF88>{word.Substring(0, idx)}</color>" : "";
        }
        if (untypedPartText != null)
        {
            untypedPartText.text = idx < word.Length ? $"<color=#888888>{word.Substring(idx)}</color>" : "";
        }

        // 预览词
        if (previewWordsText != null && typingEngine.PreviewWords != null)
        {
            var previews = typingEngine.PreviewWords;
            previewWordsText.text = "Next: " + string.Join("  ", previews);
        }

        // 输入提示闪烁
        if (inputPromptText != null)
        {
            float blink = Mathf.Abs(Mathf.Sin(Time.time * 3f));
            inputPromptText.alpha = Mathf.Lerp(0.4f, 1f, blink);
            inputPromptText.text = idx < word.Length
                ? $"> type: <b>{char.ToUpper(word[idx])}</b>..."
                : "> ✅";
        }
    }

    void UpdateStatusBar()
    {
        if (typingEngine == null || gameData == null) return;

        if (cpmText != null)
        {
            float cpm = typingEngine.Cpm;
            cpmText.text = $"CPM: <b>{cpm:F0}</b>";
            cpmText.color = cpm switch
            {
                < 20 => new Color(0.8f, 0.2f, 0.2f),
                < 50 => new Color(0.9f, 0.7f, 0.1f),
                < 80 => new Color(0.2f, 0.9f, 0.2f),
                _    => new Color(0.2f, 0.6f, 1f)
            };
        }

        if (comboText != null)
        {
            int combo = typingEngine.Combo;
            comboText.text = combo > 0 ? $"Combo: <b>x{combo}</b>" : "";
            comboText.color = combo switch
            {
                < 5  => Color.white,
                < 10 => new Color(0.9f, 0.7f, 0.1f),
                < 20 => new Color(1f, 0.5f, 0.1f),
                _    => new Color(1f, 0.2f, 0.2f)
            };
            comboText.transform.localScale = combo > 0
                ? Vector3.one * (1f + Mathf.Sin(Time.time * 4f) * 0.05f)
                : Vector3.one;
        }

        if (speedText != null && player != null)
        {
            speedText.text = $"Speed: <b>{player.CurrentSpeed:F1}</b>";
        }
    }

    void UpdateTimer()
    {
        if (timerText == null || gameData == null) return;

        if (gameData.totalGameTime > 0)
        {
            float remaining = Mathf.Max(0, gameData.totalGameTime - gameTime);
            int min = (int)(remaining / 60);
            int sec = (int)(remaining % 60);
            timerText.text = $"⏱ {min:D2}:{sec:D2}";

            // 最后 30 秒红色闪烁
            if (remaining < 30)
            {
                float blink = Mathf.Abs(Mathf.Sin(Time.time * 4f));
                timerText.color = Color.Lerp(Color.white, Color.red, blink);
            }
        }
        else
        {
            // 无尽模式
            int min = (int)(gameTime / 60);
            int sec = (int)(gameTime % 60);
            timerText.text = $"⏱ {min:D2}:{sec:D2}";
        }
    }

    void UpdateDistanceBar()
    {
        if (distanceSlider == null || distanceManager == null || gameData == null) return;

        // 映射距离到 slider 0~1
        float maxDist = gameData.farGap;
        float normalized = Mathf.Clamp01(distanceManager.Distance / maxDist);

        // 反转：左边=怪物(0), 右边=玩家(1)
        distanceSlider.value = normalized;

        if (distanceFill != null)
            distanceFill.color = distanceManager.GetZoneColor();

        if (distanceLabel != null)
        {
            string icon = distanceManager.CurrentZone switch
            {
                DistanceManager.Zone.Critical => "💀",
                DistanceManager.Zone.Danger => "⚠️",
                DistanceManager.Zone.Safe => "✅",
                DistanceManager.Zone.Far => "💨",
                DistanceManager.Zone.TooFar => "⚡",
                _ => ""
            };
            distanceLabel.text = $"{icon} {distanceManager.Distance:F1}m";
            distanceLabel.color = distanceManager.GetZoneColor();
        }
    }

    void UpdateSpeedGauge()
    {
        if (speedGaugeFill == null || player == null || gameData == null) return;

        float maxSpeed = gameData.maxSpeedBonus + gameData.playerBaseSpeed;
        float fill = Mathf.Clamp01(player.CurrentSpeed / maxSpeed);

        speedGaugeFill.fillAmount = fill;
        speedGaugeFill.color = fill switch
        {
            < 0.25f => new Color(0.6f, 0.2f, 0.2f),
            < 0.5f => new Color(0.8f, 0.6f, 0.1f),
            < 0.75f => new Color(0.2f, 0.8f, 0.2f),
            _      => new Color(0.2f, 0.4f, 1f)
        };

        if (speedGaugeText != null)
        {
            speedGaugeText.text = $"Speed: {player.CurrentSpeed:F1} m/s";
        }
    }

    // === 游戏结束 UI ===
    public void ShowGameOver(bool survived)
    {
        if (gameOverPanel == null) return;
        gameOverPanel.SetActive(true);

        float cpm = typingEngine != null ? typingEngine.Cpm : 0;
        int chars = typingEngine != null ? typingEngine.TotalChars : 0;
        int errors = typingEngine != null ? typingEngine.Errors : 0;
        float accuracy = chars + errors > 0 ? (float)chars / (chars + errors) * 100f : 100f;

        if (gameOverTitle != null)
            gameOverTitle.text = survived ? "🎉 你成功逃脱了！" : "💀 被怪物抓住了！";

        if (gameOverStats != null)
        {
            gameOverStats.text = $"平均 CPM: {cpm:F0}\n"
                               + $"总输入: {chars} 字符\n"
                               + $"错误: {errors} 次\n"
                               + $"准确率: {accuracy:F1}%\n"
                               + $"存活: {gameTime:F1} 秒";
        }
    }

    private void RestartGame()
    {
        if (gameManager != null)
            gameManager.Restart();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
