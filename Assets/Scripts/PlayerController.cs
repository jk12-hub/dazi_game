using UnityEngine;

/// <summary>
/// 玩家控制器 — 根据打字速度驱动移动、动画状态
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("引用")]
    public TypingEngine typingEngine;
    public GameData gameData;
    public DistanceManager distanceManager;

    private SpriteRenderer sr;
    private Transform cachedTransform;
    private float positionX = 0f;
    private float animTimer = 0f;
    private float dustTimer = 0f;

    // 弹跳动画参数
    private float bobOffset = 0f;
    private float bobSpeed = 0f;

    [Header("特效")]
    public ParticleSystem dustParticles; // 高速烟尘（可选）

    public float WorldX => positionX;
    public float CurrentSpeed { get; private set; }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        cachedTransform = transform;
    }

    void Start()
    {
        if (typingEngine == null) typingEngine = FindAnyObjectByType<TypingEngine>();
        if (gameData == null) gameData = FindAnyObjectByType<GameData>()?.GetComponent<GameData>() ?? Resources.Load<GameData>("GameData/DefaultGameConfig");
        if (distanceManager == null) distanceManager = FindAnyObjectByType<DistanceManager>();
    }

    void Update()
    {
        if (typingEngine == null || gameData == null) return;

        // 计算速度
        float cpmRatio = typingEngine.Cpm / GetTargetCpm();
        float comboMult = 1f + (typingEngine.Combo / 10f) * gameData.comboMultiplierPer10;

        // 打错惩罚
        float recentlyWrong = typingEngine.Errors > 0 && typingEngine.IsTyping ? 1f - gameData.errorSpeedPenalty : 1f;

        float speed = gameData.playerBaseSpeed +
                      cpmRatio * gameData.maxSpeedBonus * comboMult * recentlyWrong;

        speed = Mathf.Max(gameData.playerBaseSpeed * 0.3f, speed);
        CurrentSpeed = speed;

        // 移动
        positionX += speed * Time.deltaTime;
        cachedTransform.position = new Vector3(positionX, cachedTransform.position.y, 0);

        // 奔跑弹跳动画
        UpdateBobAnimation(speed);

        // 烟尘特效（高速时激活）
        UpdateDust(speed);
    }

    void UpdateBobAnimation(float speed)
    {
        float normalizedSpeed = Mathf.Clamp01(speed / (gameData.maxSpeedBonus + gameData.playerBaseSpeed));
        bobSpeed = Mathf.Lerp(2f, 12f, normalizedSpeed);
        animTimer += Time.deltaTime * bobSpeed;
        bobOffset = Mathf.Abs(Mathf.Sin(animTimer)) * 0.15f * normalizedSpeed;

        // 弹跳偏移
        float baseY = -2f;
        cachedTransform.position = new Vector3(cachedTransform.position.x, baseY + bobOffset, 0);

        // 微小的伸缩（squash/stretch 感的替代：简单的 scale）
        float scaleY = 1f - bobOffset * 1.5f;
        float scaleX = 1f + bobOffset * 1.5f;
        cachedTransform.localScale = new Vector3(scaleX, scaleY, 1f);

        // 颜色随速度变化：慢=正常，快=暖色光晕
        if (sr != null)
        {
            Color c = Color.Lerp(Color.white, new Color(1f, 0.9f, 0.5f), normalizedSpeed * 0.4f);
            sr.color = c;
        }
    }

    void UpdateDust(float speed)
    {
        if (dustParticles == null) return;

        float threshold = (gameData.maxSpeedBonus + gameData.playerBaseSpeed) * 0.5f;
        bool fast = speed > threshold;

        if (fast)
        {
            if (!dustParticles.isPlaying)
                dustParticles.Play();

            var main = dustParticles.main;
            main.startSpeed = Mathf.Lerp(0.5f, 3f, speed / (gameData.maxSpeedBonus + gameData.playerBaseSpeed));
        }
        else
        {
            if (dustParticles.isPlaying)
                dustParticles.Stop();
        }
    }

    float GetTargetCpm()
    {
        if (gameData == null) return 60f;
        float t = typingEngine.TotalTime / gameData.totalGameTime;
        return Mathf.Lerp(gameData.targetCpmEarly, gameData.targetCpmLate, t);
    }

    /// <summary>重置玩家位置</summary>
    public void Reset()
    {
        positionX = 0f;
        cachedTransform.position = new Vector3(0, -2f, 0);
        cachedTransform.localScale = Vector3.one;
        CurrentSpeed = 0;
        if (dustParticles != null && dustParticles.isPlaying)
            dustParticles.Stop();
    }
}
