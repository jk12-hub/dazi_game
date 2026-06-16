# CLAUDE.md

本文件为 Claude Code (claude.ai/code) 在此仓库中工作时提供指导。

## 项目概述

打字逃生游戏 — 使用 **团结引擎 (Tuanjie) 1.6.10**（Unity 2022.3.61t11 中国版）制作的 2D 打字追逐游戏。玩家通过打字让角色向前跑，打字越快速度越快。身后怪物变速追赶（3段变速 + 正弦波动 + 随机尖刺 + 追赶修正）。

## 一键部署

**双击项目根目录下的 `build_and_deploy.bat`**，脚本会自动：
1. 找到本机的 Tuanjie/Unity Editor
2. 无头模式构建 WebGL
3. 自动 commit 并 push 到 GitHub

如果安装的是标准 Unity 2022.3.x（非团结引擎），需要把 `BuildScript.cs` 第 29 行的 `Tuanjie.exe` 改为 `Unity.exe`。

前提条件：Editor 必须装有 **WebGL Build Support** 模块。

## 编辑器内运行

用 Tuanjie Hub 打开项目，直接点 **Play**。`SceneSetup.cs` 中的 `SceneAutoBuilder` 会在运行时自动构建整个场景——无需任何手动搭场景操作。

## 初次配置

- 用 **Tuanjie Hub** 打开（版本 2022.3.61t11 / 1.6.10）
- 确保安装了 **WebGL Build Support**
- 如果 TMP 文字不显示：`Window > TextMeshPro > Import TMP Essential Resources`

## 架构

所有游戏逻辑在 `Assets/Scripts/` 中。**无手动场景搭建** — `SceneAutoBuilder` 在场景加载时用代码创建所有 GameObject、Canvas、UI 元素并连线引用。

### 核心系统（每帧数据流）

```
TypingEngine     →  打字输入 → CPM/连击计算
PlayerController →  玩家移动 (速度 ∝ CPM)
MonsterAI        →  怪物变速 AI (3段 + 波动 + 追赶加速)
DistanceManager  →  距离计算 → 区间判定 → 事件触发
FeedbackSystem   →  红雾/震动/消息/音效
UIManager        →  所有 UI 刷新
GameManager      →  主循环/状态机/相机跟随
```

### ScriptableObject（数据配置，纯代码，无需 .asset 文件）
- `GameData.cs` — 所有可调游戏参数
- `WordPool.cs` — 4 级难度英文词库

### 程序化图像生成
`SpriteGenerator.cs` 在运行时用代码生成所有像素精灵（玩家、怪物、地面、背景），不需要任何外部图片资源。

### 自动场景构建模式
`SceneSetup.cs` 包含 `SceneAutoBuilder` —— 使用 `[RuntimeInitializeOnLoadMethod]` 的静态类，在第一帧之前构建完整场景层级、Canvas UI（通过 TextMeshPro）并连线所有组件引用。克隆仓库后无需任何手动配置即可运行。

## 重要说明
- 这是**团结引擎项目，不是标准 Unity** — 使用 `https://packages.tuanjie.cn` 的包，需要 Tuanjie Hub 才能正常打开
- 所有 UI 使用 **TextMeshPro**，TMP Essential Resources 只需导入一次
- `build_and_deploy.bat` 把 WebGL 构建输出到 `WebGL/` 目录，然后 force-add 到 git（默认被 `.gitignore` 排除）
- `DistanceManager.Zone` 枚举（Critical/Danger/Safe/Far/TooFar）驱动所有视觉反馈——红雾强度、屏幕震动、消息文字都依赖它
- 怪物追赶修正：距离 > 12 时渐进加速，距离 < 8 时渐进减速（无瞬移，不破坏体验）
