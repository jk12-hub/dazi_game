# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

打字逃生游戏 — a 2D typing-chase game built with **团结引擎 (Tuanjie) 1.6.10** (Unity 2022.3.61t11 fork). The player types words to run; the faster they type, the faster the character runs. A monster chases from behind with variable speed AI (3 phases + sine wave + random spikes + catch-up boost).

## Key Commands

### Run in Editor
Open the project in Tuanjie Hub / Unity Editor and press **Play**. The `SceneAutoBuilder` script (in `SceneSetup.cs`) auto-builds the entire scene at runtime — no manual scene setup needed.

### WebGL Build & Deploy (一键部署)
Double-click `build_and_deploy.bat` in the project root. It will:
1. Find the Tuanjie/Unity Editor on your machine
2. Run a headless WebGL build
3. Commit and push to GitHub

Requires Tuanjie Editor with WebGL module installed.

### First-Time Setup
- Open project in **Tuanjie Hub** (version 2022.3.61t11 / 1.6.10)
- Ensure **WebGL Build Support** module is installed
- If TMP fonts don't render: `Window > TextMeshPro > Import TMP Essential Resources`

## Architecture

All game logic is in `Assets/Scripts/`. There is **no manual scene setup** — `SceneAutoBuilder` creates every GameObject, Canvas, and UI element programmatically on scene load.

### Core Systems (data flow per frame)

```
TypingEngine     →  打字输入 → CPM/连击计算
PlayerController →  玩家移动 (speed ∝ CPM)
MonsterAI        →  怪物变速 AI (3-phase + wave + chase boost)
DistanceManager  →  距离计算 → 区间判定 → 事件触发
FeedbackSystem   →  红雾/震动/消息/音效
UIManager        →  所有 UI 刷新
GameManager      →  主循环/状态机/相机跟随
```

### ScriptableObjects (data configs, code-only, no .asset files needed)
- `GameData.cs` — all tunable game parameters
- `WordPool.cs` — 4 difficulty tiers of English words

### Sprite Generation
`SpriteGenerator.cs` generates all pixel-art sprites (player, monster, ground, background) procedurally at runtime. No external image assets needed.

### SceneAutoBuilder Pattern
`SceneSetup.cs` contains `SceneAutoBuilder` — a static class with `[RuntimeInitializeOnLoadMethod]` that builds the entire scene hierarchy, Canvas UI (via TextMeshPro), and wires all component references before the first frame. This means the project works immediately after cloning, with zero manual configuration.

## Important Notes
- This is **not standard Unity** — it uses packages from `https://packages.tuanjie.cn`. Tuanjie Hub is required to open it.
- The project uses **TextMeshPro** for all UI text. TMP Essential Resources must be imported once.
- `build_and_deploy.bat` builds WebGL output into `WebGL/` folder, then force-adds it to git (ignored by default in `.gitignore`).
- The `DistanceManager.Zone` enum (Critical/Danger/Safe/Far/TooFar) drives all visual feedback — red vignette intensity, screen shake, and message text all read from it.
- Monster AI chase boost ramps up when distance > 12 and ramps down when < 8 (progressive, no teleportation).
