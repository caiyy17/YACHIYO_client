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
| `SampleScene3D_Smpl` | SMPL-H 动作回放 + humanoid 角色 |
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
| ContentModule | 通过 TextMesh Pro 显示文本，转发动作数据 |
| ActionModule | 传统动画触发，通过 ActionDict 查表 |
| SmplhActionModule | SMPL-H 动作回放（替代 ActionModule 用于 SMPL 配置） |

### Pipeline 配置（SMPL 场景示例）

```
KWSModule → VADModule → RecordingModule → WebSocketClientModule → AudioModule → ContentModule → SmplhActionModule
```

### 消息路由

- `destination = -2`：传递给下一个模块（默认）
- `destination = -1`：跳过所有模块，直送 pipeline 末端
- `destination = N`：传递给索引为 N 的模块
- 未被识别的信号会自动转发给下游

## 子系统

### 音频录制（`Recorder/`）

- **MicrophoneManager**：单例管理连续麦克风输入（16kHz）
- **VoiceDetector**：基于响度的 VAD，支持可配置阈值
- **KeywordDetector**：可选的关键词唤醒检测

### 流传输（`Stream/`）

- **WebRTCClient**：双向音视频，两个 DataChannel（`client-signals` 发送 VAD 信号，`server-data` 接收响应）
- **WebSocketClient**：替代 WebRTC 的 WebSocket 传输方案
- **AudioLoader**：音频片段顺序播放
- **ContentLoader**：动态文本显示，支持动作提示

### 动作系统（`Action/`）

- **ActionDict**（ScriptableObject）：将动作名称映射到动画/表情列表，支持基于 layer 的优先级
- **ActionLoader**：查找动作字符串，从候选列表中随机选择，通过 UnityEvent 触发 Animator
- 由 `ActionModule` 用于传统动画触发流程

### SMPL-H 动作系统（`SmplhMotion/`）

- **SmplhMotionData**：解码后的 SMPL-H 参数（poses/trans/betas 为 float 数组）
- **SmplhConverter**：轴角 → 四元数转换，坐标 X 轴镜像
- **SmplhMotionPlayer**：帧缓冲播放，支持交叉淡入淡出和自动补充 idle 动作
- **IdleInitializer**：从 `Resources/idle_motion.json` 加载预烘焙的 idle 动作
- 由 `SmplhActionModule` 用于服务器生成的 humanoid 动作

### 状态机（`States/`）

| 状态 | 说明 |
|------|------|
| IdleState | 等待状态，监听激活信号 |
| ReadyState | 准备阶段（检查 AudioModule 和角色就绪） |
| ListeningState | 录音中（VAD 激活） |
| AnsweringState | 服务器处理 + 响应播放 |

由 **YYStateManager** 通过 **SignalManager** 事件路由管理。

### 设置（`Setting/`）

- **GameSettingsData**：服务器地址、用户 ID、pipeline 配置、角色选择
- **CharacterSettingsData**：每个角色的模型、Animator、ActionDict 引用
- **SetupPipeline**：服务器健康检查 → 注册 → 初始化 pipeline 流程

## 客户端-服务器流程

```
1. POST /register/                    → 注册客户端
2. POST /init_pipeline/{client_id}    → 加载 pipeline 配置
3. WS /ws/{client_id}                 → 连接 WebSocket（或 WebRTC /offer）
4. Send: {"audio_file": "base64...", "timestamp": 123.45}
5. Recv: {"text": "...", "audio_data": "base64...", "action": "..."}
```

## 主要依赖

| 包 | 版本 | 用途 |
|----|------|------|
| Universal RP | 17.3.0 | 渲染管线 |
| WebRTC | 3.0.0-pre.7 | 实时通信 |
| Input System | 1.18.0 | 新版输入系统 |
| Newtonsoft JSON | 3.2.2 | JSON 序列化 |
| uLipSync | — | 音频口型同步 |
| UniVRM | — | VRM 角色支持 |

## 项目结构

```
Assets/Custom/
├── YACHIYO/
│   ├── Action/          — ActionDict、ActionLoader、动画控制
│   ├── ModelControl/    — Anim3D（直接 Animator 控制）
│   ├── Pipeline/        — ProcessingPipeline、ProcessingModule、所有模块
│   ├── Recorder/        — MicrophoneManager、VoiceDetector、KeywordDetector
│   ├── Setting/         — 游戏/角色设置、pipeline 初始化
│   ├── SmplhMotion/     — SMPL-H 转换器、播放器、idle 初始化
│   ├── States/          — 状态机（Idle/Ready/Listening/Answering）
│   ├── Stream/          — WebRTC/WebSocket 客户端、AudioLoader、ContentLoader
│   └── Utils/           — SignalManager、自定义事件类型、工具函数
├── Scripts/
│   ├── Editor/          — FBX 动画工具
│   ├── GameStart/       — 启动器 UI、场景加载
│   └── SampleScene/     — 场景专用 UI 脚本
└── UXUI/                — 音频按钮、加载画面、UI 组件
```

## 许可证

本项目源代码基于 [MIT License](LICENSE) 开源。

### 第三方资产

以下资产适用各自的许可证：

- **Unity-Chan**（`Assets/UnityChan/`）：© Unity Technologies Japan/UCL — [Unity-Chan License Terms (UCL 2.02)](https://unity-chan.com/contents/license_jp/)
- **40+ Simple Icons - Free**（`Assets/40+ Simple Icons - Free/`）：[Unity Asset Store EULA](https://unity.com/legal/as-terms)
- **SmileySans / Source Han Sans**（`Assets/Fonts/`）：SIL Open Font License (OFL)
