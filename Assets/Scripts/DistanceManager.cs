using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 距离管理 — 计算玩家与怪物的距离，触发距离区间事件
/// </summary>
public class DistanceManager : MonoBehaviour
{
    [Header("引用")]
    public PlayerController player;
    public MonsterAI monster;
    public GameData gameData;

    public enum Zone
    {
        Critical,   // < 2
        Danger,     // 2-4
        Safe,       // 4-8
        Far,        // 8-12
        TooFar      // > 12  (过远触发自动加速)
    }

    public Zone CurrentZone { get; private set; } = Zone.Safe;
    public float Distance { get; private set; } = 6f;
    public float NormalizedDistance { get; private set; } = 0.5f; // 0=被抓, 1=太远

    [HideInInspector] public UnityEvent<Zone, Zone> onZoneChanged; // (from, to)
    [HideInInspector] public UnityEvent<Zone> onZoneStay;
    [HideInInspector] public UnityEvent onTooClose;   // 进入极危
    [HideInInspector] public UnityEvent onTooFar;     // 进入过远
    [HideInInspector] public UnityEvent onReturnSafe; // 回到安全

    private float timeInZone = 0f;
    private float lastMessageTime = -10f;

    void Start()
    {
        if (player == null) player = FindAnyObjectByType<PlayerController>();
        if (monster == null) monster = FindAnyObjectByType<MonsterAI>();
        if (gameData == null) gameData = Resources.Load<GameData>("GameData/DefaultGameConfig");
    }

    void Update()
    {
        if (player == null || monster == null || gameData == null) return;

        // 距离 = 玩家 X - 怪物 X（正值表示玩家在前）
        Distance = player.WorldX - monster.WorldX;

        // 归一化距离（0=被抓, 1=farGap）
        NormalizedDistance = Mathf.Clamp01(Distance / gameData.farGap);

        // 判断区间
        Zone newZone;
        if (Distance < gameData.dangerCritical)       newZone = Zone.Critical;
        else if (Distance < gameData.dangerClose)     newZone = Zone.Danger;
        else if (Distance < gameData.safeFar)         newZone = Zone.Safe;
        else if (Distance < gameData.farGap)          newZone = Zone.Far;
        else                                          newZone = Zone.TooFar;

        if (newZone != CurrentZone)
        {
            var old = CurrentZone;
            CurrentZone = newZone;
            timeInZone = 0;
            onZoneChanged?.Invoke(old, newZone);

            // 特殊事件
            if (newZone == Zone.Critical) onTooClose?.Invoke();
            if (newZone == Zone.TooFar) onTooFar?.Invoke();
            if (old == Zone.Critical || old == Zone.Danger)
                if (newZone == Zone.Safe || newZone == Zone.Far)
                    onReturnSafe?.Invoke();
        }
        else
        {
            timeInZone += Time.deltaTime;
            onZoneStay?.Invoke(CurrentZone);
        }
    }

    /// <summary>获取距离区间的提示文字</summary>
    public string GetZoneMessage()
    {
        bool raging = monster != null && monster.IsRaging;

        return CurrentZone switch
        {
            Zone.Critical => raging ? "💀 怪物暴怒中！快逃！！" : "⚠️ 怪物就在身后！快打字！！",
            Zone.Danger    => "🔥 加快打字！怪物逼近中！",
            Zone.Safe      => "👍 保持节奏，稳住！",
            Zone.Far        => "⚡ 甩开怪物了！继续！",
            Zone.TooFar     => "🐉 怪物锁定了你的气息...疯狂加速中！",
            _               => ""
        };
    }

    /// <summary>获取距离区间的颜色</summary>
    public Color GetZoneColor()
    {
        return CurrentZone switch
        {
            Zone.Critical => new Color(0.8f, 0.05f, 0.05f),
            Zone.Danger   => new Color(0.9f, 0.4f, 0.05f),
            Zone.Safe     => new Color(0.2f, 0.8f, 0.2f),
            Zone.Far      => new Color(0.2f, 0.5f, 1f),
            Zone.TooFar   => new Color(1f, 0.2f, 0.2f),
            _             => Color.white
        };
    }
}
