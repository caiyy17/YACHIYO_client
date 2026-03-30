# YACHIYO Client

A Unity client for real-time AI assistant interaction. Supports voice input, text display, audio playback, and character animation (both traditional and SMPL-H motion), with WebSocket and WebRTC server connections. Designed as the frontend counterpart to [YACHIYO Server](https://github.com/caiyy17/YACHIYO_server).

## Quick Start

1. Open project in **Unity 6000.3.10f1** (URP)
2. Configure server address in GameStart scene settings
3. Build or play from one of the sample scenes

## Scenes

| Scene | Description |
|-------|-------------|
| `GameStart` | Launcher with settings input and scene loading |
| `SampleScene3D_Default` | Standard action system with UnityChan character |
| `SampleScene3D_Smpl` | SMPL-H motion playback with humanoid character |
| `SampleScene3D_WebRTC` | Real-time bidirectional audio/video via WebRTC |

## Architecture

```
Microphone → Pipeline Modules → Server (WebSocket / WebRTC) → Pipeline Modules → Character
```

The client uses a **modular processing pipeline** (`ProcessingPipeline`) where messages flow through chained modules via thread-safe queues. Each module can capture specific signals, process data, or forward messages downstream.

### Pipeline Modules

| Module | Function |
|--------|----------|
| KWSModule | Keyword spotting (optional voice activation) |
| VADModule | Voice activity detection, emits recording_start/stop |
| RecordingModule | Captures microphone audio, encodes to base64 WAV |
| DirectSendModule | Sends predefined text messages (bypasses recording) |
| WebSocketClientModule | WebSocket connection to server |
| WebRTCClientModule | WebRTC connection with audio/video tracks + DataChannels |
| AudioModule | Decodes base64 audio responses, sequential playback |
| ContentModule | Text display via TextMesh Pro, action forwarding |
| ActionModule | Generic field consumer — extracts configured JSON fields and fires UnityEvents |
| SmplhActionModule | SMPL-H motion playback (replaces ActionModule for SMPL configs) |

### Pipeline Configuration (SMPL scene example)

```
KWSModule → VADModule → RecordingModule → WebSocketClientModule → AudioModule → ContentModule → SmplhActionModule
```

### Message Routing

- `destination = -2`: deliver to next module (default)
- `destination = -1`: skip all modules, deliver to pipeline tail
- `destination = N`: deliver to module at index N
- Unrecognized signals are auto-forwarded downstream

## Subsystems

### Audio Recording (`Recorder/`)

- **MicrophoneManager**: Singleton managing continuous mic input (16kHz)
- **VoiceDetector**: Loudness-based VAD with configurable thresholds
- **KeywordDetector**: Optional keyword spotting activation

### Streaming (`Stream/`)

- **WebRTCClient**: Bidirectional audio/video with two DataChannels (`client-signals` for VAD, `server-data` for responses)
- **WebSocketClient**: Alternative WebSocket transport
- **AudioLoader**: Sequential audio clip playback
- **ContentLoader**: Dynamic text display with action hints

### Model Control (`ModelControl/`)

- **ActionMap** (ScriptableObject): Reusable mapping from action keys → variants with layer-based priority
- **Anim3D**: Multi-target animation controller supporting multiple Animators (motion), SkinnedMeshRenderers (expression), and mouth lip-sync targets, each with its own ActionMap

### SMPL-H Motion System (`SmplhMotion/`)

- **SmplhMotionData**: Decoded SMPL-H parameters (poses/trans/betas as float arrays)
- **SmplhConverter**: Axis-angle → quaternion conversion with coordinate X-mirroring
- **SmplhMotionPlayer**: Frame-buffer playback with crossfade blending and auto-refill idle
- **IdleInitializer**: Loads pre-baked idle motion from `Resources/idle_motion.json`
- Used by `SmplhActionModule` for server-generated humanoid motions

### State Machine (`States/`)

| State | Description |
|-------|-------------|
| IdleState | Waiting; listens for activation signals |
| ReadyState | Preparation (checks AudioModule & character ready) |
| ListeningState | Recording in progress (VAD active) |
| AnsweringState | Server processing + response playback |

Managed by **YYStateManager** via **SignalManager** event routing.

### Settings (`Setting/`)

- **GameSettingsData**: Server URLs, user ID, pipeline config, character selection
- **CharacterSettingsData**: Per-character model, animator, action dict references
- **SetupPipeline**: Server health check → register → init_pipeline workflow

## Client-Server Flow

```
1. POST /register/                    → Register client
2. POST /init_pipeline/{client_id}    → Load pipeline config
3. WS /ws/{client_id}                 → Connect WebSocket (or WebRTC /offer)
4. Send: {"audio_file": "base64...", "timestamp": 123.45}
5. Recv: {"text": "...", "audio_data": "base64...", "action": "..."}
```

## Key Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Universal RP | 17.3.0 | Rendering pipeline |
| WebRTC | 3.0.0-pre.7 | Real-time communication |
| Input System | 1.18.0 | New Input System |
| Newtonsoft JSON | 3.2.2 | JSON serialization |
| uLipSync | — | Lip-sync from audio |
| UniVRM | — | VRM character support |

## Project Structure

```
Assets/Custom/
├── YACHIYO/
│   ├── ModelControl/    — Anim3D (multi-target animation), ActionMap (ScriptableObject)
│   ├── Pipeline/        — ProcessingPipeline, ProcessingModule, all modules
│   ├── Recorder/        — MicrophoneManager, VoiceDetector, KeywordDetector
│   ├── Setting/         — Game/character settings, pipeline setup
│   ├── SmplhMotion/     — SMPL-H converter, player, idle initializer
│   ├── States/          — State machine (Idle/Ready/Listening/Answering)
│   ├── Stream/          — WebRTC/WebSocket clients, AudioLoader, ContentLoader
│   └── Utils/           — SignalManager, custom event types, utilities
├── Scripts/
│   ├── Editor/          — FBX animation tools
│   ├── GameStart/       — Launcher UI, scene loading
│   └── SampleScene/     — Scene-specific UI scripts
└── UXUI/                — Audio button, loading screen, UI components
```

## License

This project's source code is licensed under the [MIT License](LICENSE).

### Third-Party Assets

The following assets are subject to their own licenses:

- **Unity-Chan** (`Assets/UnityChan/`): © Unity Technologies Japan/UCL — [Unity-Chan License Terms (UCL 2.02)](https://unity-chan.com/contents/license_jp/)
- **40+ Simple Icons - Free** (`Assets/40+ Simple Icons - Free/`): [Unity Asset Store EULA](https://unity.com/legal/as-terms)
- **SmileySans / Source Han Sans** (`Assets/Fonts/`): SIL Open Font License (OFL)
