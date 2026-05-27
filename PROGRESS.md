# 进度

## 已完成：WebcamManager 单例 + 摄像头选择 UI

### 新增文件
- `Assets/Custom/YACHIYO/Recorder/WebcamManager.cs` — 摄像头管理单例（同 MicrophoneManager 模式）
  - 设备枚举、启停、切换、分辨率设置
  - PlayerPrefs 持久化（`webcamDeviceName`、`webcamWidth`、`webcamHeight`）
  - 移动端显示 Front/Back 后缀
  - Awake 不自动启动，由 WebRTCClient 按需调用

### WebRTCClient.cs 修改
- 移除 `_webcamTex`、`webcamDeviceName`、`webcamWidth`、`webcamHeight` 字段
- `SetupVideoTrack()` Webcam 分支改为调用 `WebcamManager.Instance.StartCapture()`
- `Update()` 的 Blit 改用 `WebcamManager.Instance.WebcamTexture`
- `Cleanup()` 改为调用 `WebcamManager.Instance.StopCapture()`

### GameStartUI.cs 修改
- 新增 `public TMP_Dropdown webcamDropdown` 字段
- AppSettings 打开时填充摄像头下拉列表（显示名 + Front/Back）
- AppSettings 关闭时应用选择，通过 `WebcamManager.Instance.SwitchDevice()` 切换

### 场景配置（需手动）
- 在持久化 GameObject 上添加 WebcamManager 组件（与 MicrophoneManager 同级）
- 在 GameStart 设置面板中添加 TMP_Dropdown 并绑定到 `webcamDropdown` 字段

## 已完成：ActionModule 通用化 + Anim3D 整合

### ActionModule.cs
- 单字段消费管线模块：一个 `fieldName` + 一个 `onValue` UnityEvent + `sosValue`/`eosValue`
- 过滤空字符串值（防止 LLM 空字段触发事件）
- 多个 ActionModule 链式使用（action → expression）

### Anim3D.cs
- **MotionTarget**：Animator + ActionMap，trigger 缓存，idle 超时（20s）
- **ExpressionTarget**：SkinnedMeshRenderer + ActionMap，平滑 blendshape 过渡（Lerp），表情超时（5s）自动回 neutral
- **BlinkTarget**：定时自动眨眼，单 timer 驱动所有 target，与 expression 互斥（expression 激活时暂停眨眼）
- **MouthTarget**：音频 RMS + EMA 平滑口型同步，每个 target 独立阈值

### SmplhMotionPlayer.cs
- 新增 `PlayMotion(string actionJson)` 方法，ActionModule 直接调用
- SmplhActionModule 已删除（逻辑吸收）

### ActionMap 资产
- `Models/MapExpressionUnityChan.asset`：UnityChan MTH_DEF blendshape 映射（14 条）

### 已删除脚本
- `Action/ActionLoader.cs`、`Action/ActionDict.cs`、`Action/MouthAnim.cs`
- `Scripts/AutoBlink.cs`、`Scripts/MMDExpressionModule.cs`
- `Pipeline/Modules/SmplhActionModule.cs`

### 场景配置
所有场景：pipeline 末尾为 ActionModule(action) → ActionModule(expression)，expression eosValue=neutral

| 场景 | Action 目标 | Anim3D motion | Blink targets |
|------|------------|---------------|---------------|
| Scenes/Default | SetMotion (Anim3D) | Animator + map | 2 个 (idx 6) |
| Scenes/Live | PlayMotion (SmplhPlayer) | 空 | 2 个 (idx 6) |
| Scenes/Smpl | PlayMotion (SmplhPlayer) | 空 | 2 个 (idx 6) |
