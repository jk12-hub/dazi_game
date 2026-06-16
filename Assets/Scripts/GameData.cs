using UnityEngine;

/// <summary>
/// 全局游戏参数配置 — ScriptableObject
/// 在 Unity Editor 中可实时调参
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "TypingEscape/GameConfig", order = 0)]
public class GameData : ScriptableObject
{
    [Header("玩家")]
    [Tooltip("最低速度（不打字也慢慢走）")]
    public float playerBaseSpeed = 0.5f;
    [Tooltip("打字最大加成速度")]
    public float maxSpeedBonus = 6f;
    [Tooltip("初期目标 CPM")]
    public float targetCpmEarly = 40f;
    [Tooltip("后期目标 CPM")]
    public float targetCpmLate = 80f;
    [Tooltip("连击衰减时间（秒），多久不输入连击归零")]
    public float comboDecayTime = 2f;
    [Tooltip("每 10 连击速度加成比例")]
    public float comboMultiplierPer10 = 0.1f;
    [Tooltip("打错字速度惩罚比例")]
    public float errorSpeedPenalty = 0.2f;

    [Header("怪物")]
    [Tooltip("怪物基础速度")]
    public float monsterBaseSpeed = 1.2f;
    [Tooltip("怪物速度随时间增长上限")]
    public float monsterSpeedGrowth = 0.8f;
    [Tooltip("三阶段时间节点（秒）")]
    public float[] phaseTimes = { 0f, 40f, 80f };
    [Tooltip("三阶段速度系数")]
    public float[] phaseMultipliers = { 0.6f, 1.0f, 1.4f };
    [Tooltip("正弦波动幅度")]
    public float waveAmplitude = 0.2f;
    [Tooltip("正弦波动频率")]
    public float waveFrequency = 0.3f;
    [Tooltip("随机尖刺范围")]
    public float randomSpikeRange = 0.1f;

    [Header("距离")]
    [Tooltip("极危距离阈值")]
    public float dangerCritical = 2f;
    [Tooltip("危险距离阈值")]
    public float dangerClose = 4f;
    [Tooltip("安全距离上限")]
    public float safeFar = 8f;
    [Tooltip("拉开距离阈值")]
    public float farGap = 12f;
    [Tooltip("追赶修正最大加速比例")]
    public float maxChaseBoost = 0.5f;
    [Tooltip("追赶修正加速速度（lerp 因子）")]
    public float chaseBoostRampUp = 0.5f;
    [Tooltip("追赶修正减速速度")]
    public float chaseBoostRampDown = 0.3f;

    [Header("游戏总时长")]
    [Tooltip("游戏时长（秒），0 表示无尽模式")]
    public float totalGameTime = 180f;

    [Header("显示")]
    [Tooltip("CPM 平滑窗口（秒）")]
    public float cpmSmoothWindow = 3f;
}
