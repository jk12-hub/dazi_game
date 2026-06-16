using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 反馈系统 — 屏幕特效（红雾、震动、闪烁）、消息提示
/// </summary>
public class FeedbackSystem : MonoBehaviour
{
    [Header("引用")]
    public DistanceManager distanceManager;
    public MonsterAI monster;
    public TypingEngine typingEngine;

    [Header("屏幕特效")]
    public CanvasGroup vignetteGroup;
    public UnityEngine.UI.Image vignetteImage;
    public RectTransform screenShakeTarget;

    [Header("消息")]
    public TMP_Text messageText;
    public float messageDuration = 2.5f;

    [Header("音效 (可选 — 用 AudioSource.PlayOneShot)")]
    public AudioSource audioSource;
    public AudioClip correctClip;
    public AudioClip completeClip;
    public AudioClip errorClip;
    public AudioClip dangerClip;
    public AudioClip comboClip;

    private Vector3 screenOriginalPos;
    private float vignetteTargetAlpha = 0;
    private float vignetteCurrentAlpha = 0;
    private float messageTimer = 0;
    private Coroutine messageCoroutine;
    private DistanceManager.Zone lastZone;

    void Start()
    {
        if (distanceManager == null) distanceManager = FindAnyObjectByType<DistanceManager>();
        if (monster == null) monster = FindAnyObjectByType<MonsterAI>();
        if (typingEngine == null) typingEngine = FindAnyObjectByType<TypingEngine>();

        if (screenShakeTarget != null)
            screenOriginalPos = screenShakeTarget.localPosition;

        // 订阅打字事件
        if (typingEngine != null)
        {
            typingEngine.onCorrectChar.AddListener(_ => PlayCorrectSound());
            typingEngine.onWordCompleted.AddListener(_ => PlayCompleteSound());
            typingEngine.onWrongChar.AddListener(_ => PlayErrorSound());
            typingEngine.onComboBreak.AddListener(() => { });
        }

        // 订阅距离事件
        if (distanceManager != null)
        {
            distanceManager.onTooClose.AddListener(OnTooClose);
            distanceManager.onTooFar.AddListener(OnTooFar);
            distanceManager.onReturnSafe.AddListener(() => OnReturnSafe());
        }
    }

    void Update()
    {
        if (distanceManager == null) return;

        // 红雾效果随距离变化
        UpdateVignette();

        // 距离变化时的消息
        var zone = distanceManager.CurrentZone;
        if (zone != lastZone)
        {
            ShowMessage(distanceManager.GetZoneMessage());
            lastZone = zone;
        }

        // 保持极危 / 过远状态的消息刷新
        if (zone == DistanceManager.Zone.Critical || zone == DistanceManager.Zone.TooFar)
        {
            messageTimer += Time.deltaTime;
            if (messageTimer > messageDuration)
            {
                ShowMessage(distanceManager.GetZoneMessage());
                messageTimer = 0;
            }
        }
    }

    void UpdateVignette()
    {
        if (vignetteImage == null) return;

        float dist = distanceManager.Distance;
        float critical = distanceManager.gameData.dangerCritical;
        float close = distanceManager.gameData.dangerClose;

        // 距离越近越红
        if (dist < critical)
        {
            // 极危：脉冲式红闪
            float pulse = Mathf.Abs(Mathf.Sin(Time.time * 5f));
            vignetteTargetAlpha = Mathf.Lerp(0.6f, 0.9f, pulse);
        }
        else if (dist < close)
        {
            vignetteTargetAlpha = Mathf.Lerp(0.35f, 0, (dist - critical) / (close - critical));
        }
        else
        {
            vignetteTargetAlpha = 0;
        }

        vignetteCurrentAlpha = Mathf.Lerp(vignetteCurrentAlpha, vignetteTargetAlpha, Time.deltaTime * 4f);

        if (vignetteGroup != null)
            vignetteGroup.alpha = vignetteCurrentAlpha;

        if (vignetteImage != null)
        {
            vignetteImage.color = new Color(0.8f, 0.05f, 0.05f, vignetteCurrentAlpha);
        }
    }

    // === 震动效果 ===
    void OnTooClose()
    {
        ShowMessage("💀 怪物就在身后！打快点！！");
        StartCoroutine(ShakeRoutine(0.12f, 0.3f));
    }

    void OnTooFar()
    {
        ShowMessage("🐉 你甩太远了！怪物发怒了！！");
        StartCoroutine(ShakeRoutine(0.06f, 0.5f));
    }

    void OnReturnSafe()
    {
        ShowMessage("✅ 已安全！保持住！");
    }

    IEnumerator ShakeRoutine(float intensity, float duration)
    {
        if (screenShakeTarget == null) yield break;

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float decay = 1f - elapsed / duration;
            float x = Random.Range(-intensity, intensity) * decay;
            float y = Random.Range(-intensity, intensity) * decay;
            screenShakeTarget.localPosition = screenOriginalPos + new Vector3(x, y, 0);
            yield return null;
        }
        screenShakeTarget.localPosition = screenOriginalPos;
    }

    // === 消息 ===
    public void ShowMessage(string msg)
    {
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(MessageRoutine(msg));
    }

    IEnumerator MessageRoutine(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            messageText.alpha = 1;
            messageText.transform.localScale = Vector3.one * 1.1f;
        }

        // 缩小回正常
        float t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            if (messageText != null)
                messageText.transform.localScale = Vector3.Lerp(Vector3.one * 1.1f, Vector3.one, t / 0.2f);
            yield return null;
        }

        yield return new WaitForSeconds(messageDuration - 0.2f);

        // 淡出
        float fade = 0;
        while (fade < 0.5f)
        {
            fade += Time.deltaTime;
            if (messageText != null)
                messageText.alpha = 1f - fade / 0.5f;
            yield return null;
        }
    }

    // === 音效 ===
    void PlayCorrectSound()
    {
        if (audioSource != null && correctClip != null)
            audioSource.PlayOneShot(correctClip, 0.4f);
    }

    void PlayCompleteSound()
    {
        if (audioSource != null && completeClip != null)
            audioSource.PlayOneShot(completeClip, 0.7f);
    }

    void PlayErrorSound()
    {
        if (audioSource != null && errorClip != null)
            audioSource.PlayOneShot(errorClip, 0.5f);
    }

    /// <summary>重置</summary>
    public void ResetAll()
    {
        vignetteCurrentAlpha = 0;
        vignetteTargetAlpha = 0;
        if (vignetteGroup != null) vignetteGroup.alpha = 0;
        if (messageText != null) { messageText.alpha = 0; messageText.text = ""; }
        if (screenShakeTarget != null) screenShakeTarget.localPosition = screenOriginalPos;
        lastZone = DistanceManager.Zone.Safe;
    }
}
