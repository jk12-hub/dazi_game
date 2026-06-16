using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 核心打字引擎 — 捕获键盘输入、验证单词、计算 CPM 和连击
/// </summary>
public class TypingEngine : MonoBehaviour
{
    [Header("引用")]
    public WordPool wordPool;
    public GameData gameData;

    [Header("状态 (只读)")]
    [SerializeField] private string currentWord = "";
    [SerializeField] private int typedIndex = 0;
    [SerializeField] private float currentCpm = 0f;
    [SerializeField] private int comboCount = 0;
    [SerializeField] private int totalCharsTyped = 0;
    [SerializeField] private int totalErrors = 0;
    [SerializeField] private float elapsedTypingTime = 0f;

    // 平滑 CPM
    private Queue<float> cpmHistory = new Queue<float>();
    private float lastInputTime = -1f;
    private float gameStartTime;

    // 预览
    private List<string> previewWords = new List<string>();

    // 事件
    [HideInInspector] public UnityEvent<string> onWordCompleted;   // word
    [HideInInspector] public UnityEvent<char> onCorrectChar;       // typed char
    [HideInInspector] public UnityEvent<char> onWrongChar;         // wrong char
    [HideInInspector] public UnityEvent onComboBreak;

    public float Cpm => currentCpm;
    public int Combo => comboCount;
    public int TotalChars => totalCharsTyped;
    public int Errors => totalErrors;
    public string CurrentWord => currentWord;
    public int TypedIndex => typedIndex;
    public float TotalTime => elapsedTypingTime;
    public List<string> PreviewWords => previewWords;
    public bool IsTyping => Time.time - lastInputTime < gameData.comboDecayTime && lastInputTime > 0;

    void Start()
    {
        if (gameData == null)
        {
            Debug.LogError("TypingEngine: GameData 未赋值！");
            return;
        }
        if (wordPool == null)
        {
            Debug.LogError("TypingEngine: WordPool 未赋值！");
            return;
        }

        gameStartTime = Time.time;
        PickNewWord();
        UpdatePreview();
    }

    void Update()
    {
        elapsedTypingTime = Time.time - gameStartTime;

        // 连击衰减
        if (IsTyping == false && comboCount > 0)
        {
            comboCount = 0;
            onComboBreak?.Invoke();
        }

        // CPM 平滑
        UpdateCpm();
    }

    void OnGUI()
    {
        // 捕获键盘事件（Input.inputString 需要 OnGUI 才能工作）
        if (Event.current.type == EventType.KeyDown && Event.current.character != '\0')
        {
            char c = Event.current.character;
            // 忽略控制字符（退格、回车等）和修饰键
            if (c == '\b' || c == '\n' || c == '\r' || c == '\t' || c == ' ')
                return;
            if (Event.current.keyCode == KeyCode.Escape || Event.current.keyCode == KeyCode.None)
                return;
            if (char.IsControl(c))
                return;

            // 忽略 Shift / Ctrl 等纯修饰键
            if (Event.current.keyCode == KeyCode.LeftShift || Event.current.keyCode == KeyCode.RightShift ||
                Event.current.keyCode == KeyCode.LeftControl || Event.current.keyCode == KeyCode.RightControl ||
                Event.current.keyCode == KeyCode.LeftAlt || Event.current.keyCode == KeyCode.RightAlt ||
                Event.current.keyCode == KeyCode.LeftCommand || Event.current.keyCode == KeyCode.RightCommand ||
                Event.current.keyCode == KeyCode.LeftWindows || Event.current.keyCode == KeyCode.RightWindows)
                return;

            ProcessCharacter(c);
        }
    }

    void ProcessCharacter(char c)
    {
        if (string.IsNullOrEmpty(currentWord))
            return;

        lastInputTime = Time.time;
        char lowerTyped = char.ToLower(c);
        char lowerExpected = char.ToLower(currentWord[typedIndex]);

        if (lowerTyped == lowerExpected)
        {
            // 正确
            typedIndex++;
            totalCharsTyped++;
            onCorrectChar?.Invoke(c);

            if (typedIndex >= currentWord.Length)
            {
                // 单词完成
                comboCount++;
                onWordCompleted?.Invoke(currentWord);
                PickNewWord();
            }
        }
        else
        {
            // 错误
            totalErrors++;
            comboCount = 0;
            onWrongChar?.Invoke(c);
            onComboBreak?.Invoke();
            // 错误后从头开始当前单词
            typedIndex = 0;
        }
    }

    void PickNewWord()
    {
        currentWord = wordPool.GetWord(elapsedTypingTime);
        typedIndex = 0;
        UpdatePreview();
    }

    void UpdatePreview()
    {
        previewWords = wordPool.GetPreviewWords(elapsedTypingTime, 4);
    }

    void UpdateCpm()
    {
        if (totalCharsTyped == 0 || elapsedTypingTime < 0.5f)
        {
            currentCpm = 0;
            return;
        }

        float instantCpm = (totalCharsTyped / elapsedTypingTime) * 60f;
        cpmHistory.Enqueue(instantCpm);

        float window = gameData.cpmSmoothWindow;
        while (cpmHistory.Count > 1 && cpmHistory.Count > window * 60)
            cpmHistory.Dequeue();

        // 加权平均（直接遍历 Queue 避免每帧 ToArray 分配）
        float sum = 0, weightTotal = 0;
        int n = cpmHistory.Count;
        int idx = 0;
        foreach (float val in cpmHistory)
        {
            float w = idx + 1; // 越新的权重越大
            sum += val * w;
            weightTotal += w;
            idx++;
        }
        currentCpm = weightTotal > 0 ? sum / weightTotal : 0;
    }

    /// <summary>重置所有状态（新游戏）</summary>
    public void Reset()
    {
        totalCharsTyped = 0;
        totalErrors = 0;
        comboCount = 0;
        elapsedTypingTime = 0;
        gameStartTime = Time.time;
        lastInputTime = -1;
        cpmHistory.Clear();
        currentCpm = 0;
        PickNewWord();
        UpdatePreview();
    }
}
