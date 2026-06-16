using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 程序化生成像素风 Sprite，无需任何外部图片
/// 在游戏启动时自动生成所有需要的精灵
/// </summary>
public static class SpriteGenerator
{
    private const int Ppu = 32; // pixels per unit

    /// <summary>创建玩家像素小人 (32×32)</summary>
    public static Sprite CreatePlayerSprite()
    {
        int w = 32, h = 32;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Clear(tex, Color.clear);

        // === 身体各部分的颜色 ===
        Color skin   = new Color(1f, 0.85f, 0.65f);  // 肤色
        Color shirt  = new Color(0.2f, 0.6f, 1f);    // 蓝衣
        Color pants  = new Color(0.15f, 0.2f, 0.5f); // 深蓝裤
        Color shoe   = new Color(0.8f, 0.2f, 0.2f);  // 红鞋
        Color hair   = new Color(0.3f, 0.15f, 0.05f); // 深棕发
        Color eye    = new Color(0.1f, 0.1f, 0.1f);  // 黑眼
        Color outline = new Color(0.05f, 0.05f, 0.1f, 0.6f);

        // === 头部 (含头发) ===
        // 脸 11-17 x, 23-28 y
        FillRect(tex, 11, 23, 7, 6, skin);
        // 头发顶
        FillRect(tex, 10, 28, 9, 3, hair);
        FillPixel(tex, 9, 27, hair); FillPixel(tex, 18, 27, hair);
        FillPixel(tex, 8, 26, hair); FillPixel(tex, 19, 26, hair);
        // 眼
        FillPixel(tex, 12, 25, eye); FillPixel(tex, 15, 25, eye);
        // 嘴
        FillPixel(tex, 13, 23, new Color(0.8f, 0.4f, 0.3f));

        // === 身体 (躯干) ===
        FillRect(tex, 11, 17, 7, 6, shirt);
        // V 领
        FillPixel(tex, 14, 22, skin);
        FillPixel(tex, 13, 21, skin); FillPixel(tex, 15, 21, skin);
        // 手臂 (奔跑姿势: 前后摆动)
        // 前臂 (伸向前)
        FillRect(tex, 18, 19, 3, 2, skin);
        FillRect(tex, 19, 17, 2, 2, skin);
        // 后臂 (甩向后)
        FillRect(tex, 8, 19, 3, 2, skin);
        FillRect(tex, 7, 17, 2, 2, skin);

        // === 腿 (奔跑动作: 前后叉开) ===
        // 前腿
        FillRect(tex, 13, 11, 3, 6, pants);
        FillRect(tex, 13, 9, 3, 2, shoe);
        // 后腿
        FillRect(tex, 10, 11, 3, 6, pants);
        FillRect(tex, 10, 9, 3, 2, shoe);

        // === 描边 ===
        DrawOutline(tex, outline);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), Ppu);
    }

    /// <summary>创建怪物像素图 (40×36) — 暗紫色追人野兽</summary>
    public static Sprite CreateMonsterSprite()
    {
        int w = 40, h = 36;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Clear(tex, Color.clear);

        Color body    = new Color(0.35f, 0.1f, 0.45f); // 暗紫
        Color belly   = new Color(0.5f, 0.25f, 0.55f); // 浅紫腹
        Color eyeColor = new Color(1f, 0.15f, 0.05f);  // 血红眼
        Color pupil   = new Color(0.1f, 0.05f, 0.05f);
        Color claw    = new Color(0.6f, 0.58f, 0.55f); // 灰色爪
        Color horn    = new Color(0.15f, 0.08f, 0.2f); // 深紫角
        Color outline = new Color(0.05f, 0, 0.08f, 0.7f);

        // 身体 (庞大的躯干)
        FillRect(tex, 8, 6, 24, 16, body);
        FillRect(tex, 10, 4, 20, 5, body); // 肩/颈向上延伸

        // 腹部 (浅色)
        FillRect(tex, 12, 6, 16, 10, belly);

        // 头部
        FillRect(tex, 5, 16, 14, 12, body);
        FillRect(tex, 4, 18, 2, 8, body);
        FillRect(tex, 18, 16, 4, 3, body);

        // 角 (两只)
        FillRect(tex, 6, 26, 3, 5, horn);
        FillRect(tex, 13, 26, 3, 5, horn);
        FillPixel(tex, 7, 30, horn); FillPixel(tex, 14, 30, horn);

        // 眼 (血红大眼，两只)
        FillRect(tex, 7, 22, 4, 4, eyeColor);
        FillRect(tex, 14, 22, 4, 4, eyeColor);
        // 瞳孔
        FillPixel(tex, 8, 23, pupil); FillPixel(tex, 15, 23, pupil);
        FillPixel(tex, 9, 23, pupil); FillPixel(tex, 16, 23, pupil);
        // 眼白
        FillPixel(tex, 8, 22, Color.white); FillPixel(tex, 15, 22, Color.white);
        FillPixel(tex, 9, 22, Color.white); FillPixel(tex, 16, 22, Color.white);

        // 嘴 (张开的血盆大口)
        FillRect(tex, 8, 16, 7, 3, new Color(0.7f, 0.1f, 0.05f));
        // 牙齿
        FillPixel(tex, 8, 17, Color.white); FillPixel(tex, 10, 17, Color.white);
        FillPixel(tex, 12, 17, Color.white); FillPixel(tex, 14, 17, Color.white);

        // 前腿/臂 (粗壮，向前伸)
        FillRect(tex, 26, 8, 6, 5, body);
        FillRect(tex, 28, 3, 4, 5, body);
        // 前爪
        FillRect(tex, 27, 1, 5, 2, claw);
        FillPixel(tex, 27, 0, claw); FillPixel(tex, 29, 0, claw);
        FillPixel(tex, 31, 0, claw);

        // 后腿 (蹬地)
        FillRect(tex, 6, 2, 6, 5, body);
        FillRect(tex, 4, 0, 4, 3, body);
        FillRect(tex, 2, 0, 2, 2, claw);

        // 尾巴 (向上扬起)
        FillPixel(tex, 3, 8, body); FillPixel(tex, 2, 10, body);
        FillPixel(tex, 1, 12, body); FillPixel(tex, 0, 13, body);
        FillPixel(tex, 0, 14, body);
        FillRect(tex, 2, 6, 3, 2, body);

        // 背脊
        FillPixel(tex, 10, 25, new Color(0.5f, 0.2f, 0.6f));
        FillPixel(tex, 14, 26, new Color(0.5f, 0.2f, 0.6f));
        FillPixel(tex, 18, 24, new Color(0.5f, 0.2f, 0.6f));

        DrawOutline(tex, outline);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), Ppu);
    }

    /// <summary>创建地面纹理 (128×16，可平铺)</summary>
    public static Sprite CreateGroundSprite()
    {
        int w = 128, h = 16;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Clear(tex, Color.clear);

        Color dirtTop   = new Color(0.35f, 0.55f, 0.2f);  // 草地绿
        Color dirtMid   = new Color(0.55f, 0.35f, 0.15f); // 泥土棕
        Color dirtDeep  = new Color(0.3f, 0.2f, 0.1f);    // 深土
        Color stone     = new Color(0.5f, 0.48f, 0.45f);  // 石头

        // 草层
        for (int x = 0; x < w; x++)
        {
            float fbm = Mathf.PerlinNoise(x * 0.1f, 0) * 3f;
            FillPixel(tex, x, (int)(14 - fbm), dirtTop);
            FillPixel(tex, x, (int)(13 - fbm), dirtTop);
            FillPixel(tex, x, (int)(12 - fbm), dirtTop);
        }

        // 泥土层
        FillRect(tex, 0, 8, w, 5, dirtMid);
        FillRect(tex, 0, 4, w, 4, dirtDeep);
        FillRect(tex, 0, 0, w, 4, new Color(0.25f, 0.15f, 0.08f));

        // 随机石头
        for (int i = 0; i < 12; i++)
        {
            int sx = Random.Range(2, w - 4);
            int sy = Random.Range(8, 13);
            FillRect(tex, sx, sy, Random.Range(2, 4), Random.Range(1, 3), stone);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0, 0f), Ppu, 0, SpriteMeshType.FullRect);
    }

    /// <summary>创建背景天空渐变 (256×128)</summary>
    public static Sprite CreateBackgroundSprite()
    {
        int w = 256, h = 128;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color skyTop    = new Color(0.05f, 0.02f, 0.15f); // 深夜空
        Color skyMid    = new Color(0.1f, 0.05f, 0.3f);
        Color skyBottom = new Color(0.2f, 0.1f, 0.35f);

        for (int y = 0; y < h; y++)
        {
            float t = (float)y / h;
            Color c = Color.Lerp(skyBottom, skyTop, t);
            // 加些星点
            for (int x = 0; x < w; x++)
            {
                Color pixel = c;
                if (y > h * 0.6f && Random.value > 0.995f)
                    pixel = new Color(1f, 1f, 0.9f, Random.Range(0.3f, 0.9f)); // 星星
                tex.SetPixel(x, y, pixel);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
    }

    /// <summary>创建红色暗角覆盖 (64×64)</summary>
    public static Sprite CreateVignetteSprite()
    {
        int s = 64;
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float cx = s / 2f, cy = s / 2f;
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / (s * 0.7f);
                float alpha = Mathf.Clamp01(dist - 0.3f) * 0.8f;
                tex.SetPixel(x, y, new Color(0.8f, 0.05f, 0.05f, alpha));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 16);
    }

    // ========== 辅助方法 ==========

    private static void Clear(Texture2D tex, Color c)
    {
        for (int y = 0; y < tex.height; y++)
            for (int x = 0; x < tex.width; x++)
                tex.SetPixel(x, y, c);
    }

    private static void FillPixel(Texture2D tex, int x, int y, Color c)
    {
        if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
            tex.SetPixel(x, y, c);
    }

    private static void FillRect(Texture2D tex, int x, int y, int w, int h, Color c)
    {
        for (int dy = 0; dy < h; dy++)
            for (int dx = 0; dx < w; dx++)
                FillPixel(tex, x + dx, y + dy, c);
    }

    private static void DrawOutline(Texture2D tex, Color outline)
    {
        var orig = tex.GetPixels();
        for (int y = 1; y < tex.height - 1; y++)
        {
            for (int x = 1; x < tex.width - 1; x++)
            {
                int i = y * tex.width + x;
                if (orig[i].a > 0.5f) continue;
                // 检查 8 邻域
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int ni = (y + dy) * tex.width + (x + dx);
                        if (ni >= 0 && ni < orig.Length && orig[ni].a > 0.5f)
                        {
                            tex.SetPixel(x, y, outline);
                            goto NextPixel;
                        }
                    }
                }
                NextPixel:;
            }
        }
    }
}
