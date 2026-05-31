# Quoridor

这是一个使用 Unity 6.3 制作的 2D Quoridor（步步为营）桌游 Demo。项目支持本地双人对战、局域网房间发现、角色选择、棋子移动、墙体放置、路径合法性验证、胜负判定和基础菜单流程。

## 开发环境

- Unity：6.3 LTS
- 语言：C#
- 渲染：Unity 2D，项目内保留 URP 配置
- 版本管理：本地 Git 仓库
- 联机方案：Mirror 局域网发现和房间流程

## 运行方式

1. 使用 Unity 打开项目。
2. 打开 `Assets/Scenes/MainMenu.unity`。
3. 点击 Play。
4. 选择 `双人游戏`。
5. 选择 `本地` 进入本地双人，或选择 `局域网` 进入局域网大厅。

## 操作说明

- 左键点击：移动到高亮格子，或放置当前墙体预览
- Tab：切换移动模式和放墙模式
- R：旋转墙体方向
- Player 1 胜利条件：到达 `y = 8`
- Player 2 胜利条件：到达 `y = 0`

## 局域网测试流程

1. 从 Unity 构建一个独立客户端。
2. 同时运行 Unity Editor 和构建出来的客户端。
3. 在主机端进入局域网大厅并创建房间。
4. 在客户端进入局域网大厅并加入发现到的房间。
5. 双方选择角色后开始游戏。
6. 分别验证移动、放墙、回合切换、胜利和返回大厅。

建议同时测试两种方向：

- Unity Editor 作为主机，构建客户端加入
- 构建客户端作为主机，Unity Editor 加入

## 当前功能

- 9x9 棋盘
- 本地双人对战
- 局域网房间发现和加入
- 角色选择
- HUD 头像和棋子角色显示
- 棋子合法移动
- 横向和纵向墙体放置
- 墙体边界、重叠、交叉和路径合法性验证
- BFS 路径验证
- 墙体剩余数量显示
- 回合切换
- 胜利弹窗和返回大厅
- 距离胜利最短路径 4 步以内的视觉提醒
- 显示设置中的窗口、全屏和分辨率选项

## 项目结构

```text
Assets/
  Art/
    Board/        棋盘贴图
    Characters/   角色贴图
    Materials/    运行时材质
    Pawn/         棋子备用贴图
    Shaders/      选中和临近终点特效
    UI/           HUD 和菜单框体资源
    Wall/         墙体贴图
  Config/         ScriptableObject 配置
  Prefabs/
    Board/        Cell 预制体
    Pawn/         Pawn 预制体
    UI/           HUD 和房间玩家预制体
    Wall/         Wall 预制体
  Scenes/         MainMenu 和 QuoridorDemo 场景
  Scripts/
    Board/        棋盘和格子视图
    Config/       配置定义和本地角色选择缓存
    Core/         纯 C# 规则层
    Editor/       编辑期棋盘生成工具
    GameFlow/     对局流程控制
    Input/        集中输入路由
    Menu/         主菜单和显示设置
    Network/      Mirror 局域网和对局同步
    Pawn/         棋子控制和表现
    UI/           对局 HUD 绑定
    Wall/         墙体控制和表现
  Tests/
    EditMode/     核心规则测试
```

## 架构说明

- `Assets/Scripts/Core` 是纯 C# 规则层，不依赖 `MonoBehaviour`、场景对象或 Unity UI。
- 场景对象主要通过 Inspector 序列化引用连接，避免运行时到处查找对象。
- 棋盘格子由 `Assets/Scripts/Editor/BoardGeneration` 在编辑期生成，不在运行时动态生成整张棋盘。
- 输入由 `InputRouter` 统一路由，避免每个 View 分散写输入轮询。
- 墙体校验拆分为边界、重叠、交叉和路径验证。
- 对局控制、棋子表现、墙体表现、HUD 和菜单逻辑分散在小类中，避免单个巨型管理器。

## Git Flow

项目采用 `develop` 作为开发主线，功能和修复通过短生命周期分支合并。当前版本等待审核后才能合并到 `main`，并在确认发布时打 `v0.1.0` 标签。

## v0.1.0 验收清单

- 打开 `MainMenu` 后菜单可正常切换。
- 本地双人可以移动和放墙。
- 墙体不能越界、重叠、交叉或完全封死路径。
- 游戏结束后可以返回大厅。
- Unity Editor 作为主机时，构建客户端可以加入并游玩。
- 构建客户端作为主机时，Unity Editor 可以加入并游玩。
- 角色选择能同步到 HUD 和棋子显示。
- 显示设置在窗口和全屏模式下布局正常。
