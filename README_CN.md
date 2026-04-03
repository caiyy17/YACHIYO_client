# YACHIYO Client

实时 AI 助手的 Unity 客户端。支持语音输入、文本显示、音频播放和角色动画（传统动画触发和 SMPL-H 动作生成），提供 WebSocket 和 WebRTC 两种服务器连接方式。作为 [YACHIYO Server](https://github.com/caiyy17/YACHIYO_server) 的前端配套。

## 快速开始

1. 使用 **Unity 6000.3.10f1**（URP）打开项目
2. 在 GameStart 场景中配置服务器地址
3. 从示例场景构建或运行

## 场景

| 场景 | 说明 |
|------|------|
| `GameStart` | 启动器，设置输入和场景加载 |
| `SampleScene3D_Default` | 传统动画系统 + UnityChan 角色 |
| `SampleScene3D_Live` | 直播模式 — 通过 gateway 获取外部交互信息，SMPL-H 动作 + UnityChan |
| `SampleScene3D_Smpl` | Default 的 SMPL-H 动作版本（用 SMPL-H 替代传统动画） |
| `SampleScene3D_WebRTC` | 基于 WebRTC 的实时双向音视频 |

## 架构

```
麦克风 → Pipeline 模块 → 服务器 (WebSocket / WebRTC) → Pipeline 模块 → 角色
```

客户端使用**模块化处理 pipeline**（`ProcessingPipeline`），消息通过线程安全队列在链式模块之间流动。每个模块可以捕获特定信号、处理数据或将消息转发给下游。

### Pipeline 模块

| 模块 | 功能 |
|------|------|
| KWSModule | 关键词唤醒（可选语音激活） |
| VADModule | 语音活动检测，发出 recording_start/stop 信号 |
| RecordingModule | 捕获麦克风音频，编码为 base64 WAV |
| DirectSendModule | 发送预设文本消息（跳过录音流程） |
| WebSocketClientModule | WebSocket 服务器连接 |
| WebRTCClientModule | WebRTC 连接，含音视频轨道 + DataChannel |
| AudioModule | 解码 base64 音频响应，顺序播放 |
| ContentModule | 通过 TextMesh Pro 显示文本 |
| ActionModule | 单字段消费模块 — 提取 JSON 中指定字段并触发 UnityEvent |

### 消息路由

- `destination = -2`：传递给下一个模块（默认）
- `destination = -1`：跳过所有模块，直送 pipeline 末端
- `destination = N`：传递给索引为 N 的模块
- 未识别的信号自动向下游转发

## 子系统

### 音频录制（`Recorder/`）

- **MicrophoneManager**：单例管理连续麦克风输入（16kHz）
- **VoiceDetector**：基于响度的 VAD，支持可配置阈值
- **KeywordDetector**：可选的关键词唤醒检测

### 流传输（`Stream/`）

- **WebRTCClient**：双向音视频，支持可配置视频源：
  - 视频源模式：`None`（空白占位）/ `Camera`（Unity 摄像机）/ `Webcam`（物理摄像头）
  - 可配置分辨率和帧率（需与服务器 pipeline 配置一致）
  - Inspector 中实时显示收发分辨率和帧率（通过 WebRTC GetStats API）
  - 音频固定 48kHz（Unity WebRTC 限制）
- **WebSocketClient**：替代 WebRTC 的 WebSocket 传输方案

### NDI 输出（`Scripts/`）

- **NdiSendManager**：通过 KlakNDI 从 Unity 摄像机创建 NDI 发送流，每个摄像机成为独立 NDI 源

### 模型控制（`ModelControl/`）

- **ActionMap**（ScriptableObject）：可复用的动作键 → 变体映射，支持 layer 优先级
- **Anim3D**：多目标动画控制器（motion、expression、blink、口型同步）

### SMPL-H 动作系统（`SmplhMotion/`）

- **SmplhMotionData**：解码后的 SMPL-H 参数（poses/trans/betas 浮点数组）
- **SmplhConverter**：轴角 → 四元数转换，坐标 X 轴镜像
- **SmplhMotionPlayer**：帧缓冲播放，支持交叉淡入淡出和自动补充 idle
- **IdleInitializer**：从 `Resources/idle_motion.json` 加载预烘焙的 idle 动作

### 状态机（`States/`）

| 状态 | 说明 |
|------|------|
| IdleState | 等待状态，监听激活信号 |
| ReadyState | 准备阶段（检查 AudioModule 和角色就绪） |
| ListeningState | 录音中（VAD 激活） |
| AnsweringState | 服务器处理 + 响应播放 |

由 **YYStateManager** 管理，通过 **SignalManager** 进行事件路由。

### 设置（`Setting/`）

- **GameSettingsData**：服务器地址、用户 ID、pipeline 配置、角色选择
- **CharacterSettingsData**：每个角色的模型、Animator、ActionDict 引用
- **SetupPipeline**：服务器健康检查 → 注册 → 初始化 pipeline 流程

## 客户端-服务器流程

### WebSocket 模式

```
1. POST /register/                 → 注册客户端
2. POST /init_pipeline/{client_id} → 加载 pipeline 配置
3. WS   /ws/{client_id}           → 双向消息通信
```

### WebRTC 模式

```
1. POST /register/                 → 注册客户端
2. POST /init_pipeline/{client_id} → 加载 pipeline 配置
3. POST /offer/{client_id}        → SDP offer + 视频参数（fps/width/height）
4. 双向流：音频轨道 + 视频轨道 + DataChannel
```

视频参数（`video_fps`、`video_width`、`video_height`）需在 Unity 客户端、服务器默认值和 pipeline 配置（`frame_splitter` 节点）之间保持一致。

## 主要依赖

| 包 | 版本 | 用途 |
|----|------|------|
| Universal RP | 17.3.0 | 渲染管线 |
| WebRTC | 3.0.0-pre.7 | 实时通信 |
| KlakNDI | 2.1.5 | NDI 视频输出 |
| Input System | 1.18.0 | 输入处理 |
| Newtonsoft JSON | 3.2.2 | JSON 序列化 |

## 项目结构

```
Assets/Custom/
├── YACHIYO/
│   ├── ModelControl/    — Anim3D（多目标动画）、ActionMap（ScriptableObject）
│   ├── Pipeline/        — ProcessingPipeline、ProcessingModule、所有模块
│   ├── Recorder/        — MicrophoneManager、VoiceDetector、KeywordDetector
│   ├── Setting/         — 游戏/角色设置、pipeline 初始化
│   ├── SmplhMotion/     — SMPL-H 转换器、播放器、idle 初始化
│   ├── States/          — 状态机（Idle/Ready/Listening/Answering）
│   ├── Stream/          — WebRTC/WebSocket 客户端
│   └── Utils/           — SignalManager、自定义事件类型、工具函数
├── Scripts/
│   ├── GameStart/       — 启动器 UI、场景加载
│   ├── SampleScene/     — 场景专用 UI 脚本
│   └── NdiSendManager   — NDI 摄像机输出
Assets/Models/           — ActionMap 资产、设置数据
Assets/UXUI/             — UI 组件、图标、背景
```

## 许可证

本项目源代码基于 [MIT License](LICENSE) 开源。

### 第三方资产

- **Unity-Chan**（`Assets/UnityChan/`）：© Unity Technologies Japan/UCL — [Unity-Chan License Terms (UCL 2.02)](https://unity-chan.com/contents/license_jp/)
- **40+ Simple Icons - Free**（`Assets/40+ Simple Icons - Free/`）：[Unity Asset Store EULA](https://unity.com/legal/as-terms)
- **SmileySans / Source Han Sans**（`Assets/Fonts/`）：SIL Open Font License (OFL)
