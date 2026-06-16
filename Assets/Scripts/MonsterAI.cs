using UnityEngine;

/// <summary>
/// 怪物 AI — 三段变速 + 正弦波动 + 随机尖刺 + 追赶修正
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class MonsterAI : MonoBehaviour
{
    [Header("引用")]
    public GameData gameData;
    public DistanceManager distanceManager;

    private SpriteRenderer sr;
    private Transform cachedTransform;
    private float positionX = 10f; // 初始在玩家右方
    private float chaseBoost = 0f;
    private float gameStartTime;
    private float elapsedTime;
    private float eyeGlowTimer = 0f;
    private bool isRaging = false;
    private Color baseEyeColor = new Color(1f, 0.15f, 0.05f);

    public float WorldX => positionX;
    public float CurrentSpeed { get; private set; }
    public float ChaseBoost => chaseBoost;
    public int CurrentPhase { get; private set; }
    public bool IsRaging => isRaging;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        cachedTransform = transform;
    }

    void Start()
    {
        if (gameData == null) gameData = Resources.Load<GameData>("GameData/DefaultGameConfig");
        if (distanceManager == null) distanceManager = FindAnyObjectByType<DistanceManager>();

        gameStartTime = Time.time;
    }

    void Update()
    {
        elapsedTime = Time.time - gameStartTime;
        CurrentPhase = GetPhase();

        float distance = distanceManager != null ? distanceManager.Distance : 5f;

        // 追赶修正
        UpdateChaseBoost(distance);

        // 计算速度
        float phaseMult = GetPhaseMultiplier();
        float totalTime = gameData.totalGameTime > 0 ? gameData.totalGameTime : 180f;
        float baseSpeed = gameData.monsterBaseSpeed +
                          (elapsedTime / totalTime) * gameData.monsterSpeedGrowth;
        float wave = Mathf.Sin(elapsedTime * gameData.waveFrequency) * gameData.waveAmplitude;
        float spike = Random.Range(-gameData.randomSpikeRange, gameData.randomSpikeRange);

        float speed = baseSpeed * phaseMult * (1f + wave + spike);
        speed *= (1f + chaseBoost);
        CurrentSpeed = speed;

        // 移动
        positionX += speed * Time.deltaTime;
        cachedTransform.position = new Vector3(positionX, cachedTransform.position.y, 0);

        // 视觉反馈
        UpdateVisuals();
    }

    int GetPhase()
    {
        if (elapsedTime < gameData.phaseTimes[1]) return 0;
        if (elapsedTime < gameData.phaseTimes[2]) return 1;
        return 2;
    }

    float GetPhaseMultiplier()
    {
        int phase = GetPhase();
        return gameData.phaseMultipliers[Mathf.Clamp(phase, 0, gameData.phaseMultipliers.Length - 1)];
    }

    void UpdateChaseBoost(float distance)
    {
        if (distance > gameData.farGap)
        {
            // 拉开太远 → 不断加速追
            chaseBoost = Mathf.Lerp(chaseBoost, gameData.maxChaseBoost,
                                    Time.deltaTime * gameData.chaseBoostRampUp);
            if (!isRaging)
            {
                isRaging = true;
                eyeGlowTimer = 0;
            }
        }
        else if (distance < gameData.safeFar)
        {
            // 追上了 → 逐渐减速
            chaseBoost = Mathf.Lerp(chaseBoost, 0,
                                    Time.deltaTime * gameData.chaseBoostRampDown);
            if (chaseBoost < 0.02f)
            {
                chaseBoost = 0;
                isRaging = false;
            }
        }
    }

    void UpdateVisuals()
    {
        if (sr == null) return;

        // 怪物大小随速度变化（紧张感）
        float size = 1f + CurrentSpeed * 0.04f;
        cachedTransform.localScale = new Vector3(size, size, 1f);

        // 狂暴时眼睛闪烁
        if (isRaging)
        {
            eyeGlowTimer += Time.deltaTime * 10f;
            float glow = Mathf.Abs(Mathf.Sin(eyeGlowTimer));
            sr.color = Color.Lerp(new Color(0.35f, 0.1f, 0.45f), new Color(0.7f, 0.15f, 0.2f), glow);
        }
        else
        {
            sr.color = new Color(0.35f, 0.1f, 0.45f);
        }

        // Phase 3 体型变大
        if (CurrentPhase >= 2)
        {
            float angrySize = 1f + Mathf.Sin(elapsedTime * 2f) * 0.05f;
            cachedTransform.localScale = new Vector3(size + 0.2f, size + 0.2f, 1f) * angrySize;
        }
    }

    /// <summary>重置怪物状态</summary>
    public void Reset()
    {
        positionX = 10f;
        chaseBoost = 0f;
        isRaging = false;
        gameStartTime = Time.time;
        CurrentSpeed = 0;
        cachedTransform.position = new Vector3(10, -2.3f, 0);
        cachedTransform.localScale = Vector3.one;
    }
}
