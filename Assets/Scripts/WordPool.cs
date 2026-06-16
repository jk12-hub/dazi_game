using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 词库 — 按难度分级的单词池
/// </summary>
[CreateAssetMenu(fileName = "EnglishWords", menuName = "TypingEscape/WordPool", order = 1)]
public class WordPool : ScriptableObject
{
    [Header("Easy — 3~4 字母")]
    public List<string> easyWords = new List<string>
    {
        "run", "fly", "go",  "hop", "fast","dash","jump",
        "race","zip","skip","bolt","rush","dart","zoom",
        "flee","hide","move","step","leap","spin"
    };

    [Header("Medium — 5~6 字母")]
    public List<string> mediumWords = new List<string>
    {
        "sprint","escape","dodge", "flee", "hurry",
        "scurry","stride","gallop","rocket","launch",
        "bounce","hustle","travel","depart","vanish",
        "rocket","pursue","outrun","scamper","jogger"
    };

    [Header("Hard — 7~8 字母")]
    public List<string> hardWords = new List<string>
    {
        "outrun",  "survive", "persist", "vanish",
        "shatter", "stampede","trample", "catapult",
        "scramble","velocity","escapade","marathon",
        "lighting", "thunder", "runaway", "freefall",
        "overcome", "conquer", "momentum","frontier"
    };

    [Header("Extreme — 9+ 字母 / 短语")]
    public List<string> extremeWords = new List<string>
    {
        "sprintaway","runforlife","dontstop",
        "keepmoving","fullspeed","lightning",
        "unstoppable","breakfree","rapidescape",
        "vanishingpoint","fasterthanever"
    };

    /// <summary>根据游戏时间选取难度合适的随机单词</summary>
    public string GetWord(float elapsedSeconds)
    {
        List<string> pool;

        if (elapsedSeconds < 30f)
            pool = easyWords;
        else if (elapsedSeconds < 60f)
            pool = mediumWords;
        else if (elapsedSeconds < 100f)
            pool = hardWords;
        else
            pool = extremeWords;

        return pool[Random.Range(0, pool.Count)];
    }

    /// <summary>获取多个预览单词</summary>
    public List<string> GetPreviewWords(float elapsedSeconds, int count = 3)
    {
        var preview = new List<string>();
        for (int i = 0; i < count; i++)
            preview.Add(GetWord(elapsedSeconds));
        return preview;
    }
}
